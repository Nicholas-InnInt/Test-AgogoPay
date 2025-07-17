using Neptune.NsPay.RedisExtensions.Models;

namespace Neptune.NsPay.Web.Api.Models
{
    public class ScratchCardViewModel
    {
        public string orderid { get; set; }
        public int paymentid { get; set; }
        public decimal money { get; set; }
        public int status { get; set; }

        public List<NameValueRedisModel> TypeCard { get; set; }
    }
}
