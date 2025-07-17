using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace Neptune.NsPay.PayOrderDeposits
{
    [Table("PayOrderDeposits")]
    public class PayOrderDeposit : Entity<long>
    {

        [StringLength(PayOrderDepositConsts.MaxRefNoLength, MinimumLength = PayOrderDepositConsts.MinRefNoLength)]
        public virtual string RefNo { get; set; }

        public virtual long BankOrderId { get; set; }

        public virtual int PayType { get; set; }

        [StringLength(PayOrderDepositConsts.MaxTypeLength, MinimumLength = PayOrderDepositConsts.MinTypeLength)]
        public virtual string Type { get; set; }

        [StringLength(PayOrderDepositConsts.MaxDescriptionLength, MinimumLength = PayOrderDepositConsts.MinDescriptionLength)]
        public virtual string Description { get; set; }

        public virtual decimal CreditAmount { get; set; }

        public virtual decimal DebitAmount { get; set; }

        public virtual decimal AvailableBalance { get; set; }

        [StringLength(PayOrderDepositConsts.MaxCreditBankLength, MinimumLength = PayOrderDepositConsts.MinCreditBankLength)]
        public virtual string CreditBank { get; set; }

        [StringLength(PayOrderDepositConsts.MaxCreditAcctNoLength, MinimumLength = PayOrderDepositConsts.MinCreditAcctNoLength)]
        public virtual string CreditAcctNo { get; set; }

        [StringLength(PayOrderDepositConsts.MaxCreditAcctNameLength, MinimumLength = PayOrderDepositConsts.MinCreditAcctNameLength)]
        public virtual string CreditAcctName { get; set; }

        [StringLength(PayOrderDepositConsts.MaxDebitBankLength, MinimumLength = PayOrderDepositConsts.MinDebitBankLength)]
        public virtual string DebitBank { get; set; }

        [StringLength(PayOrderDepositConsts.MaxDebitAcctNoLength, MinimumLength = PayOrderDepositConsts.MinDebitAcctNoLength)]
        public virtual string DebitAcctNo { get; set; }

        [StringLength(PayOrderDepositConsts.MaxDebitAcctNameLength, MinimumLength = PayOrderDepositConsts.MinDebitAcctNameLength)]
        public virtual string DebitAcctName { get; set; }

        public virtual DateTime TransactionTime { get; set; }

        public virtual DateTime CreationTime { get; set; }

        public virtual long CreationUnixTime { get; set; }

        public virtual long OrderId { get; set; }

        public virtual int MerchantId { get; set; }

        [StringLength(PayOrderDepositConsts.MaxMerchantCodeLength, MinimumLength = PayOrderDepositConsts.MinMerchantCodeLength)]
        public virtual string MerchantCode { get; set; }

        [StringLength(PayOrderDepositConsts.MaxRejectRemarkLength, MinimumLength = PayOrderDepositConsts.MinRejectRemarkLength)]
        public virtual string RejectRemark { get; set; }

        [StringLength(PayOrderDepositConsts.MaxUserNameLength, MinimumLength = PayOrderDepositConsts.MinUserNameLength)]
        public virtual string UserName { get; set; }

        [StringLength(PayOrderDepositConsts.MaxAccountNoLength, MinimumLength = PayOrderDepositConsts.MinAccountNoLength)]
        public virtual string AccountNo { get; set; }

        public virtual int PayMentId { get; set; }

        public virtual DateTime OperateTime { get; set; }

        public virtual long UserId { get; set; }

    }
}