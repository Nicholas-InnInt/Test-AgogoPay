using Neptune.NsPay.BankInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RedisExtensions.Models
{
    public class WithdrawalOrderRedisModel
    {
        public string Id { get; set; }
        public string MerchantCode { get; set; }
        public int MerchantId { get; set; }
        public string OrderNo { get; set; }
        public string BenBankName { get; set; }
        public string BenAccountNo { get; set; }
        public string BenAccountName { get; set; }
        public decimal OrderMoney { get; set; }
        public bool IsAccountNameVerified { get; set; }
    }
}
