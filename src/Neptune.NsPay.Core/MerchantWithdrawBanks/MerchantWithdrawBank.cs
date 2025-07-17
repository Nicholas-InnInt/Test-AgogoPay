using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Neptune.NsPay.MerchantWithdrawBanks
{
    [Table("MerchantWithdrawBanks")]
    public class MerchantWithdrawBank : FullAuditedEntity
    {

        public virtual int MerchantId { get; set; }

        [StringLength(MerchantWithdrawBankConsts.MaxMerchantCodeLength, MinimumLength = MerchantWithdrawBankConsts.MinMerchantCodeLength)]
        public virtual string MerchantCode { get; set; }

        [StringLength(MerchantWithdrawBankConsts.MaxBankNameLength, MinimumLength = MerchantWithdrawBankConsts.MinBankNameLength)]
        public virtual string BankName { get; set; }

        [StringLength(MerchantWithdrawBankConsts.MaxReceivCardLength, MinimumLength = MerchantWithdrawBankConsts.MinReceivCardLength)]
        public virtual string ReceivCard { get; set; }

        [StringLength(MerchantWithdrawBankConsts.MaxReceivNameLength, MinimumLength = MerchantWithdrawBankConsts.MinReceivNameLength)]
        public virtual string ReceivName { get; set; }

        public virtual bool Status { get; set; }

    }
}