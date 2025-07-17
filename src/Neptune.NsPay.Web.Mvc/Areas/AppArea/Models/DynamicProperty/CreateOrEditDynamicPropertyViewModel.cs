using System.Collections.Generic;
using Neptune.NsPay.DynamicEntityProperties.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.DynamicProperty
{
    public class CreateOrEditDynamicPropertyViewModel
    {
        public DynamicPropertyDto DynamicPropertyDto { get; set; }

        public List<string> AllowedInputTypes { get; set; }
    }
}
