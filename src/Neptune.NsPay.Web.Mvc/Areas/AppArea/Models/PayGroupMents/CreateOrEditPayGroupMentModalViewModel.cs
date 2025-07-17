using Neptune.NsPay.PayGroupMents.Dtos;

using Abp.Extensions;
using Neptune.NsPay.PayMents.Dtos;
using System.Collections.Generic;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.PayGroupMents
{
    public class CreateOrEditPayGroupMentModalViewModel
    {
        public CreateOrEditPayGroupMentDto PayGroupMent { get; set; }

		public List<CreateOrEditPayMentDto> PayMents { get; set; }

		public bool IsEditMode => PayGroupMent.Id.HasValue;
    }
}