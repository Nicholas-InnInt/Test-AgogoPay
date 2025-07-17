using System.Threading.Tasks;
using Abp.Application.Services;
using Neptune.NsPay.Configuration.Tenants.Dto;

namespace Neptune.NsPay.Configuration.Tenants
{
    public interface ITenantSettingsAppService : IApplicationService
    {
        Task<TenantSettingsEditDto> GetAllSettings();

        Task UpdateAllSettings(TenantSettingsEditDto input);

        Task ClearDarkLogo();
        
        Task ClearDarkLogoMinimal();

        Task ClearLightLogo();
        
        Task ClearLightLogoMinimal();

        Task ClearCustomCss();
    }
}
