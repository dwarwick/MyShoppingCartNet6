using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyShoppingCart.Data;
using MyShoppingCart.Helpers.AzureManagementAPI;
using MyShoppingCart.Models;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyShoppingCart.Helpers.Jobs
{
    //public class BindCertificateJob : IJob
    //{
    //    private readonly IServiceProvider _provider;        
    //    static IConfiguration _configuration = Startup.StaticConfig;

    //    public BindCertificateJob(IServiceProvider provider)
    //    {
    //        _provider = provider;           
    //    }

    //    //public async Task Execute(IJobExecutionContext context)
    //    //{            
    //    //    Log.Information("Starting Bind Certificate Job in IJOB.");

    //    //    JobDataMap dataMap = context.JobDetail.JobDataMap;

    //    //    string sHostName = dataMap.GetString("hostName");            

    //    //    if (await AzureCustomDomain.DoesCertificateExistAsync(sHostName))
    //    //    {
    //    //        HttpResponseMessage responseMessage = await AzureCustomDomain.BindCertificateToCustomDomain(sHostName);

    //    //        if (responseMessage.IsSuccessStatusCode)
    //    //        {
    //    //            Log.Information($"Certificate for {sHostName} successfully bound.");

    //    //            await context.Scheduler.Clear();

    //    //            Log.Information($"Killed bind certificate job for hostname {sHostName}.");
    //    //        }
    //    //    }
    //    //}
    //    //
    //    //public async Task Execute(IJobExecutionContext context)
    //    //{
    //    //    Log.Information("Starting Bind Certificate Job in IJOB.");

    //    //    AppDbContext dbContext;


    //    //    using (var scope = _provider.CreateScope())
    //    //    {
    //    //        dbContext = scope.ServiceProvider.GetService<AppDbContext>();

    //    //        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();

    //    //        List<ApplicationUser> users = await GetCertificatesNotBoundAsync(dbContext);

    //    //        foreach(ApplicationUser user in users)
    //    //        {
    //    //            if (await AzureCustomDomain.DoesCertificateExistAsync(user.Domain))
    //    //            {
    //    //                user.CertificateExists = true;
    //    //                await userManager.UpdateAsync(user);

    //    //                HttpResponseMessage responseMessage = await AzureCustomDomain.BindCertificateToCustomDomain(user.Domain);

    //    //                if (responseMessage.IsSuccessStatusCode)
    //    //                {
    //    //                    user.CertificateBound = true;                            
    //    //                    await userManager.UpdateAsync(user);
    //    //                    Log.Information($"Certificate for {user.Domain} successfully bound.");
    //    //                }
    //    //            }
    //    //        }
    //    //    }

    //    //    Log.Information("Finished Bind Certificate Job in IJOB.");
    //    //}

    //    //private static async Task<List<ApplicationUser>> GetCertificatesNotBoundAsync(AppDbContext context) 
    //    //    => await context.Users.Where(x => x.DomainBound == true 
    //    //                                    && x.CertificateBound == false).ToListAsync();
    //}
}
