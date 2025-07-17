using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.NsPayBackgroundJobs.Dtos;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.NsPayBackgroundJobs
{
    public interface INsPayBackgroundJobsAppService : IApplicationService
    {
        Task<PagedResultDto<GetNsPayBackgroundJobForViewDto>> GetAll(GetAllNsPayBackgroundJobsInput input);

        Task<GetNsPayBackgroundJobForViewDto> GetNsPayBackgroundJobForView(Guid id);

        Task<GetNsPayBackgroundJobForEditOutput> GetNsPayBackgroundJobForEdit(EntityDto<Guid> input);

        Task CreateOrEdit(CreateOrEditNsPayBackgroundJobDto input);

        Task Delete(EntityDto<Guid> input);

        Task Pause(EntityDto<Guid> input);
        Task Restart(EntityDto<Guid> input);
    }
}