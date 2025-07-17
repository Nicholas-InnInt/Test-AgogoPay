using Neptune.NsPay.PayOrders;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Neptune.NsPay.Merchants;

namespace Neptune.NsPay.PayOrders
{
    [Table("PayOrders")]
    public class PayOrder : Entity<long>
    {

        [StringLength(PayOrderConsts.MaxMerchantCodeLength, MinimumLength = PayOrderConsts.MinMerchantCodeLength)]
        public virtual string MerchantCode { get; set; }

        public virtual int MerchantId { get; set; }

        [StringLength(PayOrderConsts.MaxOrderNoLength, MinimumLength = PayOrderConsts.MinOrderNoLength)]
        public virtual string OrderNo { get; set; }

        [StringLength(PayOrderConsts.MaxTransactionNoLength, MinimumLength = PayOrderConsts.MinTransactionNoLength)]
        public virtual string TransactionNo { get; set; }

        public virtual PayOrderOrderTypeEnum OrderType { get; set; }

        public virtual PayOrderOrderStatusEnum OrderStatus { get; set; }

        public virtual decimal OrderMoney { get; set; }

        public virtual decimal Rate { get; set; }

        public virtual decimal FeeMoney { get; set; }

        public virtual DateTime TransactionTime { get; set; }

        public virtual DateTime CreationTime { get; set; }

        public virtual long CreationUnixTime { get; set; }

        [StringLength(PayOrderConsts.MaxOrderMarkLength, MinimumLength = PayOrderConsts.MinOrderMarkLength)]
        public virtual string OrderMark { get; set; }

        [StringLength(PayOrderConsts.MaxOrderNumberLength, MinimumLength = PayOrderConsts.MinOrderNumberLength)]
        public virtual string OrderNumber { get; set; }

        [StringLength(PayOrderConsts.MaxPlatformCodeLength, MinimumLength = PayOrderConsts.MinPlatformCodeLength)]
        public virtual string PlatformCode { get; set; }

        public virtual int PayMentId { get; set; }

        [StringLength(PayOrderConsts.MaxScCodeLength, MinimumLength = PayOrderConsts.MinScCodeLength)]
        public virtual string ScCode { get; set; }

        [StringLength(PayOrderConsts.MaxScSeriLength, MinimumLength = PayOrderConsts.MinScSeriLength)]
        public virtual string ScSeri { get; set; }

        [StringLength(PayOrderConsts.MaxNotifyUrlLength, MinimumLength = PayOrderConsts.MinNotifyUrlLength)]
        public virtual string NotifyUrl { get; set; }

        [StringLength(PayOrderConsts.MaxUserIdLength, MinimumLength = PayOrderConsts.MinUserIdLength)]
        public virtual string UserId { get; set; }

        [StringLength(PayOrderConsts.MaxUserNoLength, MinimumLength = PayOrderConsts.MinUserNoLength)]
        public virtual string UserNo { get; set; }

        public virtual PayOrderScoreStatusEnum ScoreStatus { get; set; }

        public virtual int PayType { get; set; }

        public virtual int ScoreNumber { get; set; }

        public virtual decimal TradeMoney { get; set; }

        [StringLength(PayOrderConsts.MaxIPAddressLength, MinimumLength = PayOrderConsts.MinIPAddressLength)]
        public virtual string IPAddress { get; set; }

        [StringLength(PayOrderConsts.MaxErrorMsgLength, MinimumLength = PayOrderConsts.MinErrorMsgLength)]
        public virtual string ErrorMsg { get; set; }

        [StringLength(PayOrderConsts.MaxRemarkLength, MinimumLength = PayOrderConsts.MinRemarkLength)]
        public virtual string Remark { get; set; }

        public virtual PaymentChannelEnum PaymentChannel { get; set; }
		public MerchantTypeEnum MerchantType { get; set; }

	}
}