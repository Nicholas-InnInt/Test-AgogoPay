namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class TransferWinAccountInput
    {
        public string? MerchantCode { get; set; }
        public string Account { get; set; }
        public string Sign { get; set; }
    }
}
