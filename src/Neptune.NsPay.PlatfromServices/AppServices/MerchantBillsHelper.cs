using Abp.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.WithdrawalOrders;

namespace Neptune.NsPay.PlatfromServices.AppServices
{
    //public class MerchantBillsHelper: IMerchantBillsHelper
    //{
    //    private readonly IRedisService _redisService;

    //    public MerchantBillsHelper(IRedisService redisService,
    //        IMerchantBillsMongoService merchantBillsMongoService)
    //    {
    //        _redisService = redisService;
    //    }

    //    #region 代收订单

    //    public async Task<bool> UpdateWithRetryAddPayOrderBillAsync(string merchantCode, PayOrdersMongoEntity payOrder)
    //    {
    //        int maxRetries = 5; // 最大重试次数
    //        int retryCount = 0;
    //        bool success = false;

    //        while (retryCount < maxRetries)
    //        {
    //            try
    //            {
    //                // 执行更新操作
    //                //只有当WriteConflict 才会重试 ， 其他直接终止重试机制
    //                var result = await AddPayOrderBill(merchantCode, payOrder); // if hitting error will direct thrown out exception
    //                success = result;
    //                break;
    //            }
    //            catch (MongoCommandException ex)
    //            {
    //                if (ex.Code == 112)// 112 表示 WriteConflict
    //                {
    //                    retryCount++;
    //                }
    //                else
    //                {
    //                    throw; // 除了112 使用重试机制其他的一律报错
    //                }
    //                if (retryCount >= maxRetries)
    //                {
    //                    NlogLogger.Error("添加代收流水错误：" + "数据Merchant：" + merchantCode + ",数据PayOrder：" + payOrder.ToJsonString() + "，错误信息" + ex);
    //                    throw; // 超过最大重试次数，抛出异常
    //                }

    //                // 使用指数退避策略进行延迟
    //                int delay = (int)Math.Pow(2, retryCount) * 100; // 2 的指数递增延迟
    //                await Task.Delay(delay);
    //            }
    //        }

    //        return success;
    //    }


    //    /// <summary>
    //    /// 代收订单
    //    /// </summary>
    //    /// <param name="merchantCode"></param>
    //    /// <param name="payOrder"></param>
    //    /// <returns></returns>
    //    private async Task<bool> AddPayOrderBill(string merchantCode, PayOrdersMongoEntity payOrder)
    //    {
    //        try
    //        {
    //            var cacheKey = $"payorder-order:{merchantCode}:{payOrder.OrderNumber}";
    //            var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
    //            if (isProcessed)
    //            {
    //                NlogLogger.Info("PayOrder order already processed: " + payOrder.OrderNumber);
    //                return true; // 直接返回，避免重复处理
    //            }

    //            using (var TN = DB.Transaction())
    //            {
    //                var merchantFund = await TN.Find<MerchantFundsMongoEntity>()
    //                                   .Match(r => r.MerchantCode == merchantCode)
    //                                   .ExecuteSingleAsync();
    //                var balance = 0M;
    //                var BalanceBefore = 0M;
    //                if (merchantFund != null)
    //                {
    //                    BalanceBefore = merchantFund.Balance;
    //                    balance = merchantFund.Balance;
    //                }

    //                long transactionUnixTime = 0;
    //                if (payOrder.TransactionTime == DateTime.MinValue)
    //                {
    //                    transactionUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);
    //                }
    //                else
    //                {
    //                    transactionUnixTime = TimeHelper.GetUnixTimeStamp(payOrder.TransactionTime);
    //                }

    //                var existingBill = await TN.Find<MerchantBillsMongoEntity>()
    //                    .Match(b => b.BillNo == payOrder.OrderNumber && b.MerchantCode == payOrder.MerchantCode)
    //                    .ExecuteSingleAsync();

    //                if (existingBill != null)
    //                {
    //                    throw new Exception($"Duplicate bill detected: {payOrder.OrderNumber}");
    //                }

    //                //// 检查交易时间是否有效，确保按时间顺序处理
    //                //if (payOrder.TransactionTime < merchantFund.LastPayOrderTransactionTime)
    //                //{
    //                //    // 如果当前交易时间早于上次交易时间，表示无效或重复的交易
    //                //    NlogLogger.Info($"Transaction time {payOrder.TransactionTime} is earlier than last transaction time {merchantFund.LastPayOrderTransactionTime}. Skipping {payOrder.OrderNumber}");
    //                //    return false;
    //                //}

    //                //插入流水
    //                MerchantBillsMongoEntity merchantBillsMongo = new MerchantBillsMongoEntity()
    //                {
    //                    MerchantCode = payOrder.MerchantCode,
    //                    MerchantId = payOrder.MerchantId,
    //                    PlatformCode = payOrder.PlatformCode,
    //                    BillNo = payOrder.OrderNumber,
    //                    BillType = MerchantBillTypeEnum.OrderIn,
    //                    Money = payOrder.OrderMoney,
    //                    Rate = payOrder.Rate,
    //                    FeeMoney = payOrder.FeeMoney,
    //                    BalanceBefore = BalanceBefore,
    //                    BalanceAfter = balance + (payOrder.OrderMoney - payOrder.FeeMoney),
    //                    //CreationTime = payOrder.TransactionTime,
    //                    //CreationUnixTime = transactionUnixTime,
    //                    OrderTime = payOrder.CreationTime,
    //                    OrderUnixTime = payOrder.CreationUnixTime,
    //                    TransactionTime = payOrder.TransactionTime,
    //                    TransactionUnixTime = transactionUnixTime
    //                };
    //                await TN.SaveAsync(merchantBillsMongo);

    //                var depositAmount = 0M;
    //                var rateFeeBalance = 0M;

    //                //更新余额
    //                if (merchantFund != null)
    //                {
    //                    depositAmount = merchantFund.DepositAmount + payOrder.OrderMoney;
    //                    rateFeeBalance = merchantFund.RateFeeBalance + payOrder.FeeMoney;
    //                    balance = merchantFund.Balance + (payOrder.OrderMoney - payOrder.FeeMoney);
    //                    await TN.Update<MerchantFundsMongoEntity>()
    //                            .Match(r => r.MerchantCode == merchantCode)
    //                            .Modify(r => r.DepositAmount, depositAmount)
    //                            .Modify(r => r.RateFeeBalance, rateFeeBalance)
    //                            .Modify(r => r.LastPayOrderTransactionTime, payOrder.TransactionTime) // 更新上次交易时间
    //                            .Modify(r => r.Balance, balance)
    //                            .Modify(r => r.UpdateTime, DateTime.Now)
    //                            .Modify(r => r.UpdateUnixTime, TimeHelper.GetUnixTimeStamp(DateTime.Now))
    //                            .ExecuteAsync();
    //                }
    //                else
    //                {
    //                    depositAmount = payOrder.OrderMoney;
    //                    rateFeeBalance = payOrder.FeeMoney;
    //                    balance = payOrder.OrderMoney - payOrder.FeeMoney;
    //                    MerchantFundsMongoEntity fundsMongoEntity = new MerchantFundsMongoEntity()
    //                    {
    //                        MerchantCode = merchantCode,
    //                        MerchantId = payOrder.MerchantId,
    //                        DepositAmount = depositAmount,
    //                        TranferAmount = 0,
    //                        WithdrawalAmount = 0,
    //                        RateFeeBalance = rateFeeBalance,
    //                        LastPayOrderTransactionTime = payOrder.TransactionTime,
    //                        Balance = balance,
    //                        UpdateTime = DateTime.Now,
    //                        UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now),
    //                    };
    //                    await TN.SaveAsync(fundsMongoEntity);
    //                }

    //                await TN.CommitAsync();
    //            }

    //            //// 在 Redis 中标记提现订单已处理
    //            _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromMinutes(30)); // 设置过期时间

    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            NlogLogger.Error("PayOrder" + payOrder.ToJsonString() + "添加代收流水错误：" + ex);
    //            throw ex;
    //            return false;
    //        }
    //    }

    //    #endregion

    //    #region 代付订单

    //    public async Task<bool> UpdateWithRetryAddWithdrawalOrderBillAsync(string merchantCode, WithdrawalOrdersMongoEntity withdrawalOrder)
    //    {
    //        int maxRetries = 5; // 最大重试次数
    //        int retryCount = 0;
    //        bool success = false;

    //        while (retryCount < maxRetries)
    //        {
    //            try
    //            {
    //                // 执行更新操作
    //                //只有当WriteConflict 才会重试 ， 其他直接终止重试机制
    //                var result = await AddWithdrawalOrderBill(merchantCode, withdrawalOrder);
    //                success = result;
    //                break;
    //            }
    //            catch (MongoCommandException ex) //when (ex.Code == 112) // 112 表示 WriteConflict
    //            {


    //                if (ex.Code == 112)// 112 表示 WriteConflict
    //                {
    //                    retryCount++;
    //                }
    //                else
    //                {
    //                    throw; // 除了112 使用重试机制其他的一律报错
    //                }
    //                if (retryCount >= maxRetries)
    //                {
    //                    NlogLogger.Error("添加代付流水错误：" + "数据Merchant：" + merchantCode + ",数据withdrawalOrder：" + withdrawalOrder.ToJsonString() + "，错误信息" + ex);
    //                    throw; // 超过最大重试次数，抛出异常
    //                }

    //                #region commented out


    //                #endregion
    //            }
    //        }

    //        return success;
    //    }

    //    /// <summary>
    //    /// 代付订单
    //    /// </summary>
    //    /// <param name="merchantCode"></param>
    //    /// <param name="withdrawalOrder"></param>
    //    /// <returns></returns>
    //    private async Task<bool> AddWithdrawalOrderBill(string merchantCode, WithdrawalOrdersMongoEntity withdrawalOrder)
    //    {
    //        try
    //        {
    //            var cacheKey = $"withdrawalorder-order:{merchantCode}:{withdrawalOrder.OrderNumber}";
    //            var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
    //            if (isProcessed)
    //            {
    //                NlogLogger.Info("WithdrawalOrder order already processed: " + withdrawalOrder.OrderNumber);
    //                return true; // 直接返回，避免重复处理
    //            }

    //            using (var TN = DB.Transaction())
    //            {
    //                var merchantFund = await TN.Find<MerchantFundsMongoEntity>()
    //                                   .Match(r => r.MerchantCode == merchantCode)
    //                                   .ExecuteSingleAsync();
    //                var balance = 0M;
    //                var BalanceBefore = 0M;

    //                if (merchantFund != null)
    //                {
    //                    BalanceBefore = merchantFund.Balance;
    //                    balance = merchantFund.Balance;
    //                }

    //                long transactionUnixTime = 0;
    //                if (withdrawalOrder.TransactionTime == DateTime.MinValue)
    //                {
    //                    transactionUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);
    //                }
    //                else
    //                {
    //                    transactionUnixTime = TimeHelper.GetUnixTimeStamp(withdrawalOrder.TransactionTime);
    //                }

    //                var existingBill = await TN.Find<MerchantBillsMongoEntity>()
    //                .Match(b => b.BillNo == withdrawalOrder.OrderNumber && b.MerchantCode == withdrawalOrder.MerchantCode)
    //                .ExecuteSingleAsync();

    //                if (existingBill != null)
    //                {
    //                    throw new Exception($"Duplicate bill detected: {withdrawalOrder.OrderNumber}");
    //                }

    //                //// 检查交易时间是否有效，确保按时间顺序处理
    //                //if (withdrawalOrder.TransactionTime < merchantFund.LastWithdrawalOrderTransactionTime)
    //                //{
    //                //    // 如果当前交易时间早于上次交易时间，表示无效或重复的交易
    //                //    NlogLogger.Info($"Transaction time {withdrawalOrder.TransactionTime} is earlier than last transaction time {merchantFund.LastWithdrawalOrderTransactionTime}. Skipping {withdrawalOrder.OrderNumber}");
    //                //    return false;
    //                //}

    //                //插入流水
    //                MerchantBillsMongoEntity merchantBillsMongo = new MerchantBillsMongoEntity()
    //                {
    //                    MerchantCode = withdrawalOrder.MerchantCode,
    //                    MerchantId = withdrawalOrder.MerchantId,
    //                    PlatformCode = withdrawalOrder.PlatformCode,
    //                    BillNo = withdrawalOrder.OrderNumber,
    //                    BillType = MerchantBillTypeEnum.Withdraw,
    //                    Money = withdrawalOrder.OrderMoney,
    //                    Rate = withdrawalOrder.Rate,
    //                    FeeMoney = withdrawalOrder.FeeMoney,
    //                    BalanceBefore = BalanceBefore,
    //                    BalanceAfter = balance - (withdrawalOrder.OrderMoney + withdrawalOrder.FeeMoney),
    //                    //CreationTime = withdrawalOrder.TransactionTime,
    //                    //CreationUnixTime = transactionUnixTime,
    //                    OrderTime = withdrawalOrder.CreationTime,
    //                    OrderUnixTime = withdrawalOrder.CreationUnixTime,
    //                    TransactionTime = withdrawalOrder.TransactionTime,
    //                    TransactionUnixTime = transactionUnixTime
    //                };
    //                await TN.SaveAsync(merchantBillsMongo);

    //                var tranferAmount = 0M;
    //                var rateFeeBalance = 0M;

    //                //更新余额
    //                if (merchantFund != null)
    //                {
    //                    tranferAmount = merchantFund.TranferAmount + withdrawalOrder.OrderMoney;
    //                    rateFeeBalance = merchantFund.RateFeeBalance + withdrawalOrder.FeeMoney;
    //                    balance = merchantFund.Balance - (withdrawalOrder.OrderMoney + withdrawalOrder.FeeMoney);
    //                    await TN.Update<MerchantFundsMongoEntity>()
    //                            .Match(r => r.MerchantCode == merchantCode)
    //                            .Modify(r => r.TranferAmount, tranferAmount)
    //                            .Modify(r => r.RateFeeBalance, rateFeeBalance)
    //                            .Modify(r => r.LastWithdrawalOrderTransactionTime, withdrawalOrder.TransactionTime) // 更新上次交易时间
    //                            .Modify(r => r.Balance, balance)
    //                            .Modify(r => r.UpdateTime, DateTime.Now)
    //                            .Modify(r => r.UpdateUnixTime, TimeHelper.GetUnixTimeStamp(DateTime.Now))
    //                            .ExecuteAsync();
    //                }
    //                else
    //                {
    //                    tranferAmount = withdrawalOrder.OrderMoney;
    //                    rateFeeBalance = withdrawalOrder.FeeMoney;
    //                    balance = 0 - (withdrawalOrder.OrderMoney + withdrawalOrder.FeeMoney);
    //                    MerchantFundsMongoEntity fundsMongoEntity = new MerchantFundsMongoEntity()
    //                    {
    //                        MerchantCode = merchantCode,
    //                        MerchantId = withdrawalOrder.MerchantId,
    //                        TranferAmount = tranferAmount,
    //                        DepositAmount = 0,
    //                        WithdrawalAmount = 0,
    //                        RateFeeBalance = rateFeeBalance,
    //                        Balance = balance,
    //                        LastWithdrawalOrderTransactionTime = withdrawalOrder.TransactionTime,
    //                        UpdateTime = DateTime.Now,
    //                        UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now),
    //                    };
    //                    await TN.SaveAsync(fundsMongoEntity);
    //                }

    //                await TN.CommitAsync();
    //            }

    //            //// 在 Redis 中标记提现订单已处理
    //            _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromMinutes(30)); // 设置过期时间

    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            NlogLogger.Error("WithdrawalOrder:"+ withdrawalOrder.ToJsonString() + "添加代付流水错误：" + ex);
    //            throw ex;
    //            return false;
    //        }
    //    }

    //    #endregion

    //    #region 商户提现

    //    public async Task<bool> UpdateWithRetryAddMerchantWithdrawBillBillAsync(string merchantCode, MerchantWithdrawMongoEntity merchantWithdraw)
    //    {
    //        int maxRetries = 5; // 最大重试次数
    //        int retryCount = 0;
    //        bool success = false;

    //        while (retryCount < maxRetries)
    //        {
    //            try
    //            {
    //                // 执行更新操作
    //                //只有当WriteConflict 才会重试 ， 其他直接终止重试机制
    //                var result = await AddMerchantWithdrawBill(merchantCode, merchantWithdraw);
    //                success = result;
    //                break;

    //            }
    //            catch (MongoCommandException ex)// when (ex.Code == 112) // 112 表示 WriteConflict
    //            {
    //                //retryCount++;
    //                if (ex.Code == 112)// 112 表示 WriteConflict
    //                {
    //                    retryCount++;
    //                }
    //                else
    //                {
    //                    throw; // 除了112 使用重试机制其他的一律报错
    //                }

    //                if (retryCount >= maxRetries)
    //                {
    //                    NlogLogger.Error("添加出款流水错误：" + "数据Merchant：" + merchantCode + ",数据merchantWithdraw：" + merchantWithdraw.ToJsonString() + "，错误信息" + ex);
    //                    throw; // 超过最大重试次数，抛出异常
    //                }

    //                // 使用指数退避策略进行延迟
    //                int delay = (int)Math.Pow(2, retryCount) * 100; // 2 的指数递增延迟
    //                await Task.Delay(delay);
    //            }
    //        }

    //        return success;
    //    }

    //    /// <summary>
    //    /// 商户提现
    //    /// </summary>
    //    /// <param name="merchantCode"></param>
    //    /// <param name="merchantWithdraw"></param>
    //    /// <returns></returns>
    //    public async Task<bool> AddMerchantWithdrawBill(string merchantCode, MerchantWithdrawMongoEntity merchantWithdraw)
    //    {
    //        try
    //        {
    //            var cacheKey = $"merchantwithdraw-order:{merchantCode}:{merchantWithdraw.WithDrawNo}";
    //            var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
    //            if (isProcessed)
    //            {
    //                NlogLogger.Info("MerchantWithdraw order already processed: " + merchantWithdraw.WithDrawNo);
    //                return true; // 直接返回，避免重复处理
    //            }

    //            using (var TN = DB.Transaction())
    //            {
    //                var merchantFund = await TN.Find<MerchantFundsMongoEntity>()
    //                                   .Match(r => r.MerchantCode == merchantCode)
    //                                   .ExecuteSingleAsync();
    //                var BalanceBefore = 0M;
    //                var balance = 0M;
    //                if (merchantFund != null)
    //                {
    //                    BalanceBefore = merchantFund.Balance;
    //                    balance = merchantFund.Balance;
    //                }

    //                long transactionUnixTime = 0;
    //                if (merchantWithdraw.ReviewTime == DateTime.MinValue)
    //                {
    //                    transactionUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);
    //                }
    //                else
    //                {
    //                    transactionUnixTime = TimeHelper.GetUnixTimeStamp(merchantWithdraw.ReviewTime);
    //                }

    //                var existingBill = await TN.Find<MerchantBillsMongoEntity>()
    //                    .Match(b => b.BillNo == merchantWithdraw.WithDrawNo && b.MerchantCode == merchantWithdraw.MerchantCode)
    //                    .ExecuteSingleAsync();

    //                if (existingBill != null)
    //                {
    //                    throw new Exception($"Duplicate bill detected: {merchantWithdraw.WithDrawNo}");
    //                }

    //                // 检查交易时间是否有效，确保按时间顺序处理
    //                //if (merchantWithdraw.ReviewTime < merchantFund.LastMerchantWithdrawalTransactionTime)
    //                //{
    //                //    // 如果当前交易时间早于上次交易时间，表示无效或重复的交易
    //                //    NlogLogger.Info($"Transaction time {merchantWithdraw.ReviewTime} is earlier than last transaction time {merchantFund.LastMerchantWithdrawalTransactionTime}. Skipping {merchantWithdraw.WithDrawNo}");
    //                //    return false;
    //                //}


    //                //插入流水
    //                MerchantBillsMongoEntity merchantBillsMongo = new MerchantBillsMongoEntity()
    //                {
    //                    MerchantCode = merchantWithdraw.MerchantCode,
    //                    MerchantId = merchantWithdraw.MerchantId,
    //                    BillNo = merchantWithdraw.WithDrawNo,
    //                    BillType = MerchantBillTypeEnum.Withdraw,
    //                    Money = merchantWithdraw.Money,
    //                    Rate = 0,
    //                    FeeMoney = 0,
    //                    BalanceBefore = BalanceBefore,
    //                    BalanceAfter = balance - merchantWithdraw.Money,
    //                    //CreationTime = merchantWithdraw.ReviewTime,
    //                    //CreationUnixTime = transactionUnixTime,
    //                    OrderTime = merchantWithdraw.CreationTime,
    //                    OrderUnixTime = TimeHelper.GetUnixTimeStamp(merchantWithdraw.CreationTime),
    //                    TransactionTime = merchantWithdraw.ReviewTime,
    //                    TransactionUnixTime = transactionUnixTime
    //                };
    //                await TN.SaveAsync(merchantBillsMongo);

    //                var withdrawalAmount = 0M;

    //                //更新余额
    //                if (merchantFund != null)
    //                {
    //                    withdrawalAmount = merchantFund.WithdrawalAmount + merchantWithdraw.Money;
    //                    balance = merchantFund.Balance - merchantWithdraw.Money;
    //                    await TN.Update<MerchantFundsMongoEntity>()
    //                            .Match(r => r.MerchantCode == merchantCode)
    //                            .Modify(r => r.WithdrawalAmount, withdrawalAmount)
    //                            .Modify(r => r.LastMerchantWithdrawalTransactionTime, merchantWithdraw.ReviewTime)
    //                            .Modify(r => r.Balance, balance)
    //                            .Modify(r => r.UpdateTime, DateTime.Now)
    //                            .Modify(r => r.UpdateUnixTime, TimeHelper.GetUnixTimeStamp(DateTime.Now))
    //                            .ExecuteAsync();
    //                }
    //                else
    //                {
    //                    withdrawalAmount = merchantWithdraw.Money;
    //                    balance = 0 - merchantWithdraw.Money;
    //                    MerchantFundsMongoEntity fundsMongoEntity = new MerchantFundsMongoEntity()
    //                    {
    //                        MerchantCode = merchantCode,
    //                        MerchantId = merchantWithdraw.MerchantId,
    //                        TranferAmount = 0,
    //                        DepositAmount = 0,
    //                        WithdrawalAmount = withdrawalAmount,
    //                        RateFeeBalance = 0,
    //                        LastMerchantWithdrawalTransactionTime = merchantWithdraw.ReviewTime,
    //                        Balance = balance,
    //                        UpdateTime = DateTime.Now,
    //                        UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now),
    //                    };
    //                    await TN.SaveAsync(fundsMongoEntity);
    //                }

    //                await TN.CommitAsync();
    //            }


    //            //// 在 Redis 中标记提现订单已处理
    //            _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromMinutes(30)); // 设置过期时间

    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            NlogLogger.Error("MerchantWithdrawal:"+ merchantWithdraw.ToJsonString() + "添加商户提现流水错误：" + ex);
    //            throw ex;
    //            return false;
    //        }
    //    }


    //    #endregion
    //}
}
