using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Neptune.NsPay.MerchantSettings.Dtos;
using Neptune.NsPay.Dto;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Neptune.NsPay.Storage;

namespace Neptune.NsPay.MerchantSettings
{
    [AbpAuthorize(AppPermissions.Pages_MerchantSettings)]
    public class MerchantSettingsAppService : NsPayAppServiceBase, IMerchantSettingsAppService
    {
        private readonly IRepository<MerchantSetting> _merchantSettingRepository;

        public MerchantSettingsAppService(IRepository<MerchantSetting> merchantSettingRepository)
        {
            _merchantSettingRepository = merchantSettingRepository;

        }

        public virtual async Task<PagedResultDto<GetMerchantSettingForViewDto>> GetAll(GetAllMerchantSettingsInput input)
        {

            var filteredMerchantSettings = _merchantSettingRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.MerchantCode.Contains(input.Filter) || e.NsPayTitle.Contains(input.Filter) || e.LogoUrl.Contains(input.Filter) || e.LoginIpAddress.Contains(input.Filter) || e.BankNotify.Contains(input.Filter) || e.BankNotifyText.Contains(input.Filter) || e.TelegramNotifyBotId.Contains(input.Filter) || e.TelegramNotifyChatId.Contains(input.Filter) || e.PlatformUrl.Contains(input.Filter) || e.PlatformUserName.Contains(input.Filter) || e.PlatformPassWord.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.MerchantCodeFilter), e => e.MerchantCode.Contains(input.MerchantCodeFilter))
                        .WhereIf(input.MinMerchantIdFilter != null, e => e.MerchantId >= input.MinMerchantIdFilter)
                        .WhereIf(input.MaxMerchantIdFilter != null, e => e.MerchantId <= input.MaxMerchantIdFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NsPayTitleFilter), e => e.NsPayTitle.Contains(input.NsPayTitleFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.LogoUrlFilter), e => e.LogoUrl.Contains(input.LogoUrlFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.LoginIpAddressFilter), e => e.LoginIpAddress.Contains(input.LoginIpAddressFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.BankNotifyFilter), e => e.BankNotify.Contains(input.BankNotifyFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.BankNotifyTextFilter), e => e.BankNotifyText.Contains(input.BankNotifyTextFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.TelegramNotifyBotIdFilter), e => e.TelegramNotifyBotId.Contains(input.TelegramNotifyBotIdFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.TelegramNotifyChatIdFilter), e => e.TelegramNotifyChatId.Contains(input.TelegramNotifyChatIdFilter))
                        .WhereIf(input.OpenRiskWithdrawalFilter.HasValue && input.OpenRiskWithdrawalFilter > -1, e => (input.OpenRiskWithdrawalFilter == 1 && e.OpenRiskWithdrawal) || (input.OpenRiskWithdrawalFilter == 0 && !e.OpenRiskWithdrawal))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PlatformUrlFilter), e => e.PlatformUrl.Contains(input.PlatformUrlFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PlatformUserNameFilter), e => e.PlatformUserName.Contains(input.PlatformUserNameFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PlatformPassWordFilter), e => e.PlatformPassWord.Contains(input.PlatformPassWordFilter))
                        .WhereIf(input.MinPlatformLimitMoneyFilter != null, e => e.PlatformLimitMoney >= input.MinPlatformLimitMoneyFilter)
                        .WhereIf(input.MaxPlatformLimitMoneyFilter != null, e => e.PlatformLimitMoney <= input.MaxPlatformLimitMoneyFilter);

            var pagedAndFilteredMerchantSettings = filteredMerchantSettings
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var merchantSettings = from o in pagedAndFilteredMerchantSettings
                                   select new
                                   {

                                       o.MerchantCode,
                                       o.MerchantId,
                                       o.NsPayTitle,
                                       o.LogoUrl,
                                       o.LoginIpAddress,
                                       o.BankNotify,
                                       o.BankNotifyText,
                                       o.TelegramNotifyBotId,
                                       o.TelegramNotifyChatId,
                                       o.OpenRiskWithdrawal,
                                       o.PlatformUrl,
                                       o.PlatformUserName,
                                       o.PlatformPassWord,
                                       o.PlatformLimitMoney,
                                       Id = o.Id
                                   };

            var totalCount = await filteredMerchantSettings.CountAsync();

            var dbList = await merchantSettings.ToListAsync();
            var results = new List<GetMerchantSettingForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetMerchantSettingForViewDto()
                {
                    MerchantSetting = new MerchantSettingDto
                    {

                        MerchantCode = o.MerchantCode,
                        MerchantId = o.MerchantId,
                        NsPayTitle = o.NsPayTitle,
                        LogoUrl = o.LogoUrl,
                        LoginIpAddress = o.LoginIpAddress,
                        BankNotify = o.BankNotify,
                        BankNotifyText = o.BankNotifyText,
                        TelegramNotifyBotId = o.TelegramNotifyBotId,
                        TelegramNotifyChatId = o.TelegramNotifyChatId,
                        OpenRiskWithdrawal = o.OpenRiskWithdrawal,
                        PlatformUrl = o.PlatformUrl,
                        PlatformUserName = o.PlatformUserName,
                        PlatformPassWord = o.PlatformPassWord,
                        PlatformLimitMoney = o.PlatformLimitMoney,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetMerchantSettingForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetMerchantSettingForViewDto> GetMerchantSettingForView(int id)
        {
            var merchantSetting = await _merchantSettingRepository.GetAsync(id);

            var output = new GetMerchantSettingForViewDto { MerchantSetting = ObjectMapper.Map<MerchantSettingDto>(merchantSetting) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantSettings_Edit)]
        public virtual async Task<GetMerchantSettingForEditOutput> GetMerchantSettingForEdit(EntityDto input)
        {
            var merchantSetting = await _merchantSettingRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetMerchantSettingForEditOutput { MerchantSetting = ObjectMapper.Map<CreateOrEditMerchantSettingDto>(merchantSetting) };

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditMerchantSettingDto input)
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

        [AbpAuthorize(AppPermissions.Pages_MerchantSettings_Create)]
        protected virtual async Task Create(CreateOrEditMerchantSettingDto input)
        {
            var merchantSetting = ObjectMapper.Map<MerchantSetting>(input);

            await _merchantSettingRepository.InsertAsync(merchantSetting);

        }

        [AbpAuthorize(AppPermissions.Pages_MerchantSettings_Edit)]
        protected virtual async Task Update(CreateOrEditMerchantSettingDto input)
        {
            var merchantSetting = await _merchantSettingRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, merchantSetting);

        }

        [AbpAuthorize(AppPermissions.Pages_MerchantSettings_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _merchantSettingRepository.DeleteAsync(input.Id);
        }

    }
}