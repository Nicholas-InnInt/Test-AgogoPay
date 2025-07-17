using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.WithdrawalOrders;

namespace Neptune.NsPay.AutoTransferWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly ITransferCallBackService _transferCallBackService;
        public Worker(ILogger<Worker> logger,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            ITransferCallBackService transferCallBackService)
        {
            _logger = logger;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _transferCallBackService = transferCallBackService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                try
                {
                    #region 获取状态为8

                    var startTime = DateTime.Now.AddHours(-6);
                    var withdrawOrderFailds = await _withdrawalOrdersMongoService.GetWithdrawOrderByWaitPhone(startTime);
                    foreach (var item in withdrawOrderFailds)
                    {
                        var dateNow = TimeHelper.GetUnixTimeStamp(DateTime.Now.AddMinutes(-3));
                        if (dateNow >= item.TransactionUnixTime)
                        {
                            var info = await _withdrawalOrdersMongoService.GetById(item.ID);
                            if (info.OrderStatus != WithdrawalOrderStatusEnum.Success && info.NotifyStatus == WithdrawalNotifyStatusEnum.Wait)
                            {
                                info.OrderStatus = WithdrawalOrderStatusEnum.Fail;
                                info.Remark = "手机未回调";
                                await _withdrawalOrdersMongoService.UpdateAsync(info);
                            }
                        }
                    }
                    _logger.LogInformation("Update Time Out Count:" + withdrawOrderFailds.Count, DateTimeOffset.Now);


                    #endregion

                    var dateTime = DateTime.Now.AddHours(-12);
                    var withdrawalOrders = await _withdrawalOrdersMongoService.GetWithdrawOrderCallBack(dateTime);
                    _logger.LogInformation("Score Count:" + withdrawalOrders.Count, DateTimeOffset.Now);
                    int sum = 0;
                    foreach (var item in withdrawalOrders)
                    {
                        try
                        {
                            var result = await _transferCallBackService.TransferCallBackPost(item.ID);
                            if (result)
                                sum += 1;
                        }
                        catch (Exception ex)
                        {
                            NlogLogger.Error("出款回调异常错误：" + ex);
                        }
                    }
                    _logger.LogInformation("Transfer Success Count:" + sum, DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    NlogLogger.Error("出款程序异常错误：" + ex);
                }

                await Task.Delay(1000*10, stoppingToken);
            }
        }
    }
}
