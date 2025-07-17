using Abp;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.ObjectMapping;
using AutoMapper.Internal.Mappers;
using Neptune.NsPay.AbpUserMerchants;
using Neptune.NsPay.Common;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.Notifications;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayMents.Dtos;
using Neptune.NsPay.PayOrders.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neptune.NsPay.PayOrders.Exporting
{
    public class PayOrderExcelJob : AsyncBackgroundJob<PayOrderExcelJobArgs>, ITransientDependency
    {
        private readonly IAppNotifier _appNotifier;
        private readonly Abp.ObjectMapping.IObjectMapper _objectMapper;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IPayOrdersExcelExporter _payOrdersExcelExporter;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IRepository<Merchant> _merchantRepository;
        private readonly IRepository<AbpUserMerchant> _abpUserMerchantRepository;
        private readonly IRepository<PayMent> _payMentRepository;
        public PayOrderExcelJob(
            IRepository<Merchant> merchantRepository,
            IAppNotifier appNotifier,
            Abp.ObjectMapping.IObjectMapper objectMapper,
            IUnitOfWorkManager unitOfWorkManager,
            IPayOrdersExcelExporter payOrdersExcelExporter,
            IPayOrdersMongoService payOrdersMongoService,
            IRepository<AbpUserMerchant> abpUserMerchantRepository,
            IRepository<PayMent> payMentRepository
            )
        {
            _merchantRepository = merchantRepository;
            _appNotifier = appNotifier;
            _objectMapper = objectMapper;
            _unitOfWorkManager = unitOfWorkManager;
            _payOrdersExcelExporter = payOrdersExcelExporter;
            _payOrdersMongoService = payOrdersMongoService;
            _abpUserMerchantRepository = abpUserMerchantRepository;
            _payMentRepository = payMentRepository;
        }

        public override async Task ExecuteAsync(PayOrderExcelJobArgs args)
        {
            var user = args.User;
            var input = args.input;

            using (var uow = _unitOfWorkManager.Begin())
            {
                var orderLists = await GetPayOrderExportAsync(user, input);

                var pageNumber = 1;
                var rowCount = 1000000;
                for (int i = 0; i < orderLists.Count; i += rowCount)
                {

                    int currentBatchSize = Math.Min(rowCount, orderLists.Count - i);

                    var file = _payOrdersExcelExporter.ExportToFile(orderLists.GetRange(i, currentBatchSize), pageNumber, args.UserType);

                    await _appNotifier.ExporterPayOrderAsync(args.User, file.FileToken, file.FileType, file.FileName);

                    pageNumber++;
                }

                await uow.CompleteAsync();
            }
        }

        public async Task<List<GetPayOrderForViewDto>> GetPayOrderExportAsync(UserIdentifier userIdentifier, GetAllPayOrdersForExcelInput input)
        {
            var countryCode = "";

            /* ND-386 EXPORT PAYORDER ONLY IN GMT+7 */

            //if (input.UtcTimeFilter == "GMT8+")
            //{
            //    countryCode = CultureTimeHelper.TimeCodeZhCN;
            //}
            //if (input.UtcTimeFilter == "GMT7+")
            //{
            //    countryCode = CultureTimeHelper.TimeCodeViVn;
            //}
            //if (input.UtcTimeFilter == "GMT4-")
            //{
            //    countryCode = CultureTimeHelper.TimeCodeEST;
            //}
            //if (countryCode.IsNullOrEmpty())
            //{
            //    countryCode = CultureTimeHelper.TimeCodeZhCN;
            //}

            if (input.userMerchantIds == null)
            {
                var merchantIds = (await _abpUserMerchantRepository.GetAllListAsync(r => r.UserId == userIdentifier.UserId)).Select(r => r.MerchantId);
                var merchants = await _merchantRepository.GetAllListAsync(r => merchantIds.Contains(r.Id));
                input.userMerchantIds = merchants.Select(r => r.Id).ToList();
            }

            var filteredPayOrders = await _payOrdersMongoService.GetAll(input, input.userMerchantIds);
            var uniquePaymentId = filteredPayOrders.Select(x => x.PayMentId).Distinct();
            var payMentsDict = _payMentRepository.GetAll().Where(x => uniquePaymentId.Contains(x.Id)).ToDictionary(x => x.Id, x => x);

            var results = new List<GetPayOrderForViewDto>();

            results.AddRange(filteredPayOrders.Select(o => new GetPayOrderForViewDto()
            {
                PayOrder = new PayOrderDto
                {
                    MerchantCode = o.MerchantCode,
                    OrderNo = o.OrderNo,
                    TransactionNo = o.TransactionNo,
                    OrderType = o.OrderType,
                    OrderStatus = o.OrderStatus,
                    OrderMoney = o.OrderMoney.ToString("F0"),
                    Rate = o.Rate,
                    FeeMoney = o.FeeMoney.ToString("F0"),
                    OrderTime = CultureTimeHelper.GetCultureTimeInfo(o.OrderTime, CultureTimeHelper.TimeCodeViVn),
                    TransactionTime = CultureTimeHelper.GetCultureTimeInfo(o.TransactionTime, CultureTimeHelper.TimeCodeViVn),
                    CreationTime = CultureTimeHelper.GetCultureTimeInfo(o.CreationTime, CultureTimeHelper.TimeCodeViVn),
                    OrderMark = o.OrderMark,
                    OrderNumber = o.OrderNumber,
                    ScCode = o.ScCode,
                    ScSeri = o.ScSeri,
                    ScoreStatus = o.ScoreStatus,
                    PayTypeStr = o.PayType.ToString(),
                    Id = o.ID,
                },
                PayMent = _objectMapper.Map<PayMentDto>(payMentsDict.ContainsKey(o.PayMentId) ? payMentsDict[o.PayMentId] : null)
            }));

            return results;
        }


        public async Task<int> GetTotalRecordExcel(GetAllPayOrdersForExcelInput input,UserIdentifier userIdentifier)
        {
            return await _payOrdersMongoService.GetTotalExcelRecordCount(input, input.userMerchantIds);
        }


 
    }
}
