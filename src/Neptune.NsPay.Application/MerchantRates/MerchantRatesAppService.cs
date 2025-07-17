using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Neptune.NsPay.MerchantRates.Dtos;
using Neptune.NsPay.Dto;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Neptune.NsPay.Storage;

namespace Neptune.NsPay.MerchantRates
{
    [AbpAuthorize(AppPermissions.Pages_Merchants)]
    public class MerchantRatesAppService : NsPayAppServiceBase, IMerchantRatesAppService
    {
        private readonly IRepository<MerchantRate> _merchantRateRepository;

        public MerchantRatesAppService(IRepository<MerchantRate> merchantRateRepository)
        {
            _merchantRateRepository = merchantRateRepository;

        }

        public virtual async Task<PagedResultDto<GetMerchantRateForViewDto>> GetAll(GetAllMerchantRatesInput input)
        {

            var filteredMerchantRates = _merchantRateRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.MerchantCode.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.MerchantCodeFilter), e => e.MerchantCode.Contains(input.MerchantCodeFilter))
                        .WhereIf(input.MinMerchantIdFilter != null, e => e.MerchantId >= input.MinMerchantIdFilter)
                        .WhereIf(input.MaxMerchantIdFilter != null, e => e.MerchantId <= input.MaxMerchantIdFilter)
                        .WhereIf(input.MinScanBankRateFilter != null, e => e.ScanBankRate >= input.MinScanBankRateFilter)
                        .WhereIf(input.MaxScanBankRateFilter != null, e => e.ScanBankRate <= input.MaxScanBankRateFilter)
                        .WhereIf(input.MinScratchCardRateFilter != null, e => e.ScratchCardRate >= input.MinScratchCardRateFilter)
                        .WhereIf(input.MaxScratchCardRateFilter != null, e => e.ScratchCardRate <= input.MaxScratchCardRateFilter)
                        .WhereIf(input.MinMoMoRateFilter != null, e => e.MoMoRate >= input.MinMoMoRateFilter)
                        .WhereIf(input.MaxMoMoRateFilter != null, e => e.MoMoRate <= input.MaxMoMoRateFilter);

            var pagedAndFilteredMerchantRates = filteredMerchantRates
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var merchantRates = from o in pagedAndFilteredMerchantRates
                                select new
                                {

                                    o.MerchantCode,
                                    o.MerchantId,
                                    o.ScanBankRate,
                                    o.ScratchCardRate,
                                    o.MoMoRate,
                                    Id = o.Id
                                };

            var totalCount = await filteredMerchantRates.CountAsync();

            var dbList = await merchantRates.ToListAsync();
            var results = new List<GetMerchantRateForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetMerchantRateForViewDto()
                {
                    MerchantRate = new MerchantRateDto
                    {

                        MerchantCode = o.MerchantCode,
                        MerchantId = o.MerchantId,
                        ScanBankRate = o.ScanBankRate,
                        ScratchCardRate = o.ScratchCardRate,
                        MoMoRate = o.MoMoRate,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetMerchantRateForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetMerchantRateForViewDto> GetMerchantRateForView(int id)
        {
            var merchantRate = await _merchantRateRepository.GetAsync(id);

            var output = new GetMerchantRateForViewDto { MerchantRate = ObjectMapper.Map<MerchantRateDto>(merchantRate) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_Merchants_Edit)]
        public virtual async Task<GetMerchantRateForEditOutput> GetMerchantRateForEdit(EntityDto input)
        {
            var merchantRate = await _merchantRateRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetMerchantRateForEditOutput { MerchantRate = ObjectMapper.Map<CreateOrEditMerchantRateDto>(merchantRate) };

            return output;
        }

		[AbpAuthorize(AppPermissions.Pages_Merchants_Edit)]
		public virtual async Task<GetMerchantRateForEditOutput> GetMerchantRateByMerchantId(int merchantId)
		{
            var merchantRate = await _merchantRateRepository.FirstOrDefaultAsync(r => r.MerchantId == merchantId);

			var output = new GetMerchantRateForEditOutput { MerchantRate = ObjectMapper.Map<CreateOrEditMerchantRateDto>(merchantRate) };

			return output;
		}

		public virtual async Task CreateOrEdit(CreateOrEditMerchantRateDto input)
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

        [AbpAuthorize(AppPermissions.Pages_Merchants_Create)]
        protected virtual async Task Create(CreateOrEditMerchantRateDto input)
        {
            var merchantRate = ObjectMapper.Map<MerchantRate>(input);

            await _merchantRateRepository.InsertAsync(merchantRate);

        }

        [AbpAuthorize(AppPermissions.Pages_Merchants_Edit)]
        protected virtual async Task Update(CreateOrEditMerchantRateDto input)
        {
            var merchantRate = await _merchantRateRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, merchantRate);

        }

        [AbpAuthorize(AppPermissions.Pages_Merchants_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _merchantRateRepository.DeleteAsync(input.Id);
        }

    }
}