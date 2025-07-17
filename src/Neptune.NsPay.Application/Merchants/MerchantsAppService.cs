using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantFunds;
using Neptune.NsPay.MerchantFunds.Dtos;
using Neptune.NsPay.MerchantRates;
using Neptune.NsPay.MerchantRates.Dtos;
using Neptune.NsPay.Merchants.Dtos;
using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayGroups;
using Neptune.NsPay.PayGroups.Dtos;
using Neptune.NsPay.RedisExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Neptune.NsPay.Merchants
{
    [AbpAuthorize(AppPermissions.Pages_Merchants)]
    public class MerchantsAppService : NsPayAppServiceBase, IMerchantsAppService
    {
        private readonly IRepository<Merchant> _merchantRepository;
        private readonly IRepository<MerchantRate> _merchantRateRepository;
        private readonly IRepository<MerchantFund> _merchantFundRepository;
        private readonly IRepository<MerchantWithdraw, long> _merchantWithdrawRepository;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;
        private readonly IRepository<PayGroup> _payGroupRepository;
        private readonly IRedisService _redisService;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;

        public MerchantsAppService(IRepository<Merchant> merchantRepository,
            IMerchantFundsMongoService merchantFundsMongoService,
            IRepository<MerchantRate> merchantRateRepository,
            IRepository<MerchantFund> merchantFundRepository,
            IRepository<PayGroup> payGroupRepository,
            IRedisService redisService,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            IRepository<MerchantWithdraw, long> merchantWithdrawRepository)
        {
            _merchantRepository = merchantRepository;
            _merchantRateRepository = merchantRateRepository;
            _merchantFundRepository = merchantFundRepository;
            _merchantFundsMongoService = merchantFundsMongoService;
            _payGroupRepository = payGroupRepository;
            _redisService = redisService;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _merchantWithdrawRepository = merchantWithdrawRepository;
        }

        public virtual async Task<PagedResultDto<GetMerchantForViewDto>> GetAll(GetAllMerchantsInput input)
        {
            var filteredMerchants = _merchantRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.MerchantCode.Contains(input.Filter) || e.PlatformCode.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.Contains(input.NameFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.MerchantCodeFilter), e => e.MerchantCode.Contains(input.MerchantCodeFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PlatformCodeFilter), e => e.PlatformCode.Contains(input.PlatformCodeFilter));

            var pagedAndFilteredMerchants = filteredMerchants
                .OrderBy(input.Sorting ?? "id desc")
                .PageBy(input);

            var merchants = from o in pagedAndFilteredMerchants
                            select new
                            {
                                o.Name,
                                o.Mail,
                                o.Phone,
                                o.MerchantCode,
                                o.MerchantSecret,
                                o.PlatformCode,
                                o.PayGroupId,
                                o.CountryType,
                                o.Remark,
                                Id = o.Id
                            };

            var totalCount = await filteredMerchants.CountAsync();

            var dbList = await merchants.ToListAsync();
            var results = new List<GetMerchantForViewDto>();
            var paygroups = await _payGroupRepository.GetAllListAsync();
            var merchantRates = await _merchantRateRepository.GetAllListAsync();

            var merchantFunds = await _merchantFundsMongoService.GetFundsAll();

            foreach (var o in dbList)
            {
                var rateInfo = merchantRates.FirstOrDefault(r => r.MerchantId == o.Id) ?? new MerchantRate();
                var fundInfo = merchantFunds.FirstOrDefault(r => r.MerchantCode == o.MerchantCode);
                MerchantFundDto merchantFundDto = new MerchantFundDto();
                if (fundInfo != null)
                {
                    merchantFundDto.MerchantCode = o.MerchantCode;
                    merchantFundDto.MerchantId = o.Id;
                    merchantFundDto.DepositAmount = fundInfo.DepositAmount;
                    merchantFundDto.WithdrawalAmount = fundInfo.WithdrawalAmount;
                    merchantFundDto.TranferAmount = fundInfo.TranferAmount;
                    merchantFundDto.RateFeeBalance = fundInfo.RateFeeBalance;
                    merchantFundDto.Balance = fundInfo.Balance;
                    merchantFundDto.FrozenBalance = fundInfo.FrozenBalance;
                }
                var res = new GetMerchantForViewDto()
                {
                    Merchant = new MerchantDto
                    {
                        Name = o.Name,
                        MerchantCode = o.MerchantCode,
                        MerchantSecret = o.MerchantSecret,
                        PlatformCode = o.PlatformCode,
                        PayGroupId = o.PayGroupId,
                        CountryType = o.CountryType,
                        Remark = o.Remark,
                        PayGroupName = paygroups.Where(r => r.Id == o.PayGroupId).FirstOrDefault()?.GroupName,
                        Id = o.Id,
                    },
                    MerchantRate = ObjectMapper.Map<MerchantRateDto>(rateInfo),
                    MerchantFund = merchantFundDto
                };

                results.Add(res);
            }

            return new PagedResultDto<GetMerchantForViewDto>(
                totalCount,
                results
            );
        }

        public virtual async Task<GetMerchantForViewDto> GetMerchantForView(int id)
        {
            var merchant = await _merchantRepository.GetAsync(id);

            var output = new GetMerchantForViewDto { Merchant = ObjectMapper.Map<MerchantDto>(merchant) };

            return output;
        }

        public async Task<List<PayGroupDto>> GetPayGroups()
        {
            var payGroups = await _payGroupRepository.GetAllListAsync(r => r.Status == true);
            var results = new List<PayGroupDto>();
            foreach (var o in payGroups)
            {
                var PayGroup = new PayGroupDto
                {
                    GroupName = o.GroupName,
                    Status = o.Status,
                    Id = o.Id,
                };
                results.Add(PayGroup);
            }
            return results;
        }

        [AbpAuthorize(AppPermissions.Pages_Merchants_Edit)]
        public virtual async Task<GetMerchantForEditOutput> GetMerchantForEdit(EntityDto input)
        {
            var merchant = await _merchantRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetMerchantForEditOutput { Merchant = ObjectMapper.Map<CreateOrEditMerchantDto>(merchant) };

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditMerchantDto input)
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
        protected virtual async Task Create(CreateOrEditMerchantDto input)
        {
            var merchant = ObjectMapper.Map<Merchant>(input);
            merchant.MerchantCode = GetMerchantCode();
            merchant.MerchantSecret = Guid.NewGuid().ToString("N");
            var merchantId = await _merchantRepository.InsertAndGetIdAsync(merchant);

            //添加rate表
            var merchantRate = new MerchantRate()
            {
                MerchantCode = merchant.MerchantCode,
                MerchantId = merchantId,
                ScanBankRate = input.ScanBankRate,
                ScratchCardRate = input.ScratchCardRate,
                MoMoRate = input.MoMoRate,
                USDTFixedFees = input.USDTFixedFees,
                USDTRateFees = input.USDTRateFees,
            };
            await _merchantRateRepository.InsertAsync(merchantRate);

            //添加资金表
            var merchantFund = new MerchantFund()
            {
                MerchantCode = merchant.MerchantCode,
                MerchantId = merchantId,
                DepositAmount = 0,
                WithdrawalAmount = 0,
                RateFeeBalance = 0,
                Balance = 0,
                CreationTime = DateTime.Now,
            };
            await _merchantFundRepository.InsertAsync(merchantFund);

            //添加缓存
            _redisService.AddRedisValue(NsPayRedisKeyConst.MerchantKey + merchant.MerchantCode, merchant);
            _redisService.AddRedisValue(NsPayRedisKeyConst.MerchantRateKey + merchant.MerchantCode, merchantRate);
            _redisService.AddRedisValue(NsPayRedisKeyConst.MerchantFundKey + merchant.MerchantCode, merchantFund);
        }

        [AbpAuthorize(AppPermissions.Pages_Merchants_Edit)]
        protected virtual async Task Update(CreateOrEditMerchantDto input)
        {
            var merchant = await _merchantRepository.FirstOrDefaultAsync((int)input.Id);
            input.MerchantCode = merchant.MerchantCode;
            input.MerchantSecret = merchant.MerchantSecret;
            ObjectMapper.Map(input, merchant);

            //更新rate表
            var merchantRate = await _merchantRateRepository.FirstOrDefaultAsync(r => r.MerchantId == merchant.Id);
            merchantRate.ScanBankRate = input.ScanBankRate;
            merchantRate.ScratchCardRate = input.ScratchCardRate;
            merchantRate.MoMoRate = input.MoMoRate;
            merchantRate.USDTFixedFees = input.USDTFixedFees;
            merchantRate.USDTRateFees = input.USDTRateFees;
            await _merchantRateRepository.UpdateAsync(merchantRate);

            //更新缓存
            _redisService.AddRedisValue(NsPayRedisKeyConst.MerchantKey + merchant.MerchantCode, merchant);
            _redisService.AddRedisValue(NsPayRedisKeyConst.MerchantRateKey + merchant.MerchantCode, merchantRate);
        }

        [AbpAuthorize(AppPermissions.Pages_Merchants_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            var merchant = await _merchantRepository.FirstOrDefaultAsync((int)input.Id);
            await _merchantRepository.DeleteAsync(input.Id);

            //删除rate表
            var merchantRate = await _merchantRateRepository.FirstOrDefaultAsync(r => r.MerchantId == merchant.Id);
            //更新缓存
            _redisService.RemoveRedisValue(NsPayRedisKeyConst.MerchantKey + merchant.MerchantCode);
            _redisService.RemoveRedisValue(NsPayRedisKeyConst.MerchantRateKey + merchant.MerchantCode);
        }

        [AbpAuthorize(AppPermissions.Pages_Merchants_Recal_LockBalance)]
        public virtual async Task<bool> ResetLoackedBalance(EntityDto input)
        {
            var haveUpdate = false;
            var merchant = await _merchantRepository.FirstOrDefaultAsync((int)input.Id);
            var currentMerchantLockedAmount = await _withdrawalOrdersMongoService.GetAllPendingReleaseOrder(merchant.Id);
            var currentMerchantLockedWithdrawal = await _merchantWithdrawRepository.GetAllListAsync(x => x.MerchantCode == merchant.MerchantCode && x.Status == MerchantWithdrawStatusEnum.Pending);

            if (currentMerchantLockedWithdrawal != null)
            {
                currentMerchantLockedAmount += (currentMerchantLockedWithdrawal.Sum(x => x.Money));
            }

            if (await _merchantFundsMongoService.ResetMerchantFrozenBalance(merchant.MerchantCode, currentMerchantLockedAmount))
            {
                haveUpdate = true;
            }

            return haveUpdate;
        }

        private string GetMerchantCode()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
                i *= ((int)b + 1);
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }
    }
}