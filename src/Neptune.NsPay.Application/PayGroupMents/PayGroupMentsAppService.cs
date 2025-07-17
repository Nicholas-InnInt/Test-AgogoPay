using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Neptune.NsPay.PayGroupMents.Dtos;
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
using Neptune.NsPay.PayGroups;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.Common;
using Abp.Collections.Extensions;
using System.Text.RegularExpressions;

namespace Neptune.NsPay.PayGroupMents
{
    [AbpAuthorize(AppPermissions.Pages_PayGroups)]
    public class PayGroupMentsAppService : NsPayAppServiceBase, IPayGroupMentsAppService
    {
        private readonly IRepository<PayGroupMent> _payGroupMentRepository;
        private readonly IRepository<PayGroup> _payGroupRepository;
        private readonly IRepository<PayMent> _payMentRepository;
        private readonly ICommonPayGroupMentRedisAppService _commonPayGroupMentRedisAppService;
        private readonly IRedisService _redisService;

        public PayGroupMentsAppService(IRepository<PayGroupMent> payGroupMentRepository,
            IRepository<PayGroup> payGroupRepository,
            IRepository<PayMent> payMentRepository,
            ICommonPayGroupMentRedisAppService commonPayGroupMentRedisAppService,
            IRedisService redisService)
        {
            _payGroupMentRepository = payGroupMentRepository;
            _payGroupRepository = payGroupRepository;
            _payMentRepository = payMentRepository;
            _commonPayGroupMentRedisAppService = commonPayGroupMentRedisAppService;
            _redisService = redisService;
        }

        public virtual async Task<PagedResultDto<GetPayGroupMentForViewDto>> GetAll(GetAllPayGroupMentsInput input)
        {
     
                var payments = _payMentRepository.GetAll().WhereIf(!string.IsNullOrEmpty(input.Filter), e => e.Name.Contains(input.Filter));

                var filteredPayGroupMents = _payGroupMentRepository.GetAll()
                            .Where(r => r.IsDeleted == false)
                            .WhereIf(input.PayGroupsIdFilter > 0, e => e.GroupId == input.PayGroupsIdFilter)
                .Join(payments, t1 => t1.PayMentId, t2 => t2.Id, (t1, t2) => new { Id = t1.Id, PaymentName = t2.Name, PayMentId = t2.Id, Status = t1.Status, GroupId = t1.GroupId, Type = t2.Type });//new { PG=t1 , PM = new { Name = t2.Name , PType =  t2.Type , PID = t2.Id  } } ).Where(x=>x.PM.PID >0);



                var pagedAndFilteredPayGroupMents = filteredPayGroupMents
                    .OrderBy(input.Sorting ?? " id desc")
                    .PageBy(input);

                var payGroupMents = from o in pagedAndFilteredPayGroupMents
                                    select new
                                    {
                                        o.GroupId,
                                        o.PayMentId,
                                        o.Status,
                                        Id = o.Id,
                                        o.PaymentName,
                                        o.Type

                                    };

                var totalCount = await filteredPayGroupMents.CountAsync();

                var dbList = await payGroupMents.ToListAsync();
                var results = new List<GetPayGroupMentForViewDto>();


                foreach (var o in dbList)
                {
                    var res = new GetPayGroupMentForViewDto()
                    {
                        PayGroupMent = new PayGroupMentDto
                        {
                            GroupId = o.GroupId,
                            PayMentName = o.PaymentName,
                            PayMentId = o.PayMentId,
                            PayMentType = o.Type,
                            Status = o.Status,
                            Id = o.Id,
                        }
                    };

                    results.Add(res);
                }

                return new PagedResultDto<GetPayGroupMentForViewDto>(
                    totalCount,
                    results
                );
        }

        public virtual async Task<GetPayGroupMentForViewDto> GetPayGroupMentForView(int id)
        {
            var payGroupMent = await _payGroupMentRepository.GetAsync(id);

            var output = new GetPayGroupMentForViewDto { PayGroupMent = ObjectMapper.Map<PayGroupMentDto>(payGroupMent) };

            return output;
        }
		public async Task<List<CreateOrEditPayMentDto>> GetAllPayMentsByCreate()
		{
			var payments = await _payMentRepository.GetAllListAsync();

			var list = ObjectMapper.Map<List<CreateOrEditPayMentDto>>(payments);
			return list;
		}

		[AbpAuthorize(AppPermissions.Pages_PayGroups_Edit)]
        public virtual async Task<GetPayGroupMentForEditOutput> GetPayGroupMentForEdit(int payGroupId)
        {
            var payGroupMent = await _payGroupMentRepository.GetAll().Where(x=>x.GroupId == payGroupId).ToListAsync();

            var output = new GetPayGroupMentForEditOutput { PayGroupMent = new CreateOrEditPayGroupMentDto() { GroupId = payGroupId , PayMentIds = payGroupMent.Select(x=>x.PayMentId).ToList() } };
            output.PayGroupMent.GroupName = (await _payGroupRepository.FirstOrDefaultAsync(r => r.Id == payGroupId))?.GroupName;
            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditPayGroupMentDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PayGroups_Create)]
        protected virtual async Task Create(CreateOrEditPayGroupMentDto input)
        {
            if (input.PayMentIds != null)
            {
                if (input.PayMentIds.Count > 0)
                {
                    foreach (var item in input.PayMentIds)
                    {
                        var info = new CreateOrEditPayGroupMentDto()
                        {
                            Id = input.Id,
                            GroupId = input.GroupId,
                            PayMentId = item,
                            Status = input.Status,
                        };
                        var payGroupMent = ObjectMapper.Map<PayGroupMent>(info);
                        //判断是否存在
                        var checkInfo = await _payGroupMentRepository.GetAllListAsync(r => r.GroupId == input.GroupId && r.PayMentId == item && r.IsDeleted == false);
                        if (checkInfo.Count <= 0)
                        {
                            await _payGroupMentRepository.InsertAsync(payGroupMent);
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }
                    //更新缓存
                    await _commonPayGroupMentRedisAppService.AddPayGroupMentRedisValue(input.GroupId);
                }
            }

        }
        [AbpAuthorize(AppPermissions.Pages_PayGroups_Edit)]
        public virtual async Task Enabled(CreateOrEditPayGroupMentDto input)
        {
            var payGroupMent = await _payGroupMentRepository.FirstOrDefaultAsync((int)input.Id);
            if (payGroupMent.Status)
            {
                payGroupMent.Status = false;
            }
            else
            {
                payGroupMent.Status = true;
            }
            await _payGroupMentRepository.UpdateAsync(payGroupMent);
            await CurrentUnitOfWork.SaveChangesAsync();
            //更新缓存
            await _commonPayGroupMentRedisAppService.AddPayGroupMentRedisValue(payGroupMent.GroupId);
        }

        [AbpAuthorize(AppPermissions.Pages_PayGroups_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            var paygroupment = await _payGroupMentRepository.FirstOrDefaultAsync(r => r.Id == (int)input.Id);
            await _payGroupMentRepository.DeleteAsync(input.Id);
            await CurrentUnitOfWork.SaveChangesAsync();
            //更新缓存
            await _commonPayGroupMentRedisAppService.AddPayGroupMentRedisValue(paygroupment.GroupId);
        }


        [AbpAuthorize(AppPermissions.Pages_PayGroups_Delete)]
        public virtual async Task DeleteByPayGroup(int payGroupId)
        {
            var payGroupMent = await _payGroupMentRepository.GetAll().Where(x => x.GroupId == payGroupId).ToListAsync();

            foreach (var singleGroupMent in payGroupMent)
            {
                await _payGroupMentRepository.DeleteAsync(singleGroupMent.Id);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            //更新缓存
            await _commonPayGroupMentRedisAppService.AddPayGroupMentRedisValue(payGroupId);
        }

    }
}