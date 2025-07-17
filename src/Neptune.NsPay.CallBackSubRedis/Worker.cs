using Abp.Extensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;

namespace Neptune.NsPay.CallBackSubRedis
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRedisService _redisService;
        private readonly ICallBackService _callBackService;

        public Worker(ILogger<Worker> logger,
            IRedisService redisService,
            ICallBackService callBackService)
        {
            _logger = logger;
            _redisService = redisService;
            _callBackService = callBackService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var reliableQueue = _redisService.GetCallBackOrderQueue(NsPayRedisKeyConst.CallBackOrder);
            reliableQueue.RetryInterval = 5;
            while (!stoppingToken.IsCancellationRequested)
            {
                var orderId = await reliableQueue.TakeOneAsync(-1);
                if (!orderId.IsNullOrEmpty())
                {
                    _logger.LogInformation("获取消息{time}--OrderId:{orderid}", DateTimeOffset.Now, orderId);
                    await _callBackService.CallBackPost(orderId);
                    var isKnow = reliableQueue.Acknowledge(orderId);
                    if (isKnow > 0)
                    {
                        _logger.LogInformation("获取消息OrderId:{orderid}完成消费", orderId);
                    }
                    else
                    {
                        _logger.LogInformation("获取消息OrderId:{orderid}完成消费", orderId);
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
