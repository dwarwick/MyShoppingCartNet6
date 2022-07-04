using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;

namespace MyShoppingCart
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build();

        public static void Main(string[] args)
        {
            string connectionString = Configuration.GetConnectionString("DefaultConnectionString");

            string sPath = string.Format(@"{0}\logs\log-.txt", Directory.GetCurrentDirectory());

            if(!Directory.Exists(string.Format(@"{0}\logs", Directory.GetCurrentDirectory())))
                Directory.CreateDirectory(string.Format(@"{0}\logs", Directory.GetCurrentDirectory()));

            var columnOptions = new ColumnOptions
            {
                AdditionalColumns = new Collection<SqlColumn>
               {
                   new SqlColumn("UserName", SqlDbType.VarChar)
               }
            }; //through this coulmnsOptions we can dynamically  add custom columns which we want to add in database  
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.MSSqlServer(connectionString, sinkOptions: new MSSqlServerSinkOptions { TableName = "Log", AutoCreateSqlTable = true }
                , null, null, LogEventLevel.Information, null, columnOptions: columnOptions, null, null)

                .WriteTo.File(path: sPath,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                
                restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();

            try
            {
                Log.Information("Application is starting.");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start.");                
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

            .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseAzureAppServices();
                }); 
    }    
}
