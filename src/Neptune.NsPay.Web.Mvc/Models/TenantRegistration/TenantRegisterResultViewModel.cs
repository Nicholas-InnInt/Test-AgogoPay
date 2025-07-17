using Abp.AutoMapper;
using Neptune.NsPay.MultiTenancy.Dto;

namespace Neptune.NsPay.Web.Models.TenantRegistration
{
    [AutoMapFrom(typeof(RegisterTenantOutput))]
    public class TenantRegisterResultViewModel : RegisterTenantOutput
    {
        public string TenantLoginAddress { get; set; }
    }
}