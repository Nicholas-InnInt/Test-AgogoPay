using Confluent.Kafka;
using Neptune.NsPay.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Neptune.NsPay.KafkaExtensions
{
    public class KafkaProducer: IKafkaProducer
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(KafkaOptions options)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = options.BootstrapServers,
                //SaslUsername = options.SaslUsername,
                //SaslPassword = options.SaslPassword,
                //SecurityProtocol = SecurityProtocol.SaslSsl,
                //SaslMechanism = SaslMechanism.Plain
            };
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceAsync<T>(string topic,string key, T message)
        {
            try
            {
                string value;
                if (message is string str)
                {
                    value = str;
                }
                else if (message == null)
                {
                    value = null;
                }
                else
                {
                    var type = typeof(T);
                    if (type.IsPrimitive ||
                        type.IsEnum ||
                        type == typeof(decimal) ||
                        type == typeof(DateTime) ||
                        type == typeof(DateTimeOffset) ||
                        type == typeof(Guid) ||
                        type == typeof(TimeSpan))
                    {
                        value = message.ToString();
                    }
                    else
                    {
                        value = JsonSerializer.Serialize(message);
                    }
                }

                await _producer.ProduceAsync(topic, new Message<string, string>
                {
                    Key = key,
                    Value = value
                });
            }
            catch (Exception ex)
            {
                NlogLogger.Error("推送Kafka失败：" + ex);
                throw new Exception("Kafka message sending failed.", ex);
            }
        }
    }
}
