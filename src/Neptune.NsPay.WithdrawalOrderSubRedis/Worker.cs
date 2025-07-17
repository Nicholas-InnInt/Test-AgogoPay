using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.PlatfromServices.AppServices;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;
using PayPalCheckoutSdk.Orders;

namespace Neptune.NsPay.WithdrawalOrderSubRedis
{
    public class Worker : BackgroundService
    {
        private readonly IRedisService _redisService;
        private readonly ITransferCallBackService _transferCallBackService;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger,
            IRedisService redisService,
            ITransferCallBackService transferCallBackService)
        {
            _logger = logger;
            _redisService = redisService;
            _transferCallBackService = transferCallBackService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    if (_logger.IsEnabled(LogLevel.Information))
            //    {
            //        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    }
            //    await Task.Delay(1000, stoppingToken);
            //}
            var reliableQueue = _redisService.GetWithdrawalOrderQueue(NsPayRedisKeyConst.WithdrawalOrder);
            reliableQueue.RetryTimesWhenSendFailed = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                var bankOrder = await reliableQueue.TakeOneAsync(-1);
                if (bankOrder != null)
                {
                    try
                    {
                        //回调订单
                        var result = await _transferCallBackService.TransferCallBackPost(bankOrder.OrderId);
                        var isKnow = reliableQueue.Acknowledge(bankOrder.ToJsonString());
                        if (isKnow > 0)
                        {
                            _logger.LogInformation("获取消息：OrderId:{orderid}，完成消费", bankOrder.OrderId);
                        }
                        else
                        {
                            _logger.LogInformation("获取消息：OrderId:{orderid}，失败消费", bankOrder.OrderId);
                        }
                    }
                    catch (Exception ex) 
                    {
                        NlogLogger.Error("回调订单异常：" + bankOrder.ToJsonString());
                    }
                }
                else
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}
