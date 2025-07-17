using Abp.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.WithdrawalOrders;
using SqlSugar;

namespace Neptune.NsPay.BillingExtensions
{
    public class MerchantBillsHelper : IMerchantBillsHelper
    {
        private readonly IRedisService _redisService;
        private readonly ISqlSugarClient _Db;

        public MerchantBillsHelper(IRedisService redisService, ISqlSugarClient Db)
        {
            _redisService = redisService;
            _Db = Db;
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
                        merchantFund.VersionNumber += 1;
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

                    //payOrder.IsBilled = true; // 更新订单状态为已生成流水
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

        #endregion 代收订单

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
                    bool withdrawalHaveUpdated = false;

                    if (withdrawalOrder.ReleaseStatus == NsPay.WithdrawalOrders.WithdrawalReleaseStatusEnum.PendingRelease)
                    {
                        //更新订单标记
                        withdrawalOrder.ReleaseStatus = NsPay.WithdrawalOrders.WithdrawalReleaseStatusEnum.Released;
                        withdrawalHaveUpdated = true;
                    }

                    if (!withdrawalOrder.isBilled)
                    {
                        withdrawalOrder.isBilled = true;
                        withdrawalHaveUpdated = true;
                    }

                    if (withdrawalHaveUpdated)
                    {
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

        #endregion 代付订单

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

        #endregion 商户提现

        #region 代收订单 V2

        public async Task<bool> AddRetryPayOrderBillAsync(string merchantCode, string orderId)
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
                    var result = await AddPayOrderBillAndBalance(merchantCode, orderId); // if hitting error will direct thrown out exception
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
                    await Task.Delay(DBHelper.GetRetryInterval(retryCount));// 2 的指数递增延迟
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
        private async Task<bool> AddPayOrderBillAndBalance(string merchantCode, string orderId)
        {
            var payOrder = await DB.Find<PayOrdersMongoEntity>().OneAsync(orderId);
            if (payOrder == null) { return false; }
            var cacheKey = $"payorder-bill:{merchantCode}:{payOrder.OrderNumber}";
            var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
            if (isProcessed)
            {
                NlogLogger.Info("PayOrder order already processed: " + payOrder.OrderNumber);
                return true; // 直接返回，避免重复处理
            }
            var totalCost = payOrder.OrderMoney - payOrder.FeeMoney;
            var transactionTime = payOrder.TransactionTime == DateTime.MinValue
                ? DateTime.Now
                : payOrder.TransactionTime;
            var transactionUnixTime = TimeHelper.GetUnixTimeStamp(transactionTime);

            using (var TN = DB.Transaction())
            {
                try
                {
                    var existingBill = await TN.Find<MerchantBillsMongoEntity>()
                        .Match(b => b.BillNo == payOrder.OrderNumber && b.MerchantCode == payOrder.MerchantCode)
                        .ExecuteAnyAsync();

                    if (existingBill)
                    {
                        throw new Exception($"Duplicate bill detected: {payOrder.OrderNumber}");
                    }

                    var merchantFund = await TN.Find<MerchantFundsMongoEntity>()
                                       .Match(r => r.MerchantCode == merchantCode)
                                       .ExecuteSingleAsync();

                    decimal balanceBefore = merchantFund?.Balance ?? 0M;
                    decimal depositAmount = (merchantFund?.DepositAmount ?? 0) + payOrder.OrderMoney;
                    decimal rateFeeBalance = (merchantFund?.RateFeeBalance ?? 0) + payOrder.FeeMoney;
                    decimal balanceAfter = balanceBefore + (payOrder.OrderMoney - payOrder.FeeMoney);

                    if (merchantFund != null)
                    {
                        merchantFund.DepositAmount += payOrder.OrderMoney;
                        merchantFund.RateFeeBalance += payOrder.FeeMoney;
                        merchantFund.Balance = balanceAfter;
                        merchantFund.LastPayOrderTransactionTime = payOrder.TransactionTime;
                        merchantFund.UpdateTime = DateTime.Now;
                        merchantFund.UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);
                        merchantFund.VersionNumber += 1;
                        await TN.SaveAsync(merchantFund);
                    }
                    else
                    {
                        var newFund = new MerchantFundsMongoEntity
                        {
                            MerchantCode = merchantCode,
                            MerchantId = payOrder.MerchantId,
                            DepositAmount = payOrder.OrderMoney,
                            TranferAmount = 0,
                            WithdrawalAmount = 0,
                            FrozenBalance = 0,
                            RateFeeBalance = payOrder.FeeMoney,
                            Balance = balanceAfter,
                            LastPayOrderTransactionTime = payOrder.TransactionTime,
                            UpdateTime = DateTime.Now,
                            VersionNumber = 1,
                            UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now)
                        };
                        await TN.SaveAsync(newFund);
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
                        BalanceAfter = balanceAfter,
                        OrderTime = payOrder.CreationTime,
                        OrderUnixTime = payOrder.CreationUnixTime,
                        TransactionTime = payOrder.TransactionTime,
                        TransactionUnixTime = transactionUnixTime
                    };
                    await TN.SaveAsync(merchantBillsMongo);

                    payOrder.IsBilled = true; // 更新订单状态为已生成流水
                    await TN.SaveAsync(payOrder);

                    await TN.CommitAsync();

                    //// 在 Redis 中标记提现订单已处理
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

        public async Task<bool> AddRetryPayOrderCryptoBillAsync(string merchantCode, string orderId)
        {
            var maxRetries = 5; // 最大重试次数
            var retryCount = 0;
            var success = false;

            while (retryCount < maxRetries)
            {
                try
                {
                    var payOrder = await DB.Find<PayOrdersMongoEntity>().OneAsync(orderId);
                    if (payOrder is null) return false;

                    try
                    {
                        var merchantFund = await DB.Find<MerchantFundsMongoEntity>().Match(r => r.MerchantCode == merchantCode).ExecuteFirstAsync();
                        if (merchantFund is null)
                        {
                            merchantFund = new MerchantFundsMongoEntity
                            {
                                MerchantCode = merchantCode,
                                MerchantId = payOrder.MerchantId,
                                DepositAmount = 0,
                                TranferAmount = 0,
                                WithdrawalAmount = 0,
                                RateFeeBalance = 0,
                                FrozenBalance = 0,
                                Balance = 0,
                                USDTDepositAmount = 0,
                                USDTWithdrawalAmount = 0,
                                USDTTranferAmount = 0,
                                USDTRateFeeBalance = 0,
                                USDTBalance = 0,
                                USDTFrozenBalance = 0,
                                LastPayOrderTransactionTime = DateTime.MinValue,
                                UpdateTime = DateTime.Now,
                                UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now),
                                VersionNumber = 0
                            };
                            await merchantFund.SaveAsync();
                        }

                        var existingBill = await DB.Find<MerchantBillsMongoEntity>()
                            .Match(b => b.BillNo == payOrder.OrderNumber && b.MerchantCode == payOrder.MerchantCode)
                            .ExecuteAnyAsync();

                        if (existingBill)
                        {
                            throw new Exception($"Duplicate bill detected: {payOrder.OrderNumber}");
                        }

                        var oldBalance = merchantFund.USDTBalance;
                        var fees = payOrder.FeeMoney + (payOrder.OrderMoney * payOrder.Rate);
                        var netMoney = payOrder.OrderMoney - fees;
                        var transactionUnixTime = TimeHelper.GetUnixTimeStamp(payOrder.TransactionTime == DateTime.MinValue ? DateTime.Now : payOrder.TransactionTime);

                        //插入流水
                        var merchantBillsMongo = new MerchantBillsMongoEntity
                        {
                            MerchantCode = payOrder.MerchantCode,
                            MerchantId = payOrder.MerchantId,
                            PlatformCode = payOrder.PlatformCode,
                            BillNo = payOrder.OrderNumber,
                            BillType = MerchantBillTypeEnum.OrderIn,
                            MoneyType = PayMentMethodEnum.USDTCrypto,
                            Money = payOrder.OrderMoney,
                            Rate = payOrder.Rate,
                            FeeMoney = payOrder.FeeMoney,
                            BalanceBefore = oldBalance,
                            BalanceAfter = oldBalance + netMoney,
                            OrderTime = payOrder.CreationTime,
                            OrderUnixTime = payOrder.CreationUnixTime,
                            TransactionTime = payOrder.TransactionTime,
                            TransactionUnixTime = transactionUnixTime
                        };

                        //更新余额
                        merchantFund.USDTDepositAmount += payOrder.OrderMoney;
                        merchantFund.USDTRateFeeBalance += fees;
                        merchantFund.USDTBalance += netMoney;
                        merchantFund.LastPayOrderTransactionTime = payOrder.TransactionTime;
                        merchantFund.UpdateTime = DateTime.Now;
                        merchantFund.UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);
                        merchantFund.VersionNumber += 1;

                        payOrder.IsBilled = true;

                        using (var TN = DB.Transaction())
                        {
                            await TN.SaveAsync(merchantBillsMongo);

                            await TN.Update<MerchantFundsMongoEntity>()
                                .MatchID(merchantFund.ID)
                                .ModifyOnly(x => new
                                {
                                    x.USDTDepositAmount,
                                    x.USDTRateFeeBalance,
                                    x.USDTBalance,
                                    x.LastPayOrderTransactionTime,
                                    x.UpdateTime,
                                    x.UpdateUnixTime,
                                    x.VersionNumber,
                                }, merchantFund)
                                .ExecuteAsync();

                            await TN.Update<PayOrdersMongoEntity>()
                                .MatchID(orderId)
                                .ModifyOnly(x => new { x.IsBilled }, payOrder)
                                .ExecuteAsync();

                            await TN.CommitAsync();
                        }

                        success = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        NlogLogger.Error("PayOrder" + payOrder.ToJsonString() + "添加代收流水错误：" + ex);
                        throw;
                    }
                }
                catch (MongoCommandException ex)
                {
                    if (ex.Code != 112)// 112 表示 WriteConflict
                        throw; // 除了112 使用重试机制其他的一律报错

                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        NlogLogger.Error("添加代收流水错误：" + "数据Merchant：" + merchantCode + ",数据PayOrderCrypto：" + orderId + "，错误信息" + ex);
                        throw; // 超过最大重试次数，抛出异常
                    }

                    // 使用指数退避策略进行延迟
                    await Task.Delay(DBHelper.GetRetryInterval(retryCount));
                }
            }

            return success;
        }

        #endregion 代收订单 V2

        #region 代付订单 V2

        public async Task<bool> AddRetryWithdrawalOrderBillAsync(string merchantCode, string orderId)
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
                    var result = await AddWithdrawalOrderBillAndBalance(merchantCode, orderId);
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
        private async Task<bool> AddWithdrawalOrderBillAndBalance(string merchantCode, string orderId)
        {
            var withdrawalOrder = await DB.Find<WithdrawalOrdersMongoEntity>().OneAsync(orderId);
            if (withdrawalOrder == null) { return false; }
            var cacheKey = $"withdrawalorder-bill:{merchantCode}:{withdrawalOrder.OrderNumber}";
            var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
            if (isProcessed)
            {
                NlogLogger.Info("WithdrawalOrder order already processed: " + withdrawalOrder.OrderNumber);
                return true; // 直接返回，避免重复处理
            }

            var totalCost = withdrawalOrder.OrderMoney + withdrawalOrder.FeeMoney;
            var transactionTime = withdrawalOrder.TransactionTime == DateTime.MinValue
                ? DateTime.Now
                : withdrawalOrder.TransactionTime;
            var transactionUnixTime = TimeHelper.GetUnixTimeStamp(transactionTime);

            using (var TN = DB.Transaction())
            {
                try
                {
                    var existingBill = await TN.Find<MerchantBillsMongoEntity>()
                                                .Match(b => b.BillNo == withdrawalOrder.OrderNumber && b.MerchantCode == withdrawalOrder.MerchantCode)
                                                .ExecuteAnyAsync();
                    if (existingBill)
                    {
                        throw new Exception($"Duplicate bill detected: {withdrawalOrder.OrderNumber}");
                    }

                    var merchantFund = await TN.Find<MerchantFundsMongoEntity>()
                                       .Match(r => r.MerchantCode == merchantCode)
                                       .ExecuteSingleAsync();

                    if (merchantFund == null)
                    {
                        throw new Exception($"Insufficient balance for withdrawal: {withdrawalOrder.OrderNumber}");
                    }

                    var updater = TN.UpdateAndGet<MerchantFundsMongoEntity>()
                        .Match(x => x.MerchantCode == merchantCode)
                        .Modify(m => m
                            .Inc(x => x.Balance, -totalCost)
                            .Inc(x => x.TranferAmount, withdrawalOrder.OrderMoney)
                            .Inc(x => x.RateFeeBalance, withdrawalOrder.FeeMoney)
                            .Inc(x => x.VersionNumber, 1)
                            .Set(x => x.LastWithdrawalOrderTransactionTime, transactionTime)
                            .Set(x => x.UpdateTime, DateTime.Now)
                            .Set(x => x.UpdateUnixTime, TimeHelper.GetUnixTimeStamp(DateTime.Now))
                        );

                    if (withdrawalOrder.ReleaseStatus == WithdrawalReleaseStatusEnum.PendingRelease)
                    {
                        updater = updater.Modify(m => m.Inc(x => x.FrozenBalance, -withdrawalOrder.OrderMoney));
                    }

                    var fundBefore = await updater.Option(o => o.ReturnDocument = ReturnDocument.Before).ExecuteAsync();
                    if (fundBefore == null)
                    {
                        await TN.AbortAsync();
                        NlogLogger.Warn($"WithdrawalOrder skipped: update failed or already processed: {withdrawalOrder.OrderNumber}");
                        return false;
                    }

                    var balanceBefore = fundBefore.Balance;
                    var balanceAfter = balanceBefore - totalCost;

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
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        OrderTime = withdrawalOrder.CreationTime,
                        OrderUnixTime = withdrawalOrder.CreationUnixTime,
                        TransactionTime = withdrawalOrder.TransactionTime,
                        TransactionUnixTime = transactionUnixTime
                    };
                    await TN.SaveAsync(merchantBillsMongo);

                    //更新订单标记
                    withdrawalOrder.isBilled = true;

                    if (withdrawalOrder.ReleaseStatus == WithdrawalReleaseStatusEnum.PendingRelease)
                    {
                        withdrawalOrder.ReleaseStatus = WithdrawalReleaseStatusEnum.Released;
                    }

                    await TN.SaveAsync(withdrawalOrder);

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

        #endregion 代付订单 V2

        #region 商户提现 V2

        public async Task<bool> AddRetryMerchantWithdrawBillAsync(string merchantCode, MerchantWithdrawMongoEntity merchantWithdraw)
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
                    var result = await AddMerchantWithdrawBillAndBalance(merchantCode, merchantWithdraw);
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
                    await Task.Delay(DBHelper.GetRetryInterval(retryCount));// 2 的指数递增延迟
                }
            }
            return success;
        }

        public async Task<bool> AddMerchantWithdrawBillAndBalance(string merchantCode, MerchantWithdrawMongoEntity merchantWithdraw)
        {
            var cacheKey = $"merchantwithdraw-bill:{merchantCode}:{merchantWithdraw.WithDrawNo}";
            var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
            if (isProcessed)
            {
                NlogLogger.Info("MerchantWithdraw order already processed: " + merchantWithdraw.WithDrawNo);
                return true; // 直接返回，避免重复处理
            }

            var totalCost = merchantWithdraw.Money;
            var transactionTime = merchantWithdraw.ReviewTime == DateTime.MinValue
                ? DateTime.Now
                : merchantWithdraw.ReviewTime;
            var transactionUnixTime = TimeHelper.GetUnixTimeStamp(transactionTime);

            using (var TN = DB.Transaction())
            {
                try
                {
                    var existingBill = await TN.Find<MerchantBillsMongoEntity>()
                                                .Match(b => b.BillNo == merchantWithdraw.WithDrawNo && b.MerchantCode == merchantWithdraw.MerchantCode)
                                                .ExecuteAnyAsync();
                    if (existingBill)
                    {
                        throw new Exception($"Duplicate bill detected: {merchantWithdraw.WithDrawNo}");
                    }

                    var merchantFund = await TN.Find<MerchantFundsMongoEntity>()
                                       .Match(r => r.MerchantCode == merchantCode)
                                       .ExecuteSingleAsync();

                    if (merchantFund == null)
                    {
                        throw new Exception($"Insufficient balance for withdrawal: {merchantWithdraw.WithDrawNo}");
                    }

                    var updater = TN.UpdateAndGet<MerchantFundsMongoEntity>()
                        .Match(x => x.MerchantCode == merchantCode)
                        .Modify(m => m
                            .Inc(x => x.Balance, -totalCost)
                            .Inc(x => x.FrozenBalance, -totalCost)
                            .Inc(x => x.WithdrawalAmount, +totalCost)
                            .Inc(x => x.VersionNumber, +1)
                            .Set(x => x.LastMerchantWithdrawalTransactionTime, transactionTime)
                            .Set(x => x.UpdateTime, DateTime.Now)
                            .Set(x => x.UpdateUnixTime, TimeHelper.GetUnixTimeStamp(DateTime.Now))
                        );

                    var fundBefore = await updater.Option(o => o.ReturnDocument = ReturnDocument.Before).ExecuteAsync();
                    if (fundBefore == null)
                    {
                        await TN.AbortAsync();
                        NlogLogger.Warn($"WithdrawalOrder skipped: update failed or already processed: {merchantWithdraw.WithDrawNo}");
                        return false;
                    }

                    var balanceBefore = fundBefore.Balance;
                    var balanceAfter = balanceBefore - totalCost;

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
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        OrderTime = merchantWithdraw.CreationTime,
                        OrderUnixTime = TimeHelper.GetUnixTimeStamp(merchantWithdraw.CreationTime),
                        TransactionTime = merchantWithdraw.ReviewTime,
                        TransactionUnixTime = transactionUnixTime
                    };
                    await TN.SaveAsync(merchantBillsMongo);

                    await TN.CommitAsync();

                    //// 在 Redis 中标记提现订单已处理
                    _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromHours(1)); // 设置过期时间

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

        #endregion 商户提现 V2
    }
}