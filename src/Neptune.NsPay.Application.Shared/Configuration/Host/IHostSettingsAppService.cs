using System.Threading.Tasks;
using Abp.Application.Services;
using Neptune.NsPay.Configuration.Host.Dto;

namespace Neptune.NsPay.Configuration.Host
{
    public interface IHostSettingsAppService : IApplicationService
    {
        Task<HostSettingsEditDto> GetAllSettings();

        Task UpdateAllSettings(HostSettingsEditDto input);

        Task SendTestEmail(SendTestEmailInput input);
    }
}
