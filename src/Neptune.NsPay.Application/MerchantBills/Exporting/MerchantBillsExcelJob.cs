using Abp;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Json;
using Neptune.NsPay.AbpUserMerchants;
using Neptune.NsPay.Common;
using Neptune.NsPay.MerchantBills.Dtos;
using Neptune.NsPay.MerchantBills.Exporting;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.Notifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neptune.NsPay.PayOrders.Exporting
{
    public class MerchantBillsExcelJob: AsyncBackgroundJob<MerchantBillsExcelJobArgs>, ITransientDependency
    {
        private readonly IAppNotifier _appNotifier;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IMerchantBillsMongoService _merchantBillsMongoService;
        private readonly IMerchantBillsExcelExporter _merchantBillsExcelExporter;
        private readonly IRepository<Merchant> _merchantRepository;
        private readonly IRepository<AbpUserMerchant> _abpUserMerchantRepository;
        public MerchantBillsExcelJob(
            IRepository<Merchant> merchantRepository,
            IAppNotifier appNotifier,
            IUnitOfWorkManager unitOfWorkManager,
            IMerchantBillsMongoService merchantBillsMongoService, 
            IMerchantBillsExcelExporter merchantBillsExcelExporter,
            IRepository<AbpUserMerchant> abpUserMerchantRepository
            )
        {
            _merchantRepository = merchantRepository;
            _appNotifier = appNotifier;
            _unitOfWorkManager = unitOfWorkManager;
            _merchantBillsMongoService = merchantBillsMongoService;
            _merchantBillsExcelExporter = merchantBillsExcelExporter;
            _abpUserMerchantRepository = abpUserMerchantRepository;
        }

        public override async Task ExecuteAsync(MerchantBillsExcelJobArgs args)
        {
            var user = args.User;
            var input = args.input;

            using (var uow = _unitOfWorkManager.Begin())
            {
                var orderLists = await GetMerchantBillsExportAsync(user, input);

                var pageNumber = 1;
                var rowCount = 1000000;

                for (int i = 0; i < orderLists.Count; i += rowCount) {

                    int currentBatchSize = Math.Min(rowCount, orderLists.Count - i);

                    var file = _merchantBillsExcelExporter.ExportToFile(orderLists.GetRange(i, currentBatchSize), pageNumber);

                    await _appNotifier.ExporterMerchantBillsAsync(args.User, file.FileToken, file.FileType, file.FileName);

                    pageNumber ++;
                }

                await uow.CompleteAsync();
            }
        }

        public async Task<List<GetMerchantBillForViewDto>> GetMerchantBillsExportAsync(UserIdentifier userIdentifier, GetAllMerchantBillsForExcelInput input)
        {
            var countryCode = "";
            if (input.UtcTimeFilter == "GMT8+")
            {
                countryCode = CultureTimeHelper.TimeCodeZhCN;
            }
            if (input.UtcTimeFilter == "GMT7+")
            {
                countryCode = CultureTimeHelper.TimeCodeViVn;
            }
            if (input.UtcTimeFilter == "GMT4-")
            {
                countryCode = CultureTimeHelper.TimeCodeEST;
            }
            if (countryCode.IsNullOrEmpty())
            {
                countryCode = CultureTimeHelper.TimeCodeZhCN;
            }

            if (input.userMerchantIdsList == null)
            {

                var merchantIds = (await _abpUserMerchantRepository.GetAllListAsync(r => r.UserId == userIdentifier.UserId)).Select(r => r.MerchantId);
                var merchants = await _merchantRepository.GetAllListAsync(r => merchantIds.Contains(r.Id));
                input.userMerchantIdsList = merchants.Select(r => r.Id).ToList();
            }

            var filteredPayOrders = await _merchantBillsMongoService.GetAll(input, input.userMerchantIdsList);

            var results = new List<GetMerchantBillForViewDto>();

            foreach (var o in filteredPayOrders)
            {
                var res = new GetMerchantBillForViewDto()
                {
                    MerchantBill = new MerchantBillDto
                    {
                        MerchantCode = o.MerchantCode,
                        BillNo = o.BillNo,
                        BillType = o.BillType,
                        Money = o.Money.ToString("F0"),
                        Rate = o.Rate,
                        FeeMoney = o.FeeMoney,
                        BalanceBefore = o.BalanceBefore.ToString("F0"),
                        BalanceAfter = o.BalanceAfter.ToString("F0"),
                        CreationTime = CultureTimeHelper.GetCultureTimeInfo(o.OrderTime, countryCode),
                        TransactionTime = CultureTimeHelper.GetCultureTimeInfo(o.TransactionTime, countryCode),
                        PlatformCode = o.PlatformCode,
                        Remark = o.Remark,
                        Id = o.ID
                    }
                };
                results.Add(res);
            }
            return results;
        }

        public async Task<int> GetTotalRecordExcel(GetAllMerchantBillsForExcelInput input)
        {
            var inputStr = input.ToJsonString();
            var cloneInput = JsonConvert.DeserializeObject<GetAllMerchantBillsForExcelInput>(inputStr);
            return await _merchantBillsMongoService.GetTotalExcelRecordCount(cloneInput, cloneInput.userMerchantIdsList);
        }


    }
}
