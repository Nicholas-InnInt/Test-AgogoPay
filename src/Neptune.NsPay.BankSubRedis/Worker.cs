using Abp.Extensions;
using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using NewLife.Serialization;

namespace Neptune.NsPay.BankSubRedis
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRedisService _redisService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        private readonly IKafkaProducer _kafkaProducer;
        private string processId;
        public Worker(ILogger<Worker> logger,
                   IRedisService redisService,
                   IPayOrdersMongoService payOrdersMongoService,
                   IPayOrderDepositsMongoService payOrderDepositsMongoService,
                   IKafkaProducer kafkaProducer)
        {
            _logger = logger;
            _redisService = redisService;
            _payOrdersMongoService = payOrdersMongoService;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
            _kafkaProducer = kafkaProducer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            var type = AppSettings.Configuration["PayType"];
            var bankOrderRedisConst = BankNameConst.GetBankOrderRedisKeyByBankName(type);
            if (string.IsNullOrEmpty(bankOrderRedisConst)) return;

            var reliableQueue = _redisService.GetOrderQueue(bankOrderRedisConst);
            reliableQueue.RetryInterval = 1;

            while (!stoppingToken.IsCancellationRequested)
            {
                var bankOrder = await reliableQueue.TakeOneAsync(-1);
                processId = Guid.NewGuid().ToString();  
                if (bankOrder != null)
                {
                    try
                    {
                        _logger.LogInformation("获取消息：{time}--OrderId:{orderid}", DateTimeOffset.Now, bankOrder.PayOrderId);

                        var dateNow = DateTime.Now.AddMinutes(-300);

                        await ProcessPayOrderByBank(type, dateNow, bankOrder);
                        if (type == "BusinessTcb")
                        {
                            bankOrder.Money = decimal.Parse(bankOrder.Money.ToString("F2"));
                        }
                        var tempStr = bankOrder.ToJson();
                        if (string.IsNullOrEmpty(bankOrder.MerchantCode))
                        {
                            tempStr = tempStr.Replace("\"MerchantCode\":null,", "");
                        }
                        var isKnow = reliableQueue.Acknowledge(tempStr);
                        if (isKnow > 0)
                        {
                            _logger.LogInformation("获取消息：OrderId:{orderid}，完成消费", bankOrder.PayOrderId);
                        }
                        else
                        {
                            bankOrder.Money = decimal.Parse(bankOrder.Money.ToString("F2"));
                            var tempStr2 = bankOrder.ToJson();
                            var isKnow2 = reliableQueue.Acknowledge(tempStr2);
                            if (isKnow2 > 0)
                            {
                                _logger.LogInformation("获取消息：OrderId:{orderid}，完成消费", bankOrder.PayOrderId);
                            }
                            else
                            {
                                bankOrder.Money = decimal.Parse(bankOrder.Money.ToString("F0"));
                                var tempStr3 = bankOrder.ToJson();
                                var isKnow3 = reliableQueue.Acknowledge(tempStr3);
                                if (isKnow3 > 0)
                                {
                                    _logger.LogInformation("获取消息：OrderId:{orderid}，完成消费", bankOrder.PayOrderId);
                                }
                                else
                                {
                                    _logger.LogInformation("获取消息：OrderId:{orderid}，失败消费", bankOrder.PayOrderId);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        NlogLogger.Error(type + "请求队列异常：" + ex);
                    }
                }
                else
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private async Task<bool> ProcessPayOrderByBank(string type, DateTime dateNow, BankOrderPubModel bankOrder)
        {
            var payment = _redisService.GetPayMentInfoById(bankOrder.PayMentId);
            var payOrder = await _payOrdersMongoService.GetPayOrderByOrderId(bankOrder.PayOrderId);

            if (payment != null && payOrder != null)
            {
                if (type == BankNameConst.VCB)
                {
                    var payOrderDeposit = await _payOrderDepositsMongoService.GetPayOrderByBank(dateNow, bankOrder.Id, payment.Id, payOrder.OrderMark);
                    if (payOrderDeposit != null)
                    {
                        var successOrderCache = _redisService.GetSuccessBankOrder(payOrderDeposit.ID, type);
                        if (successOrderCache.IsNullOrEmpty())
                        {
                            //修改事务处理
                            var transactionNo = bankOrder.Id + "-" + payment.Phone;
                            var result = await _payOrderDepositsMongoService.PayOrderDepositSubRedisUpdate(payOrder.ID, transactionNo, payOrder.OrderMoney, bankOrder.Desc, payOrderDeposit.ID, payOrder.MerchantCode, payOrder.MerchantId, payment.Id, payment.Type);
                            if (result)
                            {
                               await ProcessMerchantBill(type, payOrder);
                                //加入银行缓存，如果银行订单已经处理就不更新，这个是方式用户备注填写2个或者多个，导致更新多个订单
                                _redisService.SetSuccessBankOrder(payOrderDeposit.ID, type);
                            }
                        }
                    }
                }
                else
                {
                    var payOrderDeposit = await _payOrderDepositsMongoService.GetPayOrderByBank(dateNow, bankOrder.Id, payment.Id);
                    if (payOrderDeposit != null)
                    {
                        var successOrderCache = _redisService.GetSuccessBankOrder(payOrderDeposit.ID, type);
                        if (successOrderCache.IsNullOrEmpty())
                        {
                            //修改事务处理
                            var transactionNo = bankOrder.Id + "-" + payment.Phone;
                            var result = await _payOrderDepositsMongoService.PayOrderDepositSubRedisUpdate(payOrder.ID, transactionNo, payOrder.OrderMoney, bankOrder.Desc, payOrderDeposit.ID, payOrder.MerchantCode, payOrder.MerchantId, payment.Id, payment.Type);
                            if (result)
                            {
                                await ProcessMerchantBill(type, payOrder);
                                //加入银行缓存，如果银行订单已经处理就不更新，这个是方式用户备注填写2个或者多个，导致更新多个订单
                                _redisService.SetSuccessBankOrder(payOrderDeposit.ID, type);
                            }
                        }
                    }
                }
            }

            return true;
        }

        private async Task<bool> UpdatePayOrder(string type, PayOrdersMongoEntity payOrder, BankOrderPubModel bankOrder, string userName)
        {
            payOrder.TransactionNo = bankOrder.Id + "-" + userName;
            payOrder.OrderStatus = PayOrderOrderStatusEnum.Completed;
            payOrder.TransactionTime = DateTime.Now;
            payOrder.TransactionUnixTime = TimeHelper.GetUnixTimeStamp(payOrder.TransactionTime);
            payOrder.Remark = bankOrder.Desc;
            payOrder.TradeMoney = bankOrder.Money;
            var flag = await _payOrdersMongoService.UpdateAsync(payOrder);
            if (!flag)
            {
                NlogLogger.Error(type + "更新订单错误：" + payOrder.ToJsonString());
                await _payOrdersMongoService.UpdateAsync(payOrder);
            }
            return true;
        }

        private async Task< bool> ProcessMerchantBill(string type, PayOrdersMongoEntity payOrder)
        {
            var checkOrder = _redisService.GetMerchantBillOrder(payOrder.MerchantCode, payOrder.OrderNumber);

            if (checkOrder.IsNullOrEmpty())
            {
                _redisService.SetMerchantBillOrder(payOrder.MerchantCode, payOrder.OrderNumber);
                //var redisMqDto = new PayMerchantRedisMqDto()
                //{
                //    PayMqSubType = MQSubscribeStaticConsts.MerchantBillAddBalance,
                //    MerchantCode = payOrder.MerchantCode,
                //    PayOrderId = payOrder.ID,
                //};
                //_redisService.SetMerchantMqPublish(redisMqDto);

                try
                {
                    await _kafkaProducer.ProduceAsync<PayOrderPublishDto>(KafkaTopics.PayOrder, payOrder.ID, new PayOrderPublishDto()
                    {
                        MerchantCode = payOrder.MerchantCode,
                        PayOrderId = payOrder.ID,
                        TriggerDate = DateTime.Now,
                        ProcessId = processId
                    });
                }
                catch (Exception ex)
                {
                    NlogLogger.Error("Transfer-Kafka推送失败：" + ex.ToString());
                }


                NlogLogger.Fatal(type + "订单：" + payOrder.OrderNumber + "，添加完成");
            }
            //回调平台

            try
            {
                await _kafkaProducer.ProduceAsync<PayOrderCallbackPublishDto>(KafkaTopics.PayOrderCallBack, payOrder.ID, new PayOrderCallbackPublishDto()
                {
                    PayOrderId = payOrder.ID,
                    ProcessId = processId
                });

            }
            catch (Exception ex)
            {
                NlogLogger.Error("Transfer-Kafka推送失败：" + ex.ToString());

            }
         
           // _redisService.AddCallBackOrderQueueList(NsPayRedisKeyConst.CallBackOrder, payOrder.ID);
            return true;
        }
    }
}