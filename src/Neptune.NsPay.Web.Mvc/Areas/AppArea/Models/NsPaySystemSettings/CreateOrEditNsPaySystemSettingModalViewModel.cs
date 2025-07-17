using Neptune.NsPay.NsPaySystemSettings.Dtos;

using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.NsPaySystemSettings
{
    public class CreateOrEditNsPaySystemSettingModalViewModel
    {
        public CreateOrEditNsPaySystemSettingDto NsPaySystemSetting { get; set; }

        public bool IsEditMode => NsPaySystemSetting.Id.HasValue;
    }
}