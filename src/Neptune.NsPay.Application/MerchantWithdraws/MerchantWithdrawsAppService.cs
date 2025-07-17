using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Neptune.NsPay.MerchantWithdraws.Exporting;
using Neptune.NsPay.MerchantWithdraws.Dtos;
using Neptune.NsPay.Dto;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.EntityFrameworkCore;
using Neptune.NsPay.EntityFrameworkCore;
using Neptune.NsPay.MerchantWithdrawBanks.Dtos;
using Neptune.NsPay.MerchantWithdrawBanks;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Merchants;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Localization;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Common;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.KafkaExtensions;

namespace Neptune.NsPay.MerchantWithdraws
{
    [AbpAuthorize(AppPermissions.Pages_MerchantWithdraws)]
    public class MerchantWithdrawsAppService : NsPayAppServiceBase, IMerchantWithdrawsAppService
    {
        private readonly IDbContextProvider<NsPayDbContext> _dbContextProvider;
        private readonly IRepository<MerchantWithdraw, long> _merchantWithdrawRepository;
        private readonly IMerchantWithdrawsExcelExporter _merchantWithdrawsExcelExporter;
        private readonly IRepository<MerchantWithdrawBank> _merchantWithdrawBankRepository;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;
        private readonly IRepository<Merchant> _merchantRepository;
        private readonly IRedisService _redisService;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IKafkaProducer _kafkaProducer;

        public MerchantWithdrawsAppService(IRepository<MerchantWithdraw, long> merchantWithdrawRepository,
            IMerchantWithdrawsExcelExporter merchantWithdrawsExcelExporter,
            IDbContextProvider<NsPayDbContext> dbContextProvider,
            IRepository<MerchantWithdrawBank> merchantWithdrawBankRepository,
            IAppConfigurationAccessor appConfigurationAccessor,
            IMerchantFundsMongoService merchantFundsMongoService,
            IRepository<Merchant> merchantRepository,
            IRedisService redisService,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            IKafkaProducer kafkaProducer)
        {
            _dbContextProvider = dbContextProvider;
            _merchantWithdrawRepository = merchantWithdrawRepository;
            _merchantWithdrawsExcelExporter = merchantWithdrawsExcelExporter;
            _merchantWithdrawBankRepository = merchantWithdrawBankRepository;
            _appConfiguration = appConfigurationAccessor.Configuration;
            _merchantFundsMongoService = merchantFundsMongoService;
            _merchantRepository = merchantRepository;
            _redisService = redisService;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _kafkaProducer = kafkaProducer;
        }

        public virtual async Task<PagedResultDto<GetMerchantWithdrawForViewDto>> GetAll(GetAllMerchantWithdrawsInput input)
        {
            var user = await GetCurrentUserAsync();
            var merchantCode = user.Merchants.Select(r => r.MerchantCode);
            var context = await _dbContextProvider.GetDbContextAsync();

            var statusFilter = input.StatusFilter.HasValue
                        ? (MerchantWithdrawStatusEnum)input.StatusFilter
                        : default;

            var filteredMerchantWithdraws = context.MerchantWithdraws.Where(r => context.Merchants.Any(e => e.MerchantCode == r.MerchantCode && merchantCode.Contains(r.MerchantCode)))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.MerchantCode.Contains(input.Filter) || e.WithDrawNo.Contains(input.Filter) || e.BankName.Contains(input.Filter) || e.ReceivCard.Contains(input.Filter) || e.ReceivName.Contains(input.Filter) || e.ReviewMsg.Contains(input.Filter) || e.Remark.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.MerchantCodeFilter), e => e.MerchantCode.Contains(input.MerchantCodeFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.WithDrawNoFilter), e => e.WithDrawNo.Contains(input.WithDrawNoFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.BankNameFilter), e => e.BankName.Contains(input.BankNameFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ReceivCardFilter), e => e.ReceivCard.Contains(input.ReceivCardFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ReceivNameFilter), e => e.ReceivName.Contains(input.ReceivNameFilter))
                        .WhereIf(input.StatusFilter.HasValue && input.StatusFilter > -1, e => e.Status == statusFilter)
                        .WhereIf(input.MinReviewTimeFilter != null, e => e.ReviewTime >= input.MinReviewTimeFilter)
                        .WhereIf(input.MaxReviewTimeFilter != null, e => e.ReviewTime <= input.MaxReviewTimeFilter);

            var pagedAndFilteredMerchantWithdraws = filteredMerchantWithdraws
                .OrderBy(input.Sorting ?? "id desc")
                .PageBy(input);

            var merchantWithdraws = from o in pagedAndFilteredMerchantWithdraws
                                    select new
                                    {
                                        o.MerchantCode,
                                        o.WithDrawNo,
                                        o.Money,
                                        o.BankName,
                                        o.ReceivCard,
                                        o.ReceivName,
                                        o.Status,
                                        o.ReviewTime,
                                        Id = o.Id,
                                        o.ReviewMsg,
                                        o.CreationTime,
                                    };

            var totalCount = await filteredMerchantWithdraws.CountAsync();

            var dbList = await merchantWithdraws.ToListAsync();
            var results = new List<GetMerchantWithdrawForViewDto>();
            var culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);

            foreach (var o in dbList)
            {
                var res = new GetMerchantWithdrawForViewDto()
                {
                    MerchantWithdraw = new MerchantWithdrawDto
                    {
                        MerchantCode = o.MerchantCode,
                        WithDrawNo = o.WithDrawNo,
                        Money = o.Money.ToString("C0", culInfo),
                        BankName = o.BankName,
                        ReceivCard = o.ReceivCard,
                        ReceivName = o.ReceivName,
                        Status = o.Status,
                        ReviewTime = o.ReviewTime,
                        ReviewMsg = o.ReviewMsg,
                        CreationTime = CultureTimeHelper.GetCultureTimeInfo(o.CreationTime, CultureTimeHelper.TimeCodeViVn),
                        Id = o.Id,
                    },
                    MerchantName = user.Merchants.Where(r => r.MerchantCode == o.MerchantCode).FirstOrDefault()?.Name
                };

                results.Add(res);
            }

            return new PagedResultDto<GetMerchantWithdrawForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetMerchantWithdrawForViewDto> GetMerchantWithdrawForView(long id)
        {
            var merchantWithdraw = await _merchantWithdrawRepository.GetAsync(id);

            var output = new GetMerchantWithdrawForViewDto { MerchantWithdraw = ObjectMapper.Map<MerchantWithdrawDto>(merchantWithdraw) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantWithdraws_Edit)]
        public virtual async Task<GetMerchantWithdrawForEditOutput> GetMerchantWithdrawForEdit(EntityDto<long> input)
        {
            var merchantWithdraw = await _merchantWithdrawRepository.FirstOrDefaultAsync(input.Id);
            //获取当前的商户
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants;
            if (user.UserType == UserTypeEnum.ExternalMerchant)
            {
                if (merchants.Count == 1)
                {
                    var merchant = merchants[0];
                    var merchantBanks = await _merchantWithdrawBankRepository.GetAllListAsync(r => r.Status == true && r.MerchantId == merchant.Id);
                    List<MerchantWithdrawBankDto> merchantBankDtos = new List<MerchantWithdrawBankDto>();
                    foreach (var item in merchantBanks)
                    {
                        var merchantBank = ObjectMapper.Map<MerchantWithdrawBankDto>(item);
                        merchantBankDtos.Add(merchantBank);
                    }
                    //余额减去等待提现订单
                    var funds = await _merchantFundsMongoService.GetFundsByMerchantCode(merchant.MerchantCode);
                    var hostiyLists = await _merchantWithdrawRepository.GetAllListAsync(r => r.MerchantCode == merchant.MerchantCode && r.Status == MerchantWithdrawStatusEnum.Pending);
                    var pendingReleaseWithdrawal = await _withdrawalOrdersMongoService.GetAllPendingReleaseOrder(merchant.Id);
                    //var money = Convert.ToDecimal(0);
                    //if (hostiyLists.Count > 0)
                    //{
                    //    money = hostiyLists.Sum(r => r.Money);
                    //}


                    var balance = funds?.Balance - funds?.FrozenBalance;
                    var output = new GetMerchantWithdrawForEditOutput
                    {
                        MerchantWithdraw = ObjectMapper.Map<CreateOrEditMerchantWithdrawDto>(merchantWithdraw),
                        MerchantBanks = merchantBankDtos,
                        Balance = balance,
                        PendingWithdrawalOrderAmount = pendingReleaseWithdrawal,
                        PendingMerchantWithdrawalAmount = hostiyLists!=null? hostiyLists.Sum(x=>x.Money):0 ,
                    };

                    return output;
                }
            }
            return new GetMerchantWithdrawForEditOutput();
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantWithdraws_Create)]
        public async Task<GetMerchantWithdrawForEditOutput> GetMerchantWithdrawForCreate()
        {
            //获取当前的商户
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants;
            if (user.UserType == UserTypeEnum.ExternalMerchant)
            {
                if (merchants.Count == 1)
                {
                    var merchant = merchants[0];
                    var output = new GetMerchantWithdrawForEditOutput() { MerchantWithdraw = new CreateOrEditMerchantWithdrawDto(), };
                    var merchantBanks = await _merchantWithdrawBankRepository.GetAllListAsync(r => r.Status == true && r.MerchantId == merchant.Id);
                    List<MerchantWithdrawBankDto> merchantBankDtos = new List<MerchantWithdrawBankDto>();
                    foreach (var item in merchantBanks)
                    {
                        var merchantBank = ObjectMapper.Map<MerchantWithdrawBankDto>(item);
                        merchantBankDtos.Add(merchantBank);
                    }
                    var funds = await _merchantFundsMongoService.GetFundsByMerchantCode(merchant.MerchantCode);
                    var hostiyLists = await _merchantWithdrawRepository.GetAllListAsync(r => r.MerchantCode == merchant.MerchantCode && r.Status == MerchantWithdrawStatusEnum.Pending);
                    var pendingReleaseWithdrawal = await _withdrawalOrdersMongoService.GetAllPendingReleaseOrder(merchant.Id);
     
                    var balance = funds?.Balance - funds?.FrozenBalance;
                    output.MerchantBanks = merchantBankDtos;
                    output.Balance = balance;
                    output.BalanceInit = funds?.Balance;
                    output.MerchantWithdraw.MerchantCode = merchant.MerchantCode;
                    output.PendingWithdrawalOrderAmount = pendingReleaseWithdrawal;
                    output.PendingMerchantWithdrawalAmount = hostiyLists != null ? hostiyLists.Sum(x => x.Money) : 0;

                    return output;
                }
            }
            return new GetMerchantWithdrawForEditOutput();
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantWithdraws_AuditTurndown)]
        public async Task<GetMerchantWithdrawForTurndownOutput> GetMerchantWithdrawForTurndown(EntityDto<long> input)
        {
            var merchantWithdraw = await _merchantWithdrawRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetMerchantWithdrawForTurndownOutput
            {
                MerchantWithdraw = ObjectMapper.Map<TurndownOrPassMerchantWithdrawDto>(merchantWithdraw),
            };

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditMerchantWithdrawDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantWithdraws_Create)]
        protected virtual async Task Create(CreateOrEditMerchantWithdrawDto input)
        {
            var merchantWithdraw = ObjectMapper.Map<MerchantWithdraw>(input);
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants;
            var eventId = Guid.NewGuid().ToString();
            if (user.UserType == UserTypeEnum.ExternalMerchant)
            {
                var checkMerchant = user.Merchants.FirstOrDefault(r => r.MerchantCode == input.MerchantCode);
                if (checkMerchant != null && input.BankId > 0)
                {
                    var merchant = await _merchantRepository.FirstOrDefaultAsync(r => r.MerchantCode == input.MerchantCode);
                    if (merchant != null)
                    {
                        //var hostiyLists = await _merchantWithdrawRepository.GetAllListAsync(r => r.MerchantCode == input.MerchantCode && r.Status == MerchantWithdrawStatusEnum.Pending);
                        //var money = Convert.ToDecimal(0);
                        //if (hostiyLists.Count > 0)
                        //{
                        //    money = hostiyLists.Sum(r => r.Money);
                        //}
                        var funds = await _merchantFundsMongoService.GetFundsByMerchantCode(merchant.MerchantCode);
                        //var pendingWithdrawOrder = await _withdrawalOrdersMongoService.GetMerchantPendingOrder(merchant.MerchantCode);

                        //if (pendingWithdrawOrder.Count > 0)
                        //{
                        //    money += pendingWithdrawOrder.Sum(x => x.OrderMoney);
                        //}
                        var money = Convert.ToDecimal(0);

                        if (funds?.Balance > 0 && input.Money > 0)
                        {
                            var checkBlance = funds.Balance - funds.FrozenBalance - input.Money;
                            if (checkBlance > 0)
                            {
                                merchantWithdraw.WithDrawNo = Guid.NewGuid().ToString("N");
                                merchantWithdraw.MerchantId = checkMerchant.Id;
                                merchantWithdraw.ReviewTime = DateTime.Now;
                                merchantWithdraw.Status = MerchantWithdrawStatusEnum.Pending;
                                await _merchantWithdrawRepository.InsertAsync(merchantWithdraw);

                                //var result = await  _merchantBillsHelper.UpdateFrozenBalanceMerchantWithdrawal(merchant.MerchantCode, input.Money, merchantWithdraw.Id,false,  5);

                                try
                                {
                                    var transferOrder = new MerchantBalancePublishDto()
                                    {
                                        MerchantCode = merchantWithdraw.MerchantCode,
                                        Type = MerchantBalanceType.Increase,
                                        Money = Convert.ToInt32(merchantWithdraw.Money),
                                        TriggerDate = DateTime.Now,
                                        ProcessId = eventId,
                                        Source = BalanceTriggerSource.MerchantWithdrawal,
                                        ReferenceId = merchantWithdraw.WithDrawNo
                                    };
                                    await _kafkaProducer.ProduceAsync<MerchantBalancePublishDto>(KafkaTopics.MerchantBalance, merchantWithdraw.WithDrawNo, transferOrder);
                                }
                                catch (Exception ex)
                                {
                                    NlogLogger.Error("Transfer-Kafka推送失败：" + ex.ToString());
                                }


                                ////添加冻结余额
                                ////var result = await _merchantBillsHelper.AddMerchantFundsFrozenBalance(merchant.MerchantCode, input.Money);
                                //if (result)
                                //{

                                //}
                            }
                        }
                    }
                }
            }

        }

        public async Task AuditTurndownOrPass(TurndownOrPassMerchantWithdrawDto input)
        {
            await AuditTurndown(input);
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantWithdraws_AuditTurndown)]
        protected virtual async Task AuditTurndown(TurndownOrPassMerchantWithdrawDto input)
        {
            var eventId = Guid.NewGuid().ToString();
            if (input.Id.HasValue)
            {
                var merchantWithdraw = await _merchantWithdrawRepository.FirstOrDefaultAsync(input.Id.Value);
                merchantWithdraw.Status = MerchantWithdrawStatusEnum.Turndown;
                merchantWithdraw.ReviewMsg = input.ReviewMsg;
                await _merchantWithdrawRepository.UpdateAsync(merchantWithdraw);
                try
                {
                    var transferOrder = new MerchantBalancePublishDto()
                    {
                        MerchantCode = merchantWithdraw.MerchantCode,
                        Type = MerchantBalanceType.Decrease,
                        Money = Convert.ToInt32(merchantWithdraw.Money),
                        TriggerDate = DateTime.Now,
                        ProcessId = eventId,
                        Source = BalanceTriggerSource.MerchantWithdrawal,
                        ReferenceId = merchantWithdraw.WithDrawNo
                    };
                    await _kafkaProducer.ProduceAsync<MerchantBalancePublishDto>(KafkaTopics.MerchantBalance, merchantWithdraw.WithDrawNo, transferOrder);
                }
                catch (Exception ex)
                {
                    NlogLogger.Error("Transfer-Kafka推送失败：" + ex.ToString());
                }

            }
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantWithdraws_AuditPass)]
        public virtual async Task AuditPass(EntityDto<long> input)
        {
            var merchantWithdraw = await _merchantWithdrawRepository.FirstOrDefaultAsync(input.Id);
            var merchant = await _merchantRepository.FirstOrDefaultAsync(r => r.MerchantCode == merchantWithdraw.MerchantCode);
            if (merchant != null)
            {
                merchantWithdraw.Status = MerchantWithdrawStatusEnum.Pass;
                merchantWithdraw.ReviewTime = DateTime.Now;
                var user = await GetCurrentUserAsync();
                merchantWithdraw.Remark = "用户：" + user.UserName + ",提款单号：" + merchantWithdraw.WithDrawNo + ",操作提款成功时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await _merchantWithdrawRepository.UpdateAsync(merchantWithdraw);
                await CurrentUnitOfWork.SaveChangesAsync();

                //PayMerchantRedisMqDto redisMqDto = new PayMerchantRedisMqDto()
                //{
                //    PayMqSubType = MQSubscribeStaticConsts.MerchantBillAddWithdraws,
                //    MerchantCode = merchantWithdraw.MerchantCode,
                //    MerchantWithdrawId = merchantWithdraw.Id,
                //};
                //_redisService.SetMerchantMqPublish(redisMqDto);
                var checkOrder = _redisService.GetMerchantBillOrder(merchantWithdraw.MerchantCode, merchantWithdraw.WithDrawNo);
                if (checkOrder.IsNullOrEmpty())
                {
                    _redisService.SetMerchantBillOrder(merchantWithdraw.MerchantCode, merchantWithdraw.WithDrawNo);
                    //MerchantWithdrawMongoEntity merchantWithdrawMongoEntity = new MerchantWithdrawMongoEntity()
                    //{
                    //    MerchantCode = merchantWithdraw.MerchantCode,
                    //    MerchantId = merchantWithdraw.MerchantId,
                    //    WithDrawNo = merchantWithdraw.WithDrawNo,
                    //    Money = merchantWithdraw.Money,
                    //    ReviewTime = merchantWithdraw.ReviewTime,
                    //    CreationTime = merchantWithdraw.CreationTime,
                    //};
                    //await _merchantBillsHelper.AddMerchantWithdrawBillV2(merchantWithdraw.MerchantCode, merchantWithdrawMongoEntity);
                    var order = new MerchantWithdrawalPublishDto()
                    {
                        MerchantCode = merchantWithdraw.MerchantCode,
                        MerchantWithdrawId = merchantWithdraw.Id,
                        TriggerDate = DateTime.Now,
                        ProcessId = Guid.NewGuid().ToString()
                    };
                    await _kafkaProducer.ProduceAsync<MerchantWithdrawalPublishDto>(KafkaTopics.MerchantWithdrawal, merchantWithdraw.Id.ToString(), order);


                }
            }
         }

        public virtual async Task<FileDto> GetMerchantWithdrawsToExcel(GetAllMerchantWithdrawsForExcelInput input)
        {
            var user = await GetCurrentUserAsync();
            var merchantCode = user.Merchants.Select(r => r.MerchantCode);
            var context = await _dbContextProvider.GetDbContextAsync();

            var statusFilter = input.StatusFilter.HasValue
                        ? (MerchantWithdrawStatusEnum)input.StatusFilter
                        : default;

            var filteredMerchantWithdraws = context.MerchantWithdraws.Where(r => context.Merchants.Any(e => e.MerchantCode == r.MerchantCode && merchantCode.Contains(r.MerchantCode)))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.MerchantCode.Contains(input.Filter) || e.WithDrawNo.Contains(input.Filter) || e.BankName.Contains(input.Filter) || e.ReceivCard.Contains(input.Filter) || e.ReceivName.Contains(input.Filter) || e.ReviewMsg.Contains(input.Filter) || e.Remark.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.MerchantCodeFilter), e => e.MerchantCode.Contains(input.MerchantCodeFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.WithDrawNoFilter), e => e.WithDrawNo.Contains(input.WithDrawNoFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.BankNameFilter), e => e.BankName.Contains(input.BankNameFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ReceivCardFilter), e => e.ReceivCard.Contains(input.ReceivCardFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ReceivNameFilter), e => e.ReceivName.Contains(input.ReceivNameFilter))
                        .WhereIf(input.StatusFilter.HasValue && input.StatusFilter > -1, e => e.Status == statusFilter)
                        .WhereIf(input.MinReviewTimeFilter != null, e => e.ReviewTime >= input.MinReviewTimeFilter)
                        .WhereIf(input.MaxReviewTimeFilter != null, e => e.ReviewTime <= input.MaxReviewTimeFilter);

            var culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);
            var query = (from o in filteredMerchantWithdraws
                         select new GetMerchantWithdrawForViewDto()
                         {
                             MerchantWithdraw = new MerchantWithdrawDto
                             {
                                 MerchantCode = o.MerchantCode,
                                 WithDrawNo = o.WithDrawNo,
                                 Money = o.Money.ToString("C0", culInfo),
                                 BankName = o.BankName,
                                 ReceivCard = o.ReceivCard,
                                 ReceivName = o.ReceivName,
                                 Status = o.Status,
                                 ReviewTime = o.ReviewTime,
                                 Id = o.Id
                             }
                         });

            var merchantWithdrawListDtos = await query.ToListAsync();

            return _merchantWithdrawsExcelExporter.ExportToFile(merchantWithdrawListDtos);
        }

    }
}