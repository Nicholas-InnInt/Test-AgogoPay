﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.Mvc.Extensions;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.AspNetZeroCore.Web.Authentication.JwtBearer;
using Abp.Castle.Logging.Log4Net;
using Abp.Extensions;
using Abp.Hangfire;
using Abp.PlugIns;
using Castle.Facilities.Logging;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.EntityFrameworkCore;
using Neptune.NsPay.Identity;
using Neptune.NsPay.Web.Chat.SignalR;
using Neptune.NsPay.Web.Common;
using Neptune.NsPay.Web.Swagger;
using Stripe;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using GraphQL.Server.Ui.Playground;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Neptune.NsPay.Configure;
using Neptune.NsPay.Schemas;
using Neptune.NsPay.Web.HealthCheck;
using Owl.reCAPTCHA;
using HealthChecksUISettings = HealthChecks.UI.Configuration.Settings;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Neptune.NsPay.Web.MultiTenancy;
using Abp.HtmlSanitizer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Neptune.NsPay.Web.OpenIddict;

namespace Neptune.NsPay.Web.Startup
{
    public class Startup
    {
        private const string DefaultCorsPolicyName = "localhost";

        private readonly IConfigurationRoot _appConfiguration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public Startup(IWebHostEnvironment env)
        {
            _hostingEnvironment = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //MVC
            var mvcBuilder = services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
                options.AddAbpHtmlSanitizer();
            });
#if DEBUG
            mvcBuilder.AddRazorRuntimeCompilation();
#endif

            services.AddSignalR();

            //Configure CORS for angular2 UI
            services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, builder =>
                {
                    //App:CorsOrigins in appsettings.json can contain more than one address with splitted by comma.
                    builder
                        .WithOrigins(
                            // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                            _appConfiguration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            if (bool.Parse(_appConfiguration["KestrelServer:IsEnabled"]))
            {
                ConfigureKestrel(services);
            }

            IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);

            if (bool.Parse(_appConfiguration["OpenIddict:IsEnabled"]))
            {
                OpenIddictRegistrar.Register(services, _appConfiguration);

                services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme,
                    options => { options.LoginPath = "/Ui/Login"; });
            }
            else
            {
                services.Configure<SecurityStampValidatorOptions>(opts =>
                {
                    opts.OnRefreshingPrincipal = SecurityStampValidatorCallback.UpdatePrincipal;
                });
            }

            if (WebConsts.SwaggerUiEnabled)
            {
                //Swagger - Enable this line and the related lines in Configure method to enable swagger UI
                ConfigureSwagger(services);
            }

            //Recaptcha
            services.AddreCAPTCHAV3(x =>
            {
                x.SiteKey = _appConfiguration["Recaptcha:SiteKey"];
                x.SiteSecret = _appConfiguration["Recaptcha:SecretKey"];
            });

            if (WebConsts.HangfireDashboardEnabled)
            {
                //Hangfire(Enable to use Hangfire instead of default job manager)
                services.AddHangfire(config =>
                {
                    config.UseSqlServerStorage(_appConfiguration.GetConnectionString("Default"));
                });

                services.AddHangfireServer();
            }

            if (WebConsts.GraphQL.Enabled)
            {
                services.AddAndConfigureGraphQL();
            }

            if (bool.Parse(_appConfiguration["HealthChecks:HealthChecksEnabled"]))
            {
                ConfigureHealthChecks(services);
            }

            //Configure Abp and Dependency Injection
            return services.AddAbp<NsPayWebHostModule>(options =>
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
            //Initializes ABP framework.
            app.UseAbp(options =>
            {
                options.UseAbpRequestLocalization = false; //used below: UseAbpRequestLocalization
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHsts();
            }
            else
            {
                app.UseStatusCodePagesWithRedirects("~/Error?statusCode={0}");
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            if (NsPayConsts.PreventNotExistingTenantSubdomains)
            {
                app.UseMiddleware<DomainTenantCheckMiddleware>();
            }

            app.UseRouting();

            app.UseCors(DefaultCorsPolicyName); //Enable CORS!

            app.UseAuthentication();
            app.UseJwtTokenMiddleware();

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
                //Hangfire dashboard &server(Enable to use Hangfire instead of default job manager)
                app.UseHangfireDashboard(WebConsts.HangfireDashboardEndPoint, new DashboardOptions
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
                    // to explorer API navigate https://*DOMAIN*/ui/playground
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
                // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(_appConfiguration["App:SwaggerEndPoint"], "NsPay API V1");
                    options.IndexStream = () => Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("Neptune.NsPay.Web.wwwroot.swagger.ui.index.html");
                    options.InjectBaseUrl(_appConfiguration["App:ServerRootAddress"]);
                }); //URL: /swagger
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

                //add summaries to swagger
                bool canShowSummaries = _appConfiguration.GetValue<bool>("Swagger:ShowSummaries");
                if (canShowSummaries)
                {
                    var hostXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var hostXmlPath = Path.Combine(AppContext.BaseDirectory, hostXmlFile);
                    options.IncludeXmlComments(hostXmlPath);

                    var applicationXml = $"Neptune.NsPay.Application.xml";
                    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXml);
                    options.IncludeXmlComments(applicationXmlPath);

                    var webCoreXmlFile = $"Neptune.NsPay.Web.Core.xml";
                    var webCoreXmlPath = Path.Combine(AppContext.BaseDirectory, webCoreXmlFile);
                    options.IncludeXmlComments(webCoreXmlPath);
                }
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