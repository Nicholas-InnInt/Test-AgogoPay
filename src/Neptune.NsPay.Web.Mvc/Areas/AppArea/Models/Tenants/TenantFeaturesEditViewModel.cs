using Abp.AutoMapper;
using Neptune.NsPay.MultiTenancy;
using Neptune.NsPay.MultiTenancy.Dto;
using Neptune.NsPay.Web.Areas.AppArea.Models.Common;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Tenants
{
    [AutoMapFrom(typeof (GetTenantFeaturesEditOutput))]
    public class TenantFeaturesEditViewModel : GetTenantFeaturesEditOutput, IFeatureEditViewModel
    {
        public Tenant Tenant { get; set; }
    }
}