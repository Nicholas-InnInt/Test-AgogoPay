using Abp.Application.Services.Dto;

namespace Neptune.NsPay.MerchantRates.Dtos
{
    public class MerchantRateDto : EntityDto
    {
        public string MerchantCode { get; set; }
        public int MerchantId { get; set; }
        public decimal ScanBankRate { get; set; }
        public decimal ScratchCardRate { get; set; }
        public decimal MoMoRate { get; set; }
        public decimal USDTFixedFees { get; set; }
        public decimal USDTRateFees { get; set; }
    }
}