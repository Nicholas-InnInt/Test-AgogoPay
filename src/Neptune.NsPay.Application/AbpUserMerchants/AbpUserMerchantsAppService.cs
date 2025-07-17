using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Neptune.NsPay.AbpUserMerchants.Dtos;
using Neptune.NsPay.Dto;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Neptune.NsPay.Storage;

namespace Neptune.NsPay.AbpUserMerchants
{
    [AbpAuthorize(AppPermissions.Pages_Administration_Users)]
    public class AbpUserMerchantsAppService : NsPayAppServiceBase, IAbpUserMerchantsAppService
    {
        private readonly IRepository<AbpUserMerchant> _abpUserMerchantRepository;

        public AbpUserMerchantsAppService(IRepository<AbpUserMerchant> abpUserMerchantRepository)
        {
            _abpUserMerchantRepository = abpUserMerchantRepository;

        }

        public virtual async Task<PagedResultDto<GetAbpUserMerchantForViewDto>> GetAll(GetAllAbpUserMerchantsInput input)
        {

            var filteredAbpUserMerchants = _abpUserMerchantRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false);

            var pagedAndFilteredAbpUserMerchants = filteredAbpUserMerchants
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var abpUserMerchants = from o in pagedAndFilteredAbpUserMerchants
                                   select new
                                   {

                                       Id = o.Id
                                   };

            var totalCount = await filteredAbpUserMerchants.CountAsync();

            var dbList = await abpUserMerchants.ToListAsync();
            var results = new List<GetAbpUserMerchantForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetAbpUserMerchantForViewDto()
                {
                    AbpUserMerchant = new AbpUserMerchantDto
                    {

                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetAbpUserMerchantForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetAbpUserMerchantForViewDto> GetAbpUserMerchantForView(int id)
        {
            var abpUserMerchant = await _abpUserMerchantRepository.GetAsync(id);

            var output = new GetAbpUserMerchantForViewDto { AbpUserMerchant = ObjectMapper.Map<AbpUserMerchantDto>(abpUserMerchant) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Edit)]
        public virtual async Task<GetAbpUserMerchantForEditOutput> GetAbpUserMerchantForEdit(EntityDto input)
        {
            var abpUserMerchant = await _abpUserMerchantRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetAbpUserMerchantForEditOutput { AbpUserMerchant = ObjectMapper.Map<CreateOrEditAbpUserMerchantDto>(abpUserMerchant) };

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditAbpUserMerchantDto input)
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

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Create)]
        protected virtual async Task Create(CreateOrEditAbpUserMerchantDto input)
        {
            var abpUserMerchant = ObjectMapper.Map<AbpUserMerchant>(input);

            await _abpUserMerchantRepository.InsertAsync(abpUserMerchant);

        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Edit)]
        protected virtual async Task Update(CreateOrEditAbpUserMerchantDto input)
        {
            var abpUserMerchant = await _abpUserMerchantRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, abpUserMerchant);

        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _abpUserMerchantRepository.DeleteAsync(input.Id);
        }

    }
}