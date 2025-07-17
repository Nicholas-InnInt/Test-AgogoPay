using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Neptune.NsPay.MerchantWithdrawBanks.Dtos;
using Neptune.NsPay.Dto;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Neptune.NsPay.Storage;
using Abp.EntityFrameworkCore;
using Neptune.NsPay.EntityFrameworkCore;
using Neptune.NsPay.Authorization.Users;

namespace Neptune.NsPay.MerchantWithdrawBanks
{
    [AbpAuthorize(AppPermissions.Pages_MerchantWithdrawBanks)]
    public class MerchantWithdrawBanksAppService : NsPayAppServiceBase, IMerchantWithdrawBanksAppService
    {
        private readonly IDbContextProvider<NsPayDbContext> _dbContextProvider;
        private readonly IRepository<MerchantWithdrawBank> _merchantWithdrawBankRepository;

        public MerchantWithdrawBanksAppService(IDbContextProvider<NsPayDbContext> dbContextProvider, 
            IRepository<MerchantWithdrawBank> merchantWithdrawBankRepository)
        {
            _dbContextProvider = dbContextProvider;
            _merchantWithdrawBankRepository = merchantWithdrawBankRepository;

        }

        public virtual async Task<PagedResultDto<GetMerchantWithdrawBankForViewDto>> GetAll(GetAllMerchantWithdrawBanksInput input)
        {
            var user = await GetCurrentUserAsync();
            var merchantCode = user.Merchants.Select(r => r.MerchantCode);
            var context = await _dbContextProvider.GetDbContextAsync();
            var filteredMerchantWithdrawBanks = context.MerchantWithdrawBanks.Where(r => context.Merchants.Any(e => e.MerchantCode == r.MerchantCode && merchantCode.Contains(r.MerchantCode)))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.BankName.Contains(input.Filter) || e.ReceivCard.Contains(input.Filter) || e.ReceivName.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.BankNameFilter), e => e.BankName.Contains(input.BankNameFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ReceivCardFilter), e => e.ReceivCard.Contains(input.ReceivCardFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ReceivNameFilter), e => e.ReceivName.Contains(input.ReceivNameFilter));

            var pagedAndFilteredMerchantWithdrawBanks = filteredMerchantWithdrawBanks
                .OrderBy(input.Sorting ?? "id desc")
                .PageBy(input);

            var merchantWithdrawBanks = from o in pagedAndFilteredMerchantWithdrawBanks
                                        select new
                                        {

                                            o.MerchantCode,
                                            o.BankName,
                                            o.ReceivCard,
                                            o.ReceivName,
                                            o.Status,
                                            Id = o.Id
                                        };

            var totalCount = await filteredMerchantWithdrawBanks.CountAsync();

            var dbList = await merchantWithdrawBanks.ToListAsync();
            var results = new List<GetMerchantWithdrawBankForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetMerchantWithdrawBankForViewDto()
                {
                    MerchantWithdrawBank = new MerchantWithdrawBankDto
                    {

                        MerchantCode = o.MerchantCode,
                        BankName = o.BankName,
                        ReceivCard = o.ReceivCard,
                        ReceivName = o.ReceivName,
                        Status = o.Status,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetMerchantWithdrawBankForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetMerchantWithdrawBankForViewDto> GetMerchantWithdrawBankForView(int id)
        {
            var merchantWithdrawBank = await _merchantWithdrawBankRepository.GetAsync(id);

            var output = new GetMerchantWithdrawBankForViewDto { MerchantWithdrawBank = ObjectMapper.Map<MerchantWithdrawBankDto>(merchantWithdrawBank) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantWithdrawBanks_Edit)]
        public virtual async Task<GetMerchantWithdrawBankForEditOutput> GetMerchantWithdrawBankForEdit(EntityDto input)
        {
            var merchantWithdrawBank = await _merchantWithdrawBankRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetMerchantWithdrawBankForEditOutput { MerchantWithdrawBank = ObjectMapper.Map<CreateOrEditMerchantWithdrawBankDto>(merchantWithdrawBank) };

            return output;
        }

        public virtual async Task<bool> CreateOrEdit(CreateOrEditMerchantWithdrawBankDto input)
        {
            if (input.Id == null)
            {
                return await Create(input);
            }
            else
            {
                return await Update(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantWithdrawBanks_Create)]
        protected virtual async Task<bool> Create(CreateOrEditMerchantWithdrawBankDto input)
        {
            //只有单商户可以添加
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants;
            if (user.UserType == UserTypeEnum.ExternalMerchant || user.UserType == UserTypeEnum.InternalMerchant)
            {
                if (merchants.Count == 1)
                {
                    var merchant = merchants[0];
                    input.MerchantId = merchant.Id;
                    input.MerchantCode = merchant.MerchantCode;
                    input.Status = true;
                    var merchantWithdrawBank = ObjectMapper.Map<MerchantWithdrawBank>(input);
                    await _merchantWithdrawBankRepository.InsertAsync(merchantWithdrawBank);
                    return true;
                }
            }
            return false;
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantWithdrawBanks_Edit)]
        protected virtual async Task<bool> Update(CreateOrEditMerchantWithdrawBankDto input)
        {
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants;
            if (user.UserType == UserTypeEnum.ExternalMerchant || user.UserType == UserTypeEnum.InternalMerchant)
            {
                if (merchants.Count == 1)
                {
                    var merchant = merchants[0];
                    input.MerchantId = merchant.Id;
                    input.MerchantCode = merchant.MerchantCode;
                    var merchantWithdrawBank = await _merchantWithdrawBankRepository.FirstOrDefaultAsync((int)input.Id);
                    input.Status = merchantWithdrawBank.Status;
                    ObjectMapper.Map(input, merchantWithdrawBank);
                    return true;
                }
            }
            return false;
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantWithdrawBanks_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _merchantWithdrawBankRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantWithdrawBanks_Edit)]
        public async Task Status(EntityDto input)
        {
            var merchantWithdrawBank = await _merchantWithdrawBankRepository.FirstOrDefaultAsync((int)input.Id);
            if (merchantWithdrawBank != null)
            {
                if (merchantWithdrawBank.Status)
                {
                    merchantWithdrawBank.Status = false;
                }
                else
                {
                    merchantWithdrawBank.Status = true;
                }
                await _merchantWithdrawBankRepository.UpdateAsync(merchantWithdrawBank);
            }
        }
    }
}