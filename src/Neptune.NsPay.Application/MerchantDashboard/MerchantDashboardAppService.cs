using Neptune.NsPay.Authorization;
using Abp.Authorization;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.Localization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.MerchantWithdraws;
using Abp.Collections.Extensions;
using Neptune.NsPay.MerchantDashboard;
using Neptune.NsPay.MerchantDashboard.Dtos;
using MongoDB.Driver.Linq;

namespace Neptune.NsPay.WithdrawalOrders
{
    [AbpAuthorize(AppPermissions.Pages_MerchantDashboard)]
    public class MerchantDashboardAppService : NsPayAppServiceBase, IMerchantDashboardAppService
    {
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IMerchantBillsMongoService _merchantBillsMongoService;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IRepository<MerchantWithdraw, long> _merchantWithdrawRepository;
        private readonly IConfigurationRoot _appConfiguration;

        public MerchantDashboardAppService(
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            IMerchantBillsMongoService merchantBillsMongoService,
            IMerchantFundsMongoService merchantFundsMongoService,
            IPayOrdersMongoService payOrdersMongoService,
            IRepository<MerchantWithdraw, long> merchantWithdrawRepository,
            IAppConfigurationAccessor appConfigurationAccessor
            )
        {
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _merchantBillsMongoService = merchantBillsMongoService;
            _merchantFundsMongoService = merchantFundsMongoService;
            _payOrdersMongoService = payOrdersMongoService;
            _merchantWithdrawRepository = merchantWithdrawRepository;
            _appConfiguration = appConfigurationAccessor.Configuration;
        }

        public virtual async Task<MerchantDashboardPageResultDto<MerchantDashboardDto>> GetAll(GetAllMerchantDashboardInput input)
        {
            var result = new List<MerchantDashboardDto>();
            var user = await GetCurrentUserAsync();
            var merchantCodeFilterList = !input.MerchantCodeFilter.IsNullOrEmpty() ? input.MerchantCodeFilter.Split(",").ToList() : null;
            var userMerchantList = user.Merchants.WhereIf(merchantCodeFilterList != null, item => merchantCodeFilterList.Contains(item.MerchantCode)).ToList();
            var userMerchantIdList = userMerchantList.Select(r => r.Id).ToList();

            CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);

            if (userMerchantList.Count > 0) 
            { 
                result = userMerchantList.Select(s => new MerchantDashboardDto
                {
                                                                MerchantId = s.Id,
                                                                MerchantCode = s.MerchantCode,
                                                                MerchantName = s.Name,
                                                            }).ToList();

                var merchantFundList = await _merchantFundsMongoService.GetFundsByUserMerchant(userMerchantIdList);
                var withdrawalOrderByMerchant = await _withdrawalOrdersMongoService.GetAllforMerchantDashboardSummary(input, userMerchantIdList);
                var merchantWithdrawList = await _merchantWithdrawRepository.GetAllListAsync(r => 
                                                                                                userMerchantIdList.Contains(r.MerchantId) 
                                                                                                && r.CreationTime >= input.OrderCreationTimeStartDate 
                                                                                                && r.CreationTime <= input.OrderCreationTimeEndDate
                                                                                                && r.Status == MerchantWithdrawStatusEnum.Pass
                                                                                                && !r.IsDeleted);

                var payOrderByTypeList = await _payOrdersMongoService.GetAllforMerchantDashboardSummary(input, userMerchantIdList);


                if ( withdrawalOrderByMerchant.Count > 0 || merchantFundList.Count > 0 || merchantWithdrawList.Count > 0 || payOrderByTypeList.Count > 0)
                {
                    foreach (var data in result)
                    {
                        data.TotalMerchantFund = merchantFundList.Where(f => f.MerchantId == data.MerchantId).FirstOrDefault()?.Balance ?? 0;
                        data.TotalFrozenBalance = merchantFundList.Where(f => f.MerchantId == data.MerchantId).FirstOrDefault()?.FrozenBalance ?? 0;
                        var payOrderMerchantFee = payOrderByTypeList.Where(f => f.MerchantId == data.MerchantId).Sum(s => s.CashInFeeByType) ;
                        var withdrawalOrderMerchantFee = withdrawalOrderByMerchant.Where(f => f.MerchantId == data.MerchantId).Sum(s => s.CurrentWithdrawOrderMerchantFee);
                        data.CurrentMerchantFee = payOrderMerchantFee + withdrawalOrderMerchantFee;
                        data.CurrentPayOrderCashIn = payOrderByTypeList.Where(f => f.MerchantId == data.MerchantId).Sum(s => s.CashInByType);
                        data.TotalCurrentPayOrderCashInCount = payOrderByTypeList.Where(f => f.MerchantId == data.MerchantId).Sum(s => s.CashInCountByType);
                        data.CurrentPayOrderCashInByTypes = payOrderByTypeList.Where(f => f.MerchantId == data.MerchantId).ToList();
                        data.CurrentMerchantWithdraw = merchantWithdrawList.Where(f => f.MerchantId == data.MerchantId).Sum(s => s.Money);
                        data.TotalCurrentMerchantWithdrawCount = merchantWithdrawList.Where(f => f.MerchantId == data.MerchantId).Count();
                        data.CurrentWithdrawalOrder = withdrawalOrderByMerchant.Where(f => f.MerchantId == data.MerchantId).Sum(s => s.CurrentWithdrawalOrder);
                        data.TotalCurrentWithdrawalOrderCount = withdrawalOrderByMerchant.Where(f => f.MerchantId == data.MerchantId).Sum(s => s.TotalCurrentWithdrawalOrderCount);
                    }
                }

            }

            if (result.Count == 0) 
            {
                return new MerchantDashboardPageResultDto<MerchantDashboardDto>();
            
            }

            var totalMerchantCount = result.Count;
            var totalMerchantFund = result.Sum(s => s.TotalMerchantFund);
            var totalFrozenBalance = result.Sum(s => s.TotalFrozenBalance);
            var totalMerchantFee = result.Sum(s => s.CurrentMerchantFee);
            var totalPayOrderCashIn = result.Sum(s => s.CurrentPayOrderCashIn);
            var totalPayOrderCashInCount = result.Sum(s => s.TotalCurrentPayOrderCashInCount);
            var totalMerchantBillWithdraw = result.Sum(s => s.CurrentMerchantWithdraw);
            var totalMerchantBillWithdrawCount = result.Sum(s => s.TotalCurrentMerchantWithdrawCount);
            var totalWithdrawalOrder = result.Sum(s => s.CurrentWithdrawalOrder);
            var totalWithdrawalOrderCount = result.Sum(s => s.TotalCurrentWithdrawalOrderCount);


            if (!string.IsNullOrEmpty(input.OrderColumn))
            {
                switch (input.OrderColumn)
                {
                    case "merchantCode":
                        result = input.OrderDirection == "asc" ? result.OrderBy(x => x.MerchantCode).ToList() : result.OrderByDescending(x => x.MerchantCode).ToList();
                        break;
                    case "totalMerchantFund":
                        result = input.OrderDirection == "asc" ?
                            result.OrderBy(x => x.TotalMerchantFund).ToList() : result.OrderByDescending(x => x.TotalMerchantFund).ToList();
                        break;
                    case "totalMerchantFee":
                        result = input.OrderDirection == "asc" ?
                            result.OrderBy(x => x.CurrentMerchantFee).ToList() : result.OrderByDescending(x => x.CurrentMerchantFee).ToList();
                        break;
                    case "totalMerchantBillCashIn":
                        result = input.OrderDirection == "asc" ?
                            result.OrderBy(x => x.CurrentPayOrderCashIn).ToList() : result.OrderByDescending(x => x.CurrentPayOrderCashIn).ToList();
                        break;
                    case "totalMerchantBillWithdraw":
                        result = input.OrderDirection == "asc" ?
                            result.OrderBy(x => x.CurrentMerchantWithdraw).ToList() : result.OrderByDescending(x => x.CurrentMerchantWithdraw).ToList();
                        break;
                    case "totalWithdrawalOrder":
                        result = input.OrderDirection == "asc" ?
                            result.OrderBy(x => x.CurrentWithdrawalOrder).ToList() : result.OrderByDescending(x => x.CurrentWithdrawalOrder).ToList();
                        break;

                }
            }

            // Apply paging
            var skipCount = input.SkipCount;
            var maxResultCount = input.MaxResultCount;  
            var pagedResult = result.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

            return new MerchantDashboardPageResultDto<MerchantDashboardDto>
            {
                TotalCount= result.Count,
                Items = pagedResult,
                TotalMerchantCount = totalMerchantCount,
                TotalMerchantFund = totalMerchantFund.ToString("C0", culInfo),
                TotalFrozenBalance = totalFrozenBalance.ToString("C0", culInfo),
                TotalMerchantFee = totalMerchantFee.ToString("C0", culInfo),
                TotalPayOrderCashIn = totalPayOrderCashIn.ToString("C0", culInfo),
                TotalPayOrderCashInCount = totalPayOrderCashInCount,
                TotalMerchantBillWithdraw = totalMerchantBillWithdraw.ToString("C0", culInfo),
                TotalMerchantBillWithdrawCount = totalMerchantBillWithdrawCount,
                TotalWithdrawalOrder = totalWithdrawalOrder.ToString("C0", culInfo),
                TotalWithdrawalOrderCount = totalWithdrawalOrderCount
            };
        }

        public async Task<IList<GetOrderMerchantViewDto>> GetOrderMerchants()
        {
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants.Select(r => new GetOrderMerchantViewDto
            {
                MerchantCode = r.MerchantCode,
                MerchantName = r.Name
            }).ToList();
            return merchants;
        }

    }
}