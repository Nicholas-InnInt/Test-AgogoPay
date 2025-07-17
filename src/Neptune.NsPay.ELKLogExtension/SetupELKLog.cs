using Microsoft.Extensions.DependencyInjection;
using Neptune.NsPay.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.ELKLogExtension
{
    public static class AddELKLog
    {
        public static void SetupELKLog(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var elkOptions = new ELkLogOption
            {
                HostUrl = AppSettings.Configuration["ELKLog:HostUrl"],
                Token = AppSettings.Configuration["ELKLog:Token"],
                Topic = AppSettings.Configuration["ELKLog:Topic"]
            };
            services.AddSingleton(elkOptions);
            services.AddHttpClient<IHttpPostService, ElkHttpPostService>();
            services.AddSingleton<LogOrderService>();
        }
    }

}
