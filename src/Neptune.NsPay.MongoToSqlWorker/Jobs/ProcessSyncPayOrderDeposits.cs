using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrderDeposits;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Quartz;

namespace Neptune.NsPay.MongoToSqlWorker.Jobs
{
    class ProcessSyncPayOrderDeposits : IJob
    {
        private readonly ILogger<ProcessSyncPayOrderDeposits> _logger;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        private readonly IPayOrderDepositService _payOrderDepositService;
        public ProcessSyncPayOrderDeposits(
            ILogger<ProcessSyncPayOrderDeposits> logger,
            IPayOrderDepositsMongoService payOrderDepositsMongoService,
            IPayOrderDepositService payOrderDepositService
            ) 
        {
            _logger = logger;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
            _payOrderDepositService = payOrderDepositService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            int batchSize = int.Parse(AppSettings.Configuration["BatchSize"]);
            int syncDays = int.Parse(AppSettings.Configuration["SyncDays"]);

            _logger.LogInformation("Worker running start at: {time}", DateTimeOffset.Now);
            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddDays(-syncDays);

            var payOrderDepositMongoList = await _payOrderDepositsMongoService.GetPayOrderDepositByDateRange(startDate, endDate);

            if (payOrderDepositMongoList.Any())
            {
                for (int i = 0; i < payOrderDepositMongoList.Count; i += batchSize)
                {
                    var batchData = payOrderDepositMongoList
                                    .GetRange(i, Math.Min(batchSize, payOrderDepositMongoList.Count - i))
                                    .Select(x => new PayOrderDeposit
                                    {
                                        RefNo = x.RefNo,
                                        PayType = x.PayType,
                                        Type = x.Type,
                                        Description = x.Description,
                                        CreditBank = x.CreditBank,
                                        CreditAcctNo = x.CreditAcctNo,
                                        CreditAcctName = x.CreditAcctName,
                                        CreditAmount = x.CreditAmount,
                                        DebitBank = x.DebitBank,
                                        DebitAcctNo = x.DebitAcctNo,
                                        DebitAcctName = x.DebitAcctName,
                                        DebitAmount = x.DebitAmount,
                                        AvailableBalance = x.AvailableBalance,
                                        TransactionTime = x.TransactionTime,
                                        CreationTime = x.CreationTime,
                                        CreationUnixTime = x.CreationUnixTime,
                                       // OrderId = x.OrderId,
                                       // BankOrderId = x.BankOrderId,
                                        MerchantId = x.MerchantId,
                                        MerchantCode = x.MerchantCode,
                                        RejectRemark = x.RejectRemark,
                                        UserName = x.UserName,
                                        AccountNo = x.AccountNo,
                                        PayMentId = x.PayMentId,
                                        OperateTime = x.OperateTime.Value,
                                        UserId = x.UserId
                                    }).ToList();

                    await _payOrderDepositService.UpsertPayOrderDeposit(batchData);
                }
            }

            var newInsertDataList = _payOrderDepositService.GetAll()
                                    .Where(x => x.CreationUnixTime >= TimeHelper.GetUnixTimeStamp(startDate)
                                    & x.CreationUnixTime < TimeHelper.GetUnixTimeStamp(endDate));

            _logger.LogInformation("payOrderDepositMongoCount : {payOrderMongoCount} || payOrderCount : {mssqlCount}", payOrderDepositMongoList.Count(), newInsertDataList.Count());

            var missingData = payOrderDepositMongoList.Where(a => !newInsertDataList.Any(b => b.RefNo + '_' + b.MerchantId == a.RefNo + '_' + a.MerchantId)).ToList();
            if (missingData.Any())
                _logger.LogInformation("missing data : {data} ", missingData.ToJsonString());

            _logger.LogInformation("Worker running end at: {time}", DateTimeOffset.Now);
        }
    }
}
