using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.Localization;
using Neptune.NsPay.Merchants.Dtos;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.WithdrawalDevices.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Neptune.NsPay.WithdrawalDevices
{
    [AbpAuthorize(AppPermissions.Pages_WithdrawalDevices)]
    public class WithdrawalDevicesAppService : NsPayAppServiceBase, IWithdrawalDevicesAppService
    {
        private readonly IRepository<WithdrawalDevice> _withdrawalDeviceRepository;
        private readonly IRedisService _redisService;
        private readonly IConfigurationRoot _appConfiguration;

        public WithdrawalDevicesAppService(IRepository<WithdrawalDevice> withdrawalDeviceRepository,
            IAppConfigurationAccessor appConfigurationAccessor,
            IRedisService redisService)
        {
            _withdrawalDeviceRepository = withdrawalDeviceRepository;
            _redisService = redisService;
            _appConfiguration = appConfigurationAccessor.Configuration;
        }

        public virtual async Task<PagedResultDto<GetWithdrawalDeviceForViewDto>> GetAll(GetAllWithdrawalDevicesInput input)
        {
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants;
            var merchantCode = user.Merchants.Select(r => r.MerchantCode);
            var bankTypeFilter = input.BankTypeFilter.HasValue
                        ? (WithdrawalDevicesBankTypeEnum)input.BankTypeFilter
                        : default;
            var withdrawProcessFilter = input.WithdrawProcessFilter.HasValue
                        ? (WithdrawalDevicesProcessTypeEnum)input.WithdrawProcessFilter
                        : default;

            var filteredWithdrawalDevices = _withdrawalDeviceRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.Phone.Contains(input.Filter) || e.CardName.Contains(input.Filter))
                        //.WhereIf(!string.IsNullOrWhiteSpace(input.MerchantCodeFilter), e => e.MerchantCode.Contains(input.MerchantCodeFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.Contains(input.NameFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PhoneFilter), e => e.Phone.Contains(input.PhoneFilter))
                        .WhereIf(input.BankTypeFilter.HasValue && input.BankTypeFilter > -1, e => e.BankType == bankTypeFilter)
                        .WhereIf(input.WithdrawProcessFilter.HasValue && input.WithdrawProcessFilter > -1, e => e.Process == withdrawProcessFilter);

            if (merchants.Count == 1)
            {
                filteredWithdrawalDevices = filteredWithdrawalDevices.Where(r => merchantCode.Contains(r.MerchantCode));
            }
            else
            {
                filteredWithdrawalDevices = filteredWithdrawalDevices.Where(r => r.MerchantCode.Equals(NsPayRedisKeyConst.NsPay));
            }

            //var pagedAndFilteredWithdrawalDevices = filteredWithdrawalDevices
            //    .OrderBy(input.Sorting ?? "id desc")
            //    .PageBy(input);

            var filteredResult = filteredWithdrawalDevices.ToList();

            var withdrawalDevices = from o in filteredResult
                                    select new
                                    {
                                        o.Name,
                                        o.Phone,
                                        o.BankType,
                                        o.CardName,
                                        o.Status,
                                        o.Process,
                                        o.MerchantCode,
                                        Id = o.Id,
                                        o.MaxMoney,
                                        o.MinMoney,
                                    };

            var totalCount = filteredResult.Count();

            //var dbList = await withdrawalDevices.ToListAsync();
            var results = new List<GetWithdrawalDeviceForViewDto>();
            CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);
            foreach (var o in filteredResult)
            {
                var balanceInfo = _redisService.GetWitdrawDeviceBalance(o.Id);
                var res = new GetWithdrawalDeviceForViewDto()
                {
                    WithdrawalDevice = new WithdrawalDeviceDto
                    {
                        Name = o.Name,
                        Phone = o.Phone,
                        BankType = o.BankType,
                        CardName = o.CardName,
                        Status = o.Status,
                        Process = o.Process,
                        MerchantCode = o.MerchantCode,
                        Id = o.Id,
                        Balance = balanceInfo == null ? "0" : balanceInfo.Balance.ToString("C0", culInfo),
                        MinMoney = o.MinMoney,
                        MaxMoney = o.MaxMoney,
                    },
                    MerchantName = merchants.FirstOrDefault(r => r.MerchantCode == o.MerchantCode)?.Name
                };

                results.Add(res);
            }

            results = results
                .OrderBy(x => x.WithdrawalDevice.Process)
                .ThenBy(x => x.WithdrawalDevice.Balance)
                .Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

            return new PagedResultDto<GetWithdrawalDeviceForViewDto>(
                totalCount,
                results
            );
        }

        public virtual async Task<GetWithdrawalDeviceForViewDto> GetWithdrawalDeviceForView(int id)
        {
            var withdrawalDevice = await _withdrawalDeviceRepository.GetAsync(id);

            var output = new GetWithdrawalDeviceForViewDto { WithdrawalDevice = ObjectMapper.Map<WithdrawalDeviceDto>(withdrawalDevice) };

            return output;
        }

        public virtual async Task<Dictionary<WithdrawalDevicesBankTypeEnum, List<WithdrawalDeviceDto>>> GetWithdrawalDeviceActiveBankList()
        {
            var withdrawalDevices = await _withdrawalDeviceRepository.GetAllListAsync(x => x.Process == WithdrawalDevicesProcessTypeEnum.Process);

            var withdrawalDevicesDto = ObjectMapper.Map<List<WithdrawalDeviceDto>>(withdrawalDevices);

            var result = new Dictionary<WithdrawalDevicesBankTypeEnum, List<WithdrawalDeviceDto>>();
            foreach (var device in withdrawalDevicesDto)
            {
                if (!result.ContainsKey(device.BankType))
                {
                    result[device.BankType] = new List<WithdrawalDeviceDto>();
                }
                result[device.BankType].Add(device);
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_WithdrawalDevices_Edit)]
        public virtual async Task<GetWithdrawalDeviceForEditOutput> GetWithdrawalDeviceForEdit(EntityDto input)
        {
            var withdrawalDevice = await _withdrawalDeviceRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetWithdrawalDeviceForEditOutput { WithdrawalDevice = ObjectMapper.Map<CreateOrEditWithdrawalDeviceDto>(withdrawalDevice) };

            return output;
        }

        public async Task<List<MerchantDto>> GetMerchants()
        {
            //获取当前的商户
            var user = await GetCurrentUserAsync();
            List<MerchantDto> merchantDtos = new List<MerchantDto>();
            foreach (var item in user.Merchants)
            {
                var merchantdto = ObjectMapper.Map<MerchantDto>(item);
                merchantDtos.Add(merchantdto);
            }
            return merchantDtos;
        }
        public async Task<bool> GetIsInternalMerchant()
        {
            var user = await GetCurrentUserAsync();
            if(user.Merchants.Count==1)
            {
                var merchant = user.Merchants.FirstOrDefault();
                var internalWithdrawMerchant = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.InternalWithdrawMerchant);
                if (internalWithdrawMerchant.Contains(merchant.MerchantCode))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual async Task CreateOrEdit(CreateOrEditWithdrawalDeviceDto input)
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

        public virtual async Task<string> CheckDuplicate(CreateOrEditWithdrawalDeviceDto input)
        {
            // Check if there is a match based on BankType, Phone, and CardNumber

            if (input.Id == null)
            {
                bool isMatch = _withdrawalDeviceRepository.GetAll().Any(p =>
                    p.BankType == input.BankType &&
                    p.Phone == input.Phone &&
                    p.CardNumber == input.CardNumber);

                if (!isMatch)
                {
                    return "Success";
                }
                else
                {
                    return "DuplicateEntryFound";
                }
            }
            else
            {
                //select existing devices which is not to same id checking
                bool isMatchForUpdate = _withdrawalDeviceRepository.GetAll().Any(p => p.Id != input.Id && p.BankType == input.BankType &&
                p.Phone == input.Phone && p.CardNumber == input.CardNumber);

                if (!isMatchForUpdate)
                {
                    return "Success";
                }
                else
                {
                    return "DuplicateEntryFound";
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_WithdrawalDevices_Create)]
        protected virtual async Task Create(CreateOrEditWithdrawalDeviceDto input)
        {
            var withdrawalDevice = ObjectMapper.Map<WithdrawalDevice>(input);
            withdrawalDevice.Status = true;
            withdrawalDevice.Process = WithdrawalDevicesProcessTypeEnum.Stop;
            await _withdrawalDeviceRepository.InsertAsync(withdrawalDevice);
        }

        [AbpAuthorize(AppPermissions.Pages_WithdrawalDevices_Edit)]
        protected virtual async Task Update(CreateOrEditWithdrawalDeviceDto input)
        {
            var withdrawalDevice = await _withdrawalDeviceRepository.FirstOrDefaultAsync((int)input.Id);
            //input.Status = true;
            ObjectMapper.Map(input, withdrawalDevice);
            //加入缓存
            await CurrentUnitOfWork.SaveChangesAsync();
            WithdrawalDeviceRedisModel withdrawalDeviceRedisModel = ObjectMapper.Map<WithdrawalDeviceRedisModel>(withdrawalDevice);
            _redisService.UpdateWithdrawDevice(input.MerchantCode, withdrawalDeviceRedisModel);
        }

        [AbpAuthorize(AppPermissions.Pages_WithdrawalDevices_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            var withdrawalDevice = await _withdrawalDeviceRepository.FirstOrDefaultAsync((int)input.Id);
            await _withdrawalDeviceRepository.DeleteAsync(input.Id);
            WithdrawalDeviceRedisModel withdrawalDeviceRedisModel = ObjectMapper.Map<WithdrawalDeviceRedisModel>(withdrawalDevice);
            _redisService.DeleteWithdrawDevice(withdrawalDevice.MerchantCode, withdrawalDeviceRedisModel);
        }

        //[AbpAuthorize(AppPermissions.Pages_WithdrawalDevices_ChildEdit)]
        //public async Task EditStatus(EntityDto input)
        //{
        //    var withdrawalDevice = await _withdrawalDeviceRepository.FirstOrDefaultAsync((int)input.Id);
        //    if(withdrawalDevice != null)
        //    {
        //        if (withdrawalDevice.Status)
        //        {
        //            withdrawalDevice.Status = false;
        //            await _withdrawalDeviceRepository.UpdateAsync(withdrawalDevice);
        //        }
        //        else
        //        {
        //            withdrawalDevice.Status = true;
        //            await _withdrawalDeviceRepository.UpdateAsync(withdrawalDevice);
        //        }
        //        await CurrentUnitOfWork.SaveChangesAsync();

        //        WithdrawalDeviceRedisModel withdrawalDeviceRedisModel = ObjectMapper.Map<WithdrawalDeviceRedisModel>(withdrawalDevice);
        //        _redisService.EditStatusWithdrawDevice(withdrawalDevice.MerchantCode, withdrawalDeviceRedisModel);
        //    }
        //}

        [AbpAuthorize(AppPermissions.Pages_WithdrawalDevices_ChildEdit)]
        public async Task EditProcess(EntityDto input)
        {
            var withdrawalDevice = await _withdrawalDeviceRepository.FirstOrDefaultAsync((int)input.Id);
            if (withdrawalDevice != null)
            {
                if (withdrawalDevice.Process == WithdrawalDevicesProcessTypeEnum.Process)
                {
                    withdrawalDevice.Process = WithdrawalDevicesProcessTypeEnum.Stop;
                    await _withdrawalDeviceRepository.UpdateAsync(withdrawalDevice);
                }
                else
                {
                    withdrawalDevice.Process = WithdrawalDevicesProcessTypeEnum.Process;
                    await _withdrawalDeviceRepository.UpdateAsync(withdrawalDevice);
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                WithdrawalDeviceRedisModel withdrawalDeviceRedisModel = ObjectMapper.Map<WithdrawalDeviceRedisModel>(withdrawalDevice);
                _redisService.EditProcessWithdrawDevice(withdrawalDevice.MerchantCode, withdrawalDeviceRedisModel);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_WithdrawalDevices_BatchPauseBank)]
        public async Task PauseBank([Required][EnumDataType(typeof(WithdrawalDevicesBankTypeEnum))] WithdrawalDevicesBankTypeEnum bankType)
        {
            var withdrawalDevices = await _withdrawalDeviceRepository.GetAllListAsync(x => x.BankType == bankType);
            if (withdrawalDevices is { Count: > 0 })
            {
                foreach (var withdrawalDevice in withdrawalDevices)
                {
                    if (withdrawalDevice.Process == WithdrawalDevicesProcessTypeEnum.Process)
                    {
                        withdrawalDevice.Process = WithdrawalDevicesProcessTypeEnum.Stop;
                    }

                    await _withdrawalDeviceRepository.UpdateAsync(withdrawalDevice);

                    await CurrentUnitOfWork.SaveChangesAsync();

                    var withdrawalDeviceRedisModel = ObjectMapper.Map<WithdrawalDeviceRedisModel>(withdrawalDevice);
                    _redisService.EditProcessWithdrawDevice(withdrawalDevice.MerchantCode, withdrawalDeviceRedisModel);
                }
            }
        }
    }
}
