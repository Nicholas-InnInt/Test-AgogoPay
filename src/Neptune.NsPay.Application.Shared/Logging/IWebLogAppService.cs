using Abp.Application.Services;
using Neptune.NsPay.Dto;
using Neptune.NsPay.Logging.Dto;

namespace Neptune.NsPay.Logging
{
    public interface IWebLogAppService : IApplicationService
    {
        GetLatestWebLogsOutput GetLatestWebLogs();

        FileDto DownloadWebLogs();
    }
}
