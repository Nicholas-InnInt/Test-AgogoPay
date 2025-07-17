using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.WithdrawalOrders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.BillingExtensions;

namespace Neptune.NsPay.MerchantBillsRecoverWorker
{
    public class Worker : BackgroundService
    {
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly ILogger<Worker> _logger;
        private readonly IMerchantBillsMongoService _merchantBillsMongoService;
        private readonly IRedisService _redisService;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersService;
        private readonly IMerchantWithdrawService _merchantWithdrawService;
        private readonly IMerchantBillsHelper _merchantBillsHelper;


        public Worker(ILogger<Worker> logger,
            IPayOrdersMongoService payOrdersMongoService,
            IMerchantBillsMongoService merchantBillsMongoService,
            IRedisService redisService,
            IWithdrawalOrdersMongoService withdrawalOrdersService,
            IMerchantWithdrawService merchantWithdrawService,
            IMerchantBillsHelper merchantBillsHelper)
        {
            _logger = logger;
            _payOrdersMongoService = payOrdersMongoService;
            _merchantBillsMongoService = merchantBillsMongoService;
            _redisService = redisService;
            _withdrawalOrdersService = withdrawalOrdersService;
            _merchantWithdrawService = merchantWithdrawService;
            _merchantBillsHelper = merchantBillsHelper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var minMinutesStr = AppSettings.Configuration["SyncOrderSettings:ToMinutes"];
            var maxMinutesStr = AppSettings.Configuration["SyncOrderSettings:FromMinutes"];
            var intervalStr = AppSettings.Configuration["SyncOrderSettings:Interval"];


            int minMinutes = int.TryParse(minMinutesStr, out minMinutes) ? minMinutes : 10;
            int maxMinutes = int.TryParse(maxMinutesStr, out maxMinutes)? maxMinutes:30;
            int intervalSeconds = int.TryParse(intervalStr, out intervalSeconds)? intervalSeconds:10;

            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            _logger.LogInformation("Worker Param , From Minutes {minM} min , To Minutes {minMax} min , Interval {intervalSec} seconds ", maxMinutes, minMinutes, intervalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                var processingWatch = new Stopwatch();
                var processingWatchAll = new Stopwatch();
                processingWatchAll.Start();
               
                StringBuilder logContent = new StringBuilder();
                try
                {
                    var startDate = DateTime.Now.AddMinutes(-maxMinutes);
                    var endDate = DateTime.Now.AddMinutes(-minMinutes);
                   
                    _logger.LogInformation("Worker Grabbing Order From {startTime} To {endTime}", startDate.ToString("yyyy-MM-dd HH:mm:ss fff"), endDate.ToString("yyyy-MM-dd HH:mm:ss fff"));

                    #region 充值订单流水
                    processingWatch.Start();

                    var completedOrder = await _payOrdersMongoService.GetPayOrderByDateRangeAndStatus(startDate, endDate, PayOrders.PayOrderOrderStatusEnum.Completed);

                    if (completedOrder != null && completedOrder.Count > 0)
                    {
                        var merchantOrder = await _merchantBillsMongoService.GetMerchantBillByDateRange(startDate, DateTime.Now);

                        if (merchantOrder != null)
                        {
                            foreach (var missingBills in completedOrder.GroupJoin(merchantOrder.Where(x => x.BillType == MerchantBillTypeEnum.OrderIn),
                                t1 => new { MerchantCode = t1.MerchantCode, OrderNumber = t1.OrderNumber },
                                t2 => new { MerchantCode = t2.MerchantCode, OrderNumber = t2.BillNo },
                                (t1, t2) => new { OrderNumber = t1.OrderNumber, OrderId = t1.ID, MerchantCode = t1.MerchantCode, TargetBills = t2.FirstOrDefault() }).Where(x => x.TargetBills == null))
                            {

                                using (_redisService.GetMerchantBalanceLock(missingBills.MerchantCode))
                                {
                                    await UpdatePayOrderMerchantBills(missingBills.OrderId, missingBills.OrderNumber, missingBills.MerchantCode);
                                }

                                _logger.LogInformation("process Items:  {Merchant} -  {OrderNumber}", missingBills.MerchantCode, missingBills.OrderNumber);

                            }
                        }
                    }

                    processingWatch.Stop();

                    logContent.Append( "Time Taken Pay Order - "+ processingWatch.ElapsedMilliseconds + " ms." );
                    #endregion


                    #region 商户代付流水

                    processingWatch.Restart();

                    var successTransferOrder = await _withdrawalOrdersService.GetWithdrawOrderByDateRange(startDate, endDate, WithdrawalOrderStatusEnum.Success);

                    if(successTransferOrder != null && successTransferOrder.Count>0)
                    {

                        var merchantOrder = await _merchantBillsMongoService.GetMerchantBillByDateRange(startDate, DateTime.Now);

                        if (merchantOrder != null)
                        {
                            foreach (var missingBills in successTransferOrder.GroupJoin(merchantOrder.Where(x=>x.BillType == MerchantBillTypeEnum.Withdraw),
                                t1 => new { MerchantCode = t1.MerchantCode, OrderNumber = t1.OrderNumber },
                                t2 => new { MerchantCode = t2.MerchantCode, OrderNumber = t2.BillNo },
                                (t1, t2) => new { OrderNumber = t1.OrderNumber, OrderId = t1.ID, MerchantCode = t1.MerchantCode, TargetBills = t2.FirstOrDefault() }).Where(x => x.TargetBills == null))
                            {

                                using (_redisService.GetMerchantBalanceLock(missingBills.MerchantCode))
                                {
                                    await UpdateTransferOrderMerchantBills(missingBills.OrderId, missingBills.OrderNumber, missingBills.MerchantCode);
                                }

                                _logger.LogInformation("process Items Transfer Order:  {Merchant} -  {OrderNumber}", missingBills.MerchantCode, missingBills.OrderNumber);

                            }
                        }

                    }

                    processingWatch.Stop();
                    logContent.Append("Time Taken Transfer Order - " + processingWatch.ElapsedMilliseconds + " ms.");

                    #endregion


                    #region 商户提款流水

                    processingWatch.Restart();
                    var successWithdrawalOrder = await _merchantWithdrawService.GetWithdrawalByDateRange(startDate, endDate, MerchantWithdrawStatusEnum.Pass);

                    if (successWithdrawalOrder != null && successWithdrawalOrder.Count > 0)
                    {

                        var merchantOrder = await _merchantBillsMongoService.GetMerchantBillByDateRange(startDate, DateTime.Now);

                        if (merchantOrder != null)
                        {
                            foreach (var missingBills in successWithdrawalOrder.GroupJoin(merchantOrder.Where(x => x.BillType == MerchantBillTypeEnum.Withdraw),
                                t1 => new { MerchantCode = t1.MerchantCode, OrderNumber = t1.WithDrawNo },
                                t2 => new { MerchantCode = t2.MerchantCode, OrderNumber = t2.BillNo },
                                (t1, t2) => new { OrderNumber = t1.WithDrawNo, OrderId = t1.Id, MerchantCode = t1.MerchantCode, TargetBills = t2.FirstOrDefault() }).Where(x => x.TargetBills == null))
                            {

                                using (_redisService.GetMerchantBalanceLock(missingBills.MerchantCode))
                                {
                                    await UpdateWithdrawalOrderMerchantBills(missingBills.OrderId, missingBills.OrderNumber, missingBills.MerchantCode);
                                }

                                _logger.LogInformation("process Items Withdrawal Order:  {Merchant} -  {OrderNumber}", missingBills.MerchantCode, missingBills.OrderNumber);

                            }
                        }

                    }

                    processingWatch.Stop();
                    logContent.Append("Time Taken Withdraws Order - " + processingWatch.ElapsedMilliseconds + " ms.");

                    #endregion

                }
                catch (Exception ex)
                {
                    NlogLogger.Error("商户订单流水重试异常：" + ex);
                }
                finally
                {
                    processingWatchAll.Stop();
                    logContent.Append("Total Cycle Time Taken  - " + processingWatchAll.ElapsedMilliseconds + " ms.");
                }

                _logger.LogInformation(logContent.ToString());

                await Task.Delay(1000 * intervalSeconds, stoppingToken);

            }
        }

        private async Task UpdatePayOrderMerchantBills(string payOrderId,string orderNumber , string merchantCode)
        {
            var payOrder = await _payOrdersMongoService.GetPayOrderByOrderId(payOrderId);
            if (payOrder != null)
            {
                if (payOrder.OrderStatus == PayOrderOrderStatusEnum.Completed)
                {
                    var info = await _merchantBillsMongoService.GetMerchantBillByOrderNo(merchantCode, payOrder.OrderNumber, MerchantBillTypeEnum.OrderIn);
                    if (info == null)
                    {
                        await _merchantBillsHelper.AddRetryPayOrderBillAsync(merchantCode, payOrder.ID);
                    }
                }
            }
        }

        private async Task UpdateTransferOrderMerchantBills(string transferOrderId, string orderNumber, string merchantCode)
        {

            var withdrawOrder = await _withdrawalOrdersService.GetWithdrawOrderByOrderId(merchantCode, transferOrderId);
            if (withdrawOrder != null)
            {
                if (withdrawOrder.OrderStatus == WithdrawalOrderStatusEnum.Success)
                {
                    var info = await _merchantBillsMongoService.GetMerchantBillByOrderNo(merchantCode, orderNumber, MerchantBillTypeEnum.Withdraw);
                    if (info == null)
                    {
                        await _merchantBillsHelper.AddRetryWithdrawalOrderBillAsync(merchantCode, withdrawOrder.ID);
                    }
                }
            }
        }

        private async Task UpdateWithdrawalOrderMerchantBills(long withdrawalOrderId, string orderNumber, string merchantCode)
        {
            var merchantWithdraw = await _merchantWithdrawService.GetFirstAsync(r => r.Id == withdrawalOrderId);
            if (merchantWithdraw != null)
            {
                if (merchantWithdraw.Status == MerchantWithdrawStatusEnum.Pass)
                {
                    var info = await _merchantBillsMongoService.GetMerchantBillByOrderNo(merchantCode, merchantWithdraw.WithDrawNo, MerchantBillTypeEnum.Withdraw);
                    if (info == null)
                    {
                        MerchantWithdrawMongoEntity merchantWithdrawMongoEntity = new MerchantWithdrawMongoEntity()
                        {
                            MerchantCode = merchantWithdraw.MerchantCode,
                            MerchantId = merchantWithdraw.MerchantId,
                            WithDrawNo = merchantWithdraw.WithDrawNo,
                            Money = merchantWithdraw.Money,
                            ReviewTime = merchantWithdraw.ReviewTime,
                        };
                        await _merchantBillsHelper.AddRetryMerchantWithdrawBillAsync(merchantCode, merchantWithdrawMongoEntity);
                    }
                }
            }
        }

    }
}
