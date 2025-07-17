using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.BillingExtensions
{
    public interface IMerchantBalanceHelper
    {
        Task<bool> AddMerchantFundsFrozenBalance(string merchantCode, decimal amount);
        Task<bool> UnlockFrozenBalanceAsync(string merchantCode, decimal amount);
    }
}
