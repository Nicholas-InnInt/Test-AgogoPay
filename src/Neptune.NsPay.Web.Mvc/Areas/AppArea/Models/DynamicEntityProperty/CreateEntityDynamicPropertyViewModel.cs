using System.Collections.Generic;
using Neptune.NsPay.DynamicEntityProperties.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.DynamicEntityProperty
{
    public class CreateEntityDynamicPropertyViewModel
    {
        public string EntityFullName { get; set; }

        public List<string> AllEntities { get; set; }

        public List<DynamicPropertyDto> DynamicProperties { get; set; }
    }
}
