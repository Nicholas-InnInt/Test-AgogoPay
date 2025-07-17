using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.Commons;
using Neptune.NsPay.KafkaExtensions;
using Newtonsoft.Json;
using MassTransit;

namespace Neptune.NsPay.Web.KafkaApi.Handler
{
    public class TransferOrderCallBackEventHandler : IKafkaTopicHandler
    {
        private readonly IPublishEndpoint _publishEndpoint;
        public TransferOrderCallBackEventHandler(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public string Topic => KafkaTopics.TransferOrderCallBack;
        public async Task HandleAsync(string key, string message)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Key or message cannot be null or empty.");
            }
            try
            {
                //添加日志

                //推送RabbitMQ
                var transferOrder = JsonConvert.DeserializeObject<TransferOrderCallbackPublishDto>(message);
                await _publishEndpoint.Publish(transferOrder);
            }
            catch (Exception ex)
            {
                NlogLogger.Error("推送RabbitMQ异常：" + ex);
                throw new Exception($"Error handling transfer order event: {ex.Message}", ex);
            }

        }
    }
}
