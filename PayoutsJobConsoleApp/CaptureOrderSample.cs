using Microsoft.Extensions.Configuration;
using PayoutsSdk.Payouts;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PayoutsJobConsoleApp
{
    public class CaptureOrderSample
    {
        public static IConfiguration _configuration { get; } = new ConfigurationBuilder()
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                       .Build();

        static string PayPalClientID = _configuration.GetValue<string>("Paypal:ClientID");
        static string PayPalClientSecret = _configuration.GetValue<string>("Paypal:ClientSecret");

        public static PayPalHttp.HttpClient client()
        {
            // Creating a sandbox environment
            PayPalEnvironment environment = new SandboxEnvironment(PayPalClientID, PayPalClientSecret);

            // Creating a client for the environment
            PayPalHttpClient client = new PayPalHttpClient(environment);
            return client;
        }        

        public async static Task<CreatePayoutResponse> CreatePayout(string EmailMessage, string EmailSubject, string Value, string ReceiverEmail)
        {
            var body = new CreatePayoutRequest()
            {
                SenderBatchHeader = new SenderBatchHeader()
                {
                    EmailMessage = EmailMessage,
                    EmailSubject = EmailSubject
                },
                Items = new List<PayoutItem>()  
                {
                    new PayoutItem()
                    {
                        RecipientType="EMAIL",
                        Amount=new Currency()
                        {
                            CurrencyCode="USD",
                            Value=Value,
                        },
                        Receiver= ReceiverEmail, //"sb-r43z1e9186231@business.example.com"
                    }
                }
            };
            PayoutsPostRequest request = new PayoutsPostRequest();
            request.RequestBody(body);
            var response = await client().Execute(request);
            var result = response.Result<CreatePayoutResponse>();
            Console.WriteLine($"Status: {result.BatchHeader.BatchStatus}");
            Console.WriteLine($"Batch Id: {result.BatchHeader.PayoutBatchId}");
            Console.WriteLine("Links:");
            foreach (PayoutsSdk.Payouts.LinkDescription link in result.Links)
            {
                Console.WriteLine($"\t{link.Rel}: {link.Href}\tCall Type: {link.Method}");
            }

            return result;
        }
    }
}
