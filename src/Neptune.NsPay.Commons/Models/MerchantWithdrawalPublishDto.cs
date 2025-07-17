using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Commons.Models
{
    public class MerchantWithdrawalPublishDto
    {
        public string MerchantCode { get; set; }
        public long MerchantWithdrawId { get; set; }
        public DateTime TriggerDate { get; set; }
        //public bool IsReleaseAmountNeed { get; set; } // for release balance
        public string ProcessId { get; set; } // for Reference Id
    }
}
