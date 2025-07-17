using Neptune.NsPay.MerchantSettings.Dtos;

using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.MerchantSettings
{
    public class CreateOrEditMerchantSettingModalViewModel
    {
        public CreateOrEditMerchantSettingDto MerchantSetting { get; set; }

        public bool IsEditMode => MerchantSetting.Id.HasValue;
    }
}