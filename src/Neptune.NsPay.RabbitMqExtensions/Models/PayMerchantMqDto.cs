using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RabbitMqExtensions.Models
{
    public class PayMerchantMqDto
    {
        public string MerchantCode { get; set; }
        public string PayOrderId { get; set; }
    }
}
