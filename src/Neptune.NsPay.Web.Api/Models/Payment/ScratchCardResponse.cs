using Neptune.NsPay.RedisExtensions.Models;

namespace Neptune.NsPay.Web.Api.Models.Payment
{
    public class ScratchCardResponse
    {
        public string OrderId { get; set; }
        public int PaymentId { get; set; }
        public string OrderNo { get; set; }
        public string Money { get; set; }
        public int OrderMoney { get; set; }
        public List<NameValueRedisModel> TypedCards { get; set; }
        public int SecondsToExpired { get; set; }
    }
}