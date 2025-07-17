namespace Neptune.NsPay.RedisExtensions.Models
{
    public class MerchantRateRedisModel
    {
        public decimal ScanBankRate { get; set; }
        public decimal ScratchCardRate { get; set; }
        public decimal MoMoRate { get; set; }
        public decimal ZaloRate { get; set; }
        public decimal VittelPayRate { get; set; }
        public decimal USDTFixedFees { get; set; }
        public decimal USDTRateFees { get; set; }
    }
}
