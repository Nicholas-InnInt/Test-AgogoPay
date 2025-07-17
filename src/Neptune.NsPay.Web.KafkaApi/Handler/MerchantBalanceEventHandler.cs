using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.Commons;
using Neptune.NsPay.KafkaExtensions;
using Newtonsoft.Json;
using MassTransit;

namespace Neptune.NsPay.Web.KafkaApi.Handler
{
    public class MerchantBalanceEventHandler : IKafkaTopicHandler
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;
        public MerchantBalanceEventHandler(ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider = sendEndpointProvider;
        }

        public string Topic => KafkaTopics.MerchantBalance;
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
                var balance = JsonConvert.DeserializeObject<MerchantBalancePublishDto>(message);
                var part = MerchantPartitioner.GetPartition(balance.MerchantCode);
                var routingKey = $"{RabbitMqTopics.MerchantOrderSub}.p{part}";
                var endpoint = await _sendEndpointProvider.GetSendEndpoint(
                    new Uri($"exchange:{RabbitMqTopics.MerchantOrderTopic}?type=topic")
                );
                await endpoint.Send(balance, ctx =>
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