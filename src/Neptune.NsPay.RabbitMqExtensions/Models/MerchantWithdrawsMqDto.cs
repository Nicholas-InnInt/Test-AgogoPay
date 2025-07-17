using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RabbitMqExtensions.Models
{
    public class MerchantWithdrawsMqDto
    {
        public string MerchantCode { get; set; }
        public long MerchantWithdrawId { get; set; }
    }
}
