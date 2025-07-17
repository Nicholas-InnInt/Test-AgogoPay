using Neptune.NsPay.MongoDbExtensions.Models;

namespace Neptune.NsPay.BillingExtensions
{
    public interface IMerchantBillsHelper
    {
        //Task<bool> UpdateWithRetryAddPayOrderBillAsync(string merchantCode, string orderId);
        //Task<bool> UpdateWithRetryAddWithdrawalOrderBillAsync(string merchantCode, string orderId);
        //Task<bool> UpdateWithRetryAddMerchantWithdrawBillAsync(string merchantCode, MerchantWithdrawMongoEntity merchantWithdraw);

        // 新版本 V2
        Task<bool> AddRetryPayOrderBillAsync(string merchantCode, string orderId);

        Task<bool> AddRetryWithdrawalOrderBillAsync(string merchantCode, string orderId);

        Task<bool> AddRetryMerchantWithdrawBillAsync(string merchantCode, MerchantWithdrawMongoEntity merchantWithdraw);

        Task<bool> AddRetryPayOrderCryptoBillAsync(string merchantCode, string orderId);
    }
}