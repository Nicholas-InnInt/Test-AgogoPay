using Abp.Json;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Entities;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.WithdrawalOrders;

namespace Neptune.NaPay.BillingExtensions
{
    public class MerchantBillsHelper : IMerchantBillsHelper
    {
        private readonly IRedisService _redisService;

        public MerchantBillsHelper(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<bool> AddMerchantFundsFrozenBalance(string merchantCode, decimal orderMoney)
        {
            var result = await DB.Update<MerchantFundsMongoEntity>()
                                 .Match(r => r.MerchantCode == merchantCode)
                                 .Modify(b => b.Inc(x => x.FrozenBalance, orderMoney))
                                 .Modify(b => b.Set(x => x.UpdateTime, DateTime.Now))
                                 .Modify(b => b.Set(x => x.UpdateUnixTime, TimeHelper.GetUnixTimeStamp(DateTime.Now)))
                                 .ExecuteAsync();
            return result.ModifiedCount > 0;
        }

        #region 代收订单

        public async Task<bool> UpdateWithRetryAddPayOrderBillAsync(string merchantCode, string orderId)
        {
            int maxRetries = 5; // 最大重试次数
            int retryCount = 0;
            bool success = false;

            while (retryCount < maxRetries)
            {
                try
                {
                    // 执行更新操作
                    //只有当WriteConflict 才会重试 ， 其他直接终止重试机制
                    var result = await AddPayOrderBill(merchantCode, orderId); // if hitting error will direct thrown out exception
                    success = result;
                    break;
                }
                catch (MongoCommandException ex)
                {
                    if (ex.Code == 112)// 112 表示 WriteConflict
                    {
                        retryCount++;
                    }
                    else
                    {
                        throw; // 除了112 使用重试机制其他的一律报错
                    }
                    if (retryCount >= maxRetries)
                    {
                        NlogLogger.Error("添加代收流水错误：" + "数据Merchant：" + merchantCode + ",数据PayOrder：" + orderId + "，错误信息" + ex);
                        throw; // 超过最大重试次数，抛出异常
                    }

                    // 使用指数退避策略进行延迟

                    await Task.Delay(DBHelper.GetRetryInterval(retryCount));
                }
            }

            return success;
        }


        /// <summary>
        /// 代收订单
        /// </summary>
        /// <param name="merchantCode"></param>
        /// <param name="payOrder"></param>
        /// <returns></returns>
        private async Task<bool> AddPayOrderBill(string merchantCode, string orderId)
        {
            var payOrder = await DB.Find<PayOrdersMongoEntity>().OneAsync(orderId);
            if (payOrder == null) { return false; }
            var cacheKey = $"payorder-order:{merchantCode}:{payOrder.OrderNumber}";
            var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
            if (isProcessed)
            {
                NlogLogger.Info("PayOrder order already processed: " + payOrder.OrderNumber);
                return true; // 直接返回，避免重复处理
            }
            using (var TN = DB.Transaction())
            {
                try
                {
                    var merchantFund = await TN.Find<MerchantFundsMongoEntity>()
                                       .Match(r => r.MerchantCode == merchantCode)
                                       .ExecuteSingleAsync();

                    decimal balanceBefore = merchantFund?.Balance ?? 0M;
                    decimal fee = payOrder.FeeMoney;
                    decimal amount = payOrder.OrderMoney;
                    decimal netAmount = amount - fee;

                    var transactionUnixTime = TimeHelper.GetUnixTimeStamp(
                        payOrder.TransactionTime == DateTime.MinValue ? DateTime.Now : payOrder.TransactionTime);

                    var existingBill = await TN.Find<MerchantBillsMongoEntity>()
                        .Match(b => b.BillNo == payOrder.OrderNumber && b.MerchantCode == payOrder.MerchantCode)
                        .ExecuteAnyAsync();

                    if (existingBill)
                    {
                        throw new Exception($"Duplicate bill detected: {payOrder.OrderNumber}");
                    }

                    //插入流水
                    var merchantBillsMongo = new MerchantBillsMongoEntity()
                    {
                        MerchantCode = payOrder.MerchantCode,
                        MerchantId = payOrder.MerchantId,
                        PlatformCode = payOrder.PlatformCode,
                        BillNo = payOrder.OrderNumber,
                        BillType = MerchantBillTypeEnum.OrderIn,
                        Money = payOrder.OrderMoney,
                        Rate = payOrder.Rate,
                        FeeMoney = payOrder.FeeMoney,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceBefore + (payOrder.OrderMoney - payOrder.FeeMoney),
                        OrderTime = payOrder.CreationTime,
                        OrderUnixTime = payOrder.CreationUnixTime,
                        TransactionTime = payOrder.TransactionTime,
                        TransactionUnixTime = transactionUnixTime
                    };
                    await TN.SaveAsync(merchantBillsMongo);

                    //更新余额
                    decimal depositAmount = (merchantFund?.DepositAmount ?? 0) + payOrder.OrderMoney;
                    decimal rateFeeBalance = (merchantFund?.RateFeeBalance ?? 0) + payOrder.FeeMoney;
                    decimal balance = balanceBefore + (payOrder.OrderMoney - payOrder.FeeMoney);

                    if (merchantFund != null)
                    {
                        merchantFund.DepositAmount += amount;
                        merchantFund.RateFeeBalance += fee;
                        merchantFund.Balance += netAmount;
                        merchantFund.LastPayOrderTransactionTime = payOrder.TransactionTime;
                        merchantFund.UpdateTime = DateTime.Now;
                        merchantFund.UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);
                        await TN.SaveAsync(merchantFund);
                    }
                    else
                    {
                        var newFund = new MerchantFundsMongoEntity
                        {
                            MerchantCode = merchantCode,
                            MerchantId = payOrder.MerchantId,
                            DepositAmount = amount,
                            TranferAmount = 0,
                            WithdrawalAmount = 0,
                            FrozenBalance = 0,
                            RateFeeBalance = fee,
                            Balance = netAmount,
                            LastPayOrderTransactionTime = payOrder.TransactionTime,
                            UpdateTime = DateTime.Now,
                            UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now),
                            VersionNumber = 0
                        };
                        await TN.SaveAsync(newFund);
                    }

                    payOrder.HaveInsertBills = true; // 更新订单状态为已生成流水
                    await TN.SaveAsync(payOrder);

                    await TN.CommitAsync();

                    // 在 Redis 中标记提现订单已处理
                    _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromHours(1)); // 设置过期时间

                    return true;
                }
                catch (Exception ex)
                {
                    await TN.AbortAsync();
                    NlogLogger.Error("PayOrder" + payOrder.ToJsonString() + "添加代收流水错误：" + ex);
                    throw ex;
                }
            }
        }

        #endregion

        #region 代付订单

        public async Task<bool> UpdateWithRetryAddWithdrawalOrderBillAsync(string merchantCode, string orderId)
        {
            int maxRetries = 5; // 最大重试次数
            int retryCount = 0;
            bool success = false;

            while (retryCount < maxRetries)
            {
                try
                {
                    // 执行更新操作
                    //只有当WriteConflict 才会重试 ， 其他直接终止重试机制
                    var result = await AddWithdrawalOrderBill(merchantCode, orderId);
                    success = result;
                    break;
                }
                catch (MongoCommandException ex) //when (ex.Code == 112) // 112 表示 WriteConflict
                {


                    if (ex.Code == 112)// 112 表示 WriteConflict
                    {
                        retryCount++;
                    }
                    else
                    {
                        throw; // 除了112 使用重试机制其他的一律报错
                    }
                    if (retryCount >= maxRetries)
                    {
                        NlogLogger.Error("添加代付流水错误：" + "数据Merchant：" + merchantCode + ",数据withdrawalOrder：" + orderId + "，错误信息" + ex);
                        throw; // 超过最大重试次数，抛出异常
                    }

                    #region commented out


                    #endregion
                }


            }

            return success;
        }

        /// <summary>
        /// 代付订单
        /// </summary>
        /// <param name="merchantCode"></param>
        /// <param name="withdrawalOrder"></param>
        /// <returns></returns>
        private async Task<bool> AddWithdrawalOrderBill(string merchantCode, string orderId)
        {
            var withdrawalOrder = await DB.Find<WithdrawalOrdersMongoEntity>().OneAsync(orderId);
            if (withdrawalOrder == null) { return false; }
            var cacheKey = $"withdrawalorder-order:{merchantCode}:{withdrawalOrder.OrderNumber}";
            var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
            if (isProcessed)
            {
                NlogLogger.Info("WithdrawalOrder order already processed: " + withdrawalOrder.OrderNumber);
                return true; // 直接返回，避免重复处理
            }

            using (var TN = DB.Transaction())
            {
                try
                {
                    var merchantFund = await TN.Find<MerchantFundsMongoEntity>()
                                       .Match(r => r.MerchantCode == merchantCode)
                                       .ExecuteSingleAsync();

                    if (merchantFund == null || merchantFund.Balance < (merchantFund.FrozenBalance + withdrawalOrder.OrderMoney + withdrawalOrder.FeeMoney))
                    {
                        throw new Exception($"Insufficient balance for withdrawal: {withdrawalOrder.OrderNumber}");
                    }

                    var transactionTime = withdrawalOrder.TransactionTime == DateTime.MinValue ? DateTime.Now : withdrawalOrder.TransactionTime;
                    var transactionUnixTime = TimeHelper.GetUnixTimeStamp(transactionTime);

                    var balance = 0M;
                    var BalanceBefore = 0M;

                    if (merchantFund != null)
                    {
                        BalanceBefore = merchantFund.Balance;
                        balance = merchantFund.Balance;
                    }

                    var existingBill = await TN.Find<MerchantBillsMongoEntity>()
                        .Match(b => b.BillNo == withdrawalOrder.OrderNumber && b.MerchantCode == withdrawalOrder.MerchantCode)
                        .ExecuteAnyAsync();

                    if (existingBill)
                    {
                        throw new Exception($"Duplicate bill detected: {withdrawalOrder.OrderNumber}");
                    }

                    //插入流水
                    MerchantBillsMongoEntity merchantBillsMongo = new MerchantBillsMongoEntity()
                    {
                        MerchantCode = withdrawalOrder.MerchantCode,
                        MerchantId = withdrawalOrder.MerchantId,
                        PlatformCode = withdrawalOrder.PlatformCode,
                        BillNo = withdrawalOrder.OrderNumber,
                        BillType = MerchantBillTypeEnum.Withdraw,
                        Money = withdrawalOrder.OrderMoney,
                        Rate = withdrawalOrder.Rate,
                        FeeMoney = withdrawalOrder.FeeMoney,
                        BalanceBefore = BalanceBefore,
                        BalanceAfter = balance - (withdrawalOrder.OrderMoney + withdrawalOrder.FeeMoney),
                        OrderTime = withdrawalOrder.CreationTime,
                        OrderUnixTime = withdrawalOrder.CreationUnixTime,
                        TransactionTime = withdrawalOrder.TransactionTime,
                        TransactionUnixTime = transactionUnixTime
                    };
                    await TN.SaveAsync(merchantBillsMongo);


                    //更新余额
                    merchantFund.TranferAmount += withdrawalOrder.OrderMoney;
                    merchantFund.RateFeeBalance += withdrawalOrder.FeeMoney;
                    merchantFund.Balance -= (withdrawalOrder.OrderMoney + withdrawalOrder.FeeMoney);
                    merchantFund.LastWithdrawalOrderTransactionTime = transactionTime;
                    merchantFund.UpdateTime = DateTime.Now;
                    merchantFund.UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);

                    if (withdrawalOrder.ReleaseStatus == NsPay.WithdrawalOrders.WithdrawalReleaseStatusEnum.PendingRelease)
                    {
                        merchantFund.FrozenBalance -= withdrawalOrder.OrderMoney;
                        merchantFund.VersionNumber += 1;
                    }

                    await TN.SaveAsync(merchantFund);

                    if (withdrawalOrder.ReleaseStatus == NsPay.WithdrawalOrders.WithdrawalReleaseStatusEnum.PendingRelease)
                    {
                        //更新订单标记
                        withdrawalOrder.ReleaseStatus = NsPay.WithdrawalOrders.WithdrawalReleaseStatusEnum.Released;
                        await TN.SaveAsync(withdrawalOrder);
                    }

                    await TN.CommitAsync();

                    //// 在 Redis 中标记提现订单已处理
                    _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromHours(1)); // 设置过期时间

                    return true;
                }
                catch (Exception ex)
                {
                    await TN.AbortAsync();
                    NlogLogger.Error("WithdrawalOrder:" + withdrawalOrder.ToJsonString() + "添加代付流水错误：" + ex);
                    throw;
                }
            }
        }

        #endregion

        #region 商户提现

        public async Task<bool> UpdateWithRetryAddMerchantWithdrawBillAsync(string merchantCode, MerchantWithdrawMongoEntity merchantWithdraw)
        {
            int maxRetries = 5; // 最大重试次数
            int retryCount = 0;
            bool success = false;

            while (retryCount < maxRetries)
            {
                try
                {
                    // 执行更新操作
                    //只有当WriteConflict 才会重试 ， 其他直接终止重试机制
                    var result = await AddMerchantWithdrawBill(merchantCode, merchantWithdraw);
                    success = result;
                    break;

                }
                catch (MongoCommandException ex)// when (ex.Code == 112) // 112 表示 WriteConflict
                {
                    //retryCount++;
                    if (ex.Code == 112)// 112 表示 WriteConflict
                    {
                        retryCount++;
                    }
                    else
                    {
                        throw; // 除了112 使用重试机制其他的一律报错
                    }

                    if (retryCount >= maxRetries)
                    {
                        NlogLogger.Error("添加出款流水错误：" + "数据Merchant：" + merchantCode + ",数据merchantWithdraw：" + merchantWithdraw.ToJsonString() + "，错误信息" + ex);
                        throw; // 超过最大重试次数，抛出异常
                    }

                    // 使用指数退避策略进行延迟
                    await Task.Delay(DBHelper.GetRetryInterval(retryCount));
                }
            }

            return success;
        }

        /// <summary>
        /// 商户提现
        /// </summary>
        /// <param name="merchantCode"></param>
        /// <param name="merchantWithdraw"></param>
        /// <returns></returns>
        public async Task<bool> AddMerchantWithdrawBill(string merchantCode, MerchantWithdrawMongoEntity merchantWithdraw)
        {
            var cacheKey = $"merchantwithdraw-order:{merchantCode}:{merchantWithdraw.WithDrawNo}";
            var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
            if (isProcessed)
            {
                NlogLogger.Info("MerchantWithdraw order already processed: " + merchantWithdraw.WithDrawNo);
                return true; // 直接返回，避免重复处理
            }

            using (var TN = DB.Transaction())
            {
                try
                {
                    var merchantFund = await TN.Find<MerchantFundsMongoEntity>()
                                       .Match(r => r.MerchantCode == merchantCode)
                                       .ExecuteSingleAsync();
                    var BalanceBefore = 0M;
                    var balance = 0M;
                    if (merchantFund != null)
                    {
                        BalanceBefore = merchantFund.Balance;
                        balance = merchantFund.Balance;
                    }

                    long transactionUnixTime = 0;
                    if (merchantWithdraw.ReviewTime == DateTime.MinValue)
                    {
                        transactionUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);
                    }
                    else
                    {
                        transactionUnixTime = TimeHelper.GetUnixTimeStamp(merchantWithdraw.ReviewTime);
                    }

                    var existingBill = await TN.Find<MerchantBillsMongoEntity>()
                        .Match(b => b.BillNo == merchantWithdraw.WithDrawNo && b.MerchantCode == merchantWithdraw.MerchantCode)
                        .ExecuteAnyAsync();

                    if (existingBill)
                    {
                        throw new Exception($"Duplicate bill detected: {merchantWithdraw.WithDrawNo}");
                    }

                    //插入流水
                    MerchantBillsMongoEntity merchantBillsMongo = new MerchantBillsMongoEntity()
                    {
                        MerchantCode = merchantWithdraw.MerchantCode,
                        MerchantId = merchantWithdraw.MerchantId,
                        BillNo = merchantWithdraw.WithDrawNo,
                        BillType = MerchantBillTypeEnum.Withdraw,
                        Money = merchantWithdraw.Money,
                        Rate = 0,
                        FeeMoney = 0,
                        BalanceBefore = BalanceBefore,
                        BalanceAfter = balance - merchantWithdraw.Money,
                        OrderTime = merchantWithdraw.CreationTime,
                        OrderUnixTime = TimeHelper.GetUnixTimeStamp(merchantWithdraw.CreationTime),
                        TransactionTime = merchantWithdraw.ReviewTime,
                        TransactionUnixTime = transactionUnixTime
                    };
                    await TN.SaveAsync(merchantBillsMongo);

                    //更新余额
                    merchantFund.WithdrawalAmount += merchantWithdraw.Money;
                    merchantFund.Balance -= merchantWithdraw.Money;
                    merchantFund.FrozenBalance -= merchantWithdraw.Money;
                    merchantFund.LastMerchantWithdrawalTransactionTime = merchantWithdraw.ReviewTime;
                    merchantFund.UpdateTime = DateTime.Now;
                    merchantFund.UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);
                    merchantFund.VersionNumber += 1;
                    await TN.SaveAsync(merchantFund);


                    await TN.CommitAsync();

                    //// 在 Redis 中标记提现订单已处理
                    _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromMinutes(30)); // 设置过期时间

                    return true;
                }
                catch (Exception ex)
                {
                    await TN.AbortAsync();
                    NlogLogger.Error("MerchantWithdrawal:" + merchantWithdraw.ToJsonString() + "添加商户提现流水错误：" + ex);
                    throw;
                }
            }
        }

        #endregion


        #region 冻结金额
        public async Task<bool> UpdateFrozenBalanceWithAttempt(string merchantCode, decimal amount, string eventId, int maxAttempt = 3)
        {
            bool haveUpdate = false;

            // if want deduct need pass in negative amount 
            foreach (var attemintCount in Enumerable.Range(1, maxAttempt))
            {
                var currentFundInfo = await DB.Find<MerchantFundsMongoEntity>().Match(r => r.MerchantCode == merchantCode).ExecuteFirstAsync();

                var result = await UpdateLockedAmountWithConfirm(currentFundInfo.ID, amount, currentFundInfo.VersionNumber);

                if (result)
                {
                    haveUpdate = true;
                    break;
                }

                await Task.Delay(DBHelper.GetRetryInterval(attemintCount));
            }

            return haveUpdate;
        }

        public async Task<bool> ResetFrozenBalanceWithAttempt(string merchantCode, decimal totalAmount, string eventId, int maxAttempt = 3)
        {
            bool haveUpdate = false;
            foreach (var attemptCount in Enumerable.Range(1, maxAttempt))
            {
                var currentFundInfo = await DB.Find<MerchantFundsMongoEntity>().Match(r => r.MerchantCode == merchantCode).ExecuteFirstAsync();

                if (currentFundInfo != null)
                {
                    var result = await UpdateLatestLockedAmountWithConfirm(currentFundInfo.ID, totalAmount, currentFundInfo.VersionNumber);

                    if (result)
                    {
                        haveUpdate = true;
                        break;
                    }
                }
                else
                {
                    haveUpdate = true;
                    break;
                }


                await Task.Delay(DBHelper.GetRetryInterval(attemptCount));
            }

            return haveUpdate;
        }


        private async Task<bool> UpdateLockedAmountWithConfirm(string merchantFundId, decimal amount, long? currentVersionNumber)
        {
            var haveUpdated = false;
            var newVersionNumber = TimeHelper.GetUnixTimeStamp(DateTime.Now);

            var result = await DB.Update<MerchantFundsMongoEntity>().MatchID(merchantFundId).Match(r => r.VersionNumber == currentVersionNumber)
                .Modify(x => x.Inc(f => f.FrozenBalance, amount))
                .Modify(x => x.Inc(f => f.VersionNumber, 1))
                .ExecuteAsync();

            if (result.IsModifiedCountAvailable && result.ModifiedCount > 0)
            {
                haveUpdated = true;
            }

            return haveUpdated;
        }

        private async Task<bool> UpdateLatestLockedAmountWithConfirm(string merchantFundId, decimal totalAmount, long? currentVersionNumber)
        {
            var haveUpdated = false;
            var newVersionNumber = TimeHelper.GetUnixTimeStamp(DateTime.Now);

            var result = await DB.Update<MerchantFundsMongoEntity>().MatchID(merchantFundId)
                .Match(r => r.VersionNumber == currentVersionNumber)
                .Modify(x => x.FrozenBalance, totalAmount)
                .Modify(x => x.Inc(f => f.VersionNumber, 1))
                .ExecuteAsync();

            if (result.IsModifiedCountAvailable && result.ModifiedCount > 0)
            {
                haveUpdated = true;
            }

            return haveUpdated;
        }
        
        private async Task<bool> ReleaseWithdrawal(string withdrawalId , string user)
        {
            var result = await DB.Update<WithdrawalOrdersMongoEntity>()
                  .MatchID(withdrawalId)
                  .Match(a => a.ReleaseStatus == WithdrawalReleaseStatusEnum.PendingRelease)
                  .Modify(a => a.ReleaseStatus, WithdrawalReleaseStatusEnum.Released)
                  .Modify(a => a.ReleasedBy, user)
                  .Modify(a => a.ReleasedDate, DateTime.UtcNow)
                  .ExecuteAsync();

            return result.IsModifiedCountAvailable; // Must Matched With Condition 
        }


        public async Task<bool>ReleaseWithdrawalWithAttempt(string orderId , string user)
        {
            var haveReleaseAmount = false;
            if (await ReleaseWithdrawal(orderId, user))
            {
                var orderInfo = await DB.Find<WithdrawalOrdersMongoEntity>().MatchID(orderId).ExecuteFirstAsync();
                haveReleaseAmount = await UpdateFrozenBalanceWithAttempt(orderInfo.MerchantCode , -orderInfo.OrderMoney, Guid.NewGuid().ToString());
            }

            return haveReleaseAmount;
        }

        #endregion
    }
}
