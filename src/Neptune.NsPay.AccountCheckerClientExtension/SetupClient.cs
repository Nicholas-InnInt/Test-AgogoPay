using Microsoft.Extensions.DependencyInjection;
using Neptune.NsPay.AccountNameChecker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.RedisExtensions;

namespace Neptune.NsPay.AccountCheckerClientExtension
{
    public static class AddClient
    {
        public static void SetupClient (this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IAccountNameCheckerClient, AccountNameCheckerClient>();
            services.AddSingleton<IRecipientBankAccountMongoService, RecipientBankAccountMongoService>();
            services.AddSingleton<IRedisService, RedisService>();
            services.AddSingleton<AccountCheckerService>();

        }
    }
}

