using Abp.Extensions;
using MongoDB.Driver.Linq;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;

namespace Neptune.NsPay.AutoDepositWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ICallBackService _callBackService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IRedisService _redisService;

        public Worker(ILogger<Worker> logger,
            ICallBackService callBackService,
            IPayOrdersMongoService payOrdersMongoService,
            IRedisService redisService)
        {
            _logger = logger;
            _callBackService = callBackService;
            _payOrdersMongoService = payOrdersMongoService;
            _redisService = redisService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
			var merchantCode = AppSettings.Configuration["MerchantCode"];
            if (merchantCode.IsNullOrEmpty())
            {
                _logger.LogInformation("merchantCode is null");
                return;
			}
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                try
                {
                    #region 获取超时订单，更新订单状态

                    var failTime = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.AutoDepositFailTime).ToInt();
					var beforeTime = DateTime.Now.AddMinutes(-failTime);
                    long orderTimeOutList = 0;
					if (merchantCode == "NsPay")
					{
						orderTimeOutList = await _payOrdersMongoService.UpdatePayOrderByFailedList(merchantCode, beforeTime, MerchantTypeEnum.External);
					}
					else
					{
						orderTimeOutList = await _payOrdersMongoService.UpdatePayOrderByFailedList(merchantCode, beforeTime, MerchantTypeEnum.Internal);
					}
                    _logger.LogInformation("Update Time Out Count:" + orderTimeOutList, DateTimeOffset.Now);


                    #endregion

                    #region 支付成功订单上分

                    var successTime = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.AutoDepositSuccessTime).ToInt();
                    var startTime = DateTime.Now.AddMinutes(-successTime);
                    var payorderList = new List<MongoDbExtensions.Models.PayOrdersMongoEntity>();
                    if (merchantCode == "NsPay")
                    {
                        payorderList = await _payOrdersMongoService.GetPayOrderByCompletedList(merchantCode, startTime, MerchantTypeEnum.External);
                        payorderList = payorderList.Where(x => x.MerchantCode != "ec95de29a55dc4d4").ToList();
                    }
                    else
                    {
                        if (merchantCode == "ec95de29a55dc4d4")
                        {
                            payorderList = await _payOrdersMongoService.GetPayOrderByCompletedList(merchantCode, startTime);
                        }
                        else
                        {
                            payorderList = await _payOrdersMongoService.GetPayOrderByCompletedList(merchantCode, startTime, MerchantTypeEnum.Internal);
                        }
                    }
                    if (payorderList!=null)
                    {
                        _logger.LogInformation("Score Count:" + payorderList.Count, DateTimeOffset.Now);
                        int sum = 0;
                        foreach (var item in payorderList)
                        {
                            try
                            {
                                var result = await _callBackService.CallBackPost(item.ID);
                                if (result)
                                    sum += 1;
                            }
                            catch (Exception ex)
                            {
                                NlogLogger.Error("订单:" + item.OrderNumber + "异常错误:" + ex);
                            }
                        }
                        _logger.LogInformation("Score Success Count:" + sum, DateTimeOffset.Now);
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    NlogLogger.Error("异常错误:" + ex);
                }

                await Task.Delay(1000*5, stoppingToken);
            }
        }
    }
}
