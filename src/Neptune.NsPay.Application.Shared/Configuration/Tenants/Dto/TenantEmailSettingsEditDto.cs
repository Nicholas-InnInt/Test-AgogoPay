using Abp.Auditing;
using Neptune.NsPay.Configuration.Dto;

namespace Neptune.NsPay.Configuration.Tenants.Dto
{
    public class TenantEmailSettingsEditDto : EmailSettingsEditDto
    {
        public bool UseHostDefaultEmailSettings { get; set; }
    }
}