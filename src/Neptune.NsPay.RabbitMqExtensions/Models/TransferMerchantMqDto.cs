using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RabbitMqExtensions.Models
{
    public class TransferMerchantMqDto
    {
        public string MerchantCode { get; set; }
        public string WithdrawalOrderId { get; set; }
    }
}
