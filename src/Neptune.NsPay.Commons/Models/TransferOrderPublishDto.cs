using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Commons.Models
{
    public class TransferOrderPublishDto
    {
        public string MerchantCode { get; set; }
        public string WithdrawalOrderId { get; set; }
        public int OrderStatus { get; set; } // WithdrawalOrderStatusEnum , Success Will Check Merchant Bills And Reduce
        public DateTime TriggerDate { get; set; }
        //public bool IsReleaseAmountNeed { get; set; } // for release balance
        public string ProcessId { get; set; } // for Reference Id
    }
}
