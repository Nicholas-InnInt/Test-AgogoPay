using Abp.Auditing;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore;
using Abp.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Common;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.EntityFrameworkCore;
using Neptune.NsPay.Localization;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PayOrders.Dtos;
using Neptune.NsPay.Tenants.Dashboard.Dto;
using Neptune.NsPay.WithdrawalOrders;
using Neptune.NsPay.WithdrawalOrders.Dtos;
using NPOI.SS.Formula.Functions;
using NUglify.JavaScript.Syntax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Neptune.NsPay.Tenants.Dashboard
{
    [DisableAuditing]
    [AbpAuthorize(AppPermissions.Pages_Tenant_Dashboard)]
    public class TenantDashboardAppService : NsPayAppServiceBase, ITenantDashboardAppService
    {
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IDbContextProvider<NsPayDbContext> _dbContextProvider;
        public TenantDashboardAppService(
            IMerchantFundsMongoService merchantFundsMongoService,
            IPayOrdersMongoService payOrdersMongoService,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            IAppConfigurationAccessor appConfigurationAccessor,
            IDbContextProvider<NsPayDbContext> dbContextProvider) 
        {
            _merchantFundsMongoService = merchantFundsMongoService;
            _payOrdersMongoService = payOrdersMongoService;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _appConfiguration = appConfigurationAccessor.Configuration;
            _dbContextProvider = dbContextProvider;
        }
        public GetMemberActivityOutput GetMemberActivity()
        {
            return new GetMemberActivityOutput
            (
                DashboardRandomDataGenerator.GenerateMemberActivities()
            );
        }

        public GetDashboardDataOutput GetDashboardData(GetDashboardDataInput input)
        {
            var output = new GetDashboardDataOutput
            {
                TotalProfit = DashboardRandomDataGenerator.GetRandomInt(5000, 9000),
                NewFeedbacks = DashboardRandomDataGenerator.GetRandomInt(1000, 5000),
                NewOrders = DashboardRandomDataGenerator.GetRandomInt(100, 900),
                NewUsers = DashboardRandomDataGenerator.GetRandomInt(50, 500),
                SalesSummary = DashboardRandomDataGenerator.GenerateSalesSummaryData(input.SalesSummaryDatePeriod),
                Expenses = DashboardRandomDataGenerator.GetRandomInt(5000, 10000),
                Growth = DashboardRandomDataGenerator.GetRandomInt(5000, 10000),
                Revenue = DashboardRandomDataGenerator.GetRandomInt(1000, 9000),
                TotalSales = DashboardRandomDataGenerator.GetRandomInt(10000, 90000),
                TransactionPercent = DashboardRandomDataGenerator.GetRandomInt(10, 100),
                NewVisitPercent = DashboardRandomDataGenerator.GetRandomInt(10, 100),
                BouncePercent = DashboardRandomDataGenerator.GetRandomInt(10, 100),
                DailySales = DashboardRandomDataGenerator.GetRandomArray(30, 10, 50),
                ProfitShares = DashboardRandomDataGenerator.GetRandomPercentageArray(3)
            };

            return output;
        }

        public async Task<GetTopStatsOutput> GetTopStats(DateTime startDate, DateTime endDate)
        {
            decimal totalMerchantFund = 0;
            int totalMerchantCount = 0;
            decimal totalPayOrderFee = 0;
            decimal totalPayOrderMoney = 0;
            int totalPayOrderCount = 0;
            decimal totalMerchantWithdraw = 0;
            int totalMerchantWithdrawCount = 0;
            decimal totalTransferMoney = 0;
            int totalTransferCount = 0;

            var user = await GetCurrentUserAsync();
            CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);
            var userMerchantIdList = user.Merchants.Select(r => r.Id).ToList();

            var merchantFundList = await _merchantFundsMongoService.GetFundsByUserMerchant(userMerchantIdList);
            var merchantFundGroupByMerchantFirst = merchantFundList.GroupBy(g => g.MerchantId)
                                                                    .Select(s => new { 
                                                                        merchantId = s.Key, 
                                                                        Balance = s.FirstOrDefault()?.Balance ?? 0 
                                                                    });
            totalMerchantFund = merchantFundGroupByMerchantFirst.Sum(s => s.Balance);
            totalMerchantCount = merchantFundGroupByMerchantFirst.Count();

            //获取订单总金额
            GetAllPayOrdersForExcelInput getAllPayOrdersForExcelInput = new GetAllPayOrdersForExcelInput()
            {
                OrderStatusFilter = (int)PayOrderOrderStatusEnum.Completed,
                OrderCreationTimeStartDate = startDate,
                OrderCreationTimeEndDate = endDate,
                UtcTimeFilter = "GMT7+"
            };
            var payOrders = await _payOrdersMongoService.GetAll(getAllPayOrdersForExcelInput, userMerchantIdList);

            //获取代付
            GetAllWithdrawalOrdersForExcelInput getAllWithdrawalOrdersForExcelInput = new GetAllWithdrawalOrdersForExcelInput()
            {
                OrderStatusFilter= (int)WithdrawalOrderStatusEnum.Success,
                OrderCreationTimeStartDate= startDate,
                OrderCreationTimeEndDate= endDate,
                UtcTimeFilter = "GMT7+"
            };
            var transferOrders = await _withdrawalOrdersMongoService.GetAll(getAllWithdrawalOrdersForExcelInput,userMerchantIdList, new List<int>());


            //获取提现
            startDate = CultureTimeHelper.GetCultureTimeInfoByGTM(startDate, "GMT7+");
            endDate = CultureTimeHelper.GetCultureTimeInfoByGTM(endDate, "GMT7+");
            var context = await _dbContextProvider.GetDbContextAsync();
            var merchantWithdraws = context.MerchantWithdraws.Where(r => context.Merchants.Any(e => userMerchantIdList.Contains(r.MerchantId))
            && r.Status == MerchantWithdrawStatusEnum.Pass && r.CreationTime >= startDate && r.CreationTime <= endDate);

            return new GetTopStatsOutput
            {
                TotalMerchantFund = totalMerchantFund.ToString("C0", culInfo),
                TotalMerchantCount = totalMerchantCount,
                TotalPayOrderFee = payOrders.Sum(r => r.FeeMoney).ToString("C0", culInfo),
                TotalPayOrderMoney = payOrders.Sum(r => r.OrderMoney).ToString("C0", culInfo),
                TotalPayOrderCount = payOrders.Count(),
                TotalMerchantWithdraw = merchantWithdraws.Sum(r => r.Money).ToString("C0", culInfo),
                TotalMerchantWithdrawCount = merchantWithdraws.Count(),
                TotalTransferMoney = transferOrders.Sum(r => r.OrderMoney).ToString("C0", culInfo),
                TotalTransferCount = transferOrders.Count()
            };
        }

        public GetProfitShareOutput GetProfitShare()
        {
            return new GetProfitShareOutput
            {
                ProfitShares = DashboardRandomDataGenerator.GetRandomPercentageArray(3)
            };
        }

        public GetDailySalesOutput GetDailySales()
        {
            return new GetDailySalesOutput
            {
                DailySales = DashboardRandomDataGenerator.GetRandomArray(30, 10, 50)
            };
        }

        public GetSalesSummaryOutput GetSalesSummary(GetSalesSummaryInput input)
        {
            var salesSummary = DashboardRandomDataGenerator.GenerateSalesSummaryData(input.SalesSummaryDatePeriod);
            return new GetSalesSummaryOutput(salesSummary)
            {
                Expenses = DashboardRandomDataGenerator.GetRandomInt(0, 3000),
                Growth = DashboardRandomDataGenerator.GetRandomInt(0, 3000),
                Revenue = DashboardRandomDataGenerator.GetRandomInt(0, 3000),
                TotalSales = DashboardRandomDataGenerator.GetRandomInt(0, 3000)
            };
        }

        public async Task<GetMerchantBillsSummaryOutput> GetMerchantBillsSummary(GetMerchantBillsSummaryInput input)
        {
            var merchantBillsSummary = new List<MerchantBillsSummaryData>();
            //var user = await GetCurrentUserAsync();
            //var userMerchantIdList = user.Merchants.Select(r => r.Id).ToList();
            //if (input.startDate.HasValue)
            //{
            //    input.startDate = CultureTimeHelper.GetCultureTimeInfoByGTM(input.startDate.Value, "GMT7+");
            //}
            //if (input.endDate.HasValue)
            //{
            //    input.endDate = CultureTimeHelper.GetCultureTimeInfoByGTM(input.endDate.Value, "GMT7+");
            //}
            //var merchantBillList = await _merchantBillsMongoService.GetMerchantBillByUserMerchantDateRange(userMerchantIdList, input.startDate.Value, input.endDate.Value);
     

            //if (input.MerchantBillsSummaryDatePeriod == MerchantBillsSummaryDatePeriod.Daily)
            //{
            //    var completeDates = Enumerable.Range(0, (input.endDate.Value - input.startDate.Value).Days + 1).Select(s => input.startDate.Value.AddDays(s).ToString("yyyy-MM-dd")).ToList();

            //    merchantBillsSummary = (from date in completeDates
            //                            join merchantBill in merchantBillList
            //                            on date equals merchantBill.Date into dateGroup
            //                            from merchantBill in dateGroup.DefaultIfEmpty()
            //                            group merchantBill by new { Date = date, BillType = merchantBill?.BillType } into grouped
            //                            select new
            //                            {
            //                                Date = grouped.Key.Date,
            //                                BillType = grouped.Key.BillType,
            //                                TotalMoney = grouped.Sum(t => t?.TotalMoney ?? 0)
            //                            }
            //                            into finalGroup
            //                            group finalGroup by finalGroup.Date into dayGroup
            //                            select new MerchantBillsSummaryData(
            //                                dayGroup.Key.ToString(),
            //                                dayGroup.Where(f => f.BillType == MerchantBillTypeEnum.OrderIn && f.Date == dayGroup.Key).FirstOrDefault()?.TotalMoney.ToLong() ?? 0,
            //                                dayGroup.Where(f => f.BillType == MerchantBillTypeEnum.Withdraw && f.Date == dayGroup.Key).FirstOrDefault()?.TotalMoney.ToLong() ?? 0)
            //                            ).ToList();

            //}
            //else if (input.MerchantBillsSummaryDatePeriod == MerchantBillsSummaryDatePeriod.Weekly)
            //{
            //    var completeWeeks = Enumerable.Range(0, (input.endDate.Value - input.startDate.Value).Days / 7 + 1)
            //                                         .Select(offset => input.startDate.Value.AddDays(offset * 7).StartOfWeek(DayOfWeek.Monday))
            //                                         .Distinct()
            //                                         .ToList();

            //    merchantBillsSummary = (from week in completeWeeks
            //                            join merchantBill in merchantBillList
            //                            on new { Week = week }
            //                            equals new { Week = DateTime.Parse(merchantBill.Date).StartOfWeek(DayOfWeek.Monday) } into weekGroup
            //                            from merchantBill in weekGroup.DefaultIfEmpty()
            //                            group merchantBill by new { Week = week.StartOfWeek(DayOfWeek.Monday), merchantBill?.BillType } into grouped
            //                            select new
            //                            {
            //                                WeekStart = grouped.Key.Week,
            //                                BillType = grouped.Key.BillType,
            //                                TotalMoney = grouped.Sum(t => t?.TotalMoney ?? 0)
            //                            } into finalGroup
            //                            group finalGroup by finalGroup.WeekStart into weekGroup
            //                            select new MerchantBillsSummaryData(
            //                                weekGroup.Key.ToString("yyyy-MM-dd"),
            //                                weekGroup.Where(f => f.BillType == MerchantBillTypeEnum.OrderIn && f.WeekStart == weekGroup.Key).FirstOrDefault()?.TotalMoney.ToLong() ?? 0,
            //                                weekGroup.Where(f => f.BillType == MerchantBillTypeEnum.Withdraw && f.WeekStart == weekGroup.Key).FirstOrDefault()?.TotalMoney.ToLong() ?? 0)
            //                            ).ToList();
            //}

            return new GetMerchantBillsSummaryOutput(merchantBillsSummary);
        }

        public GetRegionalStatsOutput GetRegionalStats()
        {
            return new GetRegionalStatsOutput(
                DashboardRandomDataGenerator.GenerateRegionalStat()
            );
        }

        public GetGeneralStatsOutput GetGeneralStats()
        {
            return new GetGeneralStatsOutput
            {
                TransactionPercent = DashboardRandomDataGenerator.GetRandomInt(10, 100),
                NewVisitPercent = DashboardRandomDataGenerator.GetRandomInt(10, 100),
                BouncePercent = DashboardRandomDataGenerator.GetRandomInt(10, 100)
            };
        }
    }
}