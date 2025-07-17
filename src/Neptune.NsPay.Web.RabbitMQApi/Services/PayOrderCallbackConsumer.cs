using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Newtonsoft.Json;
using MassTransit;

namespace Neptune.NsPay.Web.RabbitMQApi.Services
{
    public class PayOrderCallbackConsumer : IConsumer<PayOrderCallbackPublishDto>
    {
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly ICallBackService _callBackService;

        public PayOrderCallbackConsumer(IPayOrdersMongoService payOrdersMongoService,
            ICallBackService callBackService,
            IRedisService redisService)
        {
            _payOrdersMongoService = payOrdersMongoService;
            _callBackService = callBackService;
        }
        //public async Task OnHandle(CallbackPayOrderPublishDto message, CancellationToken cancellationToken)
        public async Task Consume(ConsumeContext<PayOrderCallbackPublishDto> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Message cannot be null.");
            }
            var message = context.Message;
            NlogLogger.Info("代收回调接受消息：" + JsonConvert.SerializeObject(message));
            var orderId = message.PayOrderId;

            try
            {
                //内充订单回调
                var payOrder = await _payOrdersMongoService.GetPayOrderByOrderId(message.PayOrderId);
                if (payOrder != null)
                {
                    if (payOrder.OrderType != PayOrderOrderTypeEnum.TopUp)
                    {
                        //回调平台
                        await _callBackService.CallBackPost(orderId);
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("回调入款订单异常：" + ex);
            }

            await Task.CompletedTask;
        }
    }
}