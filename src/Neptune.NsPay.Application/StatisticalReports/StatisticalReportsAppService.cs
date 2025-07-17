using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Common;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.Localization;
using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.RechargeOrders;
using Neptune.NsPay.StatisticalReports.Dto;
using Neptune.NsPay.WithdrawalOrders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.StatisticalReports
{
    [AbpAuthorize(AppPermissions.Pages_StatisticalReports)]
    public class StatisticalReportsAppService : NsPayAppServiceBase, IStatisticalReportsAppService
    {
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IRepository<MerchantWithdraw, long> _merchantWithdrawRepository;
        private readonly IConfigurationRoot _appConfiguration;

        public StatisticalReportsAppService(
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            IMerchantFundsMongoService merchantFundsMongoService,
            IPayOrdersMongoService payOrdersMongoService,
            IRepository<MerchantWithdraw, long> merchantWithdrawRepository,
            IAppConfigurationAccessor appConfigurationAccessor
            )
        {
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _merchantFundsMongoService = merchantFundsMongoService;
            _payOrdersMongoService = payOrdersMongoService;
            _merchantWithdrawRepository = merchantWithdrawRepository;
            _appConfiguration = appConfigurationAccessor.Configuration;
        }

        public async Task<GetStatisticalReportViewDto> GetStatisticalReport(GetStatisticalReportInput input)
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
            GetStatisticalReportViewDto result = new GetStatisticalReportViewDto();

            //获取订单列表
            var payOrders = await _payOrdersMongoService.GetPayOrderProjectionsByDateRange(input.OrderCreationTimeStartDate.Value, input.OrderCreationTimeEndDate.Value, input.MerchantCodeFilter);
            var tempPayOrder = payOrders.Where(r => r.OrderStatus == PayOrderOrderStatusEnum.Completed);
            result.PayOrderSumCount = payOrders.Count;
            result.PayOrderSuccessCount = tempPayOrder.Count();
            var bankSuccessList = tempPayOrder.Where(r => r.PaymentChannel == PaymentChannelEnum.OnlineBank || r.PaymentChannel == PaymentChannelEnum.ScanBank || r.PaymentChannel == PaymentChannelEnum.DirectBank || r.PaymentChannel== PaymentChannelEnum.NoPay);
            var bankSumList = payOrders.Where(r => r.PaymentChannel == PaymentChannelEnum.OnlineBank || r.PaymentChannel == PaymentChannelEnum.ScanBank || r.PaymentChannel == PaymentChannelEnum.DirectBank || r.PaymentChannel == PaymentChannelEnum.NoPay);
            var bankSuccessCount = bankSuccessList.Count();
            var bankSumCount = bankSumList.Count();
            result.payOrderBankFeeMoney = bankSuccessList.Sum(r => r.FeeMoney).ToString("C0", culInfo);
            result.PayOrderSuccessBankMoney = bankSuccessList.Sum(r => r.OrderMoney).ToString("C0", culInfo);
            if (bankSumCount > 0)
            {
                result.PayOrderSuccessBankRate = (bankSuccessCount / Convert.ToDecimal(bankSumCount)).ToString("P2");
            }
            else
            {
                result.PayOrderSuccessBankRate = "0.00%";
            }
            var scSuccessList = tempPayOrder.Where(r => r.PaymentChannel == PaymentChannelEnum.Sc);
            var scSumList = payOrders.Where(r => r.PaymentChannel == PaymentChannelEnum.Sc);
            var scSuccessCount = scSuccessList.Count();
            var scSumCount = scSumList.Count();
            result.PayOrderScFeeMoney = scSuccessList.Sum(r => r.FeeMoney).ToString("C0", culInfo);
            result.PayOrderSuccessScMoney = scSuccessList.Sum(r => r.OrderMoney).ToString("C0", culInfo);
            if (scSuccessCount > 0)
            {
                result.PayOrderSuccessScRate = (scSuccessCount / Convert.ToDecimal(scSumCount)).ToString("P2");
            }
            else
            {
                result.PayOrderSuccessScRate = "0.00%";
            }

            //获取出款列表
            var withdrawOrder = await _withdrawalOrdersMongoService.GetWithdrawOrderByDateRange(input.OrderCreationTimeStartDate.Value, input.OrderCreationTimeEndDate.Value, input.MerchantCodeFilter);
            var tempWithdrawOrder = withdrawOrder.Where(r => r.OrderStatus == WithdrawalOrderStatusEnum.Success);
            result.WithdrawOrderSumCount = withdrawOrder.Count;
            result.WithdrawOrderSuccessCount = tempWithdrawOrder.Count();
            result.WithdrawOrderFeeMoney = tempWithdrawOrder.Sum(r => r.FeeMoney).ToString("C0", culInfo);
            result.WithdrawOrderSuccessMoney = tempWithdrawOrder.Sum(r => r.OrderMoney).ToString("C0", culInfo);
            if (result.WithdrawOrderSumCount > 0)
            {
                result.WithdrawOrderSuccessRate = (result.WithdrawOrderSuccessCount / Convert.ToDecimal(result.WithdrawOrderSumCount)).ToString("P2");
            }
            else
            {
                result.WithdrawOrderSuccessRate = "0.00%";
            }
            //获取商户出款
            List<MerchantWithdraw> merchantWithdraws = new List<MerchantWithdraw>();
            if (!string.IsNullOrEmpty(input.MerchantCodeFilter))
            {
                merchantWithdraws = await _merchantWithdrawRepository.GetAllListAsync(r => r.Status == MerchantWithdrawStatusEnum.Pass && r.MerchantCode == input.MerchantCodeFilter && r.CreationTime >= input.OrderCreationTimeStartDate && r.CreationTime <= input.OrderCreationTimeEndDate);
            }
            else
            {
                merchantWithdraws = await _merchantWithdrawRepository.GetAllListAsync(r => r.Status == MerchantWithdrawStatusEnum.Pass && r.CreationTime >= input.OrderCreationTimeStartDate && r.CreationTime <= input.OrderCreationTimeEndDate);
            }
            result.MerchantWithdrawMoney = merchantWithdraws.Sum(r => r.Money).ToString("C0", culInfo);
            if (!string.IsNullOrEmpty(input.MerchantCodeFilter))
            {
                var merchantFunds = await _merchantFundsMongoService.GetFundsByMerchantCode(input.MerchantCodeFilter);
                if(merchantFunds != null)
                {
                    result.MerchantSumBalance = merchantFunds.Balance.ToString("C0", culInfo);
                }
            }
            else
            {
                var merchantFunds = await _merchantFundsMongoService.GetFundsAll();
                if (merchantFunds != null)
                {
                    result.MerchantSumBalance = merchantFunds.Sum(result => result.Balance).ToString("C0", culInfo);
                    result.MerchantFrozenBalance = merchantFunds.Sum(result => result.FrozenBalance).ToString("C0", culInfo);
                }
            }

            return result;
        }


        public async Task<IList<GetOrderMerchantViewDto>> GetMerchants()
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
