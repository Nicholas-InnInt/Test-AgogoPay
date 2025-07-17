using System.Threading.Tasks;
using Abp.Application.Services;
using Neptune.NsPay.Install.Dto;

namespace Neptune.NsPay.Install
{
    public interface IInstallAppService : IApplicationService
    {
        Task Setup(InstallDto input);

        AppSettingsJsonDto GetAppSettingsJson();

        CheckDatabaseOutput CheckDatabase();
    }
}