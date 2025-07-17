using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Neptune.NsPay.PayGroups.Dtos;
using Neptune.NsPay.Dto;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Neptune.NsPay.Storage;
using Neptune.NsPay.PayMents.Dtos;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.PayGroupMents;
using Abp.AutoMapper;
using NewLife.Caching.Clusters;
using Neptune.NsPay.Common;

namespace Neptune.NsPay.PayGroups
{
    [AbpAuthorize(AppPermissions.Pages_PayGroups)]
    public class PayGroupsAppService : NsPayAppServiceBase, IPayGroupsAppService
    {
        private readonly IRepository<PayGroup> _payGroupRepository;
        private readonly IRedisService _redisService;
        private readonly ICommonPayGroupMentRedisAppService _commonPayGroupMentRedisAppService;
        private readonly IPayGroupMentsAppService _payGroupMentsAppService;

        public PayGroupsAppService(IRepository<PayGroup> payGroupRepository,
            ICommonPayGroupMentRedisAppService commonPayGroupMentRedisAppService,
            IRedisService redisService,
            IPayGroupMentsAppService payGroupMentsAppService)
        {
            _payGroupRepository = payGroupRepository;
            _commonPayGroupMentRedisAppService = commonPayGroupMentRedisAppService;
            _redisService = redisService;
            _payGroupMentsAppService = payGroupMentsAppService;
        }

        public virtual async Task<PagedResultDto<GetPayGroupForViewDto>> GetAll(GetAllPayGroupsInput input)
        {

            var filteredPayGroups = _payGroupRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.GroupName.Contains(input.Filter) || e.BankApi.Contains(input.Filter) || e.VietcomApi.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.GroupNameFilter), e => e.GroupName.Contains(input.GroupNameFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.BankApiFilter), e => e.BankApi.Contains(input.BankApiFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.VietcomApiFilter), e => e.VietcomApi.Contains(input.VietcomApiFilter))
                        .WhereIf(input.StatusFilter.HasValue && input.StatusFilter > -1, e => (input.StatusFilter == 1 && e.Status) || (input.StatusFilter == 0 && !e.Status));

            var pagedAndFilteredPayGroups = filteredPayGroups
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var payGroups = from o in pagedAndFilteredPayGroups
                            select new
                            {

                                o.GroupName,
                                o.BankApi,
                                o.VietcomApi,
                                o.Status,
                                Id = o.Id
                            };

            var totalCount = await filteredPayGroups.CountAsync();

            var dbList = await payGroups.ToListAsync();
            var results = new List<GetPayGroupForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetPayGroupForViewDto()
                {
                    PayGroup = new PayGroupDto
                    {

                        GroupName = o.GroupName,
                        BankApi = o.BankApi,
                        VietcomApi = o.VietcomApi,
                        Status = o.Status,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetPayGroupForViewDto>(
                totalCount,
                results
            );

        }

		public virtual async Task<GetPayGroupForViewDto> GetPayGroupForView(int id)
        {
            var payGroup = await _payGroupRepository.GetAsync(id);

            var output = new GetPayGroupForViewDto { PayGroup = ObjectMapper.Map<PayGroupDto>(payGroup) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_PayGroups_Edit)]
        public virtual async Task<GetPayGroupForEditOutput> GetPayGroupForEdit(EntityDto input)
        {
            var payGroup = await _payGroupRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetPayGroupForEditOutput { PayGroup = ObjectMapper.Map<CreateOrEditPayGroupDto>(payGroup) };

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditPayGroupDto input)
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

        [AbpAuthorize(AppPermissions.Pages_PayGroups_Create)]
        protected virtual async Task Create(CreateOrEditPayGroupDto input)
        {
            var payGroup = ObjectMapper.Map<PayGroup>(input);
            var paygroupId = await _payGroupRepository.InsertAndGetIdAsync(payGroup);

            await CurrentUnitOfWork.SaveChangesAsync();
            //添加缓存
            await _commonPayGroupMentRedisAppService.AddPayGroupMentRedisValue(paygroupId);
        }

        [AbpAuthorize(AppPermissions.Pages_PayGroups_Edit)]
        protected virtual async Task Update(CreateOrEditPayGroupDto input)
        {
            var payGroup = await _payGroupRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, payGroup);
            await CurrentUnitOfWork.SaveChangesAsync();
            //更新缓存
            await _commonPayGroupMentRedisAppService.AddPayGroupMentRedisValue((int)input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_PayGroups_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _payGroupRepository.DeleteAsync(input.Id);
            _redisService.DeletePayGroupMentRedisValue(input.Id.ToString());
            await _payGroupMentsAppService.DeleteByPayGroup(input.Id);
        }

    }
}