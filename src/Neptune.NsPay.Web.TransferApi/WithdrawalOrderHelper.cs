using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.Commons;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.WithdrawalOrders;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.Web.TransferApi.SignalR;
using Neptune.NsPay.ELKLogExtension;
using Neptune.NsPay.RedisExtensions.Models;
using Amazon.Runtime.Internal.Transform;

namespace Neptune.NsPay.Web.TransferApi
{
    public class WithdrawalOrderHelper
    {
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly LogOrderService _logOrderService;

        private readonly Dictionary<string, NotifyTypeEnum> remarkReasonMapping = new Dictionary<string, NotifyTypeEnum>()
        {
            { "INVALID_ACCOUNT_NUMBER" , NotifyTypeEnum.ErrorBankcard },
            { "SYSTEM_MAINTENANCE" , NotifyTypeEnum.BankMaintenance },
            { "INCORRECT_RECEIPT_NAME" , NotifyTypeEnum.ErrorHolderName },
            { "INCORRECT_ACCOUNT_NAME" , NotifyTypeEnum.ErrorHolderName },
            { "ACCOUNTNAME NOT MATCHING" , NotifyTypeEnum.HolderInfoInvalid }, // android
            {"TRANSFER RECIPIENT ACCOUNT NUMBER ERROR" , NotifyTypeEnum.ErrorBankcard},// android
            {"BANK_ERROR" , NotifyTypeEnum.TransactionError},
            {"SYSTEM_BUSY" , NotifyTypeEnum.TransactionError},
            {"BANK_UNSUPPORTED" , NotifyTypeEnum.BankNotSupported},
        };

        public WithdrawalOrderHelper(IKafkaProducer kafkaProducer,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            LogOrderService logOrderService) 
        {

            _kafkaProducer = kafkaProducer;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _logOrderService = logOrderService;
        }

        public NotifyTypeEnum? GetNotifyTypeByRemark(string remark)
        {
            var matchedRemark = remarkReasonMapping.FirstOrDefault(x => remark.ToUpper().Contains(x.Key));
            return matchedRemark.Key != null ? matchedRemark.Value : null;
        }

        public async Task<bool> HandleWithdrawalOrderComplete(string eventId , string orderId , decimal amount   ,WithdrawalOrderStatusEnum status , string merchantCode , bool isConfirmed = false)
        {
            var isSuccess = false;
            bool needCallback = false;
            bool needInsertBills = false;

            if (status == WithdrawalOrderStatusEnum.Fail)
            {
                // device failed no need to release until bo do the manual release 
                needCallback = true;

                if(isConfirmed)
                {

                    try
                    {
                        isSuccess = true;
                        var isRelease = await _withdrawalOrdersMongoService.ReleaseWithdrawal(orderId, "SYSTEM");
                        var transferOrder = new MerchantBalancePublishDto()
                        {
                            MerchantCode = merchantCode,
                            Type = MerchantBalanceType.Decrease,
                            Money = Convert.ToInt32(amount),
                            Source = BalanceTriggerSource.WithdrawalOrder,
                            ProcessId = eventId
                        };
                        await _kafkaProducer.ProduceAsync<MerchantBalancePublishDto>(KafkaTopics.MerchantBalance, orderId, transferOrder);
                    }
                    catch (Exception ex)
                    {
                        NlogLogger.Error("Transfer-Kafka推送失败：" + ex.ToString());
                    }
                }
            }
            else if(status == WithdrawalOrderStatusEnum.Success)
            {
                needCallback = true;
                needInsertBills = true;
            }


            if (needCallback)
            {
                try
                {
                    await _kafkaProducer.ProduceAsync<TransferOrderCallbackPublishDto>(KafkaTopics.TransferOrderCallBack, orderId, new TransferOrderCallbackPublishDto() { ProcessId = eventId, WithdrawalOrderId = orderId });
                }
                catch (Exception ex)
                {
                    NlogLogger.Error("Transfer-Kafka推送失败：" + ex.ToString());
                }
            }

            if (needInsertBills)
            {
                try
                {
                    isSuccess = true;
                    await _kafkaProducer.ProduceAsync<TransferOrderPublishDto>(KafkaTopics.TransferOrder, orderId, new TransferOrderPublishDto()
                    {
                        ProcessId = eventId,
                        OrderStatus = (int)status,
                        MerchantCode = merchantCode,
                        TriggerDate = DateTime.Now,
                        WithdrawalOrderId = orderId
                    });
                }
                catch (Exception ex)
                {
                    NlogLogger.Error("Transfer-Kafka推送失败：" + ex.ToString());
                }
            }

            NlogLogger.Warn("HandleWithdrawalOrderComplete [" + eventId + "] - " + orderId + "Result - " + isSuccess);

            return isSuccess;
        }


    }
}
