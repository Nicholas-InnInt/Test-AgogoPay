using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RedisExtensions.Models
{
    public class PayMerchantRedisMqDto
    {
        public string PayMqSubType { get; set; }
        public string MerchantCode { get; set; }
        public string PayOrderId { get; set; }
        public string WithdrawalOrderId { get; set; }
        public long MerchantWithdrawId { get; set; }
    }
}
