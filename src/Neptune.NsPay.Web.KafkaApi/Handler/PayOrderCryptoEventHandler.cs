using MassTransit;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.KafkaExtensions;
using Newtonsoft.Json;

namespace Neptune.NsPay.Web.KafkaApi.Handler
{
    public class PayOrderCryptoEventHandler : IKafkaTopicHandler
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public PayOrderCryptoEventHandler(ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider = sendEndpointProvider;
        }

        public string Topic => KafkaTopics.PayOrderCrypto;

        public async Task HandleAsync(string key, string message)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Key or message cannot be null or empty.");
            }

            var payOrderCryptoPublishDto = JsonConvert.DeserializeObject<PayOrderCryptoPublishDto>(message);
            if (payOrderCryptoPublishDto?.PayOrderDepositIds is not { Count: > 0 }) return;

            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqTopics.PayOrderCryptoQueue}"));
            await endpoint.Send(payOrderCryptoPublishDto);
        }
    }
}