using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Neptune.NsPay.MerchantFunds.Dtos;
using Neptune.NsPay.Dto;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Neptune.NsPay.Storage;

namespace Neptune.NsPay.MerchantFunds
{
    [AbpAuthorize(AppPermissions.Pages_MerchantFunds)]
    public class MerchantFundsAppService : NsPayAppServiceBase, IMerchantFundsAppService
    {
        private readonly IRepository<MerchantFund> _merchantFundRepository;

        public MerchantFundsAppService(IRepository<MerchantFund> merchantFundRepository)
        {
            _merchantFundRepository = merchantFundRepository;

        }

        public virtual async Task<PagedResultDto<GetMerchantFundForViewDto>> GetAll(GetAllMerchantFundsInput input)
        {

            var filteredMerchantFunds = _merchantFundRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.MerchantCode.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.MerchantCodeFilter), e => e.MerchantCode.Contains(input.MerchantCodeFilter))
                        .WhereIf(input.MinMerchantIdFilter != null, e => e.MerchantId >= input.MinMerchantIdFilter)
                        .WhereIf(input.MaxMerchantIdFilter != null, e => e.MerchantId <= input.MaxMerchantIdFilter)
                        .WhereIf(input.MinDepositAmountFilter != null, e => e.DepositAmount >= input.MinDepositAmountFilter)
                        .WhereIf(input.MaxDepositAmountFilter != null, e => e.DepositAmount <= input.MaxDepositAmountFilter)
                        .WhereIf(input.MinWithdrawalAmountFilter != null, e => e.WithdrawalAmount >= input.MinWithdrawalAmountFilter)
                        .WhereIf(input.MaxWithdrawalAmountFilter != null, e => e.WithdrawalAmount <= input.MaxWithdrawalAmountFilter)
                        .WhereIf(input.MinRateFeeBalanceFilter != null, e => e.RateFeeBalance >= input.MinRateFeeBalanceFilter)
                        .WhereIf(input.MaxRateFeeBalanceFilter != null, e => e.RateFeeBalance <= input.MaxRateFeeBalanceFilter)
                        .WhereIf(input.MinBalanceFilter != null, e => e.Balance >= input.MinBalanceFilter)
                        .WhereIf(input.MaxBalanceFilter != null, e => e.Balance <= input.MaxBalanceFilter)
                        .WhereIf(input.MinCreationTimeFilter != null, e => e.CreationTime >= input.MinCreationTimeFilter)
                        .WhereIf(input.MaxCreationTimeFilter != null, e => e.CreationTime <= input.MaxCreationTimeFilter)
                        .WhereIf(input.MinUpdateTimeFilter != null, e => e.UpdateTime >= input.MinUpdateTimeFilter)
                        .WhereIf(input.MaxUpdateTimeFilter != null, e => e.UpdateTime <= input.MaxUpdateTimeFilter);

            var pagedAndFilteredMerchantFunds = filteredMerchantFunds
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var merchantFunds = from o in pagedAndFilteredMerchantFunds
                                select new
                                {

                                    o.MerchantCode,
                                    o.MerchantId,
                                    o.DepositAmount,
                                    o.WithdrawalAmount,
                                    o.RateFeeBalance,
                                    o.Balance,
                                    o.CreationTime,
                                    o.UpdateTime,
                                    Id = o.Id
                                };

            var totalCount = await filteredMerchantFunds.CountAsync();

            var dbList = await merchantFunds.ToListAsync();
            var results = new List<GetMerchantFundForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetMerchantFundForViewDto()
                {
                    MerchantFund = new MerchantFundDto
                    {

                        MerchantCode = o.MerchantCode,
                        MerchantId = o.MerchantId,
                        DepositAmount = o.DepositAmount,
                        WithdrawalAmount = o.WithdrawalAmount,
                        RateFeeBalance = o.RateFeeBalance,
                        Balance = o.Balance,
                        CreationTime = o.CreationTime,
                        UpdateTime = o.UpdateTime,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetMerchantFundForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetMerchantFundForViewDto> GetMerchantFundForView(int id)
        {
            var merchantFund = await _merchantFundRepository.GetAsync(id);

            var output = new GetMerchantFundForViewDto { MerchantFund = ObjectMapper.Map<MerchantFundDto>(merchantFund) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantFunds_Edit)]
        public virtual async Task<GetMerchantFundForEditOutput> GetMerchantFundForEdit(EntityDto input)
        {
            var merchantFund = await _merchantFundRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetMerchantFundForEditOutput { MerchantFund = ObjectMapper.Map<CreateOrEditMerchantFundDto>(merchantFund) };

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditMerchantFundDto input)
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

        [AbpAuthorize(AppPermissions.Pages_MerchantFunds_Create)]
        protected virtual async Task Create(CreateOrEditMerchantFundDto input)
        {
            var merchantFund = ObjectMapper.Map<MerchantFund>(input);

            await _merchantFundRepository.InsertAsync(merchantFund);

        }

        [AbpAuthorize(AppPermissions.Pages_MerchantFunds_Edit)]
        protected virtual async Task Update(CreateOrEditMerchantFundDto input)
        {
            var merchantFund = await _merchantFundRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, merchantFund);

        }

        [AbpAuthorize(AppPermissions.Pages_MerchantFunds_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _merchantFundRepository.DeleteAsync(input.Id);
        }

    }
}