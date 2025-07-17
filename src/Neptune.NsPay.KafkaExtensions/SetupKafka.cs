using Microsoft.Extensions.DependencyInjection;
using Neptune.NsPay.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.KafkaExtensions
{
    public static class SetupKafka
    {
        public static void AddKafkaSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var kafkaOptions = new KafkaOptions
            {
                BootstrapServers = AppSettings.Configuration["Kafka:BootstrapServers"],
                //SaslUsername = "",
                //SaslPassword = "",
            };
            services.AddSingleton(kafkaOptions);
            //services.AddSingleton<IKafkaProducer, KafkaProducer>();
        }
    }
}
