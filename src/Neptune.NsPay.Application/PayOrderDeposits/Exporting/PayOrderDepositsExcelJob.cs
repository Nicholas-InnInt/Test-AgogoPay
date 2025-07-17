using Abp;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Microsoft.EntityFrameworkCore;
using Neptune.NsPay.AbpUserMerchants;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Common;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.Notifications;
using Neptune.NsPay.PayGroupMents;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PayOrderDeposits.Exporting;
using Neptune.NsPay.PayOrders.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neptune.NsPay.PayOrders.Exporting
{
    public class PayOrderDepositsExcelJob: AsyncBackgroundJob<PayOrderDepositsExcelJobArgs>, ITransientDependency
    {
        private readonly IAppNotifier _appNotifier;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IPayOrderDepositsExcelExporter _payOrderDepositsExcelExporter;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        private readonly UserManager _userManager;
        private readonly IRepository<PayMent> _payMentRepository;
        private readonly IRepository<PayGroupMent> _payGroupMentRepository;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IRepository<Merchant> _merchantRepository;
        private readonly IRepository<AbpUserMerchant> _abpUserMerchantRepository;
        public PayOrderDepositsExcelJob(
            IAppNotifier appNotifier,
            IUnitOfWorkManager unitOfWorkManager,
            IPayOrderDepositsExcelExporter payOrderDepositsExcelExporter,
            IPayOrderDepositsMongoService payOrderDepositsMongoService,
            UserManager userManager,
            IRepository<PayMent> payMentRepository,
            IRepository<PayGroupMent> payGroupMentRepository,
            IPayOrdersMongoService payOrdersMongoService,
            IRepository<AbpUserMerchant> abpUserMerchantRepository,
            IRepository<Merchant> merchantRepository
            )
        {
            _appNotifier = appNotifier;
            _unitOfWorkManager = unitOfWorkManager;
            _payOrderDepositsExcelExporter = payOrderDepositsExcelExporter;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
            _userManager = userManager;
            _payMentRepository = payMentRepository;
            _payGroupMentRepository = payGroupMentRepository;
            _payOrdersMongoService = payOrdersMongoService;
            _abpUserMerchantRepository = abpUserMerchantRepository;
            _merchantRepository = merchantRepository;
        }

        public override async Task ExecuteAsync(PayOrderDepositsExcelJobArgs args)
        {
            var user = args.User;
            var input = args.input;

            using (var uow = _unitOfWorkManager.Begin())
            {
                var orderLists = await GetPayOrderDepositExportAsync(user, input);

                var pageNumber = 1;
                var rowCount = 1000000;
                for (int i = 0; i < orderLists.Count; i += rowCount) {

                    int currentBatchSize = Math.Min(rowCount, orderLists.Count - i);

                    var file = _payOrderDepositsExcelExporter.ExportToFile(orderLists.GetRange(i, currentBatchSize), pageNumber);

                    await _appNotifier.ExporterPayDepositAsync(args.User, file.FileToken, file.FileType, file.FileName);

                    pageNumber ++;
                }

                await uow.CompleteAsync();
            }
        }



        private async Task<(string CountryCode, List<int> PayMentIds, List<string> Orders, List<string> PayOrderUserNos)> GetExcelFilter(GetAllPayOrderDepositsForExcelInput input, UserIdentifier userIdentifier)
        {
            // Determine the country code based on UtcTimeFilter
            var countryCode = input.UtcTimeFilter switch
            {
                "GMT8+" => CultureTimeHelper.TimeCodeZhCN,
                "GMT7+" => CultureTimeHelper.TimeCodeViVn,
                "GMT4-" => CultureTimeHelper.TimeCodeEST,
                _ => CultureTimeHelper.TimeCodeZhCN
            };


            var merchantIds = (await _abpUserMerchantRepository.GetAllListAsync(r => r.UserId == userIdentifier.UserId)).Select(r => r.MerchantId);

            var merchants = await _merchantRepository.GetAllListAsync(r => merchantIds.Contains(r.Id));

            List<int> payGroups = input.MerchantCodeFilter != null && !string.IsNullOrEmpty(input.MerchantCodeFilter)? merchants.Where(r => r.MerchantCode == input.MerchantCodeFilter)
                    .Select(r => r.PayGroupId)
                    .ToList()
                : merchants.Select(r => r.PayGroupId).ToList();


            var payMentIds = _payGroupMentRepository.GetAll()
                .Where(r => payGroups.Contains(r.GroupId))
                .Select(r => r.PayMentId)
                .ToList();

            var merchantCode = merchants.Select(r => r.MerchantCode).ToList();

            var payOrderIds = !string.IsNullOrEmpty(input.OrderNoFilter)
                ? await _payOrdersMongoService.GetPayOrdersByFilter(input.OrderNoFilter, "", merchantCode)
                : new List<string>();

            var payOrderUserNos = !string.IsNullOrEmpty(input.UserMemberFilter)
                ? await _payOrdersMongoService.GetPayOrdersByFilter("", input.UserMemberFilter, merchantCode)
                : new List<string>();

            return (countryCode, payMentIds, payOrderIds, payOrderUserNos);
        }

        public async Task<int> GetTotalRecordExcel(GetAllPayOrderDepositsForExcelInput input, UserIdentifier userIdentifier)
        {
            var (countryCode, payMentIds, orders, payOrderUserNos) = await GetExcelFilter(input, userIdentifier);
            return await _payOrderDepositsMongoService.GetTotalExcelRecordCount(input, payMentIds, orders, payOrderUserNos);
        }

        public async Task<List<GetPayOrderDepositForViewDto>> GetPayOrderDepositExportAsync(UserIdentifier userIdentifier, GetAllPayOrderDepositsForExcelInput input)
        {
            var user = userIdentifier;


            var merchantIds = (await _abpUserMerchantRepository.GetAllListAsync(r => r.UserId == userIdentifier.UserId)).Select(r => r.MerchantId);
            var merchants = await _merchantRepository.GetAllListAsync(r => merchantIds.Contains(r.Id));
            var merchantCode = merchants.Select(r => r.MerchantCode).ToList();

            var (countryCode, payMentIds, orders, payOrderUserNos) = await GetExcelFilter(input, userIdentifier);

            var filteredPayOrders = await _payOrderDepositsMongoService.GetAll(input, payMentIds, orders, payOrderUserNos);

            var results = new List<GetPayOrderDepositForViewDto>();


            var payments = await _payMentRepository.GetAll().ToListAsync();

            var users = await _userManager.Users.ToListAsync();

            IEnumerable<PayOrdersMongoEntity> payOrderList = null;
            var findType = 0;
            if (filteredPayOrders.Count() > 5000)
            {
                findType = 1;
                payOrderList = (await _payOrdersMongoService.GetPayOrderListForExcelAsync(merchantCode, input)).ToList();
            }

            foreach (var o in filteredPayOrders)
            {
                PayOrderDto payOrderDto = null;
                if (!o.OrderId.IsNullOrEmpty())
                {
                    if (findType == 0)
                    {
                        var payOrderData = await _payOrdersMongoService.GetById(o.OrderId);
                        if (payOrderData != null)
                        {
                            payOrderDto = new PayOrderDto()
                            {
                                MerchantCode = payOrderData.MerchantCode,
                                OrderNo = payOrderData.OrderNo,
                                TransactionNo = payOrderData.TransactionNo,
                                OrderType = payOrderData.OrderType,
                                OrderStatus = payOrderData.OrderStatus,
                                OrderMoney = payOrderData.OrderMoney.ToString("F0"),
                                Rate = payOrderData.Rate,
                                FeeMoney = payOrderData.FeeMoney.ToString("F0"),
                                OrderTime = CultureTimeHelper.GetCultureTimeInfo(payOrderData.OrderTime, CultureTimeHelper.TimeCodeViVn),
                                TransactionTime = CultureTimeHelper.GetCultureTimeInfo(payOrderData.TransactionTime, CultureTimeHelper.TimeCodeViVn),
                                CreationTime = CultureTimeHelper.GetCultureTimeInfo(payOrderData.CreationTime, CultureTimeHelper.TimeCodeViVn),
                                OrderMark = payOrderData.OrderMark,
                                OrderNumber = payOrderData.OrderNumber,
                                PlatformCode = payOrderData.PlatformCode,
                                ScCode = payOrderData.ScCode,
                                ScSeri = payOrderData.ScSeri,
                                ScoreStatus = payOrderData.ScoreStatus,
                                PayTypeStr = payOrderData.PayTypeStr,
                                ErrorMsg = payOrderData.ErrorMsg,
                                UserNo = payOrderData.UserNo,
                                PaymentChannel = payOrderData.PaymentChannel
                            };
                        }
                    }
                    else
                    {
                        if (payOrderList != null)
                        {
                            var payOrder = payOrderList.FirstOrDefault(r => r.ID == o.OrderId);
                            if (payOrder != null)
                            {
                                payOrderDto = new PayOrderDto()
                                {
                                    MerchantCode = payOrder.MerchantCode,
                                    OrderNo = payOrder.OrderNo,
                                    TransactionNo = payOrder.TransactionNo,
                                    OrderType = payOrder.OrderType,
                                    OrderStatus = payOrder.OrderStatus,
                                    OrderMoney = payOrder.OrderMoney.ToString("F0"),
                                    Rate = payOrder.Rate,
                                    FeeMoney = payOrder.FeeMoney.ToString("F0"),
                                    OrderTime = CultureTimeHelper.GetCultureTimeInfo(payOrder.OrderTime, CultureTimeHelper.TimeCodeViVn),
                                    TransactionTime = CultureTimeHelper.GetCultureTimeInfo(payOrder.TransactionTime, CultureTimeHelper.TimeCodeViVn),
                                    CreationTime = CultureTimeHelper.GetCultureTimeInfo(payOrder.CreationTime, CultureTimeHelper.TimeCodeViVn),
                                    OrderMark = payOrder.OrderMark,
                                    OrderNumber = payOrder.OrderNumber,
                                    PlatformCode = payOrder.PlatformCode,
                                    ScCode = payOrder.ScCode,
                                    ScSeri = payOrder.ScSeri,
                                    ScoreStatus = payOrder.ScoreStatus,
                                    PayTypeStr = payOrder.PayTypeStr,
                                    ErrorMsg = payOrder.ErrorMsg,
                                    UserNo = payOrder.UserNo,
                                    PaymentChannel = payOrder.PaymentChannel
                                };
                            }
                            else
                            {
                                var payOrderData = await _payOrdersMongoService.GetById(o.OrderId);
                                if (payOrderData != null)
                                {
                                    payOrderDto = new PayOrderDto()
                                    {
                                        MerchantCode = payOrderData.MerchantCode,
                                        OrderNo = payOrderData.OrderNo,
                                        TransactionNo = payOrderData.TransactionNo,
                                        OrderType = payOrderData.OrderType,
                                        OrderStatus = payOrderData.OrderStatus,
                                        OrderMoney = payOrderData.OrderMoney.ToString("F0"),
                                        Rate = payOrderData.Rate,
                                        FeeMoney = payOrderData.FeeMoney.ToString("F0"),
                                        OrderTime = CultureTimeHelper.GetCultureTimeInfo(payOrderData.OrderTime, CultureTimeHelper.TimeCodeViVn),
                                        TransactionTime = CultureTimeHelper.GetCultureTimeInfo(payOrderData.TransactionTime, CultureTimeHelper.TimeCodeViVn),
                                        CreationTime = CultureTimeHelper.GetCultureTimeInfo(payOrderData.CreationTime, CultureTimeHelper.TimeCodeViVn),
                                        OrderMark = payOrderData.OrderMark,
                                        OrderNumber = payOrderData.OrderNumber,
                                        PlatformCode = payOrderData.PlatformCode,
                                        ScCode = payOrderData.ScCode,
                                        ScSeri = payOrderData.ScSeri,
                                        ScoreStatus = payOrderData.ScoreStatus,
                                        PayTypeStr = payOrderData.PayTypeStr,
                                        ErrorMsg = payOrderData.ErrorMsg,
                                        UserNo = payOrderData.UserNo,
                                        PaymentChannel = payOrderData.PaymentChannel
                                    };
                                }
                            }
                        }
                    }
                }
                var operateUser = users.FirstOrDefault(r => r.Id == o.UserId);

                var res = new GetPayOrderDepositForViewDto()
                {
                    PayOrderDeposit = new PayOrderDepositDto
                    {
                        RefNo = o.RefNo,
                        PayType = (PayMentTypeEnum)o.PayType,
                        Type = o.Type,
                        Description = o.Description,

                        CreditAmount = o.CreditAmount.ToString("F0"),
                        DebitAmount = o.DebitAmount.ToString("F0"),
                        AvailableBalance = o.AvailableBalance.ToString("F0"),

                        CreditBank = o.CreditBank,
                        CreditAcctNo = o.CreditAcctNo,
                        CreditAcctName = o.CreditAcctName,

                        DebitBank = o.DebitBank,
                        DebitAcctNo = o.DebitAcctNo,
                        DebitAcctName = o.DebitAcctName,

                        TransactionTime = CultureTimeHelper.GetCultureTimeInfo(o.TransactionTime, CultureTimeHelper.TimeCodeViVn),
                        CreationTime = CultureTimeHelper.GetCultureTimeInfo(o.CreationTime, CultureTimeHelper.TimeCodeViVn),
                        // OrderId = o.OrderId,
                        MerchantId = o.MerchantId,
                        MerchantCode = o.MerchantCode,
                        UserName = o.UserName,
                        AccountNo = o.AccountNo,

                        RejectRemark = string.IsNullOrEmpty(o.RejectRemark) ? "" : o.RejectRemark,
                        OperateUser = operateUser?.UserName,
                        UserId = operateUser != null ? operateUser.Id : 0,

                        PayMentName = payments.FirstOrDefault(r => r.Id == o.PayMentId)?.Name,
                    },
                    PayOrder = payOrderDto
                };
                results.Add(res);
            }
            return results;
        }
    }
}
