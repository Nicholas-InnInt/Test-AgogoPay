using Neptune.NsPay.PayOrders;
using System.Collections.Generic;

namespace Neptune.NsPay.MerchantDashboard.Dtos
{
    public class MerchantDashboardDto 
    {
        public string MerchantCode { get; set; }
        public long MerchantId { get; set; }
        public string MerchantName { get; set; }
        public decimal TotalMerchantFund { get; set; }
        public decimal TotalFrozenBalance { get; set; }
        public decimal CurrentMerchantFee { get; set; }
        public decimal CurrentPayOrderMerchantFee { get; set; }
        public decimal CurrentWithdrawOrderMerchantFee { get; set; }
        public decimal CurrentPayOrderCashIn { get; set; }
        public List<CurrentPayOrderCashInByType> CurrentPayOrderCashInByTypes { get; set; }
        public long TotalCurrentPayOrderCashInCount { get; set; }
        public decimal CurrentMerchantWithdraw { get; set; }
        public long TotalCurrentMerchantWithdrawCount { get; set; }
        public decimal CurrentWithdrawalOrder { get; set; }
        public long TotalCurrentWithdrawalOrderCount { get; set; }
    }

    public class CurrentPayOrderCashInByType
    {
        public long MerchantId { get; set; }
        public long PaymentChannel { get; set; }
        public decimal CashInByType { get; set; }
        public decimal CashInFeeByType { get; set; }
        public long CashInCountByType { get; set; }
    }
}