using MassTransit;
using MongoDB.Entities;
using Neptune.NsPay.BillingExtensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Newtonsoft.Json;

namespace Neptune.NsPay.Web.RabbitMQApi.Services
{
    public class PayOrderCryptoConsumer : IConsumer<PayOrderCryptoPublishDto>
    {
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IMerchantBillsHelper _merchantBillsHelper;
        private readonly ICallBackService _callBackService;

        public PayOrderCryptoConsumer(
            IPayOrderDepositsMongoService payOrderDepositsMongoService,
            IPayOrdersMongoService payOrdersMongoService,
            IMerchantBillsHelper merchantBillsHelper,
            ICallBackService callBackService)
        {
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
            _payOrdersMongoService = payOrdersMongoService;
            _merchantBillsHelper = merchantBillsHelper;
            _callBackService = callBackService;
        }

        public async Task Consume(ConsumeContext<PayOrderCryptoPublishDto> context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Message cannot be null.");

            var message = context.Message;
            if (message == null)
                throw new ArgumentNullException(nameof(message), "Message cannot be null.");

            NlogLogger.Info("PayOrderCrypto Message: " + JsonConvert.SerializeObject(message));

            try
            {
                // Only process merchant bill
                if (!string.IsNullOrEmpty(message.UpdateOrderNumberBillOnly))
                {
                    var payOrder = await _payOrdersMongoService.GetPayOrderByOrderNumber(message.MerchantCode, message.UpdateOrderNumberBillOnly);
                    if (payOrder is not null)
                    {
                        await MerchantBill(message.MerchantCode, payOrder.ID);
                    }

                    return;
                }

                var payOrderDepositList = await _payOrderDepositsMongoService.GetListByIds(message.PayOrderDepositIds);
                NlogLogger.Info($"PayOrderCrypto Process PayOrderDeposits: {JsonConvert.SerializeObject(payOrderDepositList)}");

                if (payOrderDepositList is not { Count: > 0 }) return;

                foreach (var payOrderDeposit in payOrderDepositList)
                {
                    var payOrder = await _payOrdersMongoService.GetPayOrderByRemark(message.MerchantCode, message.PaymentType, null, payOrderDeposit.CreditAmount);
                    NlogLogger.Info($"PayOrderCrypto Process PayOrders: {JsonConvert.SerializeObject(payOrder)}");
                    if (payOrder is not null)
                    {
                        await DB.Update<PayOrderDepositsMongoEntity>()
                            .MatchID(payOrderDeposit.ID)
                            .Modify(x => x.OrderId, payOrder.ID)
                            .ExecuteAsync();

                        await _payOrdersMongoService.UpdateOrderStatusByOrderId(payOrder.ID, PayOrderOrderStatusEnum.Paid, payOrder.TradeMoney, payOrder.ErrorMsg);

                        await Task.WhenAll(MerchantBill(message.MerchantCode, payOrder.ID), CallBack(payOrder.ID));
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error($"PayOrderCrypto 异常: {ex.Message}", ex);
                throw new Exception($"Error handling transfer order event: {ex.Message}", ex);
            }
        }

        private async Task CallBack(string orderId)
        {
            try
            {
                await _callBackService.CallBackPost(orderId);
            }
            catch (Exception ex)
            {
                NlogLogger.Error($"PayOrderCrypto CallBack 异常: {ex.Message}", ex);
                throw new Exception($"Error during PayOrderCrypto callback: {ex.Message}", ex);
            }
        }

        private async Task MerchantBill(string merchantCode, string orderId)
        {
            try
            {
                await _merchantBillsHelper.AddRetryPayOrderCryptoBillAsync(merchantCode, orderId);
            }
            catch (Exception ex)
            {
                NlogLogger.Error($"PayOrderCrypto MerchantBill 异常: {ex.Message}", ex);
                throw new Exception($"Error during PayOrderCrypto merchantbill: {ex.Message}", ex);
            }
        }
    }
}