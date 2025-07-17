using DotNetCore.CAP;
using Neptune.NsPay.Commons;
using Neptune.NsPay.RabbitMqExtensions.Models;
using Neptune.NsPay.RedisExtensions;
using Newtonsoft.Json;

namespace Neptune.NsPay.MqPublish
{
    public class Worker : BackgroundService
    {
        private readonly ICapPublisher _capBus;
        private readonly IRedisService _redisService;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger,
            ICapPublisher capBus,
            IRedisService redisService)
        {
            _logger = logger;
            _capBus = capBus;
            _redisService = redisService;
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

            var reliableQueue = _redisService.GetMerchantMqPublish();
            reliableQueue.RetryInterval = 60;
            reliableQueue.RetryTimesWhenSendFailed = 3;
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var order = await reliableQueue.TakeOneAsync(-1);
                if (order != null)
                {
                    try
                    {
                        var publishTasks = new List<Task>();
                        if (order.PayMqSubType == MQSubscribeStaticConsts.MerchantBillAddBalance)
                        {
                            publishTasks.Add(_capBus.PublishAsync(MQSubscribeStaticConsts.MerchantBillAddBalance, new PayMerchantMqDto { MerchantCode = order.MerchantCode, PayOrderId = order.PayOrderId }));
                        }
                        if (order.PayMqSubType == MQSubscribeStaticConsts.MerchantBillReduceBalance)
                        {
                            publishTasks.Add(_capBus.PublishAsync(MQSubscribeStaticConsts.MerchantBillReduceBalance, new TransferMerchantMqDto { MerchantCode = order.MerchantCode, WithdrawalOrderId = order.WithdrawalOrderId }));
                        }
                        if (order.PayMqSubType == MQSubscribeStaticConsts.MerchantBillAddWithdraws)
                        {
                            publishTasks.Add(_capBus.PublishAsync(MQSubscribeStaticConsts.MerchantBillAddWithdraws, new MerchantWithdrawsMqDto { MerchantCode = order.MerchantCode, MerchantWithdrawId = order.MerchantWithdrawId }));
                        }

                        await Task.WhenAll(publishTasks); // 确保所有消息发布完成

                        var isKnow = reliableQueue.Acknowledge(JsonConvert.SerializeObject(order));
                        if (isKnow > 0)
                        {
                            _logger.LogInformation("获取消息：OrderId:{orderid}，完成消费", JsonConvert.SerializeObject(order));
                        }
                        else
                        {
                            _logger.LogInformation("获取消息：OrderId:{orderid}，失败消费", JsonConvert.SerializeObject(order));
                        }
                    }
                    catch (Exception ex)
                    {
                        NlogLogger.Error("队列错误异常:" + ex);
                    }
                }
                else
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(200, stoppingToken);
                }
            }

        }
    }
}
