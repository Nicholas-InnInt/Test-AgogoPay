using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Neptune.NsPay.NsPaySystemSettings.Dtos;
using Neptune.NsPay.Dto;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Neptune.NsPay.Storage;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.Commons;

namespace Neptune.NsPay.NsPaySystemSettings
{
    [AbpAuthorize(AppPermissions.Pages_NsPaySystemSettings)]
    public class NsPaySystemSettingsAppService : NsPayAppServiceBase, INsPaySystemSettingsAppService
    {
        private readonly IRepository<NsPaySystemSetting> _nsPaySystemSettingRepository;
        private readonly IRedisService _redisService;

        public NsPaySystemSettingsAppService(IRepository<NsPaySystemSetting> nsPaySystemSettingRepository,
            IRedisService redisService)
        {
            _nsPaySystemSettingRepository = nsPaySystemSettingRepository;
            _redisService = redisService;
        }

        public virtual async Task<PagedResultDto<GetNsPaySystemSettingForViewDto>> GetAll(GetAllNsPaySystemSettingsInput input)
        {

            var filteredNsPaySystemSettings = _nsPaySystemSettingRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Key.Contains(input.Filter) || e.Value.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.KeyFilter), e => e.Key.Contains(input.KeyFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ValueFilter), e => e.Value.Contains(input.ValueFilter));

            var pagedAndFilteredNsPaySystemSettings = filteredNsPaySystemSettings
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var nsPaySystemSettings = from o in pagedAndFilteredNsPaySystemSettings
                                      select new
                                      {

                                          o.Key,
                                          o.Value,
                                          Id = o.Id
                                      };

            var totalCount = await filteredNsPaySystemSettings.CountAsync();

            var dbList = await nsPaySystemSettings.ToListAsync();
            var results = new List<GetNsPaySystemSettingForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetNsPaySystemSettingForViewDto()
                {
                    NsPaySystemSetting = new NsPaySystemSettingDto
                    {

                        Key = o.Key,
                        Value = o.Value,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetNsPaySystemSettingForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetNsPaySystemSettingForViewDto> GetNsPaySystemSettingForView(int id)
        {
            var nsPaySystemSetting = await _nsPaySystemSettingRepository.GetAsync(id);

            var output = new GetNsPaySystemSettingForViewDto { NsPaySystemSetting = ObjectMapper.Map<NsPaySystemSettingDto>(nsPaySystemSetting) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_NsPaySystemSettings_Edit)]
        public virtual async Task<GetNsPaySystemSettingForEditOutput> GetNsPaySystemSettingForEdit(EntityDto input)
        {
            var nsPaySystemSetting = await _nsPaySystemSettingRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetNsPaySystemSettingForEditOutput { NsPaySystemSetting = ObjectMapper.Map<CreateOrEditNsPaySystemSettingDto>(nsPaySystemSetting) };

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditNsPaySystemSettingDto input)
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

        [AbpAuthorize(AppPermissions.Pages_NsPaySystemSettings_Create)]
        protected virtual async Task Create(CreateOrEditNsPaySystemSettingDto input)
        {
            var nsPaySystemSetting = ObjectMapper.Map<NsPaySystemSetting>(input);

            await _nsPaySystemSettingRepository.InsertAsync(nsPaySystemSetting);

            //添加缓存
            _redisService.AddRedisValue(NsPayRedisKeyConst.NsPaySystemKey + input.Key, input.Value);
        }

        [AbpAuthorize(AppPermissions.Pages_NsPaySystemSettings_Edit)]
        protected virtual async Task Update(CreateOrEditNsPaySystemSettingDto input)
        {
            var nsPaySystemSetting = await _nsPaySystemSettingRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, nsPaySystemSetting);

            //更新缓存
            _redisService.AddRedisValue(NsPayRedisKeyConst.NsPaySystemKey + input.Key, input.Value);
        }

        [AbpAuthorize(AppPermissions.Pages_NsPaySystemSettings_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            var nsPaySystemSetting = await _nsPaySystemSettingRepository.FirstOrDefaultAsync((int)input.Id);
            await _nsPaySystemSettingRepository.DeleteAsync(input.Id);

            //更新缓存
            _redisService.RemoveRedisValue(NsPayRedisKeyConst.NsPaySystemKey + nsPaySystemSetting.Key);
        }

    }
}