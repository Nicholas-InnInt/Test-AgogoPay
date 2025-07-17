namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class PayMonitorAccountInput:BaseInput
    {
        public string? MerchantCode { get; set; }
        public string Account { get; set; }
    }
}
