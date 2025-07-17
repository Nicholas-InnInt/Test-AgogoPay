using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Quartz;

namespace Neptune.NsPay.MongoToSqlWorker.Jobs
{
    class ProcessSyncMerchantBills : IJob
    {
        private readonly ILogger<ProcessSyncMerchantBills> _logger;
        private readonly IMerchantBillsMongoService _merchantBillsMongoService;
        private readonly IMerchantBillService _merchantBillService;
        public ProcessSyncMerchantBills(
            ILogger<ProcessSyncMerchantBills> logger,
            IMerchantBillsMongoService merchantBillsMongoService,
            IMerchantBillService merchantBillService
            ) {
            _logger = logger;
            _merchantBillsMongoService = merchantBillsMongoService;
            _merchantBillService = merchantBillService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            int batchSize = int.Parse(AppSettings.Configuration["BatchSize"]);
            int syncDays = int.Parse(AppSettings.Configuration["SyncDays"]);

            _logger.LogInformation("Worker running start at: {time}", DateTimeOffset.Now);
            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddDays(-syncDays);

            var merchantbillsMongoList = await _merchantBillsMongoService.GetMerchantBillByDateRange(startDate, endDate);

            if (merchantbillsMongoList.Any())
            {
                for (int i = 0; i < merchantbillsMongoList.Count; i += batchSize)
                {
                    var batchData = merchantbillsMongoList
                                    .GetRange(i, Math.Min(batchSize, merchantbillsMongoList.Count - i))
                                    .Select(x => new MerchantBill
                                    {
                                        BillNo = x.BillNo,
                                        BillType = x.BillType,
                                        MerchantId = x.MerchantId,
                                        MerchantCode = x.MerchantCode,
                                        Money = x.Money,
                                        Rate = x.Rate,
                                        FeeMoney = x.FeeMoney,
                                        BalanceBefore = x.BalanceBefore,
                                        BalanceAfter = x.BalanceAfter,
                                        PlatformCode = x.PlatformCode,
                                        CreationTime = x.CreationTime,
                                        CreationUnixTime = x.CreationUnixTime
                                    }).ToList();

                    await _merchantBillService.UpsertMerchantBills(batchData);
                }
            }

            var newInsertDataList = _merchantBillService.GetAll()
                                    .Where(x => x.CreationUnixTime >= TimeHelper.GetUnixTimeStamp(startDate)
                                    & x.CreationUnixTime < TimeHelper.GetUnixTimeStamp(endDate));

            _logger.LogInformation("merchantbillsMongoCount : {mongoCount} || merchantbillsCount : {mssqlCount}", merchantbillsMongoList.Count(), newInsertDataList.Count());

            var missingData = merchantbillsMongoList.Where(itemA => !newInsertDataList.Any(itemB => itemB.BillNo == itemA.BillNo)).ToList();
            if (missingData.Any())
                _logger.LogInformation("missing data : {data} ", missingData.ToJsonString());

            _logger.LogInformation("Worker running end at: {time}", DateTimeOffset.Now);
        }
    }
}
