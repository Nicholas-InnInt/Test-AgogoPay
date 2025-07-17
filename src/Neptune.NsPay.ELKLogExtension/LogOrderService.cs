using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Neptune.NsPay.Commons.Models;

namespace Neptune.NsPay.ELKLogExtension
{
    public class LogOrderService : LogBase<OrderLogModel>
    {
        public LogOrderService(IHttpPostService httpPostService , ELkLogOption option) : base(httpPostService, option)
        {
        }
    }

    public class LogBase<T> where T : LogModelBase
    {

        private readonly IHttpPostService _httpPostService;
        private readonly ELkLogOption _option;
        public LogBase(IHttpPostService httpPostService, ELkLogOption option) {

            _httpPostService = httpPostService;
            _option = option;
        }

        public async Task<bool> SubmitLog (T content) 
        {
            bool isSuccess = false;
            try
            {

                var result = await _httpPostService.PostAsync<LogCommonRequest, LogCommonResponse>(new LogCommonRequest() { 
                    Level = content.LogLevel,
                     Key = content.GetKey(),
                     Message = JsonSerializer.Serialize(content),
                     Method = content.ActionName,
                     Topic =_option.Topic?? "NsPayAll"
                });

                isSuccess = true;
            }
            catch (Exception ex)
            {
            }

            return isSuccess;
        }

    }


    public class LogCommonRequest
    {
        public string Topic { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty; // JSON string — can be deserialized separately
        public string Level { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
    }


    public class LogCommonResponse
    {
        public int code { get; set; } // 200 meaning success
        public string message { get; set; }
    }

    public class OrderLogModel : LogModelBase
    {
        public string OrderId  { get; set; }

        public string OrderNumber { get; set; }

        public string OrderStatus { get; set; }

        public DateTime OrderCreationDate { get; set; }
        public int DeviceId { get; set; }

        public string Desc { get; set; }
        public override string GetKey()
        {
            return OrderId;
        }
    }


    public static class ActionNameList
    {
        #region Withdrawal Order
        public readonly static string CreateWithdrawalOrder = "WithdrawalOrder.Create";
        public readonly static string WithdrawalOrderLockBalance = "WithdrawalOrder.LockBalance";
        public readonly static string SendWithdrawalOrderDevice = "WithdrawalOrder.SendDevice";
        public readonly static string WithdrawalOrderUpdateStatus = "WithdrawalOrder.UpdateStatus";
        public readonly static string WithdrawalDeviceProcessCompleted = "WithdrawalOrder.DeviceCompleted";
        public readonly static string WithdrawalUpdateMerchantBills = "WithdrawalOrder.MerchantBills";
        public readonly static string WithdrawalReleaseLockedBalance = "WithdrawalOrder.ReleaseFrozenAmount";

        #endregion

        #region Pay Order 
        public readonly static string CreatePayOrder = "PayOrder.Create";
        public readonly static string PayOrderCompleted = "PayOrder.OrderCompleted";
        #endregion

        #region Merchant Withdrawal
        public readonly static string CreateMerchantWithdrawal = "MerchantWithdrawal.Create";
        public readonly static string MerchantWithdrawalCompleted = "MerchantWithdrawal.OrderCompleted";
        public readonly static string MerchantWithdrawalRelease = "MerchantWithdrawal.ReleaseFrozen";

        #endregion
    }


    public class LogModelBase
    {
        public virtual string GetKey()
        {
            return string.Empty;
        }
        public DateTime LogDate { get; set; }
        public string LogLevel { get; set; } = "Info"; // default is Info

        public string ActionName { get; set; } // ActionNameList

        public string ProceedId { get; set; }

        public string User { get; set; } // whom trigger the Action , can be user or service name
        public long ProcessingTimeMs { get; set; }  // processing time in milisecond

    }

}

