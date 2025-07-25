using Microsoft.Extensions.DependencyInjection;
using Neptune.NsPay.Commons;
using NewLife.Redis.Core;

namespace Neptune.NsPay.RedisExtensions
{
    public static class SetupRedisExtensions
    {
        public static IServiceCollection AddStackExchangeRedisExtensions(this IServiceCollection services)
        {
            var config = AppSettings.Configuration;
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