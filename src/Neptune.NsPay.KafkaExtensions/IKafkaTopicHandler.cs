using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.KafkaExtensions
{
    public interface IKafkaTopicHandler
    {
        string Topic { get; }
        Task HandleAsync(string key, string message);
    }
}
