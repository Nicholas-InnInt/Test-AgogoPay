using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.MerchantRates.Dtos
{
    public class CreateOrEditMerchantRateDto : EntityDto<int?>
    {

        [Required]
        [StringLength(MerchantRateConsts.MaxMerchantCodeLength, MinimumLength = MerchantRateConsts.MinMerchantCodeLength)]
        public string MerchantCode { get; set; }

        public int MerchantId { get; set; }

        public decimal ScanBankRate { get; set; }

        public decimal ScratchCardRate { get; set; }

        public decimal MoMoRate { get; set; }

        public decimal USDTFixedFees { get; set; }

        public decimal USDTRateFees { get; set; }
    }
}