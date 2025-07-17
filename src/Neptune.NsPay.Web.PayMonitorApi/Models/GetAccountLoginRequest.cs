using Neptune.NsPay.PayMents;

namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class GetAccountLoginRequest
    {
        public string? Account { get; set; } // Bankcard Number
        public string? Phone { get; set; } // Account Phone Number
        public  string? TypeStr { get; set; } // Account Bank

        public string? MerchantCode { get; set; } // Merchant Code
    }
}
