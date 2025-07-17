using System;
using System.IO;
using Abp.AspNetCore;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.AspNetZeroCore.Web.Authentication.JwtBearer;
using Abp.Castle.Logging.Log4Net;
using Abp.Hangfire;
using Abp.PlugIns;
using Castle.Facilities.Logging;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.Configure;
using Neptune.NsPay.EntityFrameworkCore;
using Neptune.NsPay.Identity;
using Neptune.NsPay.Schemas;
using Neptune.NsPay.Web.Chat.SignalR;
using Neptune.NsPay.Web.Common;
using Neptune.NsPay.Web.Resources;
using Swashbuckle.AspNetCore.Swagger;
using Neptune.NsPay.Web.Swagger;
using Stripe;
using System.Reflection;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.Mvc.Caching;
using Abp.AspNetCore.Mvc.Extensions;
using Abp.HtmlSanitizer;
using HealthChecks.UI;
using HealthChecks.UI.Client;
using HealthChecksUISettings = HealthChecks.UI.Configuration.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Neptune.NsPay.Web.HealthCheck;
using Owl.reCAPTCHA;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Neptune.NsPay.Web.Extensions;
using Neptune.NsPay.Web.MultiTenancy;
using Neptune.NsPay.Web.OpenIddict;
using SecurityStampValidatorCallback = Neptune.NsPay.Identity.SecurityStampValidatorCallback;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Helpers;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices;
using Neptune.NsPay.DataEvent;
using Neptune.NsPay.SignalRClient;
using Neptune.NsPay.Web.SignalR;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.Web.SignalR.MerchantBalance;
using Neptune.NsPay.ELKLogExtension;
using Neptune.NsPay.BillingExtensions;
using Neptune.NsPay.SqlSugarExtensions;

namespace Neptune.NsPay.Web.Startup
{
    public class Startup
    {
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public Startup(IWebHostEnvironment env)
        {
            _appConfiguration = env.GetAppConfiguration();
            _hostingEnvironment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // MVC
            var mvcBuilder = services.AddControllersWithViews(options =>
                {
                    options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
                    options.AddAbpHtmlSanitizer();
                });

#if DEBUG
            mvcBuilder.AddRazorRuntimeCompilation();
#endif

            if (bool.Parse(_appConfiguration["KestrelServer:IsEnabled"]))
            {
                ConfigureKestrel(services);
            }

            IdentityRegistrar.Register(services);

            if (bool.Parse(_appConfiguration["OpenIddict:IsEnabled"]))
            {
                OpenIddictRegistrar.Register(services, _appConfiguration);
            }
            else
            {
                services.Configure<SecurityStampValidatorOptions>(opts =>
                {
                    opts.OnRefreshingPrincipal = SecurityStampValidatorCallback.UpdatePrincipal;
                });
            }

            AuthConfigurer.Configure(services, _appConfiguration);

            if (WebConsts.SwaggerUiEnabled)
            {
                ConfigureSwagger(services);
            }

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            //Recaptcha
            services.AddreCAPTCHAV3(x =>
            {
                x.SiteKey = _appConfiguration["Recaptcha:SiteKey"];
                x.SiteSecret = _appConfiguration["Recaptcha:SecretKey"];
            });

            if (WebConsts.HangfireDashboardEnabled)
            {
                // Hangfire (Enable to use Hangfire instead of default job manager)
                services.AddHangfire(config =>
                {
                    config.UseSqlServerStorage(_appConfiguration.GetConnectionString("Default"));
                });

                services.AddHangfireServer();
            }

            // Get the application root path
            string appRootPath = _hostingEnvironment.ContentRootPath;

            services.AddScoped<IWebResourceManager, WebResourceManager>();

            services.AddStackExchangeRedisExtensions();
            services.AddSingleton<IRedisService, RedisService>();

            services.AddMongoSetup();
            services.AddSingleton<IPayOrdersMongoService, PayOrdersMongoService>();
            services.AddSingleton<IPayOrderDepositsMongoService, PayOrderDepositsMongoService>();
            services.AddSingleton<IMerchantBillsMongoService, MerchantBillsMongoService>();
            services.AddSingleton<IMerchantFundsMongoService, MerchantFundsMongoService>();
            services.AddSingleton<IWithdrawalOrdersMongoService, WithdrawalOrdersMongoService>();
            services.AddSingleton<IRecipientBankAccountMongoService, RecipientBankAccountMongoService>();
            services.AddSingleton<ILogRecipientBankAccountMongoService, LogRecipientBankAccountMongoService>();
            services.AddSingleton<IDataChangedEvent, DataChangedEvent>();

            services.AddSingleton<IBankStateHelper, BankStateHelper>();
            services.AddSingleton<IBankBalanceService, BankBalanceService>();
            services.AddSingleton<ISignalRClient, NsPaySignalRClient>();
            services.AddSingleton<ITransferSignalRClient, TransferSignalRClient>();
            services.AddSingleton<ITransferCallBackService, TransferCallBackService>();

            services.AddSingleton<IConfigurationRoot, ConfigurationRoot>();
            services.AddSingleton<MerchantHubService>();
            services.AddSignalR();

            services.AddKafkaSetup();
            services.AddSingleton<IKafkaProducer, KafkaProducer>();

            services.SetupELKLog();

            services.AddSqlsugarSetup();


            services.AddSingleton<FileWatcherService>(sp =>
    new FileWatcherService(appRootPath + "\\data\\", sp.GetRequiredService<ILogger<FileWatcherService>>()));


            services.AddHostedService<SignalRTaskScheduler>();

            if (WebConsts.GraphQL.Enabled)
            {
                services.AddAndConfigureGraphQL();
            }

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.Zero;
            });

            if (bool.Parse(_appConfiguration["HealthChecks:HealthChecksEnabled"]))
            {
                ConfigureHealthChecks(services);
            }

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new RazorViewLocationExpander());
            });

            //Configure Abp and Dependency Injection
            return services.AddAbp<NsPayWebMvcModule>(options =>
            {
                //Configure Log4Net logging
                options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig(_hostingEnvironment.IsDevelopment()
                        ? "log4net.config"
                        : "log4net.Production.config")
                );

                options.PlugInSources.AddFolder(Path.Combine(_hostingEnvironment.WebRootPath, "Plugins"),
                    SearchOption.AllDirectories);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseGetScriptsResponsePerUserCache();

            //Initializes ABP framework.
            app.UseAbp(options =>
            {
                options.UseAbpRequestLocalization = false; //used below: UseAbpRequestLocalization
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseNsPayForwardedHeaders();
            }
            else
            {
                app.UseStatusCodePagesWithRedirects("~/Error?statusCode={0}");
                app.UseExceptionHandler("/Error");
                app.UseNsPayForwardedHeaders();
            }

            DataChangedEventHandler.Configure(app, _appConfiguration);

            app.UseHttpsRedirection();

            app.UseResponseCompression();

            //app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
                }
            });

            if (NsPayConsts.PreventNotExistingTenantSubdomains)
            {
                app.UseMiddleware<DomainTenantCheckMiddleware>();
            }

            app.UseRouting();

            app.UseAuthentication();

            if (bool.Parse(_appConfiguration["Authentication:JwtBearer:IsEnabled"]))
            {
                app.UseJwtTokenMiddleware();
            }

            if (bool.Parse(_appConfiguration["OpenIddict:IsEnabled"]))
            {
                app.UseAbpOpenIddictValidation();
            }

            app.UseAuthorization();

            using (var scope = app.ApplicationServices.CreateScope())
            {
                if (scope.ServiceProvider.GetService<DatabaseCheckHelper>()
                    .Exist(_appConfiguration["ConnectionStrings:Default"]))
                {
                    app.UseAbpRequestLocalization();
                }
            }

            if (WebConsts.HangfireDashboardEnabled)
            {
                //Hangfire dashboard & server (Enable to use Hangfire instead of default job manager)
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[]
                        {new AbpHangfireAuthorizationFilter(AppPermissions.Pages_Administration_HangfireDashboard)}
                });
            }

            if (bool.Parse(_appConfiguration["Payment:Stripe:IsActive"]))
            {
                StripeConfiguration.ApiKey = _appConfiguration["Payment:Stripe:SecretKey"];
            }

            if (WebConsts.GraphQL.Enabled)
            {
                app.UseGraphQL<MainSchema>(WebConsts.GraphQL.EndPoint);
                if (WebConsts.GraphQL.PlaygroundEnabled)
                {
                    //to explorer API navigate https://*DOMAIN*/ui/playground
                    app.UseGraphQLPlayground(
                        WebConsts.GraphQL.PlaygroundEndPoint,
                        new PlaygroundOptions()
                    );
                }
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<AbpCommonHub>("/signalr");
                //endpoints.MapHub<ChatHub>("/signalr-chat");
                endpoints.MapHub<MerchantHub>("/signalr-merchant");

                endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                app.ApplicationServices.GetRequiredService<IAbpAspNetCoreConfiguration>().EndpointConfiguration
                    .ConfigureAllEndpoints(endpoints);
            });

            if (bool.Parse(_appConfiguration["HealthChecks:HealthChecksEnabled"]))
            {
                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                if (bool.Parse(_appConfiguration["HealthChecks:HealthChecksUI:HealthChecksUIEnabled"]))
                {
                    app.UseHealthChecksUI();
                }
            }

            if (WebConsts.SwaggerUiEnabled)
            {
                // Enable middleware to serve generated Swagger as a JSON endpoint
                app.UseSwagger();
                //Enable middleware to serve swagger - ui assets(HTML, JS, CSS etc.)
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(_appConfiguration["App:SwaggerEndPoint"], "NsPay API V1");
                    options.IndexStream = () => Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("Neptune.NsPay.Web.wwwroot.swagger.ui.index.html");
                    options.InjectBaseUrl(_appConfiguration["App:WebSiteRootAddress"]);
                }); //URL: /swagger
            }



            var fileWatcherService = app.ApplicationServices.GetRequiredService<FileWatcherService>();

            var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

            // Register the event handlers for application stop
            lifetime.ApplicationStopping.Register(OnApplicationStopping);
            lifetime.ApplicationStopped.Register(OnApplicationStopped);
            lifetime.ApplicationStarted.Register(OnApplicationStarted);


            // Event handler for when the application is stopping
            void OnApplicationStarted()
            {
                fileWatcherService.Start();
                // Logic to execute just before the application stops
                Console.WriteLine("Application is Started...");
            }


            // Event handler for when the application is stopping
            void OnApplicationStopping()
            {
                fileWatcherService.Stop();
                // Logic to execute just before the application stops
                Console.WriteLine("Application is stopping...");
            }

            // Event handler for when the application has stopped
            void OnApplicationStopped()
            {
                // Logic to execute after the application has stopped
                Console.WriteLine("Application has stopped.");
            }


        }

        private void ConfigureKestrel(IServiceCollection services)
        {
            services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
            {
                options.Listen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 443),
                    listenOptions =>
                    {
                        var certPassword = _appConfiguration.GetValue<string>("Kestrel:Certificates:Default:Password");
                        var certPath = _appConfiguration.GetValue<string>("Kestrel:Certificates:Default:Path");
                        var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath,
                            certPassword);
                        listenOptions.UseHttps(new HttpsConnectionAdapterOptions()
                        {
                            ServerCertificate = cert
                        });
                    });
            });
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo() {Title = "NsPay API", Version = "v1"});
                options.DocInclusionPredicate((docName, description) => true);
                options.ParameterFilter<SwaggerEnumParameterFilter>();
                options.SchemaFilter<SwaggerEnumSchemaFilter>();
                options.OperationFilter<SwaggerOperationIdFilter>();
                options.OperationFilter<SwaggerOperationFilter>();
                options.CustomDefaultSchemaIdSelector();

                // Add summaries to swagger
                var canShowSummaries = _appConfiguration.GetValue<bool>("Swagger:ShowSummaries");
                if (!canShowSummaries)
                {
                    return;
                }

                var mvcXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var mvcXmlPath = Path.Combine(AppContext.BaseDirectory, mvcXmlFile);
                options.IncludeXmlComments(mvcXmlPath);

                var applicationXml = $"Neptune.NsPay.Application.xml";
                var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXml);
                options.IncludeXmlComments(applicationXmlPath);

                var webCoreXmlFile = $"Neptune.NsPay.Web.Core.xml";
                var webCoreXmlPath = Path.Combine(AppContext.BaseDirectory, webCoreXmlFile);
                options.IncludeXmlComments(webCoreXmlPath);
            });
        }

        private void ConfigureHealthChecks(IServiceCollection services)
        {
            services.AddAbpZeroHealthCheck();

            var healthCheckUISection = _appConfiguration.GetSection("HealthChecks")?.GetSection("HealthChecksUI");

            if (bool.Parse(healthCheckUISection["HealthChecksUIEnabled"]))
            {
                services.Configure<HealthChecksUISettings>(settings =>
                {
                    healthCheckUISection.Bind(settings, c => c.BindNonPublicProperties = true);
                });

                services.AddHealthChecksUI()
                    .AddInMemoryStorage();
            }
        }
    }
}
