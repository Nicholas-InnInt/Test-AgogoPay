using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RedisExtensions.Models
{
    public class WithdrawalOrderPubModel
    {
        public string MerchantCode { get; set; }
        public string OrderId { get; set; }
    }
}
