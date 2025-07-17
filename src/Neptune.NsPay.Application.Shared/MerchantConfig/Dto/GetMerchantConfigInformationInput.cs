using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.MerchantSettings;
using Abp.Localization;

namespace Neptune.NsPay.MerchantConfig.Dto
{
    public class GetMerchantConfigInformationInput
    {
        [Required]
        [StringLength(MerchantSettingConsts.MaxMerchantCodeLength, MinimumLength = MerchantSettingConsts.MinMerchantCodeLength)]
        [RegularExpression(@"^[a-zA-Z0-9\s]*$", ErrorMessage = "MerchantCode_Validation_Error")]
        public string MerchantCode { get; set; }
        public int MerchantId { get; set; }

        [Required]
        [StringLength(MerchantSettingConsts.MaxNsPayTitleLength, MinimumLength = MerchantSettingConsts.MinNsPayTitleLength)]
        [RegularExpression(@"^[a-zA-Z0-9\s]*$", ErrorMessage = "No Symbol is allow in Merchant Title")]
        public string Title { get; set; }

        public string OrderBankRemark { get; set; }
    }
}
