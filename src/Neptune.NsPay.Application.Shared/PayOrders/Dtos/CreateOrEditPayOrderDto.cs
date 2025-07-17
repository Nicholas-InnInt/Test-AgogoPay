using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.PayOrders.Dtos
{
    public class CreateOrEditPayOrderDto : EntityDto<string>
    {
        [StringLength(PayOrderConsts.MaxMerchantCodeLength, MinimumLength = PayOrderConsts.MinMerchantCodeLength)]
        public string MerchantCode { get; set; }

        [StringLength(PayOrderConsts.MaxOrderNoLength, MinimumLength = PayOrderConsts.MinOrderNoLength)]
        public string OrderNo { get; set; }

        [StringLength(PayOrderConsts.MaxTransactionNoLength, MinimumLength = PayOrderConsts.MinTransactionNoLength)]
        public string TransactionNo { get; set; }

        public PayOrderOrderTypeEnum OrderType { get; set; }

        public PayOrderOrderStatusEnum OrderStatus { get; set; }

        public decimal OrderMoney { get; set; }

        public decimal Rate { get; set; }

        public decimal FeeMoney { get; set; }


        public DateTime OrderTime { get; set; }
        public DateTime TransactionTime { get; set; }

        public DateTime CreationTime { get; set; }

        [StringLength(PayOrderConsts.MaxOrderMarkLength, MinimumLength = PayOrderConsts.MinOrderMarkLength)]
        public string OrderMark { get; set; }

        [StringLength(PayOrderConsts.MaxOrderNumberLength, MinimumLength = PayOrderConsts.MinOrderNumberLength)]
        public string OrderNumber { get; set; }

        [StringLength(PayOrderConsts.MaxPlatformCodeLength, MinimumLength = PayOrderConsts.MinPlatformCodeLength)]
        public string PlatformCode { get; set; }

        public int PayMentId { get; set; }

        [StringLength(PayOrderConsts.MaxScCodeLength, MinimumLength = PayOrderConsts.MinScCodeLength)]
        public string ScCode { get; set; }

        [StringLength(PayOrderConsts.MaxScSeriLength, MinimumLength = PayOrderConsts.MinScSeriLength)]
        public string ScSeri { get; set; }

        [StringLength(PayOrderConsts.MaxNotifyUrlLength, MinimumLength = PayOrderConsts.MinNotifyUrlLength)]
        public string NotifyUrl { get; set; }

        public PayOrderScoreStatusEnum ScoreStatus { get; set; }

        [StringLength(PayOrderConsts.MaxPayTypeStrLength, MinimumLength = PayOrderConsts.MinPayTypeStrLength)]
        public string PayTypeStr { get; set; }
    }
}