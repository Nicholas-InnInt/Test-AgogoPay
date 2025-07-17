using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using NewLife.Redis.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RedisExtensions
{
    public static class SetupRedisExtensions
    {
        public static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
            .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
            .Build();
        }

        public static IServiceCollection AddStackExchangeRedisExtensions(this IServiceCollection services)
        {
            var config = GetConfiguration();
            var rdStr = "";
            if (string.IsNullOrEmpty(config["RedisCache:Password"]))
            {
                rdStr = "server=" + config["RedisCache:Host"] + ":" + config["RedisCache:Port"] + ";db=" + config["RedisCache:Database"] + ";timeout=7000";
            }
            else
            {
                rdStr = "server=" + config["RedisCache:Host"] + ":" + config["RedisCache:Port"] + ";password=" + config["RedisCache:Password"] + ";db=" + config["RedisCache:Database"] + ";timeout=7000";
            }
            services.AddNewLifeRedis(rdStr);
            return services;
        }
    }
}
