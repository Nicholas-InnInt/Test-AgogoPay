using Abp.Extensions;
using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.Web.PayMonitorApi.Helpers;
using Neptune.NsPay.Web.PayMonitorApi.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;

namespace Neptune.NsPay.Web.PayMonitorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankRecordController : Controller
    {
        private readonly static string Sign = "dcaebf01858f33594ff3074fb7b81d73";
        private readonly IRedisService _redisService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        private readonly IBidvBankHelper _bidvBankHelper;
        private readonly IVietinBankHelper _vietinBankHelper;
        private readonly IMBBankHelper _mBBankHelper;
        private readonly ITechcomBankHelper _techcomBankHelper;
        private readonly IVietcomBankHelper _vietcomBankHelper;
        private readonly IAcbBankHelper _acbBankHelper;
        private readonly IPVcomBankHelper _pVcomBankHelper;
        private readonly IBusinessMBBankHelper _businessMBBankHelper;
        private readonly IBusinessVtbBankHelper _businessVtbBankHelper;
        private readonly IBankHelper _bankHelper;
        private readonly IMemoryCache _memoryCache;
        private readonly IPayMonitorCommonHelpers _payMonitorCommonHelpers;

        public BankRecordController(IRedisService redisService,
            IPayOrdersMongoService payOrdersMongoService,
            IPayOrderDepositsMongoService payOrderDepositsMongoService,
            IBidvBankHelper bidvBankHelper,
            IVietinBankHelper vietinBankHelper,
            IMBBankHelper mBBankHelper,
            ITechcomBankHelper techcomBankHelper,
            IVietcomBankHelper vietcomBankHelper,
            IAcbBankHelper acbBankHelper,
            IPVcomBankHelper pVcomBankHelper,
            IBusinessMBBankHelper businessMBBankHelper,
            IBankBalanceService bankBalanceService,
            IBusinessVtbBankHelper businessVtbBankHelper,
            IBankHelper bankHelper,
            IMemoryCache memoryCache,
            IPayMonitorCommonHelpers payMonitorCommonHelpers)
        {
            _redisService = redisService;
            _payOrdersMongoService = payOrdersMongoService;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
            _bidvBankHelper = bidvBankHelper;
            _vietinBankHelper = vietinBankHelper;
            _mBBankHelper = mBBankHelper;
            _techcomBankHelper = techcomBankHelper;
            _vietcomBankHelper = vietcomBankHelper;
            _acbBankHelper = acbBankHelper;
            _pVcomBankHelper = pVcomBankHelper;
            _businessMBBankHelper = businessMBBankHelper;
            _businessVtbBankHelper = businessVtbBankHelper;
            _bankHelper = bankHelper;
            _memoryCache = memoryCache;
            _payMonitorCommonHelpers = payMonitorCommonHelpers;
        }

        [HttpGet]
        public IEnumerable<string> GetTest()
        {
            return new string[] { "value1", "value2" };
        }

        private static class CacheConstants
        {
            public static readonly MemoryCacheEntryOptions DefaultCacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(300) // Cache for 30 minutes
            };
        }


        /// <summary>
        /// 获取bidv记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/BidvRecord")]
        public async Task<JsonResult> BidvRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("Bidv记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var recordsToProcess = input.BankRecords
                        .GroupBy(r => new { r.RefNo }) // Group by unique combination
                        .Select(g => g.First()) // Pick the first record from each group
                        .ToList();

                    var semaphore = new SemaphoreSlim(3); // Limit concurrent inserts
                    var existingRecordsSet = new HashSet<string>(); // Track existing records

                    foreach (var record in recordsToProcess)
                    {
                        if (existingRecordsSet.Contains(record.RefNo))
                            continue; // Skip if already processed

                        var cacheKey = $"BidvTransaction_{input.PayMentId}_{record.RefNo}";

                        if (!_memoryCache.TryGetValue(cacheKey, out string _var))
                        {
                            var existingRecord = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());

                            if (existingRecord != null)
                            {
                                _memoryCache.Set(cacheKey, "", CacheConstants.DefaultCacheOptions);
                                existingRecordsSet.Add(record.RefNo); // Store existing record locally
                            }
                        }
                        else
                        {
                            existingRecordsSet.Add(record.RefNo); // If found in cache, add to local tracking
                        }
                    }

                    // Get only new records using the local HashSet
                    var newRecords = recordsToProcess
                        .Where(r => !existingRecordsSet.Contains(r.RefNo))
                        .ToList();

                    NlogLogger.Warn("Bidv记录[New]：" + newRecords.ToJsonString());

                    var transactionLists = new ConcurrentBag<PayOrderDepositsMongoEntity>();

                    var insertTasks = new List<Task>();
                    // Process new records in parallel with SemaphoreSlim

                    foreach (var record in newRecords)
                    {
                        insertTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await semaphore.WaitAsync(); // Prevent simultaneous inserts

                                var transactionDate = _payMonitorCommonHelpers.ConvertToStandardTime(record.TransactionTime.Trim());
                                var type = record.Amount.Contains("+") ? "CRDT" : "DBIT";
                                var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");

                                var payOrderDeposit = new PayOrderDepositsMongoEntity
                                {
                                    PayType = PayMentTypeEnum.BidvBank.ToInt(),
                                    Type = type,
                                    RefNo = record.RefNo,
                                    Description = record.Description?.Trim(),
                                    UserName = input.Phone,
                                    AccountNo = input.CardNumber,
                                    CreditBank = "",
                                    CreditAcctNo = "",
                                    CreditAcctName = "",
                                    CreditAmount = amount.ParseToDecimal(),
                                    DebitBank = "",
                                    DebitAcctNo = "",
                                    DebitAcctName = "",
                                    DebitAmount = amount.ParseToDecimal(),
                                    AvailableBalance = 0,
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(transactionDate ?? DateTime.Parse(record.TransactionTime,
                                    new CultureInfo("de-DE")), "China Standard Time"),
                                    PayMentId = input.PayMentId
                                };

                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                transactionLists.Add(payOrderDeposit);

                                if (payOrderDeposit.Type == "DBIT")
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel
                                    {
                                        Type = PayMentTypeEnum.BidvBank,
                                        RefNo = payOrderDeposit.RefNo,
                                        PayMentId = payOrderDeposit.PayMentId,
                                        TransferTime = payOrderDeposit.TransactionTime,
                                        Remark = payOrderDeposit.Description,
                                        Money = amount.ParseToDecimal(),
                                    };
                                    // _redisService.AddTransferOrderQueueList(bankOrderNotify);
                                }
                            }
                            finally
                            {
                                semaphore.Release(); // Release after execution
                            }
                        }));
                    }

                    await Task.WhenAll(insertTasks); // Wait for all insert tasks to complete

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bidvBankHelper.SetLastRefNoKey(input.CardNumber, lastInfo.RefNo);
                        }

                        var matchingTasks = new List<Task>(); // List to store running tasks

                        foreach (var item in transactionLists)
                        {
                            matchingTasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await semaphore.WaitAsync(); // Wait for a slot to become available
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode,
                                        PayMentTypeEnum.BidvBank, item.Description?.Trim(), item.CreditAmount);

                                    if (payEntity != null)
                                    {
                                        if (_redisService.SetSuccessOrder(payEntity.ID))
                                        {
                                            var bankOrderPubModel = new BankOrderPubModel
                                            {
                                                MerchantCode = input.MerchantCode,
                                                Type = PayMentTypeEnum.BidvBank,
                                                PayMentId = item.PayMentId,
                                                PayOrderId = payEntity.ID,
                                                Id = item.RefNo,
                                                Money = item.CreditAmount,
                                                Desc = item.Description,
                                            };
                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.BIDVBankOrder, bankOrderPubModel);
                                        }
                                    }
                                }
                                finally
                                {
                                    semaphore.Release(); // Release the slot
                                }
                            }));
                        }
                        await Task.WhenAll(matchingTasks); // Wait for all tasks to complete
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Bidv Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("Bidv记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取Vtb记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/VtbRecord")]
        public async Task<JsonResult> VtbRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("Vtb记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var recordsToProcess = input.BankRecords
                        .GroupBy(r => new { r.RefNo, Description = r.Description?.Trim() }) // Group by unique combination
                        .Select(g => g.First()) // Pick the first record from each group
                        .ToList();

                    var semaphore = new SemaphoreSlim(3); // Limit concurrent inserts
                    var existingRecordsSet = new HashSet<Tuple<string, string>>(); // Track existing records

                    foreach (var record in recordsToProcess)
                    {
                        var cRecordUnique = Tuple.Create(record.RefNo, record.Description);
                        if (existingRecordsSet.Contains(cRecordUnique))
                            continue; // Skip if already processed

                        var cacheKey = $"VtbExistingTransactions_{input.PayMentId}_{record.RefNo}_{record.Description}";

                        if (!_memoryCache.TryGetValue(cacheKey, out string _var))
                        {
                            var existingRecord = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());

                            if (existingRecord != null)
                            {
                                _memoryCache.Set(cacheKey, "", CacheConstants.DefaultCacheOptions);
                                existingRecordsSet.Add(cRecordUnique); // Store existing record locally
                            }
                        }
                        else
                        {
                            existingRecordsSet.Add(cRecordUnique); // If found in cache, add to local tracking
                        }
                    }

                    // Filter new records
                    var newRecords = recordsToProcess
                         .Where(r => !existingRecordsSet.Contains(Tuple.Create(r.RefNo, r.Description)))
                         .ToList();


                    var transactionLists = new ConcurrentBag<PayOrderDepositsMongoEntity>();

                    NlogLogger.Warn("Vtb记录[New]：" + newRecords.ToJsonString());

                    List<Task> insertTask = new List<Task>();

                    foreach (var record in newRecords)
                    {
                        insertTask.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await semaphore.WaitAsync(); // Prevent simultaneous inserts
                                var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");

                                var payOrderDeposit = new PayOrderDepositsMongoEntity
                                {
                                    PayType = PayMentTypeEnum.VietinBank.ToInt(),
                                    Type = record.DorC == "C" ? "CRDT" : "DBIT",
                                    RefNo = record.RefNo,
                                    Description = record.Description?.Trim(),
                                    UserName = input.Phone,
                                    AccountNo = input.CardNumber,
                                    CreditBank = "",
                                    CreditAcctNo = record.CreditAcctNo,
                                    CreditAcctName = record.CreditAcctName,
                                    CreditAmount = amount.ParseToDecimal(),
                                    DebitBank = "",
                                    DebitAcctNo = record.DebitAcctNo,
                                    DebitAcctName = record.DebitAcctName,
                                    DebitAmount = amount.ParseToDecimal(),
                                    AvailableBalance = record.AvailableBalance.ParseToDecimal(),
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                    PayMentId = input.PayMentId
                                };

                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                transactionLists.Add(payOrderDeposit);

                                //加入取款缓存
                                if (payOrderDeposit.Type == "DBIT")
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel
                                    {
                                        Type = PayMentTypeEnum.VietinBank,
                                        RefNo = payOrderDeposit.RefNo,
                                        PayMentId = payOrderDeposit.PayMentId,
                                        TransferTime = payOrderDeposit.TransactionTime,
                                        Remark = payOrderDeposit.Description,
                                        Money = amount.ParseToDecimal(),
                                    };
                                    // _redisService.AddTransferOrderQueueList(bankOrderNotify);
                                }
                            }
                            finally
                            {
                                semaphore.Release(); // Release after execution
                            }
                        }));
                    }


                    await Task.WhenAll(insertTask);

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _vietinBankHelper.SetLastRefNoKey(input.CardNumber, lastInfo.RefNo);
                        }

                        var matchingTasks = new List<Task>(); // List to store running tasks

                        foreach (var item in transactionLists)
                        {
                            matchingTasks.Add(Task.Run(async () =>
                            {
                                await semaphore.WaitAsync(); // Wait for a slot to become available
                                try
                                {
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode,
                                        PayMentTypeEnum.VietinBank, item.Description?.Trim(), item.CreditAmount);

                                    if (payEntity != null)
                                    {
                                        if (_redisService.SetSuccessOrder(payEntity.ID))
                                        {
                                            var bankOrderPubModel = new BankOrderPubModel
                                            {
                                                MerchantCode = input.MerchantCode,
                                                Type = PayMentTypeEnum.VietinBank,
                                                PayMentId = item.PayMentId,
                                                PayOrderId = payEntity.ID,
                                                Id = item.RefNo,
                                                Money = item.CreditAmount,
                                                Desc = item.Description,
                                            };
                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.VTBBankOrder, bankOrderPubModel);
                                        }
                                    }
                                }
                                finally
                                {
                                    semaphore.Release(); // Release the slot
                                }
                            }));
                        }
                        await Task.WhenAll(matchingTasks); // Wait for all tasks to complete
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Vtb Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("Vtb记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取Vcb记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/VcbRecord")]
        public async Task<JsonResult> VcbRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("Vcb记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var recordsToProcess = input.BankRecords
                        .GroupBy(r => new { r.RefNo, Description = r.Description?.Trim() }) // Group by unique combination
                        .Select(g => g.First()) // Pick the first record from each group
                        .ToList();

                    var semaphore = new SemaphoreSlim(3); // Limit concurrent inserts
                    var existingRecordsSet = new HashSet<Tuple<string, string>>(); // Track existing records

                    foreach (var record in recordsToProcess)
                    {
                        var cRecordUnique = Tuple.Create(record.RefNo, record.Description);

                        if (existingRecordsSet.Contains(cRecordUnique))
                            continue; // Skip if already processed

                        var cacheKey = $"VcbExistingTransactions_{input.PayMentId}_{record.RefNo}_{record.Description}";

                        if (!_memoryCache.TryGetValue(cacheKey, out string _var))
                        {
                            var existingRecord = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());

                            if (existingRecord != null)
                            {
                                _memoryCache.Set(cacheKey, "", CacheConstants.DefaultCacheOptions);
                                existingRecordsSet.Add(cRecordUnique); // Store existing record locally
                            }
                        }
                        else
                        {
                            existingRecordsSet.Add(cRecordUnique); // If found in cache, add to local tracking
                        }
                    }
                    // Filter new records
                    var newRecords = recordsToProcess
                         .Where(r => !existingRecordsSet.Contains(Tuple.Create(r.RefNo, r.Description)))
                         .ToList();

                    var transactionLists = new ConcurrentBag<PayOrderDepositsMongoEntity>();

                    NlogLogger.Warn("Vcb记录[New]：" + newRecords.ToJsonString());

                    List<Task> insertTasks = new List<Task>();

                    foreach (var record in newRecords)
                    {
                        insertTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await semaphore.WaitAsync(); // Prevent simultaneous inserts

                                var transactionDate = _payMonitorCommonHelpers.ConvertToStandardTime(record.TransactionTime.Trim());
                                var type = record.Amount.Contains("+") ? "CRDT" : "DBIT";
                                var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");

                                var payOrderDeposit = new PayOrderDepositsMongoEntity
                                {
                                    PayType = PayMentTypeEnum.VietcomBank.ToInt(),
                                    Type = type,
                                    RefNo = record.RefNo,
                                    Description = record.Description?.Trim(),
                                    UserName = input.Phone,
                                    AccountNo = input.CardNumber,
                                    CreditBank = "",
                                    CreditAcctNo = "",
                                    CreditAcctName = "",
                                    CreditAmount = amount.ParseToDecimal(),
                                    DebitBank = "",
                                    DebitAcctNo = "",
                                    DebitAcctName = "",
                                    DebitAmount = amount.ParseToDecimal(),
                                    AvailableBalance = 0,
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(transactionDate ?? DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                    PayMentId = input.PayMentId
                                };

                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                transactionLists.Add(payOrderDeposit);

                                //加入取款缓存
                                if (payOrderDeposit.Type == "DBIT")
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel()
                                    {
                                        Type = PayMentTypeEnum.VietcomBank,
                                        RefNo = payOrderDeposit.RefNo,
                                        PayMentId = payOrderDeposit.PayMentId,
                                        TransferTime = payOrderDeposit.TransactionTime,
                                        Remark = payOrderDeposit.Description,
                                        Money = amount.ParseToDecimal(),
                                    };
                                    //  _redisService.AddTransferOrderQueueList(bankOrderNotify);
                                }
                            }
                            finally
                            {
                                semaphore.Release(); // Release after execution
                            }

                        }));
                    }

                    await Task.WhenAll(insertTasks);

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _vietcomBankHelper.SetLastRefNoKey(input.CardNumber, lastInfo.RefNo);
                        }

                        var matchingTasks = new List<Task>(); // List to store running tasks

                        foreach (var item in transactionLists)
                        {
                            matchingTasks.Add(Task.Run(async () =>
                            {
                                await semaphore.WaitAsync(); // Wait for a slot to become available
                                try
                                {
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode,
                                        PayMentTypeEnum.VietcomBank, item.Description?.Trim(), item.CreditAmount);

                                    if (payEntity != null)
                                    {
                                        if (_redisService.SetSuccessOrder(payEntity.ID))
                                        {
                                            var bankOrderPubModel = new BankOrderPubModel()
                                            {
                                                MerchantCode = input.MerchantCode,
                                                Type = PayMentTypeEnum.VietcomBank,
                                                PayMentId = item.PayMentId,
                                                PayOrderId = payEntity.ID,
                                                Id = item.RefNo,
                                                Money = item.CreditAmount,
                                                Desc = item.Description,
                                            };

                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.VCBBankOrder, bankOrderPubModel);
                                        }
                                    }
                                }
                                finally
                                {
                                    semaphore.Release(); // Release the slot
                                }
                            }));
                        }
                        await Task.WhenAll(matchingTasks); // Wait for all tasks to complete
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Vcb Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("Vcb记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取Tcb记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/TcbRecord")]
        public async Task<JsonResult> TcbRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("Tcb记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var recordsToProcess = input.BankRecords
                        .GroupBy(r => new { r.RefNo, Description = r.Description?.Trim() }) // Group by unique combination
                        .Select(g => g.First()) // Pick the first record from each group
                        .ToList();

                    var semaphore = new SemaphoreSlim(3); // Limit concurrent inserts
                    var existingRecordsSet = new HashSet<Tuple<string, string>>(); // Track existing records

                    foreach (var record in recordsToProcess)
                    {
                        var cRecordUnique = Tuple.Create(record.RefNo, record.Description);
                        if (existingRecordsSet.Contains(cRecordUnique))
                            continue; // Skip if already processed

                        var cacheKey = $"TcbExistingTransactions_{input.PayMentId}_{record.RefNo}_{record.Description}";

                        if (!_memoryCache.TryGetValue(cacheKey, out string _var))
                        {
                            var existingRecord = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());

                            if (existingRecord != null)
                            {
                                _memoryCache.Set(cacheKey, "", CacheConstants.DefaultCacheOptions);
                                existingRecordsSet.Add(cRecordUnique); // Store existing record locally
                            }
                        }
                        else
                        {
                            existingRecordsSet.Add(cRecordUnique); // If found in cache, add to local tracking
                        }
                    }

                    // Get only new records using the local HashSet
                    var newRecords = recordsToProcess
                        .Where(r => !existingRecordsSet.Contains(Tuple.Create(r.RefNo, r.Description)))
                        .ToList();

                    NlogLogger.Warn("Tcb记录[New]：" + newRecords.ToJsonString());

                    var transactionLists = new ConcurrentBag<PayOrderDepositsMongoEntity>();


                    List<Task> insertTask = new List<Task>();

                    foreach (var record in newRecords)
                    {
                        insertTask.Add(Task.Run(async () =>
                        {
                            await semaphore.WaitAsync(); // Prevent simultaneous inserts

                            try
                            {
                                var transactionDate = _payMonitorCommonHelpers.ConvertToStandardTime(record.TransactionTime.Trim());
                                var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");

                                var payOrderDeposit = new PayOrderDepositsMongoEntity
                                {
                                    PayType = PayMentTypeEnum.TechcomBank.ToInt(),
                                    Type = record.DorC,
                                    RefNo = record.RefNo,
                                    Description = record.Description?.Trim(),
                                    UserName = input.Phone,
                                    AccountNo = input.CardNumber,
                                    CreditBank = "",
                                    CreditAcctNo = record.CreditAcctNo,
                                    CreditAcctName = record.CreditAcctName,
                                    CreditAmount = amount.ParseToDecimal(),
                                    DebitBank = "",
                                    DebitAcctNo = record.CreditAcctNo,
                                    DebitAcctName = record.DebitAcctName,
                                    DebitAmount = amount.ParseToDecimal(),
                                    AvailableBalance = 0,
                                    //TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                    //TransactionTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(record.TransactionTime), TimeZoneInfo.FindSystemTimeZoneById("China Standard Time")),
                                    TransactionTime = transactionDate ?? DateTime.Parse(record.TransactionTime),
                                    PayMentId = input.PayMentId,
                                };

                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                transactionLists.Add(payOrderDeposit);

                                //加入取款缓存
                                if (payOrderDeposit.Type == "DBIT")
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel()
                                    {
                                        Type = PayMentTypeEnum.TechcomBank,
                                        RefNo = payOrderDeposit.RefNo,
                                        PayMentId = payOrderDeposit.PayMentId,
                                        TransferTime = payOrderDeposit.TransactionTime,
                                        Remark = payOrderDeposit.Description,
                                        Money = amount.ParseToDecimal(),
                                    };
                                    //_redisService.AddTransferOrderQueueList(bankOrderNotify);
                                }
                            }
                            finally
                            {
                                semaphore.Release(); // Release after execution
                            }
                        }));
                    }

                    await Task.WhenAll(insertTask);

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _techcomBankHelper.SetLastRefNoKey(input.CardNumber, lastInfo.RefNo);
                        }

                        var matchingTasks = new List<Task>(); // List to store running tasks

                        foreach (var item in transactionLists)
                        {
                            matchingTasks.Add(Task.Run(async () =>
                            {
                                await semaphore.WaitAsync(); // Wait for a slot to become available
                                try
                                {
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode,
                                        PayMentTypeEnum.TechcomBank, item.Description?.Trim(), item.CreditAmount);

                                    if (payEntity != null)
                                    {
                                        if (_redisService.SetSuccessOrder(payEntity.ID))
                                        {
                                            var bankOrderPubModel = new BankOrderPubModel
                                            {
                                                MerchantCode = input.MerchantCode,
                                                Type = PayMentTypeEnum.TechcomBank,
                                                PayMentId = item.PayMentId,
                                                PayOrderId = payEntity.ID,
                                                Id = item.RefNo,
                                                Money = item.CreditAmount,
                                                Desc = item.Description,
                                            };
                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.TCBBankOrder, bankOrderPubModel);
                                        }
                                    }
                                }
                                finally
                                {
                                    semaphore.Release(); // Release the slot
                                }
                            }));
                        }
                        await Task.WhenAll(matchingTasks); // Wait for all tasks to complete
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Tcb Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("Tcb记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取mb记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/MbRecord")]
        public async Task<JsonResult> MBRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("mb记录：" + input.ToJsonString());
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }

            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var lastrefno = _mBBankHelper.GetLastRefNoKey(input.CardNumber);
                    if (lastrefno == null)
                    {
                        lastrefno = "";
                    }
                    lastrefno = lastrefno.Replace("\"", "");
                    var transactionLists = new List<PayOrderDepositsMongoEntity>();
                    foreach (var record in input.BankRecords)
                    {
                        if (record.RefNo == lastrefno) break;
                        var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());
                        if (info == null)
                        {
                            var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "");

                            var payOrderDeposit = new PayOrderDepositsMongoEntity()
                            {
                                PayType = PayMentTypeEnum.MBBank.ToInt(),
                                Type = record.Amount.ParseToDecimal() > 0 ? "CRDT" : "DBIT",
                                RefNo = record.RefNo,
                                Description = record.Description?.Trim(),
                                UserName = input.Phone,
                                AccountNo = input.CardNumber,
                                CreditBank = record.BankName,
                                CreditAcctNo = record.CreditAcctNo,
                                CreditAcctName = record.CreditAcctName,
                                CreditAmount = amount.ParseToDecimal(),
                                DebitBank = record.BankName,
                                DebitAcctNo = record.DebitAcctNo,
                                DebitAcctName = record.DebitAcctName,
                                DebitAmount = amount.ParseToDecimal(),
                                AvailableBalance = record.AvailableBalance.ParseToDecimal(),
                                TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                PayMentId = input.PayMentId,
                            };
                            await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            transactionLists.Add(payOrderDeposit);

                            //加入取款缓存
                            if (payOrderDeposit.Type == "DBIT")
                            {
                                var bankOrderNotify = new BankOrderNotifyModel()
                                {
                                    Type = PayMentTypeEnum.MBBank,
                                    RefNo = payOrderDeposit.RefNo,
                                    PayMentId = payOrderDeposit.PayMentId,
                                    TransferTime = payOrderDeposit.TransactionTime,
                                    Remark = payOrderDeposit.Description,
                                    Money = amount.ParseToDecimal(),
                                };
                                // _redisService.AddTransferOrderQueueList(bankOrderNotify);
                            }
                        }
                    }

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _mBBankHelper.SetLastRefNoKey(input.CardNumber, lastInfo.RefNo);
                        }
                        foreach (var item in transactionLists)
                        {
                            var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.MBBank, item.Description?.Trim(), item.CreditAmount);
                            if (payEntity != null)
                            {
                                if (payEntity.PayMentId != item.PayMentId)
                                {
                                    NlogLogger.Fatal("订单方式不匹配，支付id：" + item.PayMentId + "，订单信息：" + payEntity.ToJsonString());
                                    continue;
                                }
                                //更新订单，同时增加流水表
                                if (_redisService.SetSuccessOrder(payEntity.ID))
                                {
                                    var bankOrderPubModel = new BankOrderPubModel()
                                    {
                                        MerchantCode = input.MerchantCode,
                                        Type = PayMentTypeEnum.MBBank,
                                        PayMentId = item.PayMentId,
                                        PayOrderId = payEntity.ID,
                                        Id = item.RefNo,
                                        Money = item.CreditAmount,
                                        Desc = item.Description,
                                    };

                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.MBBankOrder, bankOrderPubModel);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("mb Bank记录错误：" + ex);
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取acb记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/AcbRecord")]
        public async Task<JsonResult> AcbRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("acb记录：" + input.ToJsonString());
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }

            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var lastrefno = _acbBankHelper.GetLastRefNoKey(input.CardNumber);
                    if (lastrefno == null)
                    {
                        lastrefno = "";
                    }
                    lastrefno = lastrefno.Replace("\"", "");
                    var transactionLists = new List<PayOrderDepositsMongoEntity>();
                    foreach (var record in input.BankRecords)
                    {
                        if (record.RefNo == lastrefno) break;
                        var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());
                        if (info == null)
                        {
                            var type = record.Amount.Contains("-") ? "-" : "+";
                            var amount = record.Amount.Replace(".", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "");
                            var payOrderDeposit = new PayOrderDepositsMongoEntity()
                            {
                                PayType = PayMentTypeEnum.ACBBank.ToInt(),
                                Type = type == "+" ? "CRDT" : "DBIT",
                                RefNo = record.RefNo,
                                Description = record.Description?.Trim(),
                                UserName = input.Phone,
                                AccountNo = input.CardNumber,
                                CreditBank = "",
                                CreditAcctNo = "",
                                CreditAcctName = "",
                                CreditAmount = amount.ParseToDecimal(),
                                DebitBank = "",
                                DebitAcctNo = "",
                                DebitAcctName = "",
                                DebitAmount = amount.ParseToDecimal(),
                                AvailableBalance = record.AvailableBalance.Replace(".", "").Replace("-", "").ParseToDecimal(),
                                TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                PayMentId = input.PayMentId,
                            };
                            await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            transactionLists.Add(payOrderDeposit);

                            //加入取款缓存
                            if (payOrderDeposit.Type == "DBIT")
                            {
                                var bankOrderNotify = new BankOrderNotifyModel()
                                {
                                    Type = PayMentTypeEnum.ACBBank,
                                    RefNo = payOrderDeposit.RefNo,
                                    PayMentId = payOrderDeposit.PayMentId,
                                    TransferTime = payOrderDeposit.TransactionTime,
                                    Remark = payOrderDeposit.Description,
                                    Money = amount.ParseToDecimal(),
                                };
                                //  _redisService.AddTransferOrderQueueList(bankOrderNotify);
                            }
                        }
                    }

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.RefNo).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _acbBankHelper.SetLastRefNoKey(input.CardNumber, lastInfo.RefNo);
                        }
                        foreach (var item in transactionLists)
                        {
                            var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.ACBBank, item.Description?.Trim(), item.CreditAmount);
                            if (payEntity != null)
                            {
                                if (payEntity.PayMentId != item.PayMentId)
                                {
                                    NlogLogger.Fatal("订单方式不匹配，支付id：" + item.PayMentId + "，订单信息：" + payEntity.ToJsonString());
                                    continue;
                                }
                                //更新订单，同时增加流水表
                                if (_redisService.SetSuccessOrder(payEntity.ID))
                                {
                                    var bankOrderPubModel = new BankOrderPubModel()
                                    {
                                        MerchantCode = input.MerchantCode,
                                        Type = PayMentTypeEnum.ACBBank,
                                        PayMentId = item.PayMentId,
                                        PayOrderId = payEntity.ID,
                                        Id = item.RefNo,
                                        Money = item.CreditAmount,
                                        Desc = item.Description,
                                    };

                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.ACBBankOrder, bankOrderPubModel);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("acb Bank记录错误：" + ex);
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取BusinessMb记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/BusinessMbRecord")]
        public async Task<JsonResult> BusinessMbRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("business mb记录：" + input.ToJsonString());
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }

            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var lastrefno = _businessMBBankHelper.GetLastRefNoKey(input.CardNumber);
                    if (lastrefno == null)
                    {
                        lastrefno = "";
                    }
                    lastrefno = lastrefno.Replace("\"", "");
                    var transactionLists = new List<PayOrderDepositsMongoEntity>();
                    foreach (var record in input.BankRecords)
                    {
                        if (record.RefNo == lastrefno) break;
                        var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());
                        if (info == null)
                        {
                            var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "");

                            var payOrderDeposit = new PayOrderDepositsMongoEntity()
                            {
                                PayType = PayMentTypeEnum.BusinessMbBank.ToInt(),
                                Type = record.Amount.ParseToDecimal() > 0 ? "CRDT" : "DBIT",
                                RefNo = record.RefNo,
                                Description = record.Description?.Trim(),
                                UserName = input.Phone,
                                AccountNo = input.CardNumber,
                                CreditBank = "",
                                CreditAcctNo = "",
                                CreditAcctName = "",
                                CreditAmount = amount.ParseToDecimal(),
                                DebitBank = "",
                                DebitAcctNo = "",
                                DebitAcctName = "",
                                DebitAmount = amount.ParseToDecimal(),
                                AvailableBalance = record.AvailableBalance.ParseToDecimal(),
                                TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                PayMentId = input.PayMentId,
                            };
                            await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            transactionLists.Add(payOrderDeposit);
                        }
                    }

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _businessMBBankHelper.SetLastRefNoKey(input.CardNumber, lastInfo.RefNo);
                        }
                        foreach (var item in transactionLists)
                        {
                            var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.BusinessMbBank, item.Description?.Trim(), item.CreditAmount);
                            if (payEntity != null)
                            {
                                if (payEntity.PayMentId != item.PayMentId)
                                {
                                    NlogLogger.Fatal("订单方式不匹配，支付id：" + item.PayMentId + "，订单信息：" + payEntity.ToJsonString());
                                    continue;
                                }
                                //更新订单，同时增加流水表
                                if (_redisService.SetSuccessOrder(payEntity.ID))
                                {
                                    var bankOrderPubModel = new BankOrderPubModel()
                                    {
                                        MerchantCode = input.MerchantCode,
                                        Type = PayMentTypeEnum.BusinessMbBank,
                                        PayMentId = item.PayMentId,
                                        PayOrderId = payEntity.ID,
                                        Id = item.RefNo,
                                        Money = item.CreditAmount,
                                        Desc = item.Description,
                                    };

                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.BusinessMbBankOrder, bankOrderPubModel);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("business mb Bank记录错误：" + ex);
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取BusinessTc记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/BusinessTcbRecord")]
        public async Task<JsonResult> BusinessTcbRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("BusinessTcb记录：" + input.ToJsonString());
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count >= 0)
                {
                    var lastrefno = _techcomBankHelper.GetLastRefNoKey(input.CardNumber);
                    //if (lastrefno == null)
                    //{
                    //    lastrefno = "";
                    //}
                    //lastrefno = lastrefno.Replace("\"", "");
                    var transactionLists = new List<PayOrderDepositsMongoEntity>();
                    foreach (var record in input.BankRecords)
                    {
                        //if (record.RefNo == lastrefno) break;
                        var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());
                        if (info == null)
                        {
                            var type = "";
                            if (record.DorC == "Credit")
                            {
                                type = "CRDT";
                            }
                            if (record.DorC == "Debit")
                            {
                                type = "DBIT";
                            }
                            var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                            var payOrderDeposit = new PayOrderDepositsMongoEntity()
                            {
                                PayType = PayMentTypeEnum.BusinessTcbBank.ToInt(),
                                Type = type,
                                RefNo = record.RefNo,
                                Description = record.Description?.Trim(),
                                UserName = input.Phone,
                                AccountNo = input.CardNumber,
                                CreditBank = "",
                                CreditAcctNo = record.CreditAcctNo,
                                CreditAcctName = record.CreditAcctName,
                                CreditAmount = amount.ParseToDecimal(),
                                DebitBank = "",
                                DebitAcctNo = record.CreditAcctNo,
                                DebitAcctName = record.DebitAcctName,
                                DebitAmount = amount.ParseToDecimal(),
                                AvailableBalance = 0,
                                TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                PayMentId = input.PayMentId,
                            };
                            await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            transactionLists.Add(payOrderDeposit);
                        }
                    }

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _techcomBankHelper.SetLastRefNoKey(input.CardNumber, lastInfo.RefNo);
                        }
                        foreach (var item in transactionLists)
                        {
                            var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.BusinessTcbBank, item.Description?.Trim(), item.CreditAmount);
                            if (payEntity != null)
                            {
                                //更新订单，同时增加流水表
                                if (_redisService.SetSuccessOrder(payEntity.ID))
                                {
                                    var bankOrderPubModel = new BankOrderPubModel()
                                    {
                                        MerchantCode = input.MerchantCode,
                                        Type = PayMentTypeEnum.BusinessTcbBank,
                                        PayMentId = item.PayMentId,
                                        PayOrderId = payEntity.ID,
                                        Id = item.RefNo,
                                        Money = item.CreditAmount,
                                        Desc = item.Description,
                                    };
                                    bankOrderPubModel.Money = decimal.Parse(bankOrderPubModel.Money.ToString("F2"));
                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.BusinessTcbBankOrder, bankOrderPubModel);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("BusinessTcb记录错误：" + ex);
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取BusinessTc记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/BusinessVtbRecord")]
        public async Task<JsonResult> BusinessVtbRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("business vtb记录：" + input.ToJsonString());
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }

            try
            {
                if (input.BankRecords.Count > 0)
                {
                    //var lastrefno = _businessVtbBankHelper.GetLastRefNoKey(input.CardNumber);
                    //if (lastrefno == null)
                    //{
                    //    lastrefno = "";
                    //}
                    //lastrefno = lastrefno.Replace("\"", "");
                    var transactionLists = new List<PayOrderDepositsMongoEntity>();
                    foreach (var record in input.BankRecords)
                    {
                        //if (record.RefNo == lastrefno) break;
                        var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());
                        if (info == null)
                        {
                            var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "");
                            var payOrderDeposit = new PayOrderDepositsMongoEntity()
                            {
                                PayType = PayMentTypeEnum.BusinessVtbBank.ToInt(),
                                Type = record.DorC == "C" ? "CRDT" : "DBIT",
                                RefNo = record.RefNo,
                                Description = record.Description?.Trim(),
                                UserName = input.Phone,
                                AccountNo = input.CardNumber,
                                CreditBank = "",
                                CreditAcctNo = record.CreditAcctNo,
                                CreditAcctName = record.CreditAcctName,
                                CreditAmount = amount.ParseToDecimal(),
                                DebitBank = "",
                                DebitAcctNo = record.DebitAcctNo,
                                DebitAcctName = record.DebitAcctName,
                                DebitAmount = amount.ParseToDecimal(),
                                AvailableBalance = record.AvailableBalance.ParseToDecimal(),
                                TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                PayMentId = input.PayMentId,
                            };
                            await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            transactionLists.Add(payOrderDeposit);
                        }
                    }

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _businessVtbBankHelper.SetLastRefNoKey(input.CardNumber, lastInfo.RefNo);
                        }
                        foreach (var item in transactionLists)
                        {
                            var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.BusinessVtbBank, item.Description?.Trim(), item.CreditAmount);
                            if (payEntity != null)
                            {
                                if (payEntity.PayMentId != item.PayMentId)
                                {
                                    NlogLogger.Fatal("订单方式不匹配，支付id：" + item.PayMentId + "，订单信息：" + payEntity.ToJsonString());
                                    continue;
                                }
                                //更新订单，同时增加流水表
                                var successOrderCache = _redisService.GetSuccessOrder(payEntity.ID);
                                if (successOrderCache.IsNullOrEmpty())
                                {
                                    _redisService.SetSuccessOrder(payEntity.ID);

                                    var bankOrderPubModel = new BankOrderPubModel()
                                    {
                                        MerchantCode = input.MerchantCode,
                                        Type = PayMentTypeEnum.BusinessVtbBank,
                                        PayMentId = item.PayMentId,
                                        PayOrderId = payEntity.ID,
                                        Id = item.RefNo,
                                        Money = item.CreditAmount,
                                        Desc = item.Description,
                                    };

                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.BusinessVtbBankOrder, bankOrderPubModel);

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("business vtb Bank记录错误：" + ex);
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取pvCom记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/PVcomRecord")]
        public async Task<JsonResult> PVcomRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("pvCom记录：" + input.ToJsonString());
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }

            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var lastrefno = _pVcomBankHelper.GetLastRefNoKey(input.CardNumber);
                    if (lastrefno == null)
                    {
                        lastrefno = "";
                    }
                    lastrefno = lastrefno.Replace("\"", "");
                    var transactionLists = new List<PayOrderDepositsMongoEntity>();
                    foreach (var record in input.BankRecords)
                    {
                        if (record.RefNo == lastrefno) break;
                        var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());
                        if (info == null)
                        {
                            var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "");

                            var payOrderDeposit = new PayOrderDepositsMongoEntity()
                            {
                                PayType = PayMentTypeEnum.PVcomBank.ToInt(),
                                Type = record.Amount.ParseToDecimal() > 0 ? "CRDT" : "DBIT",
                                RefNo = record.RefNo,
                                Description = record.Description?.Trim(),
                                UserName = input.Phone,
                                AccountNo = input.CardNumber,
                                CreditBank = "",
                                CreditAcctNo = "",
                                CreditAcctName = "",
                                CreditAmount = amount.ParseToDecimal(),
                                DebitBank = "",
                                DebitAcctNo = "",
                                DebitAcctName = "",
                                DebitAmount = amount.ParseToDecimal(),
                                AvailableBalance = record.AvailableBalance.ParseToDecimal(),
                                TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                PayMentId = input.PayMentId,
                            };
                            await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            transactionLists.Add(payOrderDeposit);

                            //加入取款缓存
                            if (payOrderDeposit.Type == "DBIT")
                            {
                                var bankOrderNotify = new BankOrderNotifyModel()
                                {
                                    Type = PayMentTypeEnum.PVcomBank,
                                    RefNo = payOrderDeposit.RefNo,
                                    PayMentId = payOrderDeposit.PayMentId,
                                    TransferTime = payOrderDeposit.TransactionTime,
                                    Remark = payOrderDeposit.Description,
                                    Money = amount.ParseToDecimal(),
                                };
                                // _redisService.AddTransferOrderQueueList(bankOrderNotify);
                            }
                        }
                        else
                        {
                            info.Description = record.Description;
                            await _payOrderDepositsMongoService.UpdateAsync(info);
                        }
                    }

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _pVcomBankHelper.SetLastRefNoKey(input.CardNumber, lastInfo.RefNo);
                        }
                        foreach (var item in transactionLists)
                        {
                            var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.PVcomBank, item.Description?.Trim(), item.CreditAmount);
                            if (payEntity != null)
                            {
                                if (payEntity.PayMentId != item.PayMentId)
                                {
                                    NlogLogger.Fatal("订单方式不匹配，支付id：" + item.PayMentId + "，订单信息：" + payEntity.ToJsonString());
                                    continue;
                                }
                                //更新订单，同时增加流水表
                                if (_redisService.SetSuccessOrder(payEntity.ID))
                                {
                                    var bankOrderPubModel = new BankOrderPubModel()
                                    {
                                        MerchantCode = input.MerchantCode,
                                        Type = PayMentTypeEnum.PVcomBank,
                                        PayMentId = item.PayMentId,
                                        PayOrderId = payEntity.ID,
                                        Id = item.RefNo,
                                        Money = item.CreditAmount,
                                        Desc = item.Description,
                                    };

                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.PVcomBankOrder, bankOrderPubModel);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("PVcom Bank记录错误：" + ex);
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }


        /// <summary>
        /// 获取Msb记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/MsbRecord")]
        public async Task<JsonResult> MsbRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("Msb记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var transactionLists = new List<PayOrderDepositsMongoEntity>();
                    foreach (var record in input.BankRecords)
                    {
                        var info = await _payOrderDepositsMongoService.GetPayOrderByBankInDesc(record.Description?.Trim(), input.PayMentId);
                        if (info == null)
                        {
                            var type = record.Amount.Contains("+") ? "CRDT" : "DBIT";
                            var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                            var refNo = record.RefNo;
                            if (refNo.IsNullOrEmpty())
                            {
                                refNo = record.Description;
                                var tempArr = refNo.Split("-").ToList();
                                if (tempArr.Count > 2)
                                {
                                    refNo = tempArr[0] + tempArr[1];
                                }
                            }
                            var payOrderDeposit = new PayOrderDepositsMongoEntity()
                            {
                                PayType = PayMentTypeEnum.MsbBank.ToInt(),
                                Type = type,
                                RefNo = refNo,
                                Description = record.Description?.Trim(),
                                UserName = input.Phone,
                                AccountNo = input.CardNumber,
                                CreditBank = "",
                                CreditAcctNo = "",
                                CreditAcctName = "",
                                CreditAmount = amount.ParseToDecimal(),
                                DebitBank = "",
                                DebitAcctNo = "",
                                DebitAcctName = "",
                                DebitAmount = amount.ParseToDecimal(),
                                AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                PayMentId = input.PayMentId,
                            };
                            await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            transactionLists.Add(payOrderDeposit);

                            //加入取款缓存
                            if (payOrderDeposit.Type == "DBIT")
                            {
                                var bankOrderNotify = new BankOrderNotifyModel()
                                {
                                    Type = PayMentTypeEnum.MsbBank,
                                    RefNo = payOrderDeposit.RefNo,
                                    PayMentId = payOrderDeposit.PayMentId,
                                    TransferTime = payOrderDeposit.TransactionTime,
                                    Remark = payOrderDeposit.Description,
                                    Money = amount.ParseToDecimal(),
                                };
                                // _redisService.AddTransferOrderQueueList(bankOrderNotify);
                            }
                        }
                    }

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.MsbBank, input.CardNumber, lastInfo.RefNo);
                        }
                        foreach (var item in transactionLists)
                        {
                            var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.MsbBank, item.Description?.Trim(), item.CreditAmount);
                            if (payEntity != null)
                            {
                                //更新订单，同时增加流水表
                                if (_redisService.SetSuccessOrder(payEntity.ID))
                                {
                                    var bankOrderPubModel = new BankOrderPubModel()
                                    {
                                        MerchantCode = input.MerchantCode,
                                        Type = PayMentTypeEnum.MsbBank,
                                        PayMentId = item.PayMentId,
                                        PayOrderId = payEntity.ID,
                                        Id = item.RefNo,
                                        Money = item.CreditAmount,
                                        Desc = item.Description,
                                    };
                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.MBBankOrder, bankOrderPubModel);

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Msb Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("Msb记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取Sea记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/SeaBankRecord")]
        public async Task<JsonResult> SeaBankRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("Sea记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var transactionLists = new List<PayOrderDepositsMongoEntity>();
                    foreach (var record in input.BankRecords)
                    {
                        var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());
                        if (info == null)
                        {
                            var type = "CRDT";

                            if (record.Amount.Contains("+"))
                            {
                                type = "CRDT";
                            }
                            else if (record.Amount.Contains("-"))
                            {
                                type = "DBIT";
                            }

                            var actualTransactionDate = DateTime.MinValue;

                            if (input.Type == 0 && DateTime.TryParseExact(record.TransactionTime, "yyyyMMddHHmmss", new CultureInfo("de-DE"), DateTimeStyles.None, out var _result))
                            {
                                actualTransactionDate = _result;
                            }
                            else if (input.Type == 1 && DateTime.TryParse(record.TransactionTime, new CultureInfo("de-DE"), out var _result2))
                            {
                                actualTransactionDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(_result2, "China Standard Time");
                            }
                            else
                            {
                                actualTransactionDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time");
                            }

                            var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                            var payOrderDeposit = new PayOrderDepositsMongoEntity()
                            {
                                PayType = PayMentTypeEnum.SeaBank.ToInt(),
                                Type = type,
                                RefNo = record.RefNo,
                                Description = record.Description?.Trim(),
                                UserName = input.Phone,
                                AccountNo = input.CardNumber,
                                CreditBank = "",
                                CreditAcctNo = "",
                                CreditAcctName = "",
                                CreditAmount = amount.ParseToDecimal(),
                                DebitBank = "",
                                DebitAcctNo = "",
                                DebitAcctName = "",
                                DebitAmount = amount.ParseToDecimal(),
                                AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                TransactionTime = actualTransactionDate,
                                PayMentId = input.PayMentId,
                            };
                            await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            transactionLists.Add(payOrderDeposit);

                            //加入取款缓存
                            if (payOrderDeposit.Type == "DBIT")
                            {
                                var bankOrderNotify = new BankOrderNotifyModel()
                                {
                                    Type = PayMentTypeEnum.SeaBank,
                                    RefNo = payOrderDeposit.RefNo,
                                    PayMentId = payOrderDeposit.PayMentId,
                                    TransferTime = payOrderDeposit.TransactionTime,
                                    Remark = payOrderDeposit.Description,
                                    Money = amount.ParseToDecimal(),
                                };
                                //  _redisService.AddTransferOrderQueueList(bankOrderNotify);
                            }
                        }
                    }

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.SeaBank, input.CardNumber, lastInfo.RefNo);
                        }
                        foreach (var item in transactionLists)
                        {
                            var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.SeaBank, item.Description?.Trim(), item.CreditAmount);
                            if (payEntity != null)
                            {
                                //更新订单，同时增加流水表
                                if (_redisService.SetSuccessOrder(payEntity.ID))
                                {
                                    var bankOrderPubModel = new BankOrderPubModel()
                                    {
                                        MerchantCode = input.MerchantCode,
                                        Type = PayMentTypeEnum.SeaBank,
                                        PayMentId = item.PayMentId,
                                        PayOrderId = payEntity.ID,
                                        Id = item.RefNo,
                                        Money = item.CreditAmount,
                                        Desc = item.Description,
                                    };
                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.SeaBankOrder, bankOrderPubModel);

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Sea Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("Sea记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }


        /// <summary>
        /// 获取bv记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/BvBankRecord")]
        public async Task<JsonResult> BvBankRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("Bvb记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var transactionLists = new List<PayOrderDepositsMongoEntity>();
                    foreach (var record in input.BankRecords)
                    {
                        var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());
                        if (info == null)
                        {
                            var type = record.Amount.Contains("+") ? "CRDT" : "DBIT";
                            var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                            var payOrderDeposit = new PayOrderDepositsMongoEntity()
                            {
                                PayType = PayMentTypeEnum.BvBank.ToInt(),
                                Type = type,
                                RefNo = record.RefNo,
                                Description = record.Description?.Trim(),
                                UserName = input.Phone,
                                AccountNo = input.CardNumber,
                                CreditBank = "",
                                CreditAcctNo = "",
                                CreditAcctName = "",
                                CreditAmount = amount.ParseToDecimal(),
                                DebitBank = "",
                                DebitAcctNo = "",
                                DebitAcctName = "",
                                DebitAmount = amount.ParseToDecimal(),
                                AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                PayMentId = input.PayMentId,
                            };
                            await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            transactionLists.Add(payOrderDeposit);

                            //加入取款缓存
                            if (payOrderDeposit.Type == "DBIT")
                            {
                                var bankOrderNotify = new BankOrderNotifyModel()
                                {
                                    Type = PayMentTypeEnum.BvBank,
                                    RefNo = payOrderDeposit.RefNo,
                                    PayMentId = payOrderDeposit.PayMentId,
                                    TransferTime = payOrderDeposit.TransactionTime,
                                    Remark = payOrderDeposit.Description,
                                    Money = amount.ParseToDecimal(),
                                };
                                // _redisService.AddTransferOrderQueueList(bankOrderNotify);
                            }
                        }
                    }

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.BvBank, input.CardNumber, lastInfo.RefNo);
                        }
                        foreach (var item in transactionLists)
                        {
                            var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.BvBank, item.Description?.Trim(), item.CreditAmount);
                            if (payEntity != null)
                            {
                                //更新订单，同时增加流水表
                                if (_redisService.SetSuccessOrder(payEntity.ID))
                                {
                                    var bankOrderPubModel = new BankOrderPubModel()
                                    {
                                        MerchantCode = input.MerchantCode,
                                        Type = PayMentTypeEnum.BvBank,
                                        PayMentId = item.PayMentId,
                                        PayOrderId = payEntity.ID,
                                        Id = item.RefNo,
                                        Money = item.CreditAmount,
                                        Desc = item.Description,
                                    };
                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.BvBankOrder, bankOrderPubModel);

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Bv Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("Bvb记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取Nama记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/NamaBankRecord")]
        public async Task<JsonResult> NamaBankRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("Nama记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var transactionLists = new List<PayOrderDepositsMongoEntity>();
                    foreach (var record in input.BankRecords)
                    {
                        var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());
                        if (info == null)
                        {

                            var transactionDate = _payMonitorCommonHelpers.ConvertToStandardTime(record.TransactionTime.Trim());
                            var type = record.Amount.Contains("+") ? "CRDT" : "DBIT";
                            var amount = record.Amount.Replace("+", "").Replace(".", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                            var payOrderDeposit = new PayOrderDepositsMongoEntity()
                            {
                                PayType = PayMentTypeEnum.NamaBank.ToInt(),
                                Type = type,
                                RefNo = record.RefNo,
                                Description = record.Description?.Trim(),
                                UserName = input.Phone,
                                AccountNo = input.CardNumber,
                                CreditBank = "",
                                CreditAcctNo = "",
                                CreditAcctName = "",
                                CreditAmount = amount.ParseToDecimal(),
                                DebitBank = "",
                                DebitAcctNo = "",
                                DebitAcctName = "",
                                DebitAmount = amount.ParseToDecimal(),
                                AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId((transactionDate ?? DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE"))), "China Standard Time"),
                                PayMentId = input.PayMentId,
                            };
                            await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            transactionLists.Add(payOrderDeposit);

                            //加入取款缓存
                            if (payOrderDeposit.Type == "DBIT")
                            {
                                var bankOrderNotify = new BankOrderNotifyModel()
                                {
                                    Type = PayMentTypeEnum.NamaBank,
                                    RefNo = payOrderDeposit.RefNo,
                                    PayMentId = payOrderDeposit.PayMentId,
                                    TransferTime = payOrderDeposit.TransactionTime,
                                    Remark = payOrderDeposit.Description,
                                    Money = amount.ParseToDecimal(),
                                };
                                // _redisService.AddTransferOrderQueueList(bankOrderNotify);
                            }
                        }
                    }

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.NamaBank, input.CardNumber, lastInfo.RefNo);
                        }
                        foreach (var item in transactionLists)
                        {
                            var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.NamaBank, item.Description?.Trim(), item.CreditAmount);
                            if (payEntity != null)
                            {
                                //更新订单，同时增加流水表
                                if (_redisService.SetSuccessOrder(payEntity.ID))
                                {
                                    var bankOrderPubModel = new BankOrderPubModel()
                                    {
                                        MerchantCode = input.MerchantCode,
                                        Type = PayMentTypeEnum.NamaBank,
                                        PayMentId = item.PayMentId,
                                        PayOrderId = payEntity.ID,
                                        Id = item.RefNo,
                                        Money = item.CreditAmount,
                                        Desc = item.Description,
                                    };
                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.NamaBankOrder, bankOrderPubModel);

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Nama Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("Nama记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }



        /// <summary>
        /// 获取TP记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/TPBankRecord")]
        public async Task<JsonResult> TPBankRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("TPb记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var transactionLists = new List<PayOrderDepositsMongoEntity>();
                    foreach (var record in input.BankRecords)
                    {
                        var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());
                        if (info == null)
                        {
                            var type = record.Amount.Contains("+") ? "CRDT" : "DBIT";
                            var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                            var payOrderDeposit = new PayOrderDepositsMongoEntity()
                            {
                                PayType = PayMentTypeEnum.TPBank.ToInt(),
                                Type = type,
                                RefNo = record.RefNo,
                                Description = record.Description?.Trim(),
                                UserName = input.Phone,
                                AccountNo = input.CardNumber,
                                CreditBank = "",
                                CreditAcctNo = "",
                                CreditAcctName = "",
                                CreditAmount = amount.ParseToDecimal(),
                                DebitBank = "",
                                DebitAcctNo = "",
                                DebitAcctName = "",
                                DebitAmount = amount.ParseToDecimal(),
                                AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                PayMentId = input.PayMentId,
                            };
                            await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            transactionLists.Add(payOrderDeposit);

                            //加入取款缓存
                            if (payOrderDeposit.Type == "DBIT")
                            {
                                var bankOrderNotify = new BankOrderNotifyModel()
                                {
                                    Type = PayMentTypeEnum.TPBank,
                                    RefNo = payOrderDeposit.RefNo,
                                    PayMentId = payOrderDeposit.PayMentId,
                                    TransferTime = payOrderDeposit.TransactionTime,
                                    Remark = payOrderDeposit.Description,
                                    Money = amount.ParseToDecimal(),
                                };
                                // _redisService.AddTransferOrderQueueList(bankOrderNotify);
                            }
                        }
                    }

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.TPBank, input.CardNumber, lastInfo.RefNo);
                        }
                        foreach (var item in transactionLists)
                        {
                            var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.TPBank, item.Description?.Trim(), item.CreditAmount);
                            if (payEntity != null)
                            {
                                //更新订单，同时增加流水表
                                if (_redisService.SetSuccessOrder(payEntity.ID))
                                {
                                    var bankOrderPubModel = new BankOrderPubModel()
                                    {
                                        MerchantCode = input.MerchantCode,
                                        Type = PayMentTypeEnum.TPBank,
                                        PayMentId = item.PayMentId,
                                        PayOrderId = payEntity.ID,
                                        Id = item.RefNo,
                                        Money = item.CreditAmount,
                                        Desc = item.Description,
                                    };
                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.TPBankOrder, bankOrderPubModel);

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("TP Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("TPb记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取VPB记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/VPBBankRecord")]
        public async Task<JsonResult> VPBBankRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("VPBb记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var transactionLists = new List<PayOrderDepositsMongoEntity>();
                    foreach (var record in input.BankRecords)
                    {
                        var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());
                        if (info == null)
                        {
                            var type = "CRDT";

                            if (record.Amount.Contains("+"))
                            {
                                type = "CRDT";
                            }
                            else if (record.Amount.Contains("-"))
                            {
                                type = "DBIT";
                            }

                            var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                            var payOrderDeposit = new PayOrderDepositsMongoEntity()
                            {
                                PayType = PayMentTypeEnum.VPBBank.ToInt(),
                                Type = type,
                                RefNo = record.RefNo,
                                Description = record.Description?.Trim(),
                                UserName = input.Phone,
                                AccountNo = input.CardNumber,
                                CreditBank = "",
                                CreditAcctNo = "",
                                CreditAcctName = "",
                                CreditAmount = amount.ParseToDecimal(),
                                DebitBank = "",
                                DebitAcctNo = "",
                                DebitAcctName = "",
                                DebitAmount = amount.ParseToDecimal(),
                                AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                PayMentId = input.PayMentId,
                            };
                            await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            transactionLists.Add(payOrderDeposit);

                            //加入取款缓存
                            if (payOrderDeposit.Type == "DBIT")
                            {
                                var bankOrderNotify = new BankOrderNotifyModel()
                                {
                                    Type = PayMentTypeEnum.VPBBank,
                                    RefNo = payOrderDeposit.RefNo,
                                    PayMentId = payOrderDeposit.PayMentId,
                                    TransferTime = payOrderDeposit.TransactionTime,
                                    Remark = payOrderDeposit.Description,
                                    Money = amount.ParseToDecimal(),
                                };
                                // _redisService.AddTransferOrderQueueList(bankOrderNotify);
                            }
                        }
                    }

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.VPBBank, input.CardNumber, lastInfo.RefNo);
                        }
                        foreach (var item in transactionLists)
                        {
                            var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.VPBBank, item.Description?.Trim(), item.CreditAmount);
                            if (payEntity != null)
                            {
                                //更新订单，同时增加流水表
                                if (_redisService.SetSuccessOrder(payEntity.ID))
                                {
                                    var bankOrderPubModel = new BankOrderPubModel()
                                    {
                                        MerchantCode = input.MerchantCode,
                                        Type = PayMentTypeEnum.VPBBank,
                                        PayMentId = item.PayMentId,
                                        PayOrderId = payEntity.ID,
                                        Id = item.RefNo,
                                        Money = item.CreditAmount,
                                        Desc = item.Description,
                                    };
                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.VPBBankOrder, bankOrderPubModel);

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("VPB Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("VPBb记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }


        /// <summary>
        /// 获取OCB记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/OCBBankRecord")]
        public async Task<JsonResult> OCBBankRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("OCB 记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var recordsToProcess = input.BankRecords
            .GroupBy(r => new { r.RefNo, Description = r.Description?.Trim() }) // Group by unique combination
            .Select(g => g.First()) // Pick the first record from each group
            .ToList();

                    var semaphore = new SemaphoreSlim(3); // Limit concurrent inserts
                    var existingRecordsSet = new HashSet<Tuple<string, string>>(); // Track existing records

                    foreach (var record in recordsToProcess)
                    {
                        if (existingRecordsSet.Contains(Tuple.Create(record.RefNo, record.Description)))
                            continue; // Skip if already processed

                        var cacheKey = $"OCBTransaction_{input.PayMentId}_{record.RefNo}_{record.Description}";

                        if (!_memoryCache.TryGetValue(cacheKey, out string _var))
                        {
                            var existingRecord = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());

                            if (existingRecord != null)
                            {
                                _memoryCache.Set(cacheKey, "", CacheConstants.DefaultCacheOptions);
                                existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // Store existing record locally
                            }
                        }
                        else
                        {
                            existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // If found in cache, add to local tracking
                        }
                    }
                    // Get only new records using the local HashSet
                    var newRecords = recordsToProcess
                        .Where(r => !existingRecordsSet.Contains(Tuple.Create(r.RefNo, r.Description)))
                        .ToList();

                    NlogLogger.Warn("OCB记录[New]：" + newRecords.ToJsonString());

                    var transactionLists = new ConcurrentBag<PayOrderDepositsMongoEntity>();

                    var insertTasks = new List<Task>();

                    foreach (var record in newRecords)
                    {
                        insertTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await semaphore.WaitAsync(); // Prevent simultaneous inserts
                                var transactionDate = _payMonitorCommonHelpers.ConvertToStandardTime(record.TransactionTime.Trim());
                                var type = "CRDT";

                                if (record.Amount.Contains("+"))
                                {
                                    type = "CRDT";
                                }
                                else if (record.Amount.Contains("-"))
                                {
                                    type = "DBIT";
                                }

                                var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                                var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                {
                                    PayType = PayMentTypeEnum.OCBBank.ToInt(),
                                    Type = type,
                                    RefNo = record.RefNo,
                                    Description = record.Description?.Trim(),
                                    UserName = input.Phone,
                                    AccountNo = input.CardNumber,
                                    CreditBank = "",
                                    CreditAcctNo = "",
                                    CreditAcctName = "",
                                    CreditAmount = amount.ParseToDecimal(),
                                    DebitBank = "",
                                    DebitAcctNo = "",
                                    DebitAcctName = "",
                                    DebitAmount = amount.ParseToDecimal(),
                                    AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId((transactionDate ?? DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE"))), "China Standard Time"),
                                    PayMentId = input.PayMentId,
                                };
                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                transactionLists.Add(payOrderDeposit);

                                //加入取款缓存
                                if (payOrderDeposit.Type == "DBIT")
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel()
                                    {
                                        Type = PayMentTypeEnum.OCBBank,
                                        RefNo = payOrderDeposit.RefNo,
                                        PayMentId = payOrderDeposit.PayMentId,
                                        TransferTime = payOrderDeposit.TransactionTime,
                                        Remark = payOrderDeposit.Description,
                                        Money = amount.ParseToDecimal(),
                                    };
                                    //_redisService.AddTransferOrderQueueList(bankOrderNotify);
                                }
                            }
                            finally
                            {
                                semaphore.Release(); // Release after execution
                            }
                        }));

                    }
                    await Task.WhenAll(insertTasks); // Wait for all insert tasks to complete

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.OCBBank, input.CardNumber, lastInfo.RefNo);
                        }

                        var matchingTasks = new List<Task>(); // List to store running tasks


                        foreach (var item in transactionLists)
                        {
                            matchingTasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await semaphore.WaitAsync(); // Wait for a slot to become available
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.OCBBank, item.Description?.Trim(), item.CreditAmount);
                                    if (payEntity != null)
                                    {
                                        //更新订单，同时增加流水表
                                        if (_redisService.SetSuccessOrder(payEntity.ID))
                                        {
                                            var bankOrderPubModel = new BankOrderPubModel()
                                            {
                                                MerchantCode = input.MerchantCode,
                                                Type = PayMentTypeEnum.OCBBank,
                                                PayMentId = item.PayMentId,
                                                PayOrderId = payEntity.ID,
                                                Id = item.RefNo,
                                                Money = item.CreditAmount,
                                                Desc = item.Description,
                                            };
                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.OCBBankOrder, bankOrderPubModel);

                                        }
                                    }
                                }
                                finally
                                {
                                    semaphore.Release(); // Release the slot
                                }
                            }));
                        }

                        await Task.WhenAll(matchingTasks); // Wait for all tasks to complete
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("OCB Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("OCB 记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取OCB记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/EXIMBankRecord")]
        public async Task<JsonResult> EXIMBankRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("EXIM 记录：" + input.ToJsonString());
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var transactionLists = new List<PayOrderDepositsMongoEntity>();
                    foreach (var record in input.BankRecords)
                    {
                        var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());
                        var transactionDate = _payMonitorCommonHelpers.ConvertToStandardTime(record.TransactionTime.Trim());

                        if (info == null)
                        {
                            var type = "CRDT";

                            if (record.Amount.Contains("+"))
                            {
                                type = "CRDT";
                            }
                            else if (record.Amount.Contains("-"))
                            {
                                type = "DBIT";
                            }

                            var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                            var payOrderDeposit = new PayOrderDepositsMongoEntity()
                            {
                                PayType = PayMentTypeEnum.EXIMBank.ToInt(),
                                Type = type,
                                RefNo = record.RefNo,
                                Description = record.Description?.Trim(),
                                UserName = input.Phone,
                                AccountNo = input.CardNumber,
                                CreditBank = "",
                                CreditAcctNo = "",
                                CreditAcctName = "",
                                CreditAmount = amount.ParseToDecimal(),
                                DebitBank = "",
                                DebitAcctNo = "",
                                DebitAcctName = "",
                                DebitAmount = amount.ParseToDecimal(),
                                AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId((transactionDate ?? DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE"))), "China Standard Time"),
                                PayMentId = input.PayMentId,
                            };
                            await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            transactionLists.Add(payOrderDeposit);

                            //加入取款缓存
                            if (payOrderDeposit.Type == "DBIT")
                            {
                                var bankOrderNotify = new BankOrderNotifyModel()
                                {
                                    Type = PayMentTypeEnum.EXIMBank,
                                    RefNo = payOrderDeposit.RefNo,
                                    PayMentId = payOrderDeposit.PayMentId,
                                    TransferTime = payOrderDeposit.TransactionTime,
                                    Remark = payOrderDeposit.Description,
                                    Money = amount.ParseToDecimal(),
                                };
                                // _redisService.AddTransferOrderQueueList(bankOrderNotify);
                            }
                        }
                    }

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.EXIMBank, input.CardNumber, lastInfo.RefNo);
                        }
                        foreach (var item in transactionLists)
                        {
                            var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.EXIMBank, item.Description?.Trim(), item.CreditAmount);
                            if (payEntity != null)
                            {
                                //更新订单，同时增加流水表
                                if (_redisService.SetSuccessOrder(payEntity.ID))
                                {
                                    var bankOrderPubModel = new BankOrderPubModel()
                                    {
                                        MerchantCode = input.MerchantCode,
                                        Type = PayMentTypeEnum.EXIMBank,
                                        PayMentId = item.PayMentId,
                                        PayOrderId = payEntity.ID,
                                        Id = item.RefNo,
                                        Money = item.CreditAmount,
                                        Desc = item.Description,
                                    };
                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.EXIMBankOrder, bankOrderPubModel);

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("EXIM Bank记录错误：" + ex);
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }
        [HttpPost]
        [Route("~/PayMonitor/NCBBankRecord")]
        public async Task<JsonResult> NCBBankRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("NCB 记录：" + input.ToJsonString());
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var updateDescriptionRecords = new List<string>();
                    var recordsToProcess = input.BankRecords
                .GroupBy(r => new { r.RefNo, Description = r.Description?.Trim() }) // Group by unique combination
                .Select(g => g.First()) // Pick the first record from each group
                .ToList();

                    var semaphore = new SemaphoreSlim(3); // Limit concurrent inserts
                    var existingRecordsSet = new HashSet<Tuple<string, string>>(); // Track existing records

                    foreach (var record in recordsToProcess)
                    {
                        if (existingRecordsSet.Contains(Tuple.Create(record.RefNo, record.Description)))
                            continue; // Skip if already processed

                        var cacheKey = $"NCBTransaction_{input.PayMentId}_{record.RefNo}_{record.Description}";

                        if (!_memoryCache.TryGetValue(cacheKey, out string _var))
                        {
                            var existingRecord = await _payOrderDepositsMongoService.GetPayOrderByBankNoDesc(record.RefNo, input.PayMentId);

                            if (existingRecord != null)
                            {
                                if (existingRecord.Description == record.Description)
                                {
                                    _memoryCache.Set(cacheKey, "", CacheConstants.DefaultCacheOptions);
                                    existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // Store existing record locally
                                }
                                else
                                {
                                    // meaning Description Not Same , need to Update remark only and no need process
                                    existingRecord.Description = record.Description;
                                    updateDescriptionRecords.Add(existingRecord.RefNo);
                                    await _payOrderDepositsMongoService.UpdateAsync(existingRecord);
                                    _memoryCache.Set(cacheKey, "", CacheConstants.DefaultCacheOptions);
                                    existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // Store existing record locally
                                }

                            }
                        }
                        else
                        {
                            existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // If found in cache, add to local tracking
                        }
                    }
                    // Get only new records using the local HashSet
                    var newRecords = recordsToProcess
                        .Where(r => !existingRecordsSet.Contains(Tuple.Create(r.RefNo, r.Description)))
                        .ToList();

                    NlogLogger.Warn("NCB记录[New]：" + newRecords.ToJsonString() + " , " + "NCB记录[Update Remark]：" + updateDescriptionRecords.ToJsonString());

                    var transactionLists = new ConcurrentBag<PayOrderDepositsMongoEntity>();

                    var insertTasks = new List<Task>();

                    foreach (var record in newRecords)
                    {
                        insertTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await semaphore.WaitAsync(); // Prevent simultaneous inserts
                                var type = "CRDT";

                                if (record.Amount.Contains("+"))
                                {
                                    type = "CRDT";
                                }
                                else if (record.Amount.Contains("-"))
                                {
                                    type = "DBIT";
                                }

                                var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                                var transactionDate = _payMonitorCommonHelpers.ConvertToStandardTime(record.TransactionTime.Trim());
                                var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                {
                                    PayType = PayMentTypeEnum.NCBBank.ToInt(),
                                    Type = type,
                                    RefNo = record.RefNo,
                                    Description = record.Description?.Trim(),
                                    UserName = input.Phone,
                                    AccountNo = input.CardNumber,
                                    CreditBank = "",
                                    CreditAcctNo = "",
                                    CreditAcctName = "",
                                    CreditAmount = amount.ParseToDecimal(),
                                    DebitBank = "",
                                    DebitAcctNo = "",
                                    DebitAcctName = "",
                                    DebitAmount = amount.ParseToDecimal(),
                                    AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(transactionDate ?? DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE")), "China Standard Time"),
                                    PayMentId = input.PayMentId,
                                };
                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                transactionLists.Add(payOrderDeposit);

                                //加入取款缓存
                                if (payOrderDeposit.Type == "DBIT")
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel()
                                    {
                                        Type = PayMentTypeEnum.NCBBank,
                                        RefNo = payOrderDeposit.RefNo,
                                        PayMentId = payOrderDeposit.PayMentId,
                                        TransferTime = payOrderDeposit.TransactionTime,
                                        Remark = payOrderDeposit.Description,
                                        Money = amount.ParseToDecimal(),
                                    };
                                    //_redisService.AddTransferOrderQueueList(bankOrderNotify);
                                }
                            }
                            finally
                            {
                                semaphore.Release(); // Release after execution
                            }
                        }));

                    }

                    await Task.WhenAll(insertTasks); // Wait for all insert tasks to complete

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.NCBBank, input.CardNumber, lastInfo.RefNo);
                        }

                        var matchingTasks = new List<Task>(); // List to store running tasks

                        foreach (var item in transactionLists)
                        {

                            matchingTasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await semaphore.WaitAsync(); // Wait for a slot to become available
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.NCBBank, item.Description?.Trim(), item.CreditAmount);
                                    if (payEntity != null)
                                    {
                                        //更新订单，同时增加流水表
                                        if (_redisService.SetSuccessOrder(payEntity.ID))
                                        {
                                            var bankOrderPubModel = new BankOrderPubModel()
                                            {
                                                MerchantCode = input.MerchantCode,
                                                Type = PayMentTypeEnum.NCBBank,
                                                PayMentId = item.PayMentId,
                                                PayOrderId = payEntity.ID,
                                                Id = item.RefNo,
                                                Money = item.CreditAmount,
                                                Desc = item.Description,
                                            };
                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.NCBBankOrder, bankOrderPubModel);

                                        }
                                    }
                                }
                                finally
                                {
                                    semaphore.Release(); // Release the slot
                                }
                            }));
                        }

                        await Task.WhenAll(matchingTasks); // Wait for all tasks to complete
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("NCB Bank记录错误：" + ex);
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }



        /// <summary>
        /// 获取HD记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/HDBankRecord")]
        public async Task<JsonResult> HDBankRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("HDB 记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var recordsToProcess = input.BankRecords
            .GroupBy(r => new { r.RefNo, Description = r.Description?.Trim() }) // Group by unique combination
            .Select(g => g.First()) // Pick the first record from each group
            .ToList();

                    var semaphore = new SemaphoreSlim(3); // Limit concurrent inserts
                    var existingRecordsSet = new HashSet<Tuple<string, string>>(); // Track existing records

                    foreach (var record in recordsToProcess)
                    {
                        if (existingRecordsSet.Contains(Tuple.Create(record.RefNo, record.Description)))
                            continue; // Skip if already processed

                        var cacheKey = $"HDBTransaction_{input.PayMentId}_{record.RefNo}_{record.Description}";

                        if (!_memoryCache.TryGetValue(cacheKey, out string _var))
                        {
                            var existingRecord = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());

                            if (existingRecord != null)
                            {
                                _memoryCache.Set(cacheKey, "", CacheConstants.DefaultCacheOptions);
                                existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // Store existing record locally
                            }
                        }
                        else
                        {
                            existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // If found in cache, add to local tracking
                        }
                    }
                    // Get only new records using the local HashSet
                    var newRecords = recordsToProcess
                        .Where(r => !existingRecordsSet.Contains(Tuple.Create(r.RefNo, r.Description)))
                        .ToList();

                    NlogLogger.Warn("HDB记录[New]：" + newRecords.ToJsonString());

                    var transactionLists = new ConcurrentBag<PayOrderDepositsMongoEntity>();

                    var insertTasks = new List<Task>();

                    foreach (var record in newRecords)
                    {
                        insertTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await semaphore.WaitAsync(); // Prevent simultaneous inserts
                                var transactionDate = _payMonitorCommonHelpers.ConvertToStandardTime(record.TransactionTime.Trim());
                                var type = "CRDT";

                                if (record.Amount.Contains("+"))
                                {
                                    type = "CRDT";
                                }
                                else if (record.Amount.Contains("-"))
                                {
                                    type = "DBIT";
                                }

                                var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                                var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                {
                                    PayType = PayMentTypeEnum.HDBank.ToInt(),
                                    Type = type,
                                    RefNo = record.RefNo,
                                    Description = record.Description?.Trim(),
                                    UserName = input.Phone,
                                    AccountNo = input.CardNumber,
                                    CreditBank = "",
                                    CreditAcctNo = "",
                                    CreditAcctName = "",
                                    CreditAmount = amount.ParseToDecimal(),
                                    DebitBank = "",
                                    DebitAcctNo = "",
                                    DebitAcctName = "",
                                    DebitAmount = amount.ParseToDecimal(),
                                    AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId((transactionDate ?? DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE"))), "China Standard Time"),
                                    PayMentId = input.PayMentId,
                                };
                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                transactionLists.Add(payOrderDeposit);

                                //加入取款缓存
                                if (payOrderDeposit.Type == "DBIT")
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel()
                                    {
                                        Type = PayMentTypeEnum.HDBank,
                                        RefNo = payOrderDeposit.RefNo,
                                        PayMentId = payOrderDeposit.PayMentId,
                                        TransferTime = payOrderDeposit.TransactionTime,
                                        Remark = payOrderDeposit.Description,
                                        Money = amount.ParseToDecimal(),
                                    };
                                    //_redisService.AddTransferOrderQueueList(bankOrderNotify);
                                }
                            }
                            finally
                            {
                                semaphore.Release(); // Release after execution
                            }
                        }));

                    }
                    await Task.WhenAll(insertTasks); // Wait for all insert tasks to complete

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.HDBank, input.CardNumber, lastInfo.RefNo);
                        }

                        var matchingTasks = new List<Task>(); // List to store running tasks


                        foreach (var item in transactionLists)
                        {
                            matchingTasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await semaphore.WaitAsync(); // Wait for a slot to become available
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.HDBank, item.Description?.Trim(), item.CreditAmount);
                                    if (payEntity != null)
                                    {
                                        //更新订单，同时增加流水表
                                        if (_redisService.SetSuccessOrder(payEntity.ID))
                                        {
                                            var bankOrderPubModel = new BankOrderPubModel()
                                            {
                                                MerchantCode = input.MerchantCode,
                                                Type = PayMentTypeEnum.HDBank,
                                                PayMentId = item.PayMentId,
                                                PayOrderId = payEntity.ID,
                                                Id = item.RefNo,
                                                Money = item.CreditAmount,
                                                Desc = item.Description,
                                            };
                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.HDBankOrder, bankOrderPubModel);

                                        }
                                    }
                                }
                                finally
                                {
                                    semaphore.Release(); // Release the slot
                                }
                            }));
                        }

                        await Task.WhenAll(matchingTasks); // Wait for all tasks to complete
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("HD Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("HD 记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取LP记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/LPBankRecord")]
        public async Task<JsonResult> LPBankRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("LPB 记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var recordsToProcess = input.BankRecords
                        .GroupBy(r => new { r.RefNo, Description = r.Description?.Trim() }) // Group by unique combination
                        .Select(g => g.First()) // Pick the first record from each group
                        .ToList();

                    var semaphore = new SemaphoreSlim(3); // Limit concurrent inserts
                    var existingRecordsSet = new HashSet<Tuple<string, string>>(); // Track existing records

                    foreach (var record in recordsToProcess)
                    {
                        if (existingRecordsSet.Contains(Tuple.Create(record.RefNo, record.Description)))
                            continue; // Skip if already processed

                        var cacheKey = $"LPBTransaction_{input.PayMentId}_{record.RefNo}_{record.Description}";

                        if (!_memoryCache.TryGetValue(cacheKey, out string _var))
                        {
                            var existingRecord = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());

                            if (existingRecord != null)
                            {
                                _memoryCache.Set(cacheKey, "", CacheConstants.DefaultCacheOptions);
                                existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // Store existing record locally
                            }
                        }
                        else
                        {
                            existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // If found in cache, add to local tracking
                        }
                    }
                    // Get only new records using the local HashSet
                    var newRecords = recordsToProcess
                        .Where(r => !existingRecordsSet.Contains(Tuple.Create(r.RefNo, r.Description)))
                        .ToList();

                    NlogLogger.Warn("LPB记录[New]：" + newRecords.ToJsonString());

                    var transactionLists = new ConcurrentBag<PayOrderDepositsMongoEntity>();

                    var insertTasks = new List<Task>();

                    foreach (var record in newRecords)
                    {
                        insertTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await semaphore.WaitAsync(); // Prevent simultaneous inserts
                                var transactionDate = _payMonitorCommonHelpers.ConvertToStandardTime(record.TransactionTime.Trim());
                                var type = "CRDT";

                                if (record.Amount.Contains("+"))
                                {
                                    type = "CRDT";
                                }
                                else if (record.Amount.Contains("-"))
                                {
                                    type = "DBIT";
                                }

                                var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                                var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                {
                                    PayType = PayMentTypeEnum.LPBank.ToInt(),
                                    Type = type,
                                    RefNo = record.RefNo,
                                    Description = record.Description?.Trim(),
                                    UserName = input.Phone,
                                    AccountNo = input.CardNumber,
                                    CreditBank = "",
                                    CreditAcctNo = "",
                                    CreditAcctName = "",
                                    CreditAmount = amount.ParseToDecimal(),
                                    DebitBank = "",
                                    DebitAcctNo = "",
                                    DebitAcctName = "",
                                    DebitAmount = amount.ParseToDecimal(),
                                    AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId((transactionDate ?? DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE"))), "China Standard Time"),
                                    PayMentId = input.PayMentId,
                                };
                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                transactionLists.Add(payOrderDeposit);

                                //加入取款缓存
                                if (payOrderDeposit.Type == "DBIT")
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel()
                                    {
                                        Type = PayMentTypeEnum.LPBank,
                                        RefNo = payOrderDeposit.RefNo,
                                        PayMentId = payOrderDeposit.PayMentId,
                                        TransferTime = payOrderDeposit.TransactionTime,
                                        Remark = payOrderDeposit.Description,
                                        Money = amount.ParseToDecimal(),
                                    };
                                    //_redisService.AddTransferOrderQueueList(bankOrderNotify);
                                }
                            }
                            finally
                            {
                                semaphore.Release(); // Release after execution
                            }
                        }));

                    }
                    await Task.WhenAll(insertTasks); // Wait for all insert tasks to complete

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.LPBank, input.CardNumber, lastInfo.RefNo);
                        }

                        var matchingTasks = new List<Task>(); // List to store running tasks


                        foreach (var item in transactionLists)
                        {
                            matchingTasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await semaphore.WaitAsync(); // Wait for a slot to become available
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.LPBank, item.Description?.Trim(), item.CreditAmount);
                                    if (payEntity != null)
                                    {
                                        //更新订单，同时增加流水表
                                        if (_redisService.SetSuccessOrder(payEntity.ID))
                                        {
                                            var bankOrderPubModel = new BankOrderPubModel()
                                            {
                                                MerchantCode = input.MerchantCode,
                                                Type = PayMentTypeEnum.LPBank,
                                                PayMentId = item.PayMentId,
                                                PayOrderId = payEntity.ID,
                                                Id = item.RefNo,
                                                Money = item.CreditAmount,
                                                Desc = item.Description,
                                            };
                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.LPBankOrder, bankOrderPubModel);

                                        }
                                    }
                                }
                                finally
                                {
                                    semaphore.Release(); // Release the slot
                                }
                            }));
                        }

                        await Task.WhenAll(matchingTasks); // Wait for all tasks to complete
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("LP Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("LP 记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取PG记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/PGBankRecord")]
        public async Task<JsonResult> PGBankRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("PGB 记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var recordsToProcess = input.BankRecords
                        .GroupBy(r => new { r.RefNo, Description = r.Description?.Trim() }) // Group by unique combination
                        .Select(g => g.First()) // Pick the first record from each group
                        .ToList();

                    var semaphore = new SemaphoreSlim(3); // Limit concurrent inserts
                    var existingRecordsSet = new HashSet<Tuple<string, string>>(); // Track existing records

                    foreach (var record in recordsToProcess)
                    {
                        if (existingRecordsSet.Contains(Tuple.Create(record.RefNo, record.Description)))
                            continue; // Skip if already processed

                        var cacheKey = $"PGBTransaction_{input.PayMentId}_{record.RefNo}_{record.Description}";

                        if (!_memoryCache.TryGetValue(cacheKey, out string _var))
                        {
                            var existingRecord = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());

                            if (existingRecord != null)
                            {
                                _memoryCache.Set(cacheKey, "", CacheConstants.DefaultCacheOptions);
                                existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // Store existing record locally
                            }
                        }
                        else
                        {
                            existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // If found in cache, add to local tracking
                        }
                    }
                    // Get only new records using the local HashSet
                    var newRecords = recordsToProcess
                        .Where(r => !existingRecordsSet.Contains(Tuple.Create(r.RefNo, r.Description)))
                        .ToList();

                    NlogLogger.Warn("PGB记录[New]：" + newRecords.ToJsonString());

                    var transactionLists = new ConcurrentBag<PayOrderDepositsMongoEntity>();

                    var insertTasks = new List<Task>();

                    foreach (var record in newRecords)
                    {
                        insertTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await semaphore.WaitAsync(); // Prevent simultaneous inserts
                                var transactionDate = _payMonitorCommonHelpers.ConvertToStandardTime(record.TransactionTime.Trim());
                                var type = "CRDT";

                                if (record.Amount.Contains("+"))
                                {
                                    type = "CRDT";
                                }
                                else if (record.Amount.Contains("-"))
                                {
                                    type = "DBIT";
                                }

                                var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                                var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                {
                                    PayType = PayMentTypeEnum.PGBank.ToInt(),
                                    Type = type,
                                    RefNo = record.RefNo,
                                    Description = record.Description?.Trim(),
                                    UserName = input.Phone,
                                    AccountNo = input.CardNumber,
                                    CreditBank = "",
                                    CreditAcctNo = "",
                                    CreditAcctName = "",
                                    CreditAmount = amount.ParseToDecimal(),
                                    DebitBank = "",
                                    DebitAcctNo = "",
                                    DebitAcctName = "",
                                    DebitAmount = amount.ParseToDecimal(),
                                    AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId((transactionDate ?? DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE"))), "China Standard Time"),
                                    PayMentId = input.PayMentId,
                                };
                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                transactionLists.Add(payOrderDeposit);

                                //加入取款缓存
                                if (payOrderDeposit.Type == "DBIT")
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel()
                                    {
                                        Type = PayMentTypeEnum.PGBank,
                                        RefNo = payOrderDeposit.RefNo,
                                        PayMentId = payOrderDeposit.PayMentId,
                                        TransferTime = payOrderDeposit.TransactionTime,
                                        Remark = payOrderDeposit.Description,
                                        Money = amount.ParseToDecimal(),
                                    };
                                    //_redisService.AddTransferOrderQueueList(bankOrderNotify);
                                }
                            }
                            finally
                            {
                                semaphore.Release(); // Release after execution
                            }
                        }));

                    }
                    await Task.WhenAll(insertTasks); // Wait for all insert tasks to complete

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.PGBank, input.CardNumber, lastInfo.RefNo);
                        }

                        var matchingTasks = new List<Task>(); // List to store running tasks


                        foreach (var item in transactionLists)
                        {
                            matchingTasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await semaphore.WaitAsync(); // Wait for a slot to become available
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.PGBank, item.Description?.Trim(), item.CreditAmount);
                                    if (payEntity != null)
                                    {
                                        //更新订单，同时增加流水表
                                        if (_redisService.SetSuccessOrder(payEntity.ID))
                                        {
                                            var bankOrderPubModel = new BankOrderPubModel()
                                            {
                                                MerchantCode = input.MerchantCode,
                                                Type = PayMentTypeEnum.PGBank,
                                                PayMentId = item.PayMentId,
                                                PayOrderId = payEntity.ID,
                                                Id = item.RefNo,
                                                Money = item.CreditAmount,
                                                Desc = item.Description,
                                            };
                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.PGBankOrder, bankOrderPubModel);

                                        }
                                    }
                                }
                                finally
                                {
                                    semaphore.Release(); // Release the slot
                                }
                            }));
                        }

                        await Task.WhenAll(matchingTasks); // Wait for all tasks to complete
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("PG Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("PG 记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取Viet记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/VietBankRecord")]
        public async Task<JsonResult> VietBankRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("VietB 记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var recordsToProcess = input.BankRecords
                        .GroupBy(r => new { r.RefNo, Description = r.Description?.Trim() }) // Group by unique combination
                        .Select(g => g.First()) // Pick the first record from each group
                        .ToList();

                    var semaphore = new SemaphoreSlim(3); // Limit concurrent inserts
                    var existingRecordsSet = new HashSet<Tuple<string, string>>(); // Track existing records

                    foreach (var record in recordsToProcess)
                    {
                        if (existingRecordsSet.Contains(Tuple.Create(record.RefNo, record.Description)))
                            continue; // Skip if already processed

                        var cacheKey = $"VietBTransaction_{input.PayMentId}_{record.RefNo}_{record.Description}";

                        if (!_memoryCache.TryGetValue(cacheKey, out string _var))
                        {
                            var existingRecord = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());

                            if (existingRecord != null)
                            {
                                _memoryCache.Set(cacheKey, "", CacheConstants.DefaultCacheOptions);
                                existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // Store existing record locally
                            }
                        }
                        else
                        {
                            existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // If found in cache, add to local tracking
                        }
                    }
                    // Get only new records using the local HashSet
                    var newRecords = recordsToProcess
                        .Where(r => !existingRecordsSet.Contains(Tuple.Create(r.RefNo, r.Description)))
                        .ToList();

                    NlogLogger.Warn("VietB记录[New]：" + newRecords.ToJsonString());

                    var transactionLists = new ConcurrentBag<PayOrderDepositsMongoEntity>();

                    var insertTasks = new List<Task>();

                    foreach (var record in newRecords)
                    {
                        insertTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await semaphore.WaitAsync(); // Prevent simultaneous inserts
                                var transactionDate = _payMonitorCommonHelpers.ConvertToStandardTime(record.TransactionTime.Trim());
                                var type = "CRDT";

                                if (record.Amount.Contains("+"))
                                {
                                    type = "CRDT";
                                }
                                else if (record.Amount.Contains("-"))
                                {
                                    type = "DBIT";
                                }

                                var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                                var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                {
                                    PayType = PayMentTypeEnum.VietBank.ToInt(),
                                    Type = type,
                                    RefNo = record.RefNo,
                                    Description = record.Description?.Trim(),
                                    UserName = input.Phone,
                                    AccountNo = input.CardNumber,
                                    CreditBank = "",
                                    CreditAcctNo = "",
                                    CreditAcctName = "",
                                    CreditAmount = amount.ParseToDecimal(),
                                    DebitBank = "",
                                    DebitAcctNo = "",
                                    DebitAcctName = "",
                                    DebitAmount = amount.ParseToDecimal(),
                                    AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId((transactionDate ?? DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE"))), "China Standard Time"),
                                    PayMentId = input.PayMentId,
                                };
                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                transactionLists.Add(payOrderDeposit);

                                //加入取款缓存
                                if (payOrderDeposit.Type == "DBIT")
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel()
                                    {
                                        Type = PayMentTypeEnum.VietBank,
                                        RefNo = payOrderDeposit.RefNo,
                                        PayMentId = payOrderDeposit.PayMentId,
                                        TransferTime = payOrderDeposit.TransactionTime,
                                        Remark = payOrderDeposit.Description,
                                        Money = amount.ParseToDecimal(),
                                    };
                                    //_redisService.AddTransferOrderQueueList(bankOrderNotify);
                                }
                            }
                            finally
                            {
                                semaphore.Release(); // Release after execution
                            }
                        }));

                    }
                    await Task.WhenAll(insertTasks); // Wait for all insert tasks to complete

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.VietBank, input.CardNumber, lastInfo.RefNo);
                        }

                        var matchingTasks = new List<Task>(); // List to store running tasks


                        foreach (var item in transactionLists)
                        {
                            matchingTasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await semaphore.WaitAsync(); // Wait for a slot to become available
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.VietBank, item.Description?.Trim(), item.CreditAmount);
                                    if (payEntity != null)
                                    {
                                        //更新订单，同时增加流水表
                                        if (_redisService.SetSuccessOrder(payEntity.ID))
                                        {
                                            var bankOrderPubModel = new BankOrderPubModel()
                                            {
                                                MerchantCode = input.MerchantCode,
                                                Type = PayMentTypeEnum.VietBank,
                                                PayMentId = item.PayMentId,
                                                PayOrderId = payEntity.ID,
                                                Id = item.RefNo,
                                                Money = item.CreditAmount,
                                                Desc = item.Description,
                                            };
                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.VietBankOrder, bankOrderPubModel);

                                        }
                                    }
                                }
                                finally
                                {
                                    semaphore.Release(); // Release the slot
                                }
                            }));
                        }

                        await Task.WhenAll(matchingTasks); // Wait for all tasks to complete
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Viet Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("Viet 记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }


        /// <summary>
        /// 获取Baca记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/BacaBankRecord")]
        public async Task<JsonResult> BacaBankRecordAsync([FromBody] BankRecordModelInput input)
        {
            NlogLogger.Warn("BacaB 记录：" + input.ToJsonString());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (input.Sign.IsNullOrEmpty() || input.PayMentId == 0 || input.Phone.IsNullOrEmpty() || input.CardNumber.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.BankRecords == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            try
            {
                if (input.BankRecords.Count > 0)
                {
                    var recordsToProcess = input.BankRecords
                        .GroupBy(r => new { r.RefNo, Description = r.Description?.Trim() }) // Group by unique combination
                        .Select(g => g.First()) // Pick the first record from each group
                        .ToList();

                    var semaphore = new SemaphoreSlim(3); // Limit concurrent inserts
                    var existingRecordsSet = new HashSet<Tuple<string, string>>(); // Track existing records

                    foreach (var record in recordsToProcess)
                    {
                        if (existingRecordsSet.Contains(Tuple.Create(record.RefNo, record.Description)))
                            continue; // Skip if already processed

                        var cacheKey = $"BacaBTransaction_{input.PayMentId}_{record.RefNo}_{record.Description}";

                        if (!_memoryCache.TryGetValue(cacheKey, out string _var))
                        {
                            var existingRecord = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(record.RefNo, input.PayMentId, record.Description?.Trim());

                            if (existingRecord != null)
                            {
                                _memoryCache.Set(cacheKey, "", CacheConstants.DefaultCacheOptions);
                                existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // Store existing record locally
                            }
                        }
                        else
                        {
                            existingRecordsSet.Add(Tuple.Create(record.RefNo, record.Description)); // If found in cache, add to local tracking
                        }
                    }
                    // Get only new records using the local HashSet
                    var newRecords = recordsToProcess
                        .Where(r => !existingRecordsSet.Contains(Tuple.Create(r.RefNo, r.Description)))
                        .ToList();

                    NlogLogger.Warn("BacaB记录[New]：" + newRecords.ToJsonString());

                    var transactionLists = new ConcurrentBag<PayOrderDepositsMongoEntity>();

                    var insertTasks = new List<Task>();

                    foreach (var record in newRecords)
                    {
                        insertTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await semaphore.WaitAsync(); // Prevent simultaneous inserts
                                var transactionDate = _payMonitorCommonHelpers.ConvertToStandardTime(record.TransactionTime.Trim());
                                var type = "CRDT";

                                if (record.Amount.Contains("+"))
                                {
                                    type = "CRDT";
                                }
                                else if (record.Amount.Contains("-"))
                                {
                                    type = "DBIT";
                                }

                                var amount = record.Amount.Replace("+", "").Replace("-", "").Replace(",", "").Replace("VND", "").Replace("vnd", "").Replace(" ", "");
                                var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                {
                                    PayType = PayMentTypeEnum.BacaBank.ToInt(),
                                    Type = type,
                                    RefNo = record.RefNo,
                                    Description = record.Description?.Trim(),
                                    UserName = input.Phone,
                                    AccountNo = input.CardNumber,
                                    CreditBank = "",
                                    CreditAcctNo = "",
                                    CreditAcctName = "",
                                    CreditAmount = amount.ParseToDecimal(),
                                    DebitBank = "",
                                    DebitAcctNo = "",
                                    DebitAcctName = "",
                                    DebitAmount = amount.ParseToDecimal(),
                                    AvailableBalance = !record.AvailableBalance.IsNullOrEmpty() ? record.AvailableBalance.ParseToDecimal() : 0,
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId((transactionDate ?? DateTime.Parse(record.TransactionTime, new CultureInfo("de-DE"))), "China Standard Time"),
                                    PayMentId = input.PayMentId,
                                };
                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                transactionLists.Add(payOrderDeposit);

                                //加入取款缓存
                                if (payOrderDeposit.Type == "DBIT")
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel()
                                    {
                                        Type = PayMentTypeEnum.BacaBank,
                                        RefNo = payOrderDeposit.RefNo,
                                        PayMentId = payOrderDeposit.PayMentId,
                                        TransferTime = payOrderDeposit.TransactionTime,
                                        Remark = payOrderDeposit.Description,
                                        Money = amount.ParseToDecimal(),
                                    };
                                    //_redisService.AddTransferOrderQueueList(bankOrderNotify);
                                }
                            }
                            finally
                            {
                                semaphore.Release(); // Release after execution
                            }
                        }));

                    }
                    await Task.WhenAll(insertTasks); // Wait for all insert tasks to complete

                    if (transactionLists.Count > 0)
                    {
                        var lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        if (lastInfo != null)
                        {
                            _bankHelper.SetLastRefNoKey(PayMentTypeEnum.BacaBank, input.CardNumber, lastInfo.RefNo);
                        }

                        var matchingTasks = new List<Task>(); // List to store running tasks


                        foreach (var item in transactionLists)
                        {
                            matchingTasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await semaphore.WaitAsync(); // Wait for a slot to become available
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, PayMentTypeEnum.BacaBank, item.Description?.Trim(), item.CreditAmount);
                                    if (payEntity != null)
                                    {
                                        //更新订单，同时增加流水表
                                        if (_redisService.SetSuccessOrder(payEntity.ID))
                                        {
                                            var bankOrderPubModel = new BankOrderPubModel()
                                            {
                                                MerchantCode = input.MerchantCode,
                                                Type = PayMentTypeEnum.BacaBank,
                                                PayMentId = item.PayMentId,
                                                PayOrderId = payEntity.ID,
                                                Id = item.RefNo,
                                                Money = item.CreditAmount,
                                                Desc = item.Description,
                                            };
                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.BacaBankOrder, bankOrderPubModel);

                                        }
                                    }
                                }
                                finally
                                {
                                    semaphore.Release(); // Release the slot
                                }
                            }));
                        }

                        await Task.WhenAll(matchingTasks); // Wait for all tasks to complete
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Baca Bank记录错误：" + ex);
            }
            finally
            {
                stopwatch.Stop();
                NlogLogger.Warn("Baca 记录：Time Taken " + stopwatch.ElapsedMilliseconds + " ms");
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }
    }
}