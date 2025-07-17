namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class CheckMerchantResult:BaseReponse
    {
        public string MerchantCode { get; set; }
        public string LoginIpAddress { get; set;}
    }
}
