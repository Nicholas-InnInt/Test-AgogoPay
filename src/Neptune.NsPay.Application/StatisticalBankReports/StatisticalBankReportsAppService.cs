using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Common;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.Localization;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayGroupMents;
using Neptune.NsPay.PayGroups;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.StatisticalBankReports.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Neptune.NsPay.StatisticalBankReports
{
    [AbpAuthorize(AppPermissions.Pages_StatisticalBankReports)]
    public class StatisticalBankReportsAppService : NsPayAppServiceBase, IStatisticalBankReportsAppService
    {
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        private readonly IRepository<PayMent> _payMentRepository;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IRedisService _redisService;
        private readonly IRepository<PayGroup> _payGroupRepository;
        private readonly IRepository<PayGroupMent> _payGroupMentRepository;

        public StatisticalBankReportsAppService(
            IPayOrdersMongoService payOrdersMongoService,
            IPayOrderDepositsMongoService payOrderDepositsMongoService,
            IRepository<PayMent> payMentRepository,
            IAppConfigurationAccessor appConfigurationAccessor,
            IRedisService redisService,
            IRepository<PayGroup> payGroupRepository,
            IRepository<PayGroupMent> payGroupMentRepository
            )
        {
            _payOrdersMongoService = payOrdersMongoService;
            _payMentRepository = payMentRepository;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
            _appConfiguration = appConfigurationAccessor.Configuration;
            _redisService = redisService;
            _payGroupRepository = payGroupRepository;
            _payGroupMentRepository = payGroupMentRepository;
        }

        public async Task<Dictionary<string, List<GetOrderBankDetailViewDto>>> GetStatisticalBankReport(GetStatisticalBankReportInput input)
        {
            //初始化时间
            if (!input.OrderCreationTimeStartDate.HasValue)
            {
                input.OrderCreationTimeStartDate = DateTime.Now.Date.AddDays(-1);
            }
            if (!input.OrderCreationTimeEndDate.HasValue)
            {
                input.OrderCreationTimeEndDate = DateTime.Now.Date.AddDays(+1);
            }
            if (input.OrderCreationTimeStartDate.HasValue)
            {
                input.OrderCreationTimeStartDate = CultureTimeHelper.GetCultureTimeInfoByGTM(input.OrderCreationTimeStartDate.Value, "GMT7+");
            }
            if (input.OrderCreationTimeEndDate.HasValue)
            {
                input.OrderCreationTimeEndDate = CultureTimeHelper.GetCultureTimeInfoByGTM(input.OrderCreationTimeEndDate.Value, "GMT7+");
            }
            CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);

            List<int> payMentIds = new List<int>();
            var payMents = _payMentRepository.GetAll();
            //只查询外部使用的卡
            var groupName = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.NsPayGroupName);
            var paygroup = await _payGroupRepository.FirstOrDefaultAsync(r => r.GroupName == groupName);
            if (paygroup != null)
            {
                payMentIds = _payGroupMentRepository.GetAll().Where(r => r.GroupId == paygroup.Id).Select(r => r.PayMentId).ToList();
                if (!string.IsNullOrEmpty(input.CardNumberFilter))
                {
                    var temps = payMents.Where(r => r.CardNumber.Contains(input.CardNumberFilter)).Select(r => r.Id).ToList();
                    if (temps.Count > 0)
                    {
                        payMentIds = payMentIds.Intersect(temps).ToList();
                    }
                }
            }

            var payOrders = await _payOrdersMongoService.GetPayOrderProjectionsByCardNumberDateRange(input.OrderCreationTimeStartDate.Value, input.OrderCreationTimeEndDate.Value, payMentIds);
            var deposits = await _payOrderDepositsMongoService.GetPayOrderDepositByPaymentIdDateRange(input.OrderCreationTimeStartDate.Value, input.OrderCreationTimeEndDate.Value, payMentIds);

            var bankResultList = new Dictionary<string, List<GetOrderBankDetailViewDto>>();

            foreach (var payOrderGrouped in payOrders.GroupBy(x => x.PayType))
            {
                var orderBankList = new List<GetOrderBankDetailViewDto>();

                foreach (var paymentSummaryGroup in payOrderGrouped.GroupBy(r => r.PayMentId))
                {
                    var payment = payMents.FirstOrDefault(x => x.Id == paymentSummaryGroup.Key);
                    var orderBank = GetOrderBankDetailView(input, deposits, paymentSummaryGroup.Key);

                    orderBank.OrderCount = paymentSummaryGroup.Count();
                    orderBank.OrderMoney = paymentSummaryGroup.Sum(x => x.OrderMoney).ToString("C0", culInfo);
                    orderBank.Name = payment?.Name;
                    orderBank.CardNumber = payment?.CardNumber;

                    orderBankList.Add(orderBank);
                }

                bankResultList.Add($"Bank{payOrderGrouped.Key.GetDisplayName()}", orderBankList);
            }

            return bankResultList;
        }

        protected GetOrderBankDetailViewDto GetOrderBankDetailView(GetStatisticalBankReportInput input, List<PayOrderDepositsMongoEntity> deposits, int payMentId)
        {
            CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);
            //收款列表记录
            var depositcCRDTCount = deposits.Where(r => r.Type == "CRDT" && r.PayMentId == payMentId).Count();
            var depositcCRDTSuccessCount = deposits.Where(r => r.Type == "CRDT" && r.PayMentId == payMentId && r.OrderId != null && r.OrderId != "-1").Count();
            var depositcCRDTSuccessMoney = deposits.Where(r => r.Type == "CRDT" && r.PayMentId == payMentId && r.OrderId != null && r.OrderId != "-1").Sum(r => r.CreditAmount);
            var depositcCRDTAssociatedCount = deposits.Where(r => r.Type == "CRDT" && r.PayMentId == payMentId && r.OrderId != null && r.OrderId != "-1" && r.UserId > 0).Count();
            var depositcCRDTAssociatedMoney = deposits.Where(r => r.Type == "CRDT" && r.PayMentId == payMentId && r.OrderId != null && r.OrderId != "-1" && r.UserId > 0).Sum(r => r.CreditAmount);
            var depositcCRDTRejectCount = deposits.Where(r => r.Type == "CRDT" && r.PayMentId == payMentId && r.OrderId == "-1").Count();
            var depositcCRDTRejectMoney = deposits.Where(r => r.Type == "CRDT" && r.PayMentId == payMentId && r.OrderId == "-1").Sum(r => r.CreditAmount);
            var depositcDBITMoney = deposits.Where(r => r.Type == "DBIT" && r.PayMentId == payMentId).Sum(r => r.DebitAmount);
            GetOrderBankDetailViewDto result = new GetOrderBankDetailViewDto();
            result.DepositcCRDTCount = depositcCRDTCount;
            result.DepositcCRDTSuccessCount = depositcCRDTSuccessCount;
            result.DepositcCRDTSuccessMoney = depositcCRDTSuccessMoney.ToString("C0", culInfo);
            result.DepositcCRDTAssociatedCount = depositcCRDTAssociatedCount;
            result.DepositcCRDTAssociatedMoney = depositcCRDTAssociatedMoney.ToString("C0", culInfo);
            result.DepositcCRDTRejectCount = depositcCRDTRejectCount;
            result.DepositcCRDTRejectMoney = depositcCRDTRejectMoney.ToString("C0", culInfo);
            result.DepositcDBITMoney = depositcDBITMoney.ToString("C0", culInfo);
            return result;
        }
    }
}