using Microsoft.Extensions.Configuration;
using PayoutsJobConsoleApp.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PayoutsJobConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            PayoutsJob payoutsJob = new PayoutsJob();
            payoutsJob.Execute().Wait();
        }
    }

    class PayoutsJob
    {
        public static IConfiguration _configuration { get; } = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                        .Build();

        string _connectionString = _configuration.GetConnectionString("DefaultConnectionString");

        public async Task Execute()
        {
            int iRowCount = 0;

            Console.WriteLine("Payouts job starting.");

            List<ApplicationUser> sellers = await GetListOfSellersAsync();

            foreach (ApplicationUser seller in sellers)
            {
                List<OrderItem> orderitems = await GetListOfUnpaidItemsForSellerAsync(seller.Id);

                if (orderitems.Count == 0)
                    continue;

                var result = await CaptureOrderSample.CreatePayout(
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

                iRowCount = await InsertPayoutRecordAsync(payout);

                foreach (OrderItem orderitem in orderitems)
                {
                    orderitem.PayoutId = payout.Id;
                    iRowCount = await UpdateOrderItemRecordAsync(orderitem);
                }

                await SendSellerEmail(seller, payout.EmailMessage, payout.EmailSubject);

                Console.WriteLine($"Payout Id:{payout.Id} User Id:{seller.Id} Date:{DateTime.UtcNow}");
            }


            Console.WriteLine("Payouts job finished.");

            //Debug.WriteLine($"Status: {result.BatchHeader.BatchStatus}");
            //Debug.WriteLine($"Batch Id: {result.BatchHeader.PayoutBatchId}");
            //Debug.WriteLine("Links:");
            //foreach (PayoutsSdk.Payouts.LinkDescription link in result.Links)
            //{
            //    Debug.WriteLine($"\t{link.Rel}: {link.Href}\tCall Type: {link.Method}");
            //}
        }

        private async Task<List<ApplicationUser>> GetListOfSellersAsync() 
        {
            List<ApplicationUser> applicationUsers = new List<ApplicationUser>();
            string sSQL = "SELECT * " +
                          "FROM AspNetUsers " +
                          "WHERE Subdomain IS NOT NULL";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sSQL, conn))
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                ApplicationUser applicationUser = new ApplicationUser
                                {
                                    Address1 = reader["Address1"].ToString(),
                                    Address2 = reader["Address2"].ToString(),
                                    CertificateBound = (bool)reader["CertificateBound"],
                                    CertificateExists = (bool)reader["CertificateExists"],
                                    City = reader["City"].ToString(),
                                    Domain = reader["Domain"].ToString(),
                                    DomainBound = (bool)reader["DomainBound"],
                                    FullName = reader["FullName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Id = reader["Id"].ToString(),
                                    newUserName = reader["newUserName"].ToString(),
                                    State = reader["State"].ToString(),
                                    Subdomain = reader["Subdomain"].ToString(),
                                    Zip = reader["Zip"].ToString()
                                };
                                applicationUsers.Add(applicationUser);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error while trying to download Sellers from Payouts Job: {ex.Message}");
            }
            return applicationUsers;
        }

        private async Task<List<OrderItem>> GetListOfUnpaidItemsForSellerAsync(string SellerUserId) 
        {
            List<OrderItem> orderItems = new List<OrderItem>();

            string sSQL = string.Format("SELECT oi.Id, o.Id AS OrderID, o.OrderDate, p.Id AS ProductId, p.Name, oi.Amount, oi.Price " +
                            "FROM OrderItems oi " +
                                "INNER JOIN Orders o " +
                                    "ON oi.OrderId = o.Id " +
                                "INNER JOIN Products p " +
                                    "ON oi.ProductId = p.Id " +                                
                            "WHERE p.applicationUserId = '{0}' " +
                                "AND oi.payoutId IS NULL", SellerUserId);

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sSQL, conn))
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                OrderItem orderItem = new OrderItem
                                {
                                    Amount = (int)reader["Amount"],
                                    Id = (int)reader["Id"],
                                    Name= (string)reader["Name"],
                                    OrderDate = (DateTime)reader["OrderDate"],
                                    OrderId = (int)reader["OrderId"],
                                    Price = (decimal)reader["Price"],
                                    ProductId = (int)reader["ProductId"]
                                };
                                orderItems.Add(orderItem);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while trying to download unpaid items from Payouts Job: {ex.Message}");
            }
            return orderItems;
            //await context.OrderItems.Where(x => x.Product.applicationUser.Id == SellerUserId && x.payout == null).Include(x => x.Product).Include(x => x.Order).ToListAsync(); 
        }

        private async Task<int> InsertPayoutRecordAsync(Payout payout)
        {            
            string sSQL = "INSERT INTO Payouts ([applicationUserId],[payoutAmount],[payoutDate],[BatchStatus],[BatchId],[EmailMessage],[EmailSubject]) " +
                                        "VALUES(@applicationUserId,@payoutAmount,@payoutDate,@BatchStatus,@batchId,@EmailMessage,@EmailSubject) " +
                                        "SELECT SCOPE_IDENTITY()";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sSQL, conn)) 
                    { 
                    
                        
                        cmd.Parameters.AddWithValue("@applicationUserId", payout.applicationUser.Id);
                        cmd.Parameters.AddWithValue("@payoutAmount", payout.payoutAmount);
                        cmd.Parameters.AddWithValue("@payoutDate", payout.payoutDate);
                        cmd.Parameters.AddWithValue("@BatchStatus", payout.BatchStatus);
                        cmd.Parameters.AddWithValue("@batchId", payout.BatchId);
                        cmd.Parameters.AddWithValue("@EmailMessage", payout.EmailMessage);
                        cmd.Parameters.AddWithValue("@EmailSubject", payout.EmailSubject);
                        var ret = await cmd.ExecuteScalarAsync();

                        payout.Id = int.Parse(ret.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while trying to insert payout record from Payouts Job: {ex.Message}");
            }
            return payout.Id;            
        }

        private async Task<int> UpdateOrderItemRecordAsync(OrderItem orderItem)
        {
            int iRowCount = 0;
            string sSQL = string.Format("UPDATE [dbo].[OrderItems] " +
                                           "SET[payoutId] = {0} " +
                                           "WHERE Id = {1} "
                                        , orderItem.PayoutId, orderItem.Id); ;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sSQL, conn))
                    {
                        iRowCount = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while trying to update Order Item record from Payouts Job: {ex.Message}");
            }
            return iRowCount;
        }


        private string CreatePayoutEmail(ApplicationUser seller, List<OrderItem> orderitems)
        {
            orderitems = orderitems.OrderBy(x => x.OrderDate).ThenBy(x => x.OrderId).ToList();

            string imgSource = $"{_configuration.GetValue<string>("StorageContainerURL")}/site-files/store-logo.png";

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
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.OrderDate.ToLocalTime():g}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.OrderId}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.ProductId}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Name}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Amount}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Price:c}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Amount * item.Price:c}</td>";
                sHTMLContent += "</tr>";

                total += (item.Amount * item.Price);
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

    public static class SendGridStatic
    {
        public static async Task Execute(string sSendGridKey, string sFromEmail, string sFromName, string sSubject, string sToEmail, string sToName, string sPlainTextContent = "", string sHTMLContent = "")
        {
            var apiKey = sSendGridKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(sFromEmail, sFromName);
            var subject = sSubject;
            var to = new EmailAddress(sToEmail, sToName);
            var plainTextContent = sPlainTextContent;
            var htmlContent = sHTMLContent;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
