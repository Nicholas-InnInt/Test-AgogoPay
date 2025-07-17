using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Neptune.NsPay.NsPayBackgroundJobs.Dtos;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Neptune.NsPay.NsPayBackgroundJobs
{
    [AbpAuthorize(AppPermissions.Pages_NsPayBackgroundJobs)]
    public class NsPayBackgroundJobsAppService : NsPayAppServiceBase, INsPayBackgroundJobsAppService
    {
        private readonly IRepository<NsPayBackgroundJob, Guid> _nsPayBackgroundJobRepository;

        public NsPayBackgroundJobsAppService(IRepository<NsPayBackgroundJob, Guid> nsPayBackgroundJobRepository)
        {
            _nsPayBackgroundJobRepository = nsPayBackgroundJobRepository;

        }

        public virtual async Task<PagedResultDto<GetNsPayBackgroundJobForViewDto>> GetAll(GetAllNsPayBackgroundJobsInput input)
        {
            var requsetModeFilter = input.RequsetModeFilter.HasValue
                        ? (NsPayBackgroundJobRequsetModeEnum)input.RequsetModeFilter
                        : default;
            var stateFilter = input.StateFilter.HasValue
                ? (NsPayBackgroundJobStateEnum)input.StateFilter
                : default;

            var filteredNsPayBackgroundJobs = _nsPayBackgroundJobRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.GroupName.Contains(input.Filter) || e.Cron.Contains(input.Filter) || e.ApiUrl.Contains(input.Filter) || e.ParamData.Contains(input.Filter) || e.MerchantCode.Contains(input.Filter) || e.Description.Contains(input.Filter) || e.Remark.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.Contains(input.NameFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CronFilter), e => e.Cron.Contains(input.CronFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ApiUrlFilter), e => e.ApiUrl.Contains(input.ApiUrlFilter))
                        .WhereIf(input.RequsetModeFilter.HasValue && input.RequsetModeFilter > -1, e => e.RequsetMode == requsetModeFilter)
                        .WhereIf(input.StateFilter.HasValue && input.StateFilter > -1, e => e.State == stateFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ParamDataFilter), e => e.ParamData.Contains(input.ParamDataFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.MerchantCodeFilter), e => e.MerchantCode.Contains(input.MerchantCodeFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.DescriptionFilter), e => e.Description.Contains(input.DescriptionFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.RemarkFilter), e => e.Remark.Contains(input.RemarkFilter));

            var pagedAndFilteredNsPayBackgroundJobs = filteredNsPayBackgroundJobs
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var nsPayBackgroundJobs = from o in pagedAndFilteredNsPayBackgroundJobs
                                      select new
                                      {

                                          o.Name,
                                          o.GroupName,
                                          o.Cron,
                                          o.ApiUrl,
                                          o.RequsetMode,
                                          o.State,
                                          o.ParamData,
                                          o.MerchantCode,
                                          o.Description,
                                          o.Remark,
                                          Id = o.Id
                                      };

            var totalCount = await filteredNsPayBackgroundJobs.CountAsync();

            var dbList = await nsPayBackgroundJobs.ToListAsync();
            var results = new List<GetNsPayBackgroundJobForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetNsPayBackgroundJobForViewDto()
                {
                    NsPayBackgroundJob = new NsPayBackgroundJobDto
                    {

                        Name = o.Name,
                        GroupName = o.GroupName,
                        Cron = o.Cron,
                        ApiUrl = o.ApiUrl,
                        RequsetMode = o.RequsetMode,
                        State = o.State,
                        ParamData = o.ParamData,
                        MerchantCode = o.MerchantCode,
                        Description = o.Description,
                        Remark = o.Remark,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetNsPayBackgroundJobForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetNsPayBackgroundJobForViewDto> GetNsPayBackgroundJobForView(Guid id)
        {
            var nsPayBackgroundJob = await _nsPayBackgroundJobRepository.GetAsync(id);

            var output = new GetNsPayBackgroundJobForViewDto { NsPayBackgroundJob = ObjectMapper.Map<NsPayBackgroundJobDto>(nsPayBackgroundJob) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_NsPayBackgroundJobs_Edit)]
        public virtual async Task<GetNsPayBackgroundJobForEditOutput> GetNsPayBackgroundJobForEdit(EntityDto<Guid> input)
        {
            var nsPayBackgroundJob = await _nsPayBackgroundJobRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetNsPayBackgroundJobForEditOutput { NsPayBackgroundJob = ObjectMapper.Map<CreateOrEditNsPayBackgroundJobDto>(nsPayBackgroundJob) };

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditNsPayBackgroundJobDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_NsPayBackgroundJobs_Create)]
        protected virtual async Task Create(CreateOrEditNsPayBackgroundJobDto input)
        {
            var nsPayBackgroundJob = ObjectMapper.Map<NsPayBackgroundJob>(input);

            await _nsPayBackgroundJobRepository.InsertAsync(nsPayBackgroundJob);

        }

        [AbpAuthorize(AppPermissions.Pages_NsPayBackgroundJobs_Edit)]
        protected virtual async Task Update(CreateOrEditNsPayBackgroundJobDto input)
        {
            var nsPayBackgroundJob = await _nsPayBackgroundJobRepository.FirstOrDefaultAsync((Guid)input.Id);
            ObjectMapper.Map(input, nsPayBackgroundJob);

        }

        [AbpAuthorize(AppPermissions.Pages_NsPayBackgroundJobs_Delete)]
        public virtual async Task Delete(EntityDto<Guid> input)
        {
            var nsPayBackgroundJob = await _nsPayBackgroundJobRepository.FirstOrDefaultAsync(input.Id);
            nsPayBackgroundJob.IsDeleted = true;
        }

        [AbpAuthorize(AppPermissions.Pages_NsPayBackgroundJobs_Edit)]
        public virtual async Task Pause(EntityDto<Guid> input)
        {
            var nsPayBackgroundJob = await _nsPayBackgroundJobRepository.FirstOrDefaultAsync(input.Id);
            nsPayBackgroundJob.IsPaused = true;
            nsPayBackgroundJob.State = NsPayBackgroundJobStateEnum.Pending;
        }

        [AbpAuthorize(AppPermissions.Pages_NsPayBackgroundJobs_Edit)]
        public virtual async Task Restart(EntityDto<Guid> input)
        {
            var nsPayBackgroundJob = await _nsPayBackgroundJobRepository.FirstOrDefaultAsync(input.Id);
            nsPayBackgroundJob.IsRestart = true;
        }
    }
}