using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.PayOrders.Dtos
{
    public class PayOrderDto : EntityDto<string>
    {
        public string MerchantCode { get; set; }
        public string OrderNo { get; set; }
        public string TransactionNo { get; set; }
        public PayOrderOrderTypeEnum OrderType { get; set; }
        public PayOrderOrderStatusEnum OrderStatus { get; set; }
        public string OrderMoney { get; set; }
        public decimal OrderMoneyAmount { get; set; }
        public decimal Rate { get; set; }
        public string FeeMoney { get; set; }
        public decimal FeeMoneyAmount { get; set; }
        public DateTime OrderTime { get; set; }
        public DateTime TransactionTime { get; set; }
        public DateTime CreationTime { get; set; }
        public string OrderMark { get; set; }
        public string OrderNumber { get; set; }
        public string PlatformCode { get; set; }
        public string ScCode { get; set; }
        public string ScSeri { get; set; }
        public PayOrderScoreStatusEnum ScoreStatus { get; set; }
        public string PayTypeStr { get; set; }
        public string ErrorMsg { get; set; }
        public string UserNo { get; set; }
        public PaymentChannelEnum PaymentChannel { get; set; }
        public string Remark { get; set; }
    }
}