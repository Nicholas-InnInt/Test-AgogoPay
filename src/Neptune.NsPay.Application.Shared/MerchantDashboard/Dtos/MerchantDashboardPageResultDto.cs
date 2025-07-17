using Abp.Application.Services.Dto;

namespace Neptune.NsPay.MerchantDashboard.Dtos
{
    public class MerchantDashboardPageResultDto<T> : PagedResultDto<T>
    {
        public int TotalMerchantCount { get; set; }
        public string TotalMerchantFund { get; set; }
        public string TotalFrozenBalance { get; set; }
        public string TotalMerchantFee { get; set; }
        public string TotalPayOrderCashIn { get; set; }
        public long TotalPayOrderCashInCount { get; set; }
        public string TotalMerchantBillWithdraw { get; set; }
        public long TotalMerchantBillWithdrawCount { get; set; }
        public string TotalBillWithdrawalOrder { get; set; }
        public string TotalWithdrawalOrder { get; set; }
        public long TotalWithdrawalOrderCount { get; set; }
    }
}