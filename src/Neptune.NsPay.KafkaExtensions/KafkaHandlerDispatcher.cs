using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.KafkaExtensions
{
    public class KafkaHandlerDispatcher
    {
        private readonly IEnumerable<IKafkaTopicHandler> _handlers;

        public KafkaHandlerDispatcher(IEnumerable<IKafkaTopicHandler> handlers)
        {
            _handlers = handlers;
        }

        public async Task DispatchAsync(string topic, string key, string message)
        {
            var handler = _handlers.FirstOrDefault(h => h.Topic == topic);
            if (handler != null)
            {
                await handler.HandleAsync(key, message);
            }
            else
            {
                throw new Exception($"No handler found for topic: {topic}");
            }
        }
    }
}
