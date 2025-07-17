using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.KafkaExtensions
{
    public interface IKafkaProducer
    {
        Task ProduceAsync<T>(string topic, string key, T message);
    }
}
