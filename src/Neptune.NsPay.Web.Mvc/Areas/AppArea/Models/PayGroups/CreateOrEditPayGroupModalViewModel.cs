using Neptune.NsPay.PayGroups.Dtos;

using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.PayGroups
{
    public class CreateOrEditPayGroupModalViewModel
    {
        public CreateOrEditPayGroupDto PayGroup { get; set; }

        public bool IsEditMode => PayGroup.Id.HasValue;
    }
}