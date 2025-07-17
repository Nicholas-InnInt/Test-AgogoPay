using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions.Models;

namespace Neptune.NsPay.Web.Api.Models
{
    public class PayViewModel
    {
        public string OrderId { get; set; }
        public string OrderNo { get; set; }
        public string QrCode { get; set; }
        public string Money { get; set; }
        public int OrderMoney { get; set; }
        public string OrderMark { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }

        public List<PayMentRedisModel> PayMents { get; set; }

        public PayMentTypeEnum? PayType { get; set; }

        public string MerchantTitle { get; set; }
        public string MerchantLogoUrl { get; set; }

        public string PayTypeString { get; set; }
        public int QrType { get; set; }
    }
}
