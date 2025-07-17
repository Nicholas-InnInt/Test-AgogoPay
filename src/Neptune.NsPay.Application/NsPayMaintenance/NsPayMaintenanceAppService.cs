using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization;
using Abp.Authorization;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.Caching.Dto;
using System.Collections.Generic;
using Neptune.NsPay.Commons;
using Abp.Domain.Repositories;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.MerchantFunds;
using Neptune.NsPay.NsPaySystemSettings;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.MerchantRates;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Neptune.NsPay.NsPayMaintenance
{
    [AbpAuthorize(AppPermissions.Pages_NsPayMaintenance)]
    public class NsPayMaintenanceAppService : NsPayAppServiceBase, INsPayMaintenanceAppService
    {
        private readonly IRedisService _redisService;
        private readonly IRepository<Merchant> _merchantRepository;
        private readonly IRepository<MerchantRate> _merchantRateRepository;
        private readonly IRepository<MerchantFund> _merchantFundRepository;
        private readonly IRepository<NsPaySystemSetting> _nsPaySystemSettingRepository;
        private readonly IRepository<PayMent> _payMentRepository;
        private readonly IRepository<WithdrawalDevice> _withdrawalDeviceRepository;
        public NsPayMaintenanceAppService(
            IRedisService redisService,
            IRepository<Merchant> merchantRepository,
            IRepository<MerchantRate> merchantRateRepository,
            IRepository<MerchantFund> merchantFundRepository,
            IRepository<NsPaySystemSetting> nsPaySystemSettingRepository,
            IRepository<PayMent> payMentRepository,
            IRepository<WithdrawalDevice> withdrawalDeviceRepository
            )
        {
            _redisService = redisService;
            _merchantRepository = merchantRepository;
            _merchantRateRepository = merchantRateRepository;
            _merchantFundRepository = merchantFundRepository;
            _nsPaySystemSettingRepository = nsPaySystemSettingRepository;
            _payMentRepository = payMentRepository;
            _withdrawalDeviceRepository = withdrawalDeviceRepository;
        }

        public ListResultDto<CacheDto> GetAllCaches()
        {
            var caches = new List<CacheDto>() { 
                new CacheDto { Name = "Merchant" }, 
                new CacheDto { Name = "NsPaySystemSettings" }, 
                new CacheDto { Name = "PayMents" },
                new CacheDto { Name = "WithdrawalDevices" }
            };

            return new ListResultDto<CacheDto>(caches);
        }

        public virtual async Task ClearCache(EntityDto<string> input)
        {
            if (input.Id == "Merchant")
            {
                await ResetRedisMerchant();
            } 
            else if(input.Id == "NsPaySystemSettings")
            {
                await ResetRedisNsPaySystemSetting();
            }
            else if (input.Id == "PayMents")
            {
                await ResetRedisPayMents();
            }
            else if (input.Id == "WithdrawalDevices")
            {
                await ResetRedisWithdrawalDevice();
            }
        }

        public virtual async Task ClearAllCaches()
        {

            await ResetRedisMerchant();
            await ResetRedisNsPaySystemSetting();
            await ResetRedisPayMents();
            await ResetRedisWithdrawalDevice();
        }


        private async Task ResetRedisMerchant() 
        {
            //_redisService.RemoveRedisValueByPattern(NsPayRedisKeyConst.MerchantKey);
            //_redisService.RemoveRedisValueByPattern(NsPayRedisKeyConst.MerchantRateKey);
            //_redisService.RemoveRedisValueByPattern(NsPayRedisKeyConst.MerchantFundKey);

            var merchants = await _merchantRepository.GetAllListAsync();
            foreach (var merchant in merchants)
            {
                var merchantRate = await _merchantRateRepository.FirstOrDefaultAsync(e => e.MerchantCode == merchant.MerchantCode && e.MerchantId == merchant.Id);

                var merchantFund = await _merchantFundRepository.FirstOrDefaultAsync(e => e.MerchantCode == merchant.MerchantCode && e.MerchantId == merchant.Id);

                _redisService.AddRedisValue(NsPayRedisKeyConst.MerchantKey + merchant.MerchantCode, merchant);
                _redisService.AddRedisValue(NsPayRedisKeyConst.MerchantRateKey + merchant.MerchantCode, merchantRate);
                _redisService.AddRedisValue(NsPayRedisKeyConst.MerchantFundKey + merchant.MerchantCode, merchantFund);
            }
        }

        private async Task ResetRedisNsPaySystemSetting() 
        {
            //_redisService.RemoveRedisValue(NsPayRedisKeyConst.NsPaySystemKey);
            var nsPaySystemSetting = await _nsPaySystemSettingRepository.GetAll().ToArrayAsync();
            if (nsPaySystemSetting != null)
            {
                foreach (var item in nsPaySystemSetting)
                {
                    _redisService.AddRedisValue(NsPayRedisKeyConst.NsPaySystemKey + item.Key, item.Value);
                }
            }
        }

        private async Task ResetRedisPayMents()
        {
            _redisService.RemoveRedisValueByPattern(NsPayRedisKeyConst.PayMents);
            var payMents = await _payMentRepository.GetAll().ToArrayAsync();
            if (payMents != null)
            {
                foreach (var item in payMents)
                {
                    var redisModel = ObjectMapper.Map<PayMentRedisModel>(item);
                    _redisService.SetPayMentCaches(redisModel);
                }
            }
        }

        private async Task ResetRedisWithdrawalDevice() 
        {
            //_redisService.RemoveRedisValueByPattern(NsPayRedisKeyConst.WithdrawalDevices);
            var withdrawalDevices = await _withdrawalDeviceRepository.GetAll().ToArrayAsync();
            if (withdrawalDevices != null)
            {
                foreach (var item in withdrawalDevices)
                {
                    var redisModel = ObjectMapper.Map<WithdrawalDeviceRedisModel>(item);
                    _redisService.SetRPushWithdrawDevice(item.MerchantCode, redisModel);
                }
            }
        }
    }
}