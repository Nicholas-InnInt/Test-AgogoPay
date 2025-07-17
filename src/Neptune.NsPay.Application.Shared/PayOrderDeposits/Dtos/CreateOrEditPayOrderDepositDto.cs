using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.PayOrderDeposits.Dtos
{
    public class CreateOrEditPayOrderDepositDto : EntityDto<string>
    {

        [StringLength(PayOrderDepositConsts.MaxRefNoLength, MinimumLength = PayOrderDepositConsts.MinRefNoLength)]
        public string RefNo { get; set; }

        //public long BankOrderId { get; set; }

        public int PayType { get; set; }

        [StringLength(PayOrderDepositConsts.MaxTypeLength, MinimumLength = PayOrderDepositConsts.MinTypeLength)]
        public string Type { get; set; }

        [StringLength(PayOrderDepositConsts.MaxDescriptionLength, MinimumLength = PayOrderDepositConsts.MinDescriptionLength)]
        public string Description { get; set; }

        public decimal CreditAmount { get; set; }

        public decimal DebitAmount { get; set; }

        public decimal AvailableBalance { get; set; }

        [StringLength(PayOrderDepositConsts.MaxCreditBankLength, MinimumLength = PayOrderDepositConsts.MinCreditBankLength)]
        public string CreditBank { get; set; }

        [StringLength(PayOrderDepositConsts.MaxCreditAcctNoLength, MinimumLength = PayOrderDepositConsts.MinCreditAcctNoLength)]
        public string CreditAcctNo { get; set; }

        [StringLength(PayOrderDepositConsts.MaxCreditAcctNameLength, MinimumLength = PayOrderDepositConsts.MinCreditAcctNameLength)]
        public string CreditAcctName { get; set; }

        [StringLength(PayOrderDepositConsts.MaxDebitBankLength, MinimumLength = PayOrderDepositConsts.MinDebitBankLength)]
        public string DebitBank { get; set; }

        [StringLength(PayOrderDepositConsts.MaxDebitAcctNoLength, MinimumLength = PayOrderDepositConsts.MinDebitAcctNoLength)]
        public string DebitAcctNo { get; set; }

        [StringLength(PayOrderDepositConsts.MaxDebitAcctNameLength, MinimumLength = PayOrderDepositConsts.MinDebitAcctNameLength)]
        public string DebitAcctName { get; set; }

        public DateTime TransactionTime { get; set; }

        public DateTime CreationTime { get; set; }

        public long CreationUnixTime { get; set; }

        public string OrderId { get; set; }

        public int MerchantId { get; set; }

        [StringLength(PayOrderDepositConsts.MaxMerchantCodeLength, MinimumLength = PayOrderDepositConsts.MinMerchantCodeLength)]
        public string MerchantCode { get; set; }

        [StringLength(PayOrderDepositConsts.MaxRejectRemarkLength, MinimumLength = PayOrderDepositConsts.MinRejectRemarkLength)]
        public string RejectRemark { get; set; }

        [StringLength(PayOrderDepositConsts.MaxUserNameLength, MinimumLength = PayOrderDepositConsts.MinUserNameLength)]
        public string UserName { get; set; }

        [StringLength(PayOrderDepositConsts.MaxAccountNoLength, MinimumLength = PayOrderDepositConsts.MinAccountNoLength)]
        public string AccountNo { get; set; }

        public int PayMentId { get; set; }

        public DateTime OperateTime { get; set; }

        public long UserId { get; set; }

    }
}