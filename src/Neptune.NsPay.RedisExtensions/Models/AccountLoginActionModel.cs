using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RedisExtensions.Models
{
    public class AccountLoginActionRedisModel
    {
        public string? Account { get; set; } // Bankcard Number
        public int PayMentId { get; set; } // PaymentId
        public string? Phone { get; set; } // Account Phone Number
        public string? MerchantCode { get; set; } // Merchant Code
        public string? LoginOTP { get; set; } // LoginOTP
        public int Status { get; set; } //  0 - Pending , 1- Success , 2 - Failed
        public string? AuthOTP { get; set; }
        public string? TypeStr { get; set; }

    }
}
