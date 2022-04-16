using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyShoppingCart.Data;
using MyShoppingCart.Models;
using Quartz;
using Quartz.Spi;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Helpers.Jobs
{
    public class PayoutsJob : IJob
    {

        private readonly IServiceProvider _provider;
        static IConfiguration _configuration = Startup.StaticConfig;
        public PayoutsJob(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Log.Information("Payouts job starting.");

            AppDbContext dbContext = null;

            using (var scope = _provider.CreateScope())
            {
                dbContext = scope.ServiceProvider.GetService<AppDbContext>();
                //var emailSender = scope.ServiceProvider.GetService<IEmailSender>();
                // fetch customers, send email, update DB

                List<ApplicationUser> sellers = await GetListOfSellersAsync(dbContext);

                foreach (ApplicationUser seller in sellers)
                {
                    List<OrderItem> orderitems = await GetListOfUnpaidItemsForSellerAsync(dbContext, seller.Id);

                    if (orderitems.Count == 0)
                        continue;

                    var result = await Paypal.CaptureOrderSample.CreatePayout(
                        $"You received a payout from {seller.Subdomain}.myshoppingcart.biz",
                        $"You received a payout from {seller.Subdomain}.myshoppingcart.biz",
                        orderitems.Sum(x => x.Amount * x.Price).ToString(),
                        seller.Email);

                    Payout payout = new Payout
                    {
                        applicationUser = seller,
                        BatchId = result.BatchHeader.PayoutBatchId,
                        BatchStatus = result.BatchHeader.BatchStatus,
                        EmailMessage = CreatePayoutEmail(seller, orderitems),
                        EmailSubject = "You have received a payout from your store",
                        payoutAmount = orderitems.Sum(x => x.Amount * x.Price),
                        payoutDate = DateTime.UtcNow
                    };

                    dbContext.Payouts.Update(payout);
                    await dbContext.SaveChangesAsync();

                    foreach (OrderItem orderitem in orderitems)
                    {
                        orderitem.payout = payout;
                        dbContext.OrderItems.Update(orderitem);
                        dbContext.SaveChanges();
                    }

                    await SendSellerEmail(seller, payout.EmailMessage, payout.EmailSubject);

                    Log.Information($"Payout Id:{payout.Id} User Id:{seller.Id} Date:{DateTime.UtcNow}");
                }
            }

            Log.Information("Payouts job finished.");

            //Debug.WriteLine($"Status: {result.BatchHeader.BatchStatus}");
            //Debug.WriteLine($"Batch Id: {result.BatchHeader.PayoutBatchId}");
            //Debug.WriteLine("Links:");
            //foreach (PayoutsSdk.Payouts.LinkDescription link in result.Links)
            //{
            //    Debug.WriteLine($"\t{link.Rel}: {link.Href}\tCall Type: {link.Method}");
            //}
        }

        private static async Task<List<ApplicationUser>> GetListOfSellersAsync(AppDbContext context) => await context.Users.Where(x => x.Subdomain != null).ToListAsync();

        private static async Task<List<OrderItem>> GetListOfUnpaidItemsForSellerAsync(AppDbContext context, string SellerUserId) => await context.OrderItems.Where(x => x.Product.applicationUser.Id == SellerUserId && x.payout == null).Include(x => x.Product).Include(x => x.Order).ToListAsync();


        private string CreatePayoutEmail(ApplicationUser seller, List<OrderItem> orderitems)
        {
            orderitems = orderitems.OrderBy(x => x.Order.OrderDate).ThenBy(x => x.Order.Id).ToList();

            string imgSource = $"{_configuration.GetValue<string>("StorageContainerURL")}/store-logo.png";

            string sHTMLContent = $"<img src='{imgSource}' style='max-height:75px;width:auto' alt='store logo' />";

            sHTMLContent += $"<h3>Congratulations {seller.FullName}! You have received a payout in your Paypal account!</h3>";
            sHTMLContent += "See details below:<br><br>";

            sHTMLContent += "<table style = 'Width:600px;border: 1px solid black;'>";
            sHTMLContent += "<thead>";
            sHTMLContent += "<tr>";

            sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Order Date</th>";
            sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Order Number</th>";
            sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Product Id</th>";
            sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Product Name</th>";
            sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Quantity</th>";
            sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Price</th>";
            sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Subtotal</th>";

            sHTMLContent += "</tr>";
            sHTMLContent += "</thead>";
            sHTMLContent += "<tbody>";

            decimal total = 0;

            foreach (OrderItem item in orderitems)
            {
                sHTMLContent += "<tr>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Order.OrderDate.ToLocalTime():g}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Order.Id}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Product.Id}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Product.Name}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Amount}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Product.Price:c}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Amount * item.Product.Price:c}</td>";
                sHTMLContent += "</tr>";

                total += (item.Amount * item.Product.Price);
            }
            sHTMLContent += "</tbody>";
            sHTMLContent += "</table>";

            sHTMLContent += $"<br><br>Total Payout: {total:c}";

            return sHTMLContent;
        }

        private async Task SendSellerEmail(ApplicationUser user, string sHTMLContent, string Subject)
        {
            string sPlainTextContent = "";


            await SendGridStatic.Execute(_configuration.GetValue<string>("SendGridKey"), "customerservice@myshoppingcart.biz", "Customer Service", Subject, user.Email, user.FullName, sPlainTextContent, sHTMLContent);

        }

    }

    public class SingletonJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public SingletonJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job) { }
    }
}

