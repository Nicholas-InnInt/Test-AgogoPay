using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Caching.Dto;
using System.Threading.Tasks;

namespace Neptune.NsPay.NsPayMaintenance
{
    public interface INsPayMaintenanceAppService : IApplicationService
    {
        ListResultDto<CacheDto> GetAllCaches();
        Task ClearCache(EntityDto<string> input);
        Task ClearAllCaches();
    }
}