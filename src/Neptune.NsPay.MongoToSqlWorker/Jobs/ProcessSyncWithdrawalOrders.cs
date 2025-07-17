using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.WithdrawalOrders;
using Quartz;

namespace Neptune.NsPay.MongoToSqlWorker.Jobs
{
    class ProcessSyncWithdrawalOrders : IJob
    {
        private readonly ILogger<ProcessSyncWithdrawalOrders> _logger;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IWithdrawalOrdersService _withdrawalOrdersService;
        public ProcessSyncWithdrawalOrders(
            ILogger<ProcessSyncWithdrawalOrders> logger,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            IWithdrawalOrdersService withdrawalOrdersService
            ) {
            _logger = logger;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _withdrawalOrdersService = withdrawalOrdersService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            int batchSize = int.Parse(AppSettings.Configuration["BatchSize"]);
            int syncDays = int.Parse(AppSettings.Configuration["SyncDays"]);

            _logger.LogInformation("Worker running start at: {time}", DateTimeOffset.Now);
            var endDate = DateTime.Now.Date.AddDays(1);
            var startDate = endDate.AddDays(-2);

            var withdrawalOrdersMongoList = await _withdrawalOrdersMongoService.GetWithdrawOrderByDateRange(startDate, endDate);

            if (withdrawalOrdersMongoList.Any())
            {
                for (int i = 0; i < withdrawalOrdersMongoList.Count; i += batchSize)
                {
                    var batchData = withdrawalOrdersMongoList
                                    .GetRange(i, Math.Min(batchSize, withdrawalOrdersMongoList.Count - i))
                                    .Select(x => new WithdrawalOrder
                                    {
                                        MerchantId = x.MerchantId,
                                        MerchantCode = x.MerchantCode,
                                        PlatformCode = x.PlatformCode,
                                        WithdrawNo = x.WithdrawNo,
                                        OrderStatus = x.OrderStatus,
                                        OrderMoney = x.OrderMoney,
                                        Rate = x.Rate,
                                        FeeMoney = x.FeeMoney,
                                        TransactionNo = x.TransactionNo,
                                        TransactionTime = x.TransactionTime,
                                        BenAccountName = x.BenAccountName,
                                        BenAccountNo = x.BenAccountNo,
                                        BenBankName = x.BenBankName,
                                        NotifyUrl = x.NotifyUrl,
                                        OrderNumber = x.OrderNumber,
                                        ReceiptUrl = x.ReceiptUrl,
                                        DeviceId = x.DeviceId,
                                        OrderType = x.OrderType,
                                        NotifyStatus = x.NotifyStatus,
                                        NotifyNumber = x.NotifyNumber,
                                        Description = x.Description,
                                        Remark = x.Remark,
                                        CreationTime = x.CreationTime,
                                        CreationUnixTime = x.CreationUnixTime,
                                        CreatorUserId = x.CreatorUserId,
                                        LastModificationTime = x.LastModificationTime,
                                        LastModifierUserId = x.LastModifierUserId,
                                        IsDeleted = x.IsDeleted,
                                        DeleterUserId = x.DeleterUserId,
                                        DeletionTime = x.DeletionTime
                                    }).ToList();

                    await _withdrawalOrdersService.UpsertWithdrawalOrders(batchData);
                }
            }

            var newInsertDataList = _withdrawalOrdersService.GetAll()
                                    .Where(x => x.CreationUnixTime >= TimeHelper.GetUnixTimeStamp(startDate)
                                    & x.CreationUnixTime < TimeHelper.GetUnixTimeStamp(endDate));

            _logger.LogInformation("WithdrawalOrdersMongoCount : {WithdrawalOrdersMongoCount} || WithdrawalOrdersCount : {mssqlCount}", withdrawalOrdersMongoList.Count(), newInsertDataList.Count());

            var missingData = withdrawalOrdersMongoList.Where(a => !newInsertDataList.Any(b => b.OrderNumber + '_' + b.MerchantId == a.OrderNumber + '_' + a.MerchantId)).ToList();
            if (missingData.Any())
                _logger.LogInformation("missing data : {data} ", missingData.ToJsonString());

            _logger.LogInformation("Worker running end at: {time}", DateTimeOffset.Now);
        }
    }
}
