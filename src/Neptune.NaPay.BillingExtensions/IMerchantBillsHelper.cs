using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neptune.NsPay.MongoDbExtensions.Models;

namespace Neptune.NaPay.BillingExtensions
{
    public interface IMerchantBillsHelper
    {
        Task<bool> AddMerchantFundsFrozenBalance(string merchantCode, decimal orderMoney);
        Task<bool> UpdateWithRetryAddPayOrderBillAsync(string merchantCode, string orderId);
        Task<bool> UpdateWithRetryAddWithdrawalOrderBillAsync(string merchantCode, string orderId);
        Task<bool> UpdateWithRetryAddMerchantWithdrawBillAsync(string merchantCode, MerchantWithdrawMongoEntity merchantWithdraw);
        Task<bool> UpdateFrozenBalanceWithAttempt(string merchantCode, decimal amount, string eventId, int maxAttempt = 3);
        Task<bool> ResetFrozenBalanceWithAttempt(string merchantCode, decimal totalAmount, string eventId, int maxAttempt = 3);

        Task<bool> ReleaseWithdrawalWithAttempt(string orderId, string user);
    }
}
