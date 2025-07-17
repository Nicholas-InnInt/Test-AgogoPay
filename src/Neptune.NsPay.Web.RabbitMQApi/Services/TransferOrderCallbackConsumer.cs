using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.Commons;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Newtonsoft.Json;
using MassTransit;

namespace Neptune.NsPay.Web.RabbitMQApi.Services
{
    public class TransferOrderCallbackConsumer : IConsumer<TransferOrderCallbackPublishDto>
    {
        private readonly ITransferCallBackService _transferCallBackService;

        public TransferOrderCallbackConsumer(
            ITransferCallBackService transferCallBackService)
        {
            _transferCallBackService = transferCallBackService;
        }
        public async Task Consume(ConsumeContext<TransferOrderCallbackPublishDto> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Message cannot be null.");
            }
            var message = context.Message;

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null.");
            }
            NlogLogger.Info("代付回调接受消息：" + JsonConvert.SerializeObject(message));
            var orderId = message.WithdrawalOrderId;

            try
            {
                //回调平台
                await _transferCallBackService.TransferCallBackPost(orderId);
            }
            catch (Exception ex)
            {
                NlogLogger.Error("回调代付订单异常：" + ex);
            }

            await Task.CompletedTask;
        }
    }
}
