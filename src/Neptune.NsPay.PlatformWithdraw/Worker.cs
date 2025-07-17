using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PlatformWithdraw.Helper;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.WithdrawalOrders;

namespace Neptune.NsPay.PlatformWithdraw
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRedisService _redisService;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IWithdrawalDevicesService _withdrawalDevicesService;
        private readonly IMerchantService _merchantService;

        public Worker(ILogger<Worker> logger,
            IRedisService redisService,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            IWithdrawalDevicesService withdrawalDevicesService,
            IMerchantService merchantService)
        {
            _logger = logger;
            _redisService = redisService;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _withdrawalDevicesService = withdrawalDevicesService;
            _merchantService = merchantService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var Memo = AppSettings.Configuration["Memo"];
            var Levels = AppSettings.Configuration["Levels"];
            var MerchantCode = AppSettings.Configuration["MerchantCode"];
            var merchant = await _merchantService.GetFirstAsync(r => r.MerchantCode == MerchantCode);
            if (merchant == null)
            {
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

                    PlatformWebHelper platformWebHelper = new PlatformWebHelper(_redisService);
                    platformWebHelper.Verify();

                    #region 获取未锁定订单

                    var WithdrawList = platformWebHelper.GetVerifyWithdrawDataList(Levels, Memo);
                    if (WithdrawList != null)
                    {
                        foreach (var Withdraw in WithdrawList)
                        {
                            //获取订单详情
                            var detail = platformWebHelper.GetWithdrawDetail(Withdraw.Id);
                            if (detail != null)
                            {
                                //锁定订单
                                var result = platformWebHelper.LockWithdraw(Withdraw.Id);

                                if (result)
                                {
                                    var deviceInfo = _redisService.GetLPushWithdrawDevice(merchant.MerchantCode);
                                    if (deviceInfo != null)
                                    {
                                        //获取完后插入最后
                                        _redisService.SetRPushWithdrawDevice(merchant.MerchantCode, deviceInfo);

                                        var check = await _withdrawalOrdersMongoService.GetWithdrawOrderByOrderNumber(merchant.MerchantCode, detail.Id.ToString());
                                        if (check == null)
                                        {
                                            //添加出款订单表
                                            WithdrawalOrdersMongoEntity withdrawalOrder = new WithdrawalOrdersMongoEntity()
                                            {
                                                MerchantCode = merchant.MerchantCode,
                                                MerchantId = merchant.Id,
                                                PlatformCode = merchant.PlatformCode,
                                                OrderNumber = detail.Id.ToString(),
                                                WithdrawNo = Guid.NewGuid().ToString("N"),
                                                OrderMoney = detail.Amount * 1000,
                                                Rate = 0,
                                                FeeMoney = 0,
                                                NotifyUrl = "",
                                                BenAccountNo = detail.BankAccount.Account,
                                                BenAccountName = detail.Member.MemberInfo.Name,
                                                BenBankName = detail.BankAccount.Name,
                                                DeviceId = deviceInfo.Id,
                                                NotifyStatus = WithdrawalNotifyStatusEnum.Wait,
                                                CreationTime = DateTime.Now,
                                                OrderType = WithdrawalOrderTypeEnum.PlatformFK,
                                                Description = Withdraw.Memo
                                            };
                                            var orderId = await _withdrawalOrdersMongoService.AddAsync(withdrawalOrder);
                                            //加入缓存
                                            if (withdrawalOrder.OrderMoney < 50000)
                                            {
                                                //加入缓存队列
                                                WithdrawalOrderRedisModel orderRedisModel = new WithdrawalOrderRedisModel()
                                                {
                                                    Id = orderId,
                                                    MerchantCode = merchant.MerchantCode,
                                                    OrderNo = withdrawalOrder.OrderNumber,
                                                    BenBankName = withdrawalOrder.BenBankName,
                                                    BenAccountNo = withdrawalOrder.BenAccountNo,
                                                    BenAccountName = withdrawalOrder.BenAccountName,
                                                    OrderMoney = withdrawalOrder.OrderMoney,
                                                    MerchantId = withdrawalOrder.MerchantId
                                                };
                                                _redisService.SetRPushTransferOrder(deviceInfo.BankType.ToString(), deviceInfo.Phone, orderRedisModel);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region 获取出款订单进行成功处理

                    var time = DateTime.Now.AddMinutes(-60);
                    var orderList = await _withdrawalOrdersMongoService.GetWithdrawOrderByFk(merchant.MerchantCode);
                    var devices = _withdrawalDevicesService.GetWhere(r => r.MerchantCode == merchant.MerchantCode);
                    foreach (var order in orderList)
                    {
                        var deviceName = devices.FirstOrDefault(r => r.Id == order.DeviceId)?.Name;
                        //编辑备注
                        var callbackmemo = order.Description + " -> " + deviceName + " -> : Mã tra soát -> " + order.TransactionNo;
                        var updateMemo = platformWebHelper.UpdateMemo(Convert.ToInt32(order.OrderNumber), callbackmemo);
                        if (updateMemo)
                        {
                            //更新平台
                            var result = platformWebHelper.VerifyWithdrawAllow(Convert.ToInt32(order.OrderNumber));
                            if (result)
                            {
                                //更新状态
                                order.NotifyNumber = 1;
                                order.NotifyStatus = WithdrawalNotifyStatusEnum.Success;
                                await _withdrawalOrdersMongoService.UpdateAsync(order);
                            }
                        }
                    }

                    #endregion
                }catch (Exception ex)
                {
                    NlogLogger.Error("风控出款异常：" + ex.ToString());
                }

                await Task.Delay(1000 * 10, stoppingToken);
            }
        }
    }
}
