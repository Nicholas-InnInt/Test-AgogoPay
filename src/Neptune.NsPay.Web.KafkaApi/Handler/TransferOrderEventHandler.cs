using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.KafkaExtensions;
using Newtonsoft.Json;
using MassTransit;

namespace Neptune.NsPay.Web.KafkaApi.Handler
{
    public class TransferOrderEventHandler : IKafkaTopicHandler
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;
        public TransferOrderEventHandler(ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider = sendEndpointProvider;
        }

        public string Topic => KafkaTopics.TransferOrder;
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
                var transferOrder = JsonConvert.DeserializeObject<TransferOrderPublishDto>(message);
                var part = MerchantPartitioner.GetPartition(transferOrder.MerchantCode);
                var routingKey = $"{RabbitMqTopics.MerchantOrderSub}.p{part}";
                var endpoint = await _sendEndpointProvider.GetSendEndpoint(
                    new Uri($"exchange:{RabbitMqTopics.MerchantOrderTopic}?type=topic")
                );
                await endpoint.Send(transferOrder, ctx =>
                {
                    ctx.SetRoutingKey(routingKey);
                });
            }
            catch (Exception ex)
            {
                NlogLogger.Error("推送RabbitMQ异常：" + ex);
                throw new Exception($"Error handling transfer order event: {ex.Message}", ex);
            }
        }
    }
}