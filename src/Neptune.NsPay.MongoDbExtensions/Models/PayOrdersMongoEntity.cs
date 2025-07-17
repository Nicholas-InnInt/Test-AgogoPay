using MongoDB.Entities;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrders;

namespace Neptune.NsPay.MongoDbExtensions.Models
{
    [Collection("payorders")]
    public class PayOrdersMongoEntity : BaseMongoEntity
    {
        public string? MerchantCode { get; set; }
        public int MerchantId { get; set; }
        public string? OrderNo { get; set; }
        public string? TransactionNo { get; set; }
        public PayOrderOrderTypeEnum OrderType { get; set; }
        public PayOrderOrderStatusEnum OrderStatus { get; set; }
        public decimal OrderMoney { get; set; }
        public decimal? UnproccessMoney { get; set; }
        public decimal? ProcessedMoney { get; set; }
        public decimal? ConversionRate { get; set; }
        public decimal Rate { get; set; }
        public decimal FeeMoney { get; set; }
        public DateTime OrderTime { get; set; }
        public string? OrderMark { get; set; }
        public string? OrderNumber { get; set; }
        public string? PlatformCode { get; set; }
        public int PayMentId { get; set; }
        public string? ScCode { get; set; }
        public string? ScSeri { get; set; }
        public string? NotifyUrl { get; set; }
        public string? UserId { get; set; }
        public string? UserNo { get; set; }
        public PayOrderScoreStatusEnum ScoreStatus { get; set; }
        public string? PayTypeStr { get; set; }
        public PayMentTypeEnum PayType { get; set; }
        public int ScoreNumber { get; set; }
        public decimal TradeMoney { get; set; }
        public string? IPAddress { get; set; }
        public string? ErrorMsg { get; set; }
        public string? Remark { get; set; }
        public PaymentChannelEnum PaymentChannel { get; set; }
        public MerchantTypeEnum MerchantType { get; set; }
        public bool IsBilled { get; set; }
    }
}