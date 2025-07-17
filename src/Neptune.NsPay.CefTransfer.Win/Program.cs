using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neptune.NsPay.CefTransfer.Win.Forms;
//using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
//using Neptune.NsPay.MongoDbExtensions.Services;
//using Neptune.NsPay.RedisExtensions;
//using Neptune.NsPay.MongoDbExtensions;

namespace Neptune.NsPay.CefTransfer.Win
{
    internal static class Program
    {
        public static IConfiguration Configuration;
        public static IServiceProvider ServiceProvider { get; private set; }
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //// To customize application configuration such as set high DPI settings or default font,
            //// see https://aka.ms/applicationconfiguration.
            //ApplicationConfiguration.Initialize();
            //Application.Run(new Form1());

            ApplicationConfiguration.Initialize();

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            Configuration = builder.Build();

            var host = CreateHostBuilder().Build();
            ServiceProvider = host.Services;

            Application.Run(ServiceProvider.GetRequiredService<fBrowser>());
        }

        public static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    //services.AddStackExchangeRedisExtensions();
                    //services.AddSingleton<IRedisService, RedisService>();

                    //services.AddMongoSetup();
                    //services.AddSingleton<IWithdrawalOrdersMongoService, WithdrawalOrdersMongoService>();

                    services.AddSingleton<fBrowser>();
                });
        }
    }
}