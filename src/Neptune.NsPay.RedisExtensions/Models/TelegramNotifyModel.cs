using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RedisExtensions.Models
{
    public class TelegramNotifyModel
    {
        public DateTime TriggerDate { get; set; }
        public string MerchantCode { get; set; }
        public string OrderId { get; set; }
        public string OrderNumber { get; set; }
        public decimal OrderAmount { get; set; }
        public long OrderTimeUnix { get; set; }
        public NotifyTypeEnum Type  { get; set; }
    }
    public enum NotifyTypeEnum
    {
        ErrorBankcard,
        ErrorHolderName,
        BankNotSupported,
        BankMaintenance,
        HolderInfoInvalid,
        TransactionError,
    }

}
