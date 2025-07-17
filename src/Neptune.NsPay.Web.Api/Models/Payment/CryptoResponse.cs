namespace Neptune.NsPay.Web.Api.Models.Payment
{
    public class CryptoResponse
    {
        public string OrderId { get; set; }
        public string OrderNo { get; set; }
        public string QrCode { get; set; }
        public string Money { get; set; }
        public decimal OrderMoney { get; set; }
        public decimal ConversionRate { get; set; }
        public string ConvertedMoney { get; set; }
        public string WalletAddress { get; set; }
        public string PayType { get; set; }
        public int SecondsToExpired { get; set; }
    }
}