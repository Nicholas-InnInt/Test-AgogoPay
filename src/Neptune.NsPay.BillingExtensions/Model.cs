using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neptune.NsPay.Commons;

namespace Neptune.NsPay.BillingExtensions
{

    public class UpdateFundResult
    {
        public string fundId { get; set; }
        public decimal balanceBefore { get; set; }
        public decimal balanceAfter { get; set; }
        public decimal frozenAmount { get; set; }
        public decimal totalDeposit { get; set; }
        public decimal totalWithdrawal { get; set; }
        public decimal totalFees { get; set; }
        public long versionNumber { get; set; }
        public int retryCount { get; set; }
        public DateTime transactionDateUTC { get; set; }
        public string logId { get; set; }

    }


    public class MerchantUpdateDetails
    {
        public int type { get; set; } // 1 = withdrawal , 2 = merchant withdrawal  . 3 = payorder 
        public string orderId { get; set; }
        public decimal balanceBefore { get; set; }
        public decimal balanceAfter { get; set; }
        public int merchantId { get; set; }
        public string merchantCode { get; set; }
        public long balanceVersion { get; set; }
        public bool needReleaseFrozen { get; set; }

    }

    public class MerchantFundUpdate
    {
        public decimal BalanceChanged { get; set; } // +values meaning increase , - value meaning deducts
        public decimal FrozenChanged { get; set; }
        public decimal DepositChanged { get; set; }
        public decimal WithdrawalChanged { get; set; }
        public decimal RateChanged { get; set; }
        public bool ChangedAmountIsExactValue { get; set; }
        public DateTime? LastMerchantWithdrawalTransactionTime { get; set; }
        public DateTime? LastWithdrawalOrderTransactionTime { get; set; }
        public DateTime? LastPayOrderTransactionTime { get; set; }
    }

    public class BalanceChangedBase
    {
        public virtual string GetReferenceId() { return string.Empty; }
        public CashLogSourceType SourceType { get; set; }

        public BalanceChangedBase(CashLogSourceType sourceType)
        {
            this.SourceType = sourceType;
        }
    }

    public class WithdrawalOrderBalanceChanged : BalanceChangedBase
    {
        // auto set type = 1
        public WithdrawalOrderBalanceChanged() : base(CashLogSourceType.CreateWithdrawalOrder) { }

        public override string GetReferenceId()
        {
            return WithdrawalId;
        }
        public string WithdrawalId { get; set; }
    }

    public class MerchantWithdrawalBalanceChanged : BalanceChangedBase
    {
        // auto set type = 1
        public MerchantWithdrawalBalanceChanged() : base(CashLogSourceType.CreateMerchantWithdrawal) { }

        public override string GetReferenceId()
        {
            return MerchantWithdrawalId;
        }
        public string MerchantWithdrawalId { get; set; }
    }

    public class PayOrderBalanceChanged : BalanceChangedBase
    {
        // auto set type = 1
        public PayOrderBalanceChanged() : base(CashLogSourceType.CompletePayOrder) { }

        public override string GetReferenceId()
        {
            return PayOrderId;
        }
        public string PayOrderId { get; set; }
    }

    public class ResetBalanceChanged : BalanceChangedBase
    {
        // auto set type = 1
        public ResetBalanceChanged() : base(CashLogSourceType.CreateMerchantWithdrawal) { }

        public override string GetReferenceId()
        {
            return EventId;
        }
        public string EventId { get; set; }
    }
}
