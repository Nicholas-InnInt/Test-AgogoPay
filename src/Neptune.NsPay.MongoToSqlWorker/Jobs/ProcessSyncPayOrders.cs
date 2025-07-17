using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Quartz;

namespace Neptune.NsPay.MongoToSqlWorker.Jobs
{
    class ProcessSyncPayOrders : IJob
    {
        private readonly ILogger<ProcessSyncPayOrders> _logger;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOrderService _payOrderService;
        public ProcessSyncPayOrders(
            ILogger<ProcessSyncPayOrders> logger, 
            IPayOrdersMongoService payOrdersMongoService,
            IPayOrderService payOrderService
            ) {
            _logger = logger;
            _payOrdersMongoService = payOrdersMongoService;
            _payOrderService = payOrderService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            int batchSize = int.Parse(AppSettings.Configuration["BatchSize"]);
            int syncDays = int.Parse(AppSettings.Configuration["SyncDays"]);

            _logger.LogInformation("Worker running start at: {time}", DateTimeOffset.Now);
            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddDays(-syncDays);

            var payOrderMongoList = await _payOrdersMongoService.GetPayOrderByDateRange(startDate, endDate);

            if (payOrderMongoList.Any())
            {
                for (int i = 0; i < payOrderMongoList.Count; i += batchSize)
                {
                    var batchData = payOrderMongoList
                                    .GetRange(i, Math.Min(batchSize, payOrderMongoList.Count - i))
                                    .Select(x => new PayOrder
                                    {
                                        MerchantId = x.MerchantId,
                                        MerchantCode = x.MerchantCode,
                                        OrderNo = x.OrderNo,
                                        TransactionNo = x.TransactionNo,
                                        OrderType = x.OrderType,
                                        OrderStatus = x.OrderStatus,
                                        OrderMoney = x.OrderMoney,
                                        Rate = x.Rate,
                                        FeeMoney = x.FeeMoney,
                                        TransactionTime = x.TransactionTime,
                                        CreationTime = x.CreationTime,
                                        CreationUnixTime = x.CreationUnixTime,
                                        OrderMark = x.OrderMark,
                                        OrderNumber = x.OrderNumber,
                                        PlatformCode = x.PlatformCode,
                                        PayMentId = x.PayMentId,
                                        ScCode = x.ScCode,
                                        ScSeri = x.ScSeri,
                                        NotifyUrl = x.NotifyUrl,
                                        UserId = x.UserId,
                                        UserNo = x.UserNo,
                                        ScoreStatus = x.ScoreStatus,
                                        PayType = (int)x.PayType,
                                        ScoreNumber = x.ScoreNumber,
                                        TradeMoney = x.TradeMoney,
                                        IPAddress = x.IPAddress,
                                        ErrorMsg = x.ErrorMsg,
                                        Remark = x.Remark,
                                        PaymentChannel = x.PaymentChannel,
                                        MerchantType = x.MerchantType
                                    }).ToList();

                    await _payOrderService.UpsertPayOrder(batchData);
                }
            }

            var newInsertDataList = _payOrderService.GetAll()
                                    .Where(x => x.CreationUnixTime >= TimeHelper.GetUnixTimeStamp(startDate)
                                    & x.CreationUnixTime < TimeHelper.GetUnixTimeStamp(endDate));

            _logger.LogInformation("payOrderMongoCount : {payOrderMongoCount} || payOrderCount : {mssqlCount}", payOrderMongoList.Count(), newInsertDataList.Count());

            var missingData = payOrderMongoList.Where(a => !newInsertDataList.Any(b => b.OrderNumber + '_' + b.MerchantId == a.OrderNumber + '_' + a.MerchantId)).ToList();
            if (missingData.Any())
                _logger.LogInformation("missing data : {data} ", missingData.ToJsonString());

            _logger.LogInformation("Worker running end at: {time}", DateTimeOffset.Now);
        }
    }
}
