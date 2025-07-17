using Abp.AutoMapper;
using Abp.Organizations;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.OrganizationUnits
{
    [AutoMapFrom(typeof(OrganizationUnit))]
    public class EditOrganizationUnitModalViewModel
    {
        public long? Id { get; set; }

        public string DisplayName { get; set; }
    }
}