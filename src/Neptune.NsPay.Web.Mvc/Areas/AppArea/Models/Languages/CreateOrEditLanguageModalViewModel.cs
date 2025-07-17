using Abp.AutoMapper;
using Neptune.NsPay.Localization.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Languages
{
    [AutoMapFrom(typeof(GetLanguageForEditOutput))]
    public class CreateOrEditLanguageModalViewModel : GetLanguageForEditOutput
    {
        public bool IsEditMode => Language.Id.HasValue;
    }
}