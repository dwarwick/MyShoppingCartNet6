using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MyShoppingCart.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyShoppingCart.Helpers.AzureManagementAPI
{
    public class AzureCustomDomain
    {
        
        static string _ClientId = Startup.StaticConfig.GetValue<string>("Azure:ClientId");
        static string _ClientKey = Startup.StaticConfig.GetValue<string>("Azure:ClientSecret");
        static string _TenantId = Startup.StaticConfig.GetValue<string>("Azure:TenantId");
        static string _SubscriptionId = Startup.StaticConfig.GetValue<string>("Azure:SubscriptionId");
        static string _ResourceGroupName = Startup.StaticConfig.GetValue<string>("Azure:ResourceGroupName");
        static string _AlternateResourceGroupName = Startup.StaticConfig.GetValue<string>("Azure:AlternateResourceGroupName");
        static string _AppName = Startup.StaticConfig.GetValue<string>("Azure:AppName");
        static string _AppServicePlanName = Startup.StaticConfig.GetValue<string>("Azure:AppServicePlanName");
        static Uri _baseURI = new Uri($"https://management.azure.com/");

        private static string GetAccessToken()
        {
            var context = new AuthenticationContext("https://login.windows.net/" + _TenantId);
            ClientCredential clientCredential = new ClientCredential(_ClientId, _ClientKey);
            var tokenResponse = context.AcquireTokenAsync(_baseURI.ToString(), clientCredential).Result;
            return tokenResponse.AccessToken;
        }

        public static async Task<bool> CreateCustomDomainAndCertificate(string sHostName, string sUserEmail)
        {
            bool bRet = false;

            HttpResponseMessage responseMessage = await CreateCustomDomain(sHostName);            

            if (responseMessage.IsSuccessStatusCode)
            {
                Log.Logger.Information($"{sUserEmail} created hostname {sHostName}");

                responseMessage = await CreateAppManagedCertificate(sHostName);

                /*
                     it can take a good 5 minutes to create the certificate
                     but you get the 202 status code right away.
                     
                     You cannot bind the certificate to the custom domain
                     name until after the certificate actually exists.
                 */

                if (responseMessage.IsSuccessStatusCode)
                {
                    bRet = true;

                    //ScheduleJobs(sHostName);
                    
                    //while (!bCertificateExists && DateTime.Now < dtStart.AddMinutes(10))
                    //{
                        //bCertificateExists = await DoesCertificateExistAsync(sHostName);

                    //    Log.Logger.Information($"{sUserEmail} finished checking if certificate exists for {sHostName}");

                        
                    //}

                    //if (bCertificateExists)
                    //{
                    //    Log.Logger.Information($"{sUserEmail} created app managed certificate for hostname {sHostName}");
                    //    responseMessage = await BindCertificateToCustomDomain(sHostName);

                    //    if (responseMessage.IsSuccessStatusCode)
                    //    {
                    //        bRet = true;
                    //        Log.Logger.Information($"{sUserEmail} binded app managed certificate to hostname {sHostName}");
                    //    }
                    //    else
                    //    {
                    //        Log.Logger.Information($"{sUserEmail} failed to bind app managed certificate to hostname {sHostName}");
                    //    }
                    //}
                }
                else
                {
                    Log.Logger.Information($"{sUserEmail} failed to create app managed certificate for hostname {sHostName}");
                    return false;
                }
            }
            else
            {
                Log.Logger.Information($"{sUserEmail} failed to create hostname {sHostName}");
                return false;
            }

            return bRet;
        }

        
        
        private static async Task<HttpResponseMessage> CreateCustomDomain(string sHostName)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetAccessToken());
                string requestURl = _baseURI + $"subscriptions/{_SubscriptionId}/resourceGroups/{_ResourceGroupName}/providers/Microsoft.Web/sites/{_AppName}/hostNameBindings/{sHostName}?api-version=2016-08-01";
                string body = $"{{\"properties\": {{\"azureResourceName\": \"{_AppName}\"}}}}";

                var stringContent = new StringContent(body, Encoding.UTF8, "application/json");
                return await client.PutAsync(requestURl, stringContent);
            }
        }

        private static async Task<HttpResponseMessage> CreateAppManagedCertificate(string sHostName)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetAccessToken());
                string requestURl = _baseURI + $"subscriptions/{_SubscriptionId}/resourceGroups/{_ResourceGroupName}/providers/Microsoft.Web/certificates/{sHostName}?api-version=2021-02-01";
                string serverFarm = $"/subscriptions/{_SubscriptionId}/resourceGroups/{_AlternateResourceGroupName}/providers/Microsoft.Web/serverfarms/{_AppServicePlanName}";
                string body = $"{{\"location\": \"West US\", \"properties\": {{\"canonicalName\": \"{sHostName}\", \"hostNames\": [\"{sHostName}\"], \"serverFarmId\": \"{serverFarm}\"}}}}";

                var stringContent = new StringContent(body, Encoding.UTF8, "application/json");
                return await client.PutAsync(requestURl, stringContent);
            }

        }

        public static async Task<HttpResponseMessage> BindCertificateToCustomDomain(string sHostName)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetAccessToken());
                string requestURl = _baseURI + $"subscriptions/{_SubscriptionId}/resourceGroups/{_ResourceGroupName}/providers/Microsoft.Web/sites/{_AppName}?api-version=2016-08-01";
                string serverFarm = $"/subscriptions/{_SubscriptionId}/resourceGroups/{_AlternateResourceGroupName}/providers/Microsoft.Web/serverfarms/{_AppServicePlanName}";
                string body = $"{{\"location\": \"West US\", \"properties\": {{\"HostNameSslStates\": [ {{ \"SslState\" : \"1\", \"ToUpdate\" : \"True\", \"Name\": \"{sHostName}\"}}]}}, \"kind\": \"app\", \"location\": \"West US\", \"tags\" : {{\"hidden-related:{serverFarm}\": \"empty\"}}}}";

                var stringContent = new StringContent(body, Encoding.UTF8, "application/json");
                return await client.PutAsync(requestURl, stringContent);
            }
        }


        public static async Task<bool> DeleteHostName(string sHostName, string sUserEmail)
        {
            HttpResponseMessage responseMessage = await DeleteHostNameBindingAsync(sHostName);

            if (responseMessage.IsSuccessStatusCode)
            {
                Log.Logger.Information($"{sUserEmail} deleted hostname binding for domain {sHostName}");
                responseMessage = await DeleteHostNameCertificateAsync(sHostName);

                if (responseMessage.IsSuccessStatusCode)
                {
                    Log.Logger.Information($"{sUserEmail} deleted hostname certificate for domain {sHostName}");
                    return true;
                }
                else
                {
                    Log.Logger.Information($"{sUserEmail} failed to delete certificate binding for domain {sHostName}");
                    return false;
                }

            }
            else
            {
                Log.Logger.Information($"{sUserEmail} failed to delete hostname binding for domain {sHostName}");
                return false;
            }
        }

        private static async Task<HttpResponseMessage> DeleteHostNameBindingAsync(string sHostName)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetAccessToken());
                string requestURl = _baseURI + $"subscriptions/{_SubscriptionId}/resourceGroups/{_ResourceGroupName}/providers/Microsoft.Web/sites/{_AppName}/hostNameBindings/{sHostName}?api-version=2021-02-01";

                return await client.DeleteAsync(requestURl);
            }
        }

        private static async Task<HttpResponseMessage> DeleteHostNameCertificateAsync(string sHostName)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetAccessToken());
                string requestURl = _baseURI + $"subscriptions/{_SubscriptionId}/resourceGroups/{_ResourceGroupName}/providers/Microsoft.Web/certificates/{sHostName}?api-version=2020-12-01";

                return await client.DeleteAsync(requestURl);
            }
        }

        public static async Task<bool> DoesCertificateExistAsync(string sHostName)
        {
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetAccessToken());
                string requestURl = _baseURI + $"subscriptions/{_SubscriptionId}/providers/Microsoft.Web/certificates?api-version=2021-02-01";

                response = await client.GetAsync(requestURl);
            }
            var result = response.Content.ReadAsStringAsync().Result;
            CertificateExistResponse obj = JsonConvert.DeserializeObject<CertificateExistResponse>(result);

            return obj.value.Exists(x => x.name == sHostName);
        }


        //private static async void ScheduleJobs(string sHostName)
        //{
        //    Log.Logger.Information("Starting bind certificate job.");
        //    // Grab the Scheduler instance from the Factory
        //    _factory = new StdSchedulerFactory();
        //    _scheduler = await _factory.GetScheduler();

        //    // and start it off
        //    await _scheduler.Start();

        //    // define the job and tie it to our HelloJob class
        //    IJobDetail job = JobBuilder.Create<BindCertificateJob>()
        //        .WithIdentity("job1", "group1")
        //        .UsingJobData("hostName", sHostName)                
        //        .Build();

        //    // Trigger the job to run now, and then repeat every 10 seconds
        //    ITrigger trigger = TriggerBuilder.Create()
        //        .WithIdentity("trigger1", "group1")
        //        .StartNow()
        //        .WithSimpleSchedule(x => x
        //            .WithIntervalInSeconds(10)
        //            .RepeatForever())
        //        .Build();

        //    // Tell quartz to schedule the job using our trigger
        //    await _scheduler.ScheduleJob(job, trigger);
        //    Log.Logger.Information("Started bind certificate job.");
        //}


    }
}