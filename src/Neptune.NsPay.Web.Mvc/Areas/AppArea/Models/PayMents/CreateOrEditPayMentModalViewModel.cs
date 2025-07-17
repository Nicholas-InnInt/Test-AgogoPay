using Neptune.NsPay.PayMents.Dtos;

using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.PayMents
{
    public class CreateOrEditPayMentModalViewModel
    {
        public CreateOrEditPayMentDto PayMent { get; set; }

        public bool IsEditMode => PayMent.Id.HasValue;
    }
}