using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog.Web;

using PHIDDO.MTX.LecacyDataExtract.Models.Config;
using PHIDDO.MTX.LecacyDataExtract.Updater.Repositories;
using PHIDDO.MTX.LecacyDataExtract.Updater.Services;
using PHIDDO.MTX.LecacyDataExtract.Updater.Workers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PHIDDO.MTX.LecacyDataExtract
{
    public class Program
    {

        private static IConfigurationRefresher configurationRefresher;
        private static IConfiguration configuration;

        private static void SetConfigurationRefresher(IConfigurationRefresher value)
        {
            configurationRefresher = value;
        }

        public static async Task Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                IHost host = new HostBuilder()
                .ConfigureHostConfiguration(builder =>
                {
                    // builder.AddUserSecrets<Program>();
                    builder.AddEnvironmentVariables()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var settings = config.Build();
                    configuration = hostingContext.Configuration;
                    var azConfig = configuration["AzureAppConfig:ConnectionString"];
                    var environment = configuration["Environment"];
                    config.AddAzureAppConfiguration(options =>
                    {
                        options.Connect(azConfig)
                            .Select(KeyFilter.Any, LabelFilter.Null)
                            .Select(KeyFilter.Any, environment)
                            .ConfigureRefresh(refresh =>
                            {
                                refresh.Register("Version", true)
                                .SetCacheExpiration(TimeSpan.FromSeconds(5));
                            });
                        SetConfigurationRefresher(options.GetRefresher());
                    });
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddOptions();
                    services.Configure<DatabaseConfig>(hostingContext.Configuration.GetSection("DatabaseConfig"));
                    services.Configure<ApiConfig>(hostingContext.Configuration.GetSection("ApiConfig"));
                    services.AddHttpClient();
                    services.AddScoped<IMTXUpdateRepository, MTXUpdateRepository>();
                    services.AddScoped<IMTXUpdateService, MTXUpdateService>();
                    services.AddScoped<IMTXApi, MTXApi>();
                    services.AddHostedService<MTXUpdateWorker>();

                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                //.UseWindowsService()
                .UseNLog()
                .Build();
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Stopped application because of exception {ex.ToString()}");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        //public static void Main(string[] args)
        //{
        //    CreateHostBuilder(args).Build().Run();
        //}

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureServices((hostContext, services) =>
        //        {
        //            services.AddHostedService<Worker>();
        //        });
    }
}
