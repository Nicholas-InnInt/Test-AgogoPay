using Neptune.NsPay.WithdrawalOrders;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Neptune.NsPay.WithdrawalOrders
{
    [Table("WithdrawalOrders")]
    public class WithdrawalOrder : FullAuditedEntity<long>
    {

        [StringLength(WithdrawalOrderConsts.MaxMerchantCodeLength, MinimumLength = WithdrawalOrderConsts.MinMerchantCodeLength)]
        public virtual string MerchantCode { get; set; }

        public virtual int MerchantId { get; set; }

        [StringLength(WithdrawalOrderConsts.MaxPlatformCodeLength, MinimumLength = WithdrawalOrderConsts.MinPlatformCodeLength)]
        public virtual string PlatformCode { get; set; }

        [StringLength(WithdrawalOrderConsts.MaxWithdrawNoLength, MinimumLength = WithdrawalOrderConsts.MinWithdrawNoLength)]
        public virtual string WithdrawNo { get; set; }

        public virtual WithdrawalOrderStatusEnum OrderStatus { get; set; }

        public virtual decimal OrderMoney { get; set; }

        public virtual decimal Rate { get; set; }

        public virtual decimal FeeMoney { get; set; }

        [StringLength(WithdrawalOrderConsts.MaxTransactionNoLength, MinimumLength = WithdrawalOrderConsts.MinTransactionNoLength)]
        public virtual string TransactionNo { get; set; }

        public virtual DateTime TransactionTime { get; set; }

        [StringLength(WithdrawalOrderConsts.MaxBenAccountNameLength, MinimumLength = WithdrawalOrderConsts.MinBenAccountNameLength)]
        public virtual string BenAccountName { get; set; }

        [StringLength(WithdrawalOrderConsts.MaxBenAccountNoLength, MinimumLength = WithdrawalOrderConsts.MinBenAccountNoLength)]
        public virtual string BenAccountNo { get; set; }

        [StringLength(WithdrawalOrderConsts.MaxBenBankNameLength, MinimumLength = WithdrawalOrderConsts.MinBenBankNameLength)]
        public virtual string BenBankName { get; set; }

        [StringLength(WithdrawalOrderConsts.MaxNotifyUrlLength, MinimumLength = WithdrawalOrderConsts.MinNotifyUrlLength)]
        public virtual string NotifyUrl { get; set; }

        [StringLength(WithdrawalOrderConsts.MaxOrderNumberLength, MinimumLength = WithdrawalOrderConsts.MinOrderNumberLength)]
        public virtual string OrderNumber { get; set; }

        [StringLength(WithdrawalOrderConsts.MaxReceiptUrlLength, MinimumLength = WithdrawalOrderConsts.MinReceiptUrlLength)]
        public virtual string ReceiptUrl { get; set; }

        public virtual int DeviceId { get; set; }

        public virtual WithdrawalOrderTypeEnum OrderType { get; set; }

        public virtual WithdrawalNotifyStatusEnum NotifyStatus { get; set; }

        public virtual int NotifyNumber { get; set; }

        [StringLength(WithdrawalOrderConsts.MaxDescriptionLength, MinimumLength = WithdrawalOrderConsts.MinDescriptionLength)]
        public virtual string Description { get; set; }

        [StringLength(WithdrawalOrderConsts.MaxRemarkLength, MinimumLength = WithdrawalOrderConsts.MinRemarkLength)]
        public virtual string Remark { get; set; }

        public virtual DateTime CreationTime { get; set; }

        public virtual long CreationUnixTime { get; set; }

    }
}