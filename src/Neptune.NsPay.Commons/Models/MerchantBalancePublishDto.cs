using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Commons.Models
{
    public class MerchantBalancePublishDto
    {
        public string MerchantCode { get; set; }
        public MerchantBalanceType Type { get; set; } // 1:增加, 2:减少
        public BalanceTriggerSource Source { get; set; }    // 触发来源
        public int Money { get; set; }
        public DateTime TriggerDate { get; set; }
        //public bool IsReleaseAmountNeed { get; set; } // for release balance
        public string ProcessId { get; set; } // for Reference Id
        public string ReferenceId { get; set; } // for Source Reference Id

    }

    public enum BalanceTriggerSource
    {
        PayOrder = 1,
        WithdrawalOrder = 2,
        MerchantWithdrawal = 3
    }
    public enum MerchantBalanceType
    {
        Increase = 1, // 增加
        Decrease = 2  // 减少
    }
}