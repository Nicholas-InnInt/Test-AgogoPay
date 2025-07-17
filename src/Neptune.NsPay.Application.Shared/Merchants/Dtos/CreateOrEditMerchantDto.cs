using Abp.Application.Services.Dto;
using Neptune.NsPay.Utils;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.Merchants.Dtos
{
    public class CreateOrEditMerchantDto : EntityDto<int?>
    {
        [Required]
        [StringLength(MerchantConsts.MaxNameLength, MinimumLength = MerchantConsts.MinNameLength)]
        public string Name { get; set; }
        [Required]
        public string Mail { get; set; }

        [Required]
        [EnumDataType(typeof(CountryCodeEnum))]
        public CountryCodeEnum PhoneCountryCode { get; set; }

        [Required]
        [StringLength(MerchantConsts.MaxPhoneLength, MinimumLength = MerchantConsts.MinPhoneLength)]
        public string Phone { get; set; }

        [StringLength(MerchantConsts.MaxMerchantCodeLength, MinimumLength = MerchantConsts.MinMerchantCodeLength)]
        public string MerchantCode { get; set; }

        [StringLength(MerchantConsts.MaxMerchantSecretLength, MinimumLength = MerchantConsts.MinMerchantSecretLength)]
        public string MerchantSecret { get; set; }

        [StringLength(MerchantConsts.MaxPlatformCodeLength, MinimumLength = MerchantConsts.MinPlatformCodeLength)]
        public string PlatformCode { get; set; }

        public int PayGroupId { get; set; }

        [StringLength(MerchantConsts.MaxCountryTypeLength, MinimumLength = MerchantConsts.MinCountryTypeLength)]
        public string CountryType { get; set; }

        public string Remark { get; set; }

        public decimal ScanBankRate { get; set; }

        public decimal ScratchCardRate { get; set; }

        public decimal MoMoRate { get; set; }

        public decimal USDTFixedFees { get; set; }

        public decimal USDTRateFees { get; set; }

        public MerchantTypeEnum MerchantType { get; set; }
    }
}