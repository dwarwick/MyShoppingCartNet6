using BindCertificateJobConsoleApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BindCertificateJobConsoleApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            BindCertificate certificate = new BindCertificate();
            certificate.Execute().Wait();
        }
    }

    public class BindCertificate
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                        .Build();

        Uri _baseURI = new Uri($"https://management.azure.com/");

        string _connectionString = Configuration.GetConnectionString("DefaultConnectionString");

        string _ClientId = Configuration.GetValue<string>("Azure:ClientId");
        string _ClientKey = Configuration.GetValue<string>("Azure:ClientSecret");
        string _TenantId = Configuration.GetValue<string>("Azure:TenantId");



        string _SubscriptionId = Configuration.GetValue<string>("Azure:SubscriptionId");
        string _ResourceGroupName = Configuration.GetValue<string>("Azure:ResourceGroupName");
        string _AlternateResourceGroupName = Configuration.GetValue<string>("Azure:AlternateResourceGroupName");
        string _AppName = Configuration.GetValue<string>("Azure:AppName");
        string _AppServicePlanName = Configuration.GetValue<string>("Azure:AppServicePlanName");

        public async Task Execute()
        {
            string sSQL = "SELECT * " +
                            "FROM AspNetUsers " +
                            "WHERE DomainBound = 1 AND CertificateBound = 0";

            Console.WriteLine("Starting Bind Certificate Job in Console App.");

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    Console.WriteLine("Connected to database from job.");

                    using (SqlCommand cmd = new SqlCommand(sSQL, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine("Ran query from job.");

                            while (reader.Read())
                            {
                                string sDomain = reader["Domain"].ToString();

                                Console.WriteLine($"Processing domain {sDomain} from job.");

                                if (await DoesCertificateExistAsync(sDomain, Configuration))
                                {
                                    Console.WriteLine($"Certificate exists for domain {sDomain} from job.");

                                    int iNumRows = await UpdateDatabaseAsync($"UPDATE AspNetUsers SET CertificateExists = 1 WHERE Domain = '{sDomain}'");


                                    HttpResponseMessage responseMessage = await BindCertificateToCustomDomain(sDomain, Configuration);

                                    if (responseMessage.IsSuccessStatusCode)
                                    {
                                        iNumRows = await UpdateDatabaseAsync($"UPDATE AspNetUsers SET CertificateBound = 1 WHERE Domain = '{sDomain}'");

                                        Console.WriteLine($"Certificate for {sDomain} successfully bound.");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Certificate for {sDomain} not successfully bound.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Certificate does not exist for domain {sDomain} from job.");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while trying to bind certificate to hostname: {ex.Message}");

            }
            Console.WriteLine("Finished Bind Certificate Job in Console App.");
        }

        private async Task<int> UpdateDatabaseAsync(string sSQL)
        {
            int iRows = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(sSQL, conn))
                    {
                        iRows = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error while trying to update database with query {sSQL}: {ex.Message}");
            }
            return iRows;
        }

        private string GetAccessToken()
        {
            var context = new AuthenticationContext("https://login.windows.net/" + _TenantId);
            ClientCredential clientCredential = new ClientCredential(_ClientId, _ClientKey);
            var tokenResponse = context.AcquireTokenAsync(_baseURI.ToString(), clientCredential).Result;
            return tokenResponse.AccessToken;
        }



        private async Task<HttpResponseMessage> BindCertificateToCustomDomain(string sHostName, IConfiguration configuration)
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

        private async Task<bool> DoesCertificateExistAsync(string sHostName, IConfiguration configuration)
        {
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetAccessToken());
                string requestURl = _baseURI + $"subscriptions/{_SubscriptionId}/providers/Microsoft.Web/certificates?api-version=2021-02-01";

                try
                {
                    response = await client.GetAsync(requestURl);

                    Console.WriteLine($"Received response checking for existence of certificate: {response.Content.ReadAsStringAsync().Result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking for existence of certificate: {ex.Message}");
                }
            }
            var result = response.Content.ReadAsStringAsync().Result;
            CertificateExistResponse obj = JsonConvert.DeserializeObject<CertificateExistResponse>(result);

            return obj.value.Exists(x => x.name == sHostName);
        }
    }
}
