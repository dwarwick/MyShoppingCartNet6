using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyShoppingCart.Data;
using MyShoppingCart.Data.Cart;
using MyShoppingCart.Data.Services;
using MyShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Azure;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using Azure.Core.Extensions;
using Microsoft.AspNetCore.CookiePolicy;
using Quartz.Impl;
using Quartz;
using MyShoppingCart.Helpers.Jobs;
using Quartz.Spi;
using Serilog;

namespace MyShoppingCart
{
    public class Startup
    {
        public static IConfiguration StaticConfig { get; private set; }
        public static IServiceCollection StaticServices { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            StaticConfig = configuration;
        }


        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnectionString")));

            //Services configuration

            

            services.AddScoped<IProductsService, ProductsService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IOrdersService, OrdersService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(sc => ShoppingCart.GetShoppingCart(sc));

            
            //services.AddSingleton<IJobFactory, SingletonJobFactory>();
            //services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            // Add our job
            //services.AddSingleton<BindCertificateJob>();
            //services.AddSingleton<PayoutsJob>();

            //services.AddSingleton(new JobSchedule(
            //    jobType: typeof(PayoutsJob),
            //    cronExpression: "0/5 * * * * ?", // run every 5 seconds,
            //    numSeconds: 1200));

            //services.AddSingleton(new JobSchedule(
            //    jobType: typeof(BindCertificateJob),
            //    cronExpression: "0/5 * * * * ?", // run every 5 seconds,
            //    numSeconds: 60));


            //services.AddHostedService<QuartzHostedService>();
            services.AddSession();


            // Sets the display of the Cookie Consent banner (/Pages/Shared/_CookieConsentPartial.cshtml).
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            services.Configure<CookiePolicyOptions>(options => {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            });

            //Authentication and authorization
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);
            services.AddMemoryCache();
            services.AddSession();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            
            services.AddAzureClients(builder =>
            {
                builder.AddBlobServiceClient(Configuration["ConnectionStrings:StorageConnectionString:blob"], preferMsi: true);
                builder.AddQueueServiceClient(Configuration["ConnectionStrings:StorageConnectionString:queue"], preferMsi: true);
            });

            services.AddHttpClient();



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseExceptionHandler("/Home/Error");
            app.ConfigureExceptionHandler();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();

            //Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCookiePolicy();



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "subdomain",
                    pattern: "{controller=Products}/{action=Index}/{id?}").RequireHost("*.myshoppingcart.biz");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Products}/{action=Index}/{id?}");
            });

            AppDbInitializer.Seed(app);
            AppDbInitializer.SeedUsersAndRolesAsync(app).Wait();

            //ScheduleJobs();
        }

        //private static async void ScheduleJobs()
        //{
        //    Log.Logger.Information("Starting bind certificate job.");
        //    // Grab the Scheduler instance from the Factory
        //    StdSchedulerFactory factory = new StdSchedulerFactory();
        //    IScheduler scheduler = await factory.GetScheduler();

        //    // and start it off
        //    await scheduler.Start();

        //    // define the job and tie it to our HelloJob class
        //    IJobDetail job = JobBuilder.Create<BindCertificateJob>()
        //        .WithIdentity("job1", "group1")                
        //        .Build();

        //    // Trigger the job to run now, and then repeat every 10 seconds
        //    ITrigger trigger = TriggerBuilder.Create()
        //        .WithIdentity("trigger1", "group1")
        //        .StartNow()
        //        .WithSimpleSchedule(x => x
        //            .WithIntervalInMinutes(1)
        //            .RepeatForever())
        //        .Build();

        //    // Tell quartz to schedule the job using our trigger
        //    await scheduler.ScheduleJob(job, trigger);
        //    Log.Logger.Information("Started bind certificate job.");
        //}
    }
    internal static class StartupExtensions
    {
        public static IAzureClientBuilder<BlobServiceClient, BlobClientOptions> AddBlobServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
        {
            if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
            {
                return builder.AddBlobServiceClient(serviceUri);
            }
            else
            {
                return builder.AddBlobServiceClient(serviceUriOrConnectionString);
            }
        }
        public static IAzureClientBuilder<QueueServiceClient, QueueClientOptions> AddQueueServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
        {
            if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
            {
                return builder.AddQueueServiceClient(serviceUri);
            }
            else
            {
                return builder.AddQueueServiceClient(serviceUriOrConnectionString);
            }
        }        
    }
}
