using System.Collections.Generic;
using Neptune.NsPay.Organizations.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Common
{
    public interface IOrganizationUnitsEditViewModel
    {
        List<OrganizationUnitDto> AllOrganizationUnits { get; set; }

        List<string> MemberedOrganizationUnits { get; set; }
    }
}