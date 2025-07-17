using Neptune.NsPay.MerchantWithdraws;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Neptune.NsPay.MerchantWithdraws
{
    [Table("MerchantWithdraws")]
    public class MerchantWithdraw : FullAuditedEntity<long>
    {

        [StringLength(MerchantWithdrawConsts.MaxMerchantCodeLength, MinimumLength = MerchantWithdrawConsts.MinMerchantCodeLength)]
        public virtual string MerchantCode { get; set; }

        public virtual int MerchantId { get; set; }

        [StringLength(MerchantWithdrawConsts.MaxWithDrawNoLength, MinimumLength = MerchantWithdrawConsts.MinWithDrawNoLength)]
        public virtual string WithDrawNo { get; set; }

        public virtual decimal Money { get; set; }

        [StringLength(MerchantWithdrawConsts.MaxBankNameLength, MinimumLength = MerchantWithdrawConsts.MinBankNameLength)]
        public virtual string BankName { get; set; }

        [StringLength(MerchantWithdrawConsts.MaxReceivCardLength, MinimumLength = MerchantWithdrawConsts.MinReceivCardLength)]
        public virtual string ReceivCard { get; set; }

        [StringLength(MerchantWithdrawConsts.MaxReceivNameLength, MinimumLength = MerchantWithdrawConsts.MinReceivNameLength)]
        public virtual string ReceivName { get; set; }

        public virtual MerchantWithdrawStatusEnum Status { get; set; }

        public virtual DateTime ReviewTime { get; set; }

        [StringLength(MerchantWithdrawConsts.MaxReviewMsgLength, MinimumLength = MerchantWithdrawConsts.MinReviewMsgLength)]
        public virtual string ReviewMsg { get; set; }

        [StringLength(MerchantWithdrawConsts.MaxRemarkLength, MinimumLength = MerchantWithdrawConsts.MinRemarkLength)]
        public virtual string Remark { get; set; }

        public virtual int BankId { get; set; }

    }
}