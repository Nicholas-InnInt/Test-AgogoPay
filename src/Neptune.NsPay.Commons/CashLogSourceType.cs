using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Commons
{
    public enum CashLogSourceType
    {
        CreateWithdrawalOrder = 1 , 
        CompleteWithdrawalOrder =2,
        ReleaseFrozenWithdrawalOrder = 3,
        CreateMerchantWithdrawal = 10,
        CompleteMerchantWithdrawal =11,
        ReleaseFrozenMerchantWithdrawal = 12,
        CompletePayOrder = 20,
        ResetFrozenBalance = 999,
    }
}
