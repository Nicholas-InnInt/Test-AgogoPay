using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.BankBalance.Dto;
using Neptune.NsPay.Common;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.EntityFrameworkCore;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.Localization;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayGroupMents;
using Neptune.NsPay.PayGroups;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.RedisExtensions;
using PayPalCheckoutSdk.Orders;
using Stripe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Neptune.NsPay.BankBalance
{
    [AbpAuthorize(AppPermissions.Pages_BankBalances)]
    public class BankBalancesAppService : NsPayAppServiceBase, IBankBalancesAppService
    {
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        private readonly IRepository<PayMent> _payMentRepository;
        private readonly IRepository<PayGroupMent> _payGroupMentRepository;
        private readonly IRepository<PayGroup> _payGroupRepository;
        private readonly IRedisService _redisService;
        private readonly IBankStateHelper _bankStateHelper;
        private readonly IBankBalanceService _bankBalanceService;
        private readonly IConfigurationRoot _appConfiguration;
        public BankBalancesAppService(
            IPayOrderDepositsMongoService payOrderDepositsMongoService,
            IRepository<PayMent> payMentRepository,
            IRepository<PayGroupMent> payGroupMentRepository,
            IRepository<PayGroup> payGroupRepository,
            IRedisService redisService,
            IBankStateHelper bankStateHelper,
            IBankBalanceService bankBalanceService,
            IAppConfigurationAccessor appConfigurationAccessor)
        {
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
            _payMentRepository = payMentRepository;
            _payGroupMentRepository = payGroupMentRepository;
            _payGroupRepository = payGroupRepository;
            _redisService = redisService;
            _bankStateHelper = bankStateHelper;
            _bankBalanceService = bankBalanceService;
            _appConfiguration = appConfigurationAccessor.Configuration;
        }

        public async Task<List<GetAllBankBalancesViewDto>> GetBankBalance(GetAllBankBalanceInput input)
        {
            var user = await GetCurrentUserAsync();
            var payments = await _payMentRepository.GetAll().Where(r => r.IsDeleted == false).ToListAsync();
            var paygroups = await _payGroupMentRepository.GetAll().ToListAsync();
            var groups = await _payGroupRepository.GetAll().ToListAsync();

            var payGroups = user.Merchants.Select(r => r.PayGroupId).ToList();
            var payMentIds = paygroups.Where(r => payGroups.Contains(r.GroupId)).Select(r => r.PayMentId).ToList();
            GetAllPayOrderDepositsForExcelInput depositsForExcelInput = new GetAllPayOrderDepositsForExcelInput()
            {
                MinTransactionTimeFilter = input.MinTransactionTimeFilter,
                MaxTransactionTimeFilter = input.MaxTransactionTimeFilter,
                UserNameFilter = input.Filter
            };            
            var filteredPayOrders = await _payOrderDepositsMongoService.GetAll(depositsForExcelInput, payMentIds, new List<string>(), new List<string>());

            CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);
            List<GetAllBankBalancesViewDto> lists = new List<GetAllBankBalancesViewDto>();
            var bankOrders = filteredPayOrders.GroupBy(r => r.UserName);
            foreach (var order in bankOrders)
            {
                var key = order.Key;
                var payTypes = order.GroupBy(r => r.PayType);
                foreach (var pay in payTypes)
                {
                    var payType = pay.Key;
                    var balance = 0M;
                    var lastTime = DateTime.Now;
                    var info = pay.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                    if (info != null)
                    {
                        balance = info.AvailableBalance;
                        lastTime = info.CreationTime;
                        var payment = payments.FirstOrDefault(r => r.Phone == key && r.Type == (PayMentTypeEnum)payType);
                        var status = false;
                        var merchantName = "";
                        if (payment != null)
                        {
                            var islogin = _bankStateHelper.GetPayState(payment.Phone, payment.Type);
                            var payMentbalance = _redisService.GetBalance(payment.Id, payment.Type);
                            balance = await _bankBalanceService.GetBalance(payment.Id, payMentbalance.Balance, payMentbalance.Balance2, islogin);

                            //判断是否超过设定余额
                            if (balance > payment.BalanceLimitMoney && payment.BalanceLimitMoney > 0)
                            {
                                status = true;
                            }

                            var payGroupMent = paygroups.Where(r => payGroups.Contains(r.GroupId)).FirstOrDefault(r => r.PayMentId == payment.Id);

                            if (payGroupMent != null)
                            {
                                var group = groups.FirstOrDefault(r => r.Id == payGroupMent.GroupId);
                                if (group != null)
                                {
                                    merchantName = group.GroupName;
                                }
                            }
                        }
                        GetAllBankBalancesViewDto viewDto = new GetAllBankBalancesViewDto()
                        {
                            PayName = payment?.Name,
                            MerchantName = merchantName,
                            UserName = key,
                            PayType = (PayMentTypeEnum)payType,
                            LastTime = CultureTimeHelper.GetCultureTimeInfo(lastTime, CultureTimeHelper.TimeCodeViVn),
                            Balance = balance,
                            BalanceStr = balance.ToString("C0", culInfo),
                            Status = status
                        };
                        lists.Add(viewDto);
                    }
                }
            }
            lists = lists.OrderByDescending(r => r.Balance).ToList();
            return lists;
        }

    }
}
