using Abp.Application.Services.Dto;
using Abp.Extensions;
using Abp.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Entities;
using Neptune.NsPay.Common;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MerchantBills.Dtos;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;

namespace Neptune.NsPay.MongoDbExtensions.Services
{
    public class MerchantBillsMongoService : MongoBaseService<MerchantBillsMongoEntity>, IMerchantBillsMongoService, IDisposable
    {
        public MerchantBillsMongoService()
        {
        }

        public async Task<MerchantBillsMongoEntity> GetMerchantBillByOrderNo(string merchantCode, string orderNumber, MerchantBillTypeEnum billType)
        {
            var result = await DB.Find<MerchantBillsMongoEntity>()
                                  .Match(r => r.MerchantCode == merchantCode && r.BillNo == orderNumber && r.BillType == billType)
                                  .ExecuteSingleAsync();
            return result;
        }

        public async Task<MerchantBillsMongoEntity> GetLastMerchantBillByMerchantCode(string merchantCode)
        {
            var result = await DB.Find<MerchantBillsMongoEntity>()
                                  .Match(r => r.MerchantCode == merchantCode)
                                  .Sort(r => r.Descending(x => x.TransactionUnixTime))
                                  .ExecuteFirstAsync();
            return result;
        }

        public async Task<MerchantBillsMongoEntity> GetLastMerchantBillByMerchantId(int merchantId)
        {
            var oneDayBeforeUnix = TimeHelper.GetUnixTimeStamp(DateTime.Now.AddDays(-1));

            var result = await DB.Find<MerchantBillsMongoEntity>()
                                  .Match(r => r.MerchantId == merchantId && r.TransactionUnixTime >= oneDayBeforeUnix)
                                  .Sort(r => r.Descending(x => x.TransactionUnixTime))
                                  .ExecuteFirstAsync();
            return result;
        }

        public async Task<bool> UpdateWithRetryAddPayOrderBillAsync(string merchantCode, PayOrdersMongoEntity payOrder)
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
                    var result = await AddPayOrderBill(merchantCode, payOrder); // if hitting error will direct thrown out exception
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
                        NlogLogger.Error("添加代收流水错误：" + "数据Merchant：" + merchantCode + ",数据PayOrder：" + payOrder.ToJsonString() + "，错误信息" + ex);
                        throw; // 超过最大重试次数，抛出异常
                    }

                    // 使用指数退避策略进行延迟
                    int delay = (int)Math.Pow(2, retryCount) * 100; // 2 的指数递增延迟
                    await Task.Delay(delay);
                }
            }

            return success;
        }

        public async Task<bool> UpdateWithRetryAddWithdrawalOrderBillAsync(string merchantCode, WithdrawalOrdersMongoEntity withdrawalOrder)
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
                    var result = await AddWithdrawalOrderBill(merchantCode, withdrawalOrder);
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
                        NlogLogger.Error("添加代付流水错误：" + "数据Merchant：" + merchantCode + ",数据withdrawalOrder：" + withdrawalOrder.ToJsonString() + "，错误信息" + ex);
                        throw; // 超过最大重试次数，抛出异常
                    }

                    #region commented out

                    //// 使用指数退避策略进行延迟
                    //int delay = (int)Math.Pow(2, retryCount) * 100; // 2 的指数递增延迟
                    //await Task.Delay(delay);

                    //retryCount++;
                    //if (retryCount >= maxRetries)
                    //{
                    //    NlogLogger.Error("添加代付流水错误：" + "数据Merchant：" + merchantCode + ",数据withdrawalOrder：" + withdrawalOrder.ToJsonString() + "，错误信息" + ex);
                    //    throw; // 超过最大重试次数，抛出异常
                    //}

                    //// 使用指数退避策略进行延迟
                    //int delay = (int)Math.Pow(2, retryCount) * 100; // 2 的指数递增延迟
                    //await Task.Delay(delay);

                    #endregion commented out
                }
            }

            return success;
        }

        public async Task<bool> UpdateWithRetryAddMerchantWithdrawBillBillAsync(string merchantCode, MerchantWithdrawMongoEntity merchantWithdraw)
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
                    int delay = (int)Math.Pow(2, retryCount) * 100; // 2 的指数递增延迟
                    await Task.Delay(delay);
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
        private async Task<bool> AddPayOrderBill(string merchantCode, PayOrdersMongoEntity payOrder)
        {
            try
            {
                //var cacheKey = $"withdrawal-order:{merchantCode}:{withdrawalOrder.OrderNumber}";
                //var isProcessed = await _redisService.ExistsAsync(cacheKey);
                //if (isProcessed)
                //{
                //    _logger.LogInformation("Withdrawal order already processed: " + withdrawalOrder.OrderNumber);
                //    return true; // 直接返回，避免重复处理
                //}

                using (var TN = DB.Transaction())
                {
                    var merchantFund = await TN.Find<MerchantFundsMongoEntity>()
                                       .Match(r => r.MerchantCode == merchantCode)
                                       .ExecuteSingleAsync();
                    var balance = 0M;
                    var BalanceBefore = 0M;
                    if (merchantFund != null)
                    {
                        BalanceBefore = merchantFund.Balance;
                        balance = merchantFund.Balance;
                    }

                    long transactionUnixTime = 0;
                    if (payOrder.TransactionTime == DateTime.MinValue)
                    {
                        transactionUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);
                    }
                    else
                    {
                        transactionUnixTime = TimeHelper.GetUnixTimeStamp(payOrder.TransactionTime);
                    }

                    var existingBill = await TN.Find<MerchantBillsMongoEntity>()
                        .Match(b => b.BillNo == payOrder.OrderNumber && b.MerchantCode == payOrder.MerchantCode)
                        .ExecuteSingleAsync();

                    if (existingBill != null)
                    {
                        throw new Exception($"Duplicate bill detected: {payOrder.OrderNumber}");
                    }

                    //插入流水
                    MerchantBillsMongoEntity merchantBillsMongo = new MerchantBillsMongoEntity()
                    {
                        MerchantCode = payOrder.MerchantCode,
                        MerchantId = payOrder.MerchantId,
                        PlatformCode = payOrder.PlatformCode,
                        BillNo = payOrder.OrderNumber,
                        BillType = MerchantBillTypeEnum.OrderIn,
                        Money = payOrder.OrderMoney,
                        Rate = payOrder.Rate,
                        FeeMoney = payOrder.FeeMoney,
                        BalanceBefore = BalanceBefore,
                        BalanceAfter = balance + (payOrder.OrderMoney - payOrder.FeeMoney),
                        //CreationTime = payOrder.TransactionTime,
                        //CreationUnixTime = transactionUnixTime,
                        OrderTime = payOrder.CreationTime,
                        OrderUnixTime = payOrder.CreationUnixTime,
                        TransactionTime = payOrder.TransactionTime,
                        TransactionUnixTime = transactionUnixTime
                    };
                    await TN.SaveAsync(merchantBillsMongo);

                    var depositAmount = 0M;
                    var rateFeeBalance = 0M;

                    //更新余额
                    if (merchantFund != null)
                    {
                        depositAmount = merchantFund.DepositAmount + payOrder.OrderMoney;
                        rateFeeBalance = merchantFund.RateFeeBalance + payOrder.FeeMoney;
                        balance = merchantFund.Balance + (payOrder.OrderMoney - payOrder.FeeMoney);
                        await TN.Update<MerchantFundsMongoEntity>()
                                .Match(r => r.MerchantCode == merchantCode)
                                .Modify(r => r.DepositAmount, depositAmount)
                                .Modify(r => r.RateFeeBalance, rateFeeBalance)
                                .Modify(r => r.Balance, balance)
                                .Modify(r => r.UpdateTime, DateTime.Now)
                                .Modify(r => r.UpdateUnixTime, TimeHelper.GetUnixTimeStamp(DateTime.Now))
                                .ExecuteAsync();
                    }
                    else
                    {
                        depositAmount = payOrder.OrderMoney;
                        rateFeeBalance = payOrder.FeeMoney;
                        balance = payOrder.OrderMoney - payOrder.FeeMoney;
                        MerchantFundsMongoEntity fundsMongoEntity = new MerchantFundsMongoEntity()
                        {
                            MerchantCode = merchantCode,
                            MerchantId = payOrder.MerchantId,
                            DepositAmount = depositAmount,
                            TranferAmount = 0,
                            WithdrawalAmount = 0,
                            RateFeeBalance = rateFeeBalance,
                            Balance = balance,
                            UpdateTime = DateTime.Now,
                            UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now),
                        };
                        await TN.SaveAsync(fundsMongoEntity);
                    }

                    await TN.CommitAsync();
                }

                //// 在 Redis 中标记提现订单已处理
                //await _redisService.SetAsync(cacheKey, true, TimeSpan.FromMinutes(30)); // 设置过期时间

                return true;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("添加代付流水错误：" + ex);
                return false;
            }
        }

        /// <summary>
        /// 代付订单
        /// </summary>
        /// <param name="merchantCode"></param>
        /// <param name="withdrawalOrder"></param>
        /// <returns></returns>
        private async Task<bool> AddWithdrawalOrderBill(string merchantCode, WithdrawalOrdersMongoEntity withdrawalOrder)
        {
            try
            {
                using (var TN = DB.Transaction())
                {
                    var merchantFund = await TN.Find<MerchantFundsMongoEntity>()
                                       .Match(r => r.MerchantCode == merchantCode)
                                       .ExecuteSingleAsync();
                    var balance = 0M;
                    var BalanceBefore = 0M;

                    if (merchantFund != null)
                    {
                        BalanceBefore = merchantFund.Balance;
                        balance = merchantFund.Balance;
                    }

                    long transactionUnixTime = 0;
                    if (withdrawalOrder.TransactionTime == DateTime.MinValue)
                    {
                        transactionUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now);
                    }
                    else
                    {
                        transactionUnixTime = TimeHelper.GetUnixTimeStamp(withdrawalOrder.TransactionTime);
                    }

                    var existingBill = await TN.Find<MerchantBillsMongoEntity>()
                    .Match(b => b.BillNo == withdrawalOrder.OrderNumber && b.MerchantCode == withdrawalOrder.MerchantCode)
                    .ExecuteSingleAsync();

                    if (existingBill != null)
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
                        //CreationTime = withdrawalOrder.TransactionTime,
                        //CreationUnixTime = transactionUnixTime,
                        OrderTime = withdrawalOrder.CreationTime,
                        OrderUnixTime = withdrawalOrder.CreationUnixTime,
                        TransactionTime = withdrawalOrder.TransactionTime,
                        TransactionUnixTime = transactionUnixTime
                    };
                    await TN.SaveAsync(merchantBillsMongo);

                    var tranferAmount = 0M;
                    var rateFeeBalance = 0M;

                    //更新余额
                    if (merchantFund != null)
                    {
                        tranferAmount = merchantFund.TranferAmount + withdrawalOrder.OrderMoney;
                        rateFeeBalance = merchantFund.RateFeeBalance + withdrawalOrder.FeeMoney;
                        balance = merchantFund.Balance - (withdrawalOrder.OrderMoney + withdrawalOrder.FeeMoney);
                        await TN.Update<MerchantFundsMongoEntity>()
                                .Match(r => r.MerchantCode == merchantCode)
                                .Modify(r => r.TranferAmount, tranferAmount)
                                .Modify(r => r.RateFeeBalance, rateFeeBalance)
                                .Modify(r => r.Balance, balance)
                                .Modify(r => r.UpdateTime, DateTime.Now)
                                .Modify(r => r.UpdateUnixTime, TimeHelper.GetUnixTimeStamp(DateTime.Now))
                                .ExecuteAsync();
                    }
                    else
                    {
                        tranferAmount = withdrawalOrder.OrderMoney;
                        rateFeeBalance = withdrawalOrder.FeeMoney;
                        balance = 0 - (withdrawalOrder.OrderMoney + withdrawalOrder.FeeMoney);
                        MerchantFundsMongoEntity fundsMongoEntity = new MerchantFundsMongoEntity()
                        {
                            MerchantCode = merchantCode,
                            MerchantId = withdrawalOrder.MerchantId,
                            TranferAmount = tranferAmount,
                            DepositAmount = 0,
                            WithdrawalAmount = 0,
                            RateFeeBalance = rateFeeBalance,
                            Balance = balance,
                            UpdateTime = DateTime.Now,
                            UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now),
                        };
                        await TN.SaveAsync(fundsMongoEntity);
                    }

                    await TN.CommitAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 商户提现
        /// </summary>
        /// <param name="merchantCode"></param>
        /// <param name="merchantWithdraw"></param>
        /// <returns></returns>
        public async Task<bool> AddMerchantWithdrawBill(string merchantCode, MerchantWithdrawMongoEntity merchantWithdraw)
        {
            try
            {
                using (var TN = DB.Transaction())
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
                        .ExecuteSingleAsync();

                    if (existingBill != null)
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
                        //CreationTime = merchantWithdraw.ReviewTime,
                        //CreationUnixTime = transactionUnixTime,
                        OrderTime = merchantWithdraw.CreationTime,
                        OrderUnixTime = TimeHelper.GetUnixTimeStamp(merchantWithdraw.CreationTime),
                        TransactionTime = merchantWithdraw.ReviewTime,
                        TransactionUnixTime = transactionUnixTime
                    };
                    await TN.SaveAsync(merchantBillsMongo);

                    var withdrawalAmount = 0M;

                    //更新余额
                    if (merchantFund != null)
                    {
                        withdrawalAmount = merchantFund.WithdrawalAmount + merchantWithdraw.Money;
                        balance = merchantFund.Balance - merchantWithdraw.Money;
                        await TN.Update<MerchantFundsMongoEntity>()
                                .Match(r => r.MerchantCode == merchantCode)
                                .Modify(r => r.WithdrawalAmount, withdrawalAmount)
                                .Modify(r => r.Balance, balance)
                                .Modify(r => r.UpdateTime, DateTime.Now)
                                .Modify(r => r.UpdateUnixTime, TimeHelper.GetUnixTimeStamp(DateTime.Now))
                                .ExecuteAsync();
                    }
                    else
                    {
                        withdrawalAmount = merchantWithdraw.Money;
                        balance = 0 - merchantWithdraw.Money;
                        MerchantFundsMongoEntity fundsMongoEntity = new MerchantFundsMongoEntity()
                        {
                            MerchantCode = merchantCode,
                            MerchantId = merchantWithdraw.MerchantId,
                            TranferAmount = 0,
                            DepositAmount = 0,
                            WithdrawalAmount = withdrawalAmount,
                            RateFeeBalance = 0,
                            Balance = balance,
                            UpdateTime = DateTime.Now,
                            UpdateUnixTime = TimeHelper.GetUnixTimeStamp(DateTime.Now),
                        };
                        await TN.SaveAsync(fundsMongoEntity);
                    }

                    await TN.CommitAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
                return false;
            }
        }

        //      /// <summary>
        //      /// 人工补正
        //      /// </summary>
        //      /// <param name="merchantCode"></param>
        //      /// <param name="merchantWithdraw"></param>
        //      /// <returns></returns>
        //public async Task<bool> ArtificialMerchantWithdrawBill(string merchantCode, MerchantBillsMongoEntity merchantBillsMongoEntity)
        //      {
        //          try
        //          {
        //              using (var TN = DB.Transaction())
        //              {
        //                  var merchantFund = await TN.Find<MerchantFundsMongoEntity>()
        //                                     .Match(r => r.MerchantCode == merchantCode)
        //                                     .ExecuteSingleAsync();
        //                  var balance = 0M;
        //                  var BalanceBefore = 0M;
        //                  if (merchantFund != null)
        //                  {
        //                      BalanceBefore = merchantFund.Balance;
        //                      balance = merchantFund.Balance;
        //                  }

        //                  if (merchantBillsMongoEntity.BillType == MerchantBillTypeEnum.AddBill || merchantBillsMongoEntity.BillType == MerchantBillTypeEnum.DeleteBill)
        //                  {
        //                      var orderMoney = 0M;
        //                      if (merchantBillsMongoEntity.BillType == MerchantBillTypeEnum.AddBill)
        //                      {
        //                          orderMoney = balance + merchantBillsMongoEntity.Money;

        //                      }
        //                      if (merchantBillsMongoEntity.BillType == MerchantBillTypeEnum.DeleteBill)
        //                      {
        //                          orderMoney = balance - merchantBillsMongoEntity.Money;
        //                      }

        //                      //插入流水
        //                      MerchantBillsMongoEntity merchantBillsMongo = new MerchantBillsMongoEntity()
        //                      {
        //                          MerchantCode = merchantBillsMongoEntity.MerchantCode,
        //                          MerchantId = merchantBillsMongoEntity.MerchantId,
        //                          PlatformCode = merchantBillsMongoEntity.PlatformCode,
        //                          BillNo = merchantBillsMongoEntity.BillNo,
        //                          BillType = merchantBillsMongoEntity.BillType,
        //                          Money = merchantBillsMongoEntity.Money,
        //                          Rate = merchantBillsMongoEntity.Rate,
        //                          FeeMoney = merchantBillsMongoEntity.FeeMoney,
        //                          BalanceBefore = BalanceBefore,
        //                          BalanceAfter = orderMoney,
        //                          Remark = merchantBillsMongoEntity.Remark,
        //                      };
        //                      await TN.SaveAsync(merchantBillsMongo);

        //                      //更新余额
        //                      if (merchantFund != null)
        //                      {
        //                          balance = orderMoney;
        //                          await TN.Update<MerchantFundsMongoEntity>()
        //                                  .Match(r => r.MerchantCode == merchantCode)
        //                                  .Modify(r => r.Balance, balance)
        //                                  .Modify(r => r.UpdateTime, DateTime.Now)
        //                                  .Modify(r => r.UpdateUnixTime, TimeHelper.GetUnixTimeStamp(DateTime.Now))
        //                                  .ExecuteAsync();
        //                      }
        //                  }

        //                  await TN.CommitAsync();
        //              }
        //              return true;
        //          }
        //          catch (Exception ex)
        //          {
        //              NlogLogger.Error("添加人工补单错误：" + "数据Merchant：" + merchantCode + ",数据：" + merchantBillsMongoEntity.ToJsonString() + "，错误信息" + ex);
        //              return false;
        //          }
        //      }

        public async Task<List<MerchantBillsMongoEntity>> GetMerchantBillByDateRange(DateTime startDate, DateTime endDate)
        {
            var result = await DB.Find<MerchantBillsMongoEntity>()
                                .ManyAsync(f => f.Gte(a => a.TransactionUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                              & f.Lt(a => a.TransactionUnixTime, TimeHelper.GetUnixTimeStamp(endDate)));
            return result.ToList();
        }

        public async Task<PagedResultDto<MerchantBillsMongoEntity>> GetAllWithPagination(GetAllMerchantBillsInput input, List<int> merchantIds, List<PayMentMethodEnum?> moneyTypes = null)
        {
            var billType = input.BillTypeFilter.HasValue ? (MerchantBillTypeEnum)input.BillTypeFilter : default;

            if (input.MaxCreationTimeFilter.HasValue)
            {
                input.MaxCreationTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MaxCreationTimeFilter.Value, input.UtcTimeFilter);
            }
            if (input.MinCreationTimeFilter.HasValue)
            {
                input.MinCreationTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MinCreationTimeFilter.Value, input.UtcTimeFilter);
            }

            if (input.MaxTransactionTimeFilter.HasValue)
            {
                input.MaxTransactionTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MaxTransactionTimeFilter.Value, input.UtcTimeFilter);
            }
            if (input.MinTransactionTimeFilter.HasValue)
            {
                input.MinTransactionTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MinTransactionTimeFilter.Value, input.UtcTimeFilter);
            }

            var builder = Builders<MerchantBillsMongoEntity>.Filter;

            FilterDefinition<MerchantBillsMongoEntity> merchantIdfilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> merchantCodeFilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> moneyTypeFilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> billsfilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> billNoFilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> billTypeFilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> utcTimeFilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> minCreationTimeFilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> maxCreationTimeFilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> minTransactionTimeFilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> maxTransactionTimeFilter = builder.Empty;

            if (merchantIds.Count == 1)
            {
                merchantIdfilter = builder.Eq(e => e.MerchantId, merchantIds.FirstOrDefault());
            }
            else
            {
                merchantIdfilter = builder.In(e => e.MerchantId, merchantIds);
            }

            if (moneyTypes is null)
            {
                moneyTypeFilter = builder.Eq(e => e.MoneyType, null);
            }
            else if (moneyTypes is { Count: > 0 })
            {
                moneyTypeFilter = builder.In(e => e.MoneyType, moneyTypes);
            }

            if (!input.Filter.IsNullOrEmpty())
            {
                var textfilter = "\"" + input.Filter + "\"";
                billsfilter = builder.Text(textfilter);
            }

            if (!input.BillNoFilter.IsNullOrEmpty())
                billNoFilter = builder.Regex(e => e.BillNo, new BsonRegularExpression(input.BillNoFilter, "i"));

            if (!input.MerchantCodeFilter.IsNullOrEmpty())
                merchantCodeFilter = builder.Eq(e => e.MerchantCode, input.MerchantCodeFilter);

            if (input.BillTypeFilter.HasValue && input.BillTypeFilter > -1)
                billTypeFilter = builder.Eq(e => e.BillType, billType);

            if (input.MinCreationTimeFilter != null)
                minCreationTimeFilter = builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MinCreationTimeFilter));

            if (input.MaxCreationTimeFilter != null)
                maxCreationTimeFilter = builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MaxCreationTimeFilter));

            if (input.MinTransactionTimeFilter != null)
                minTransactionTimeFilter = builder.Gte(e => e.TransactionUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MinTransactionTimeFilter));

            if (input.MaxTransactionTimeFilter != null)
                maxTransactionTimeFilter = builder.Lte(e => e.TransactionUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MaxTransactionTimeFilter));

            var filter = builder.And(
                                merchantIdfilter,
                                merchantCodeFilter,
                                moneyTypeFilter,
                                billsfilter,
                                billNoFilter,
                                billTypeFilter,
                                minCreationTimeFilter,
                                maxCreationTimeFilter,
                                minTransactionTimeFilter,
                                maxTransactionTimeFilter
                                );

            int pageNumber = input.SkipCount == 0 ? 1 : (input.SkipCount / input.MaxResultCount) + 1;
            int limit = input.MaxResultCount;
            int totalCount = 0;

            List<MerchantBillsMongoEntity> result = new List<MerchantBillsMongoEntity>();

            List<Task> taskList =
            [
                Task.Run(async () =>
                {
                    var pipeline = new[] {
                        new BsonDocument("$match", filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<MerchantBillsMongoEntity>(), BsonSerializer.SerializerRegistry)),

                        new BsonDocument("$group", new BsonDocument
                        {
                            { "_id", BsonNull.Value },
                            { "totalCount", new BsonDocument("$sum", 1) },
                        })
                    };

                    var sumResult = await DB.Collection<MerchantBillsMongoEntity>().Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
                    if (sumResult != null)
                    {
                        totalCount = sumResult["totalCount"].ToInt32();
                    }
                }),
                Task.Run(async () =>
                {
                    var recordSorting = Builders<MerchantBillsMongoEntity>.Sort.Descending(x => x.CreationUnixTime);
                    var skip = (pageNumber - 1) * limit;

                    var response = await DB.Collection<MerchantBillsMongoEntity>()
                                   .Find(filter)
                                   .Sort(recordSorting)
                                   .Skip(skip)
                                   .Limit(limit)
                                   .ToListAsync();

                    result.AddRange(response);
                }),
            ];

            await Task.WhenAll(taskList);

            return new PagedResultDto<MerchantBillsMongoEntity>
            {
                Items = result,
                TotalCount = totalCount
            };
        }

        private static FilterDefinition<MerchantBillsMongoEntity> BuildExcelFilters(GetAllMerchantBillsForExcelInput input, List<int> merchantIds)
        {
            var billType = input.BillTypeFilter.HasValue ? (MerchantBillTypeEnum)input.BillTypeFilter : default;

            if (input.MaxCreationTimeFilter.HasValue)
            {
                input.MaxCreationTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MaxCreationTimeFilter.Value, input.UtcTimeFilter);
            }

            if (input.MinCreationTimeFilter.HasValue)
            {
                input.MinCreationTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MinCreationTimeFilter.Value, input.UtcTimeFilter);
            }

            if (input.MaxTransactionTimeFilter.HasValue)
            {
                input.MaxTransactionTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MaxTransactionTimeFilter.Value, input.UtcTimeFilter);
            }
            if (input.MinTransactionTimeFilter.HasValue)
            {
                input.MinTransactionTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MinTransactionTimeFilter.Value, input.UtcTimeFilter);
            }

            var builder = Builders<MerchantBillsMongoEntity>.Filter;

            var merchantIdFilter = merchantIds.Count switch
            {
                1 => builder.Eq(e => e.MerchantId, merchantIds.First()),
                _ => builder.In(e => e.MerchantId, merchantIds)
            };

            var billsFilter = !string.IsNullOrEmpty(input.Filter) ? builder.Text($"\"{input.Filter}\"") : builder.Empty;

            var billNoFilter = !string.IsNullOrEmpty(input.BillNoFilter)
                ? builder.Regex(e => e.BillNo, new BsonRegularExpression(input.BillNoFilter, "i"))
                : builder.Empty;

            var merchantCodeFilter = !string.IsNullOrEmpty(input.MerchantCodeFilter) ? builder.Eq(e => e.MerchantCode, input.MerchantCodeFilter)
                : builder.Empty;

            var billTypeFilter = input.BillTypeFilter.HasValue && input.BillTypeFilter > -1 ? builder.Eq(e => e.BillType, billType)
                : builder.Empty;

            var minCreationTimeFilter = input.MinCreationTimeFilter.HasValue ? builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MinCreationTimeFilter))
                : builder.Empty;

            var maxCreationTimeFilter = input.MaxCreationTimeFilter.HasValue ? builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MaxCreationTimeFilter))
                : builder.Empty;

            var minTransactionTimeFilter = input.MinTransactionTimeFilter.HasValue ? builder.Gte(e => e.TransactionUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MinTransactionTimeFilter))
        : builder.Empty;

            var maxTransactionTimeFilter = input.MaxTransactionTimeFilter.HasValue ? builder.Lte(e => e.TransactionUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MaxTransactionTimeFilter))
                : builder.Empty;

            var combinedFilter = builder.And(
                merchantIdFilter,
                merchantCodeFilter,
                billsFilter,
                billNoFilter,
                billTypeFilter,
                minCreationTimeFilter,
                maxCreationTimeFilter,
                minTransactionTimeFilter,
                maxTransactionTimeFilter
            );

            return combinedFilter;
        }

        public async Task<List<MerchantBillsMongoEntity>> GetAll(GetAllMerchantBillsForExcelInput input, List<int> merchantIds)
        {
            var filter = BuildExcelFilters(input, merchantIds);

            var response = await DB.Find<MerchantBillsMongoEntity>()
                    .Match(filter)
                    .Sort(s => s.CreationUnixTime, Order.Descending)
                    .ExecuteAsync();

            return response;
        }

        public async Task<int> GetTotalExcelRecordCount(GetAllMerchantBillsForExcelInput input, List<int> merchantIds)
        {
            var filter = BuildExcelFilters(input, merchantIds);

            var totalRecords = await DB.Collection<MerchantBillsMongoEntity>()
                .CountDocumentsAsync(filter);

            return (int)totalRecords;
        }

        public async Task<List<GetMerchatBillForDashboardDto>> GetMerchantBillSummaryByUserMerchantDateRange(List<int> merchantIds, DateTime startDate, DateTime endDate)
        {
            var response = new List<GetMerchatBillForDashboardDto>();
            var collection = DB.Collection<MerchantBillsMongoEntity>();
            var builder = Builders<MerchantBillsMongoEntity>.Filter;
            FilterDefinition<MerchantBillsMongoEntity> userMerchantCodeFilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> minCreationTimeFilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> maxCreationTimeFilter = builder.Empty;

            if (merchantIds.Count == 1)
            {
                userMerchantCodeFilter = builder.Eq(e => e.MerchantId, merchantIds.FirstOrDefault());
            }
            else
            {
                userMerchantCodeFilter = builder.In(e => e.MerchantId, merchantIds);
            }
            minCreationTimeFilter = builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate));
            maxCreationTimeFilter = builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate));

            var filter = builder.And(userMerchantCodeFilter, minCreationTimeFilter, maxCreationTimeFilter);
            var pipeline = new[] {
                new BsonDocument("$match", filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<MerchantBillsMongoEntity>(), BsonSerializer.SerializerRegistry)),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$BillType" },
                    { "totalCount", new BsonDocument("$sum", 1) },
                    { "sumMoney", new BsonDocument("$sum", "$Money") }, // Sum the "Money" field
                    { "sumFeeMoney", new BsonDocument("$sum", "$FeeMoney") } // Sum the "FeeMoney" field
                })
            };

            var sumResult = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            if (sumResult.Count > 0)
                response = sumResult.Select(s => new GetMerchatBillForDashboardDto
                {
                    BillType = (MerchantBillTypeEnum)s["_id"].AsInt32,
                    TotalCount = s["totalCount"].AsInt32,
                    TotalMoney = s["sumMoney"].AsDecimal,
                    TotalFeeMoney = s["sumFeeMoney"].AsDecimal,
                }).ToList();

            return response;
        }

        public async Task<List<GetMerchatBillForDashboardSummaryDto>> GetMerchantBillByUserMerchantDateRange(List<int> merchantIds, DateTime startDate, DateTime endDate)
        {
            var response = new List<GetMerchatBillForDashboardSummaryDto>();
            var collection = DB.Collection<MerchantBillsMongoEntity>();
            var builder = Builders<MerchantBillsMongoEntity>.Filter;
            FilterDefinition<MerchantBillsMongoEntity> userMerchantCodeFilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> minCreationTimeFilter = builder.Empty;
            FilterDefinition<MerchantBillsMongoEntity> maxCreationTimeFilter = builder.Empty;

            if (merchantIds.Count == 1)
            {
                userMerchantCodeFilter = builder.Eq(e => e.MerchantId, merchantIds.FirstOrDefault());
            }
            else
            {
                userMerchantCodeFilter = builder.In(e => e.MerchantId, merchantIds);
            }
            minCreationTimeFilter = builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate));
            maxCreationTimeFilter = builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate));

            var filter = builder.And(userMerchantCodeFilter, minCreationTimeFilter, maxCreationTimeFilter);
            var pipeline = new[] {
                new BsonDocument("$match", filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<MerchantBillsMongoEntity>(), BsonSerializer.SerializerRegistry)),
                new BsonDocument("$project", new BsonDocument
                {
                    { "date", new BsonDocument("$dateToString", new BsonDocument
                        {
                            { "format", "%Y-%m-%d" },
                            { "date", "$CreationTime" }
                        })
                    },
                    { "type", "$BillType"},
                    { "money", "$Money"}
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument{
                        { "date", "$date"},
                        { "type", "$type"} }
                    },
                    { "sumMoney", new BsonDocument("$sum", "$money") }, // Sum the "Money" field
                })
            };

            var sumResult = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            if (sumResult.Count > 0)
                response = sumResult.Select(s => new GetMerchatBillForDashboardSummaryDto
                {
                    Date = s["_id"]["date"].ToString(),
                    BillType = (MerchantBillTypeEnum)s["_id"]["type"].AsInt32,
                    TotalMoney = s["sumMoney"].AsDecimal,
                }).ToList();

            return response.ToList();
        }

        public void Dispose()
        {
        }
    }
}