using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;
using Neptune.NsPay.BankInfo;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.PayGroups;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.VietQR;
using Neptune.NsPay.WithdrawalDevices;
using NewLife.Caching;
using PayPalCheckoutSdk.Orders;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Twilio.Rest.Trusthub.V1.TrustProducts;
using ZstdSharp.Unsafe;
using static MongoDB.Driver.WriteConcern;
using static Neptune.NsPay.AccountChecker.BankChecker.MBChecker;


namespace Neptune.NsPay.AccountChecker.BankChecker
{
    public interface IBankAccountService
    {
        Task<BankAccountDetail> GetBankAccountDetails(string BankKey,string AccountNumber , string ProcessId);
        void OnStop();
    }


    public class BankAccountService : IBankAccountService
    {

        private readonly IRedisService _redisService;
        private readonly IMemoryCache _memoryCache;
        private readonly  ILogger<BankAccountService> _logger;
        private readonly int _maxAttempt = 2;
        private readonly TimeSpan _lockDuration = new TimeSpan(3000);
        private Task checkerTask;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private CancellationToken _cancellationToken;
        private readonly IBankStateHelper _bankStateHelper;
        private static  List<string> _merchantCodeList = new List<string>();
        private readonly ConcurrentDictionary<string,string> blackListedKey = new ConcurrentDictionary<string, string>();
        private readonly LogHelper _logHelper;
        private static DateTime lastRefreshCache;
        private static readonly object _lockObject = new object();
        private readonly IConfiguration _configuration;
        private readonly List<PayMentTypeEnum> _supportedBankList;
        private readonly string _baseAppPath = string.Empty;

        public static ConcurrentDictionary<BankAccountKey , BankAccountValue> bankChecker = new ConcurrentDictionary<BankAccountKey, BankAccountValue> ();
        public BankAccountService(IRedisService redisService, ILogger<BankAccountService> logger , IMemoryCache memoryCache, IBankStateHelper bankStateHelper , IConfiguration configuration , LogHelper logHelper , IHostEnvironment environment)
        {
            _logger = logger;
            _redisService = redisService;
            _memoryCache = memoryCache;
            _bankStateHelper = bankStateHelper;
            _logHelper = logHelper;

            _supportedBankList = new List<PayMentTypeEnum>();

            var merchantCode = configuration["AccountNameChecker:MerchantCode"];

            if (!string.IsNullOrEmpty(merchantCode))
            {
                _merchantCodeList = merchantCode.Split(',').Select(x=>x.Trim() ).ToList();
            }

            if (configuration["SupportedBank"]!=null )
            {
                _supportedBankList = configuration["SupportedBank"].ToString().Split(',').Where(x=> Enum.TryParse(typeof(PayMentTypeEnum) , x,out var _out)).Select(x=> (PayMentTypeEnum)Enum.Parse(typeof(PayMentTypeEnum) , x)).ToList();
            }
            else
            {
                _supportedBankList =   new List<PayMentTypeEnum>(){  PayMentTypeEnum.MBBank,PayMentTypeEnum.TechcomBank  };;
            }


            _baseAppPath = environment.ContentRootPath;
            _configuration = configuration;


            _cancellationToken = _cancellationTokenSource.Token;
            checkerTask = Task.Factory.StartNew(() => SyncChecker(_cancellationToken),
            TaskCreationOptions.LongRunning);

        }

        private void refreshCache()
        {
            try
            {
                var merchantCode = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.AccountNameCheckerMerchants);

                if (!string.IsNullOrEmpty(merchantCode))
                {
                    _merchantCodeList = merchantCode.Split(',').Select(x => x.Trim()).ToList();
                }

                NlogLogger.Info("Server Cache Merchant List Updated New Value " + merchantCode);
            }
            catch (Exception ex)
            {

                NlogLogger.Error("BankAccountService  - refreshCache Merchant Code", ex);
            }
        }

        private void getLatestCacheValue()
        {
            bool needUpdateCache = false;
            lock (_lockObject)
            {
                if (lastRefreshCache < (DateTime.Now - TimeSpan.FromMilliseconds(60000)))
                {
                    needUpdateCache = true;
                    lastRefreshCache = DateTime.Now;
                }
            }

            if (needUpdateCache)
            {
                refreshCache();
            }
        }

        private bool GetProceedLock(BankAccountKey key , string identifier)
        {

            var cacheLockKey = key.BankKey + "_" + key.AccountNumber;
            Func<ICacheEntry, string> factory = entry =>
            {
                // Optionally, set cache entry properties like expiration
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15);

                // Return the value to cache
                return identifier;
            };

            var inputResult =  _memoryCache.GetOrCreate<string>(cacheLockKey, factory);
            return inputResult == identifier ? true : false ;
        }

        private void ReleaseProceedLock(BankAccountKey key, string identifier)
        {
            var cacheLockKey = key.BankKey + "_" + key.AccountNumber;
             _memoryCache.Remove(cacheLockKey);
        }

        private bool IsAccountProcessLocked(BankAccountKey key)
        {
            var cacheLockKey = key.BankKey + "_" + key.AccountNumber;
            return _memoryCache.TryGetValue(cacheLockKey , out _);
        }

        private bool isAccountLoginBlackListed(string accountNumber , string sessionId)
        {
            var cacheKey = "BlacklistedSession:"+ accountNumber+":"+sessionId;
            return !string.IsNullOrEmpty(_memoryCache.Get<string>(cacheKey));
        }

     
        // Define cache options with expiration and eviction callback
      

        private void blackListAccountSession(string accountNumber, string sessionId)
        {
            var cacheKey = "BlacklistedSession:" + accountNumber + ":" + sessionId;
            blackListedKey.TryAdd(cacheKey , string.Empty);
        

            PostEvictionDelegate evictionCallback = (key, value, reason, state) =>
            {
                blackListedKey.TryRemove(key.ToString(), out var _value);
                _logger.LogInformation($"Item with key '{key}' was evicted. Reason: {reason} , Dictionary Value {_value??string.Empty}");
            };

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),  // Item will expire in 30 minutes
                PostEvictionCallbacks =
            {
                new PostEvictionCallbackRegistration
                {
                    EvictionCallback = evictionCallback
                }
            }
            };


            _memoryCache.Set<string>(cacheKey,"1" , cacheOptions);
        }

        private string convertPaymentTypeToBankKey(PayMentTypeEnum ptType)
        {

            string returnType = string.Empty;

            switch(ptType)
            {
                case PayMentTypeEnum.TechcomBank:
                    returnType = BankKey.TECHCOMBANK;
                    break;
                case PayMentTypeEnum.MBBank:
                    returnType = BankKey.MBBANK;
                    break;
                case PayMentTypeEnum.BidvBank:
                    returnType = BankKey.BIDV;
                    break;
                case PayMentTypeEnum.VietinBank:
                    returnType = BankKey.VIETINBANK;
                    break;
                case PayMentTypeEnum.SeaBank:
                    returnType = BankKey.SEA_BANK;
                    break;
                case PayMentTypeEnum.MsbBank:
                    returnType = BankKey.MSB;
                    break;
            }

            return returnType;

        }

        private IBankChecker createChecker(PayMentTypeEnum ptType , string cardNumber , string loginId)
        {
            if(ptType == PayMentTypeEnum.TechcomBank)
            {
                return new TCBChecker(cardNumber , _redisService , _logHelper);
            }
            else if (ptType == PayMentTypeEnum.MBBank)
            {
                return new MBChecker(cardNumber, loginId, _redisService,_logHelper ,_configuration);
            }
            else if (ptType == PayMentTypeEnum.BidvBank)
            {
                return new BIDVChecker(cardNumber, loginId, _redisService, _logHelper, _configuration, _baseAppPath);
            }
            else if (ptType == PayMentTypeEnum.VietinBank)
            {
                return new VTBChecker(cardNumber, loginId, _redisService, _logHelper, _configuration, _baseAppPath);
            }
            else if (ptType == PayMentTypeEnum.MsbBank)
            {
                return new MSBChecker(cardNumber, _redisService, _logHelper);
            }
            else if (ptType == PayMentTypeEnum.SeaBank)
            {
                return new SEAChecker(cardNumber, _redisService, _logHelper);
            }
            else
            {
                return null;
            }
        }

        private int getBankPriority(PayMentTypeEnum paymentType)
        {
            int priority = 0;

            switch(paymentType)
            {
                case PayMentTypeEnum.TechcomBank:
                    {
                        priority = 1;
                        break;
                    }
                case PayMentTypeEnum.MBBank:
                    {
                        priority = 4;
                        break;
                    }
                case PayMentTypeEnum.BidvBank:
                    {
                        priority = 3;
                        break;
                    }
                case PayMentTypeEnum.VietinBank:
                    {
                        priority = 2;
                        break;
                    }
                case PayMentTypeEnum.SeaBank:
                    {
                        priority = 2;
                        break;
                    }
                case PayMentTypeEnum.MsbBank:
                    {
                        priority = 2;
                        break;
                    }
            }

            return priority;
        }

        private void SyncChecker(CancellationToken token)
        {
            _logger.LogInformation("BankAccountService - Start");
            // var supportedType = new List<PayMentTypeEnum>(){  PayMentTypeEnum.MBBank,PayMentTypeEnum.TechcomBank , PayMentTypeEnum.BidvBank };
            var supportedType = _supportedBankList;

            var payGroupIdList = new List<int>();
            var currentMerchantList = new List<string>();
          

            while (true)
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    getLatestCacheValue();

                    // cache changed , need to sync paygroup again
                    if (currentMerchantList.Count != _merchantCodeList.Count || !currentMerchantList.All(x => _merchantCodeList.Contains(x)))
                    {
                        _logger.LogInformation("Merchant List Changed");
                        currentMerchantList.Clear();
                        currentMerchantList.AddRange(_merchantCodeList);
                        payGroupIdList.Clear();

                        foreach (var merchant in currentMerchantList)
                        {
                            if(String.Compare(merchant, "nspay", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                // 提取外部商户组

                               var externalPayGroupMent =  _redisService.GetPayGroupMentByGroupName(_redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.NsPayGroupName));

                                if(externalPayGroupMent!=null)
                                {
                                    payGroupIdList.Add(externalPayGroupMent.GroupId);
                                }
                            }
                            else
                            {
                                var currentMerchant = _redisService.GetMerchantRedis().FirstOrDefault(x => x.MerchantCode == merchant);

                                if (currentMerchant != null && currentMerchant.PayGroupId > 0 && !payGroupIdList.Contains(currentMerchant.PayGroupId))
                                {
                                    payGroupIdList.Add(currentMerchant.PayGroupId);
                                }
                            }
                        }
                    }

                    var loginAccount = new List<PayMentRedisModel>();

                    if (payGroupIdList.Count>0)
                    {
                        foreach(var payGroup in payGroupIdList)
                        {
                            var groupInfo = _redisService.GetPayGroupMentRedisValue(payGroup.ToString());

                            if(groupInfo!=null)
                            {
                              
                                loginAccount.AddRange(groupInfo.PayMents.GroupJoin(loginAccount , t1=>t1.Id , t2=>t2.Id, (t1, t2) =>  new { t1, t2 } ).Where(x=>!x.t2.Any()).Select(x=>x.t1));
                            }
                        }
                    }
      

                    _logger.LogInformation(DateTime.Now.ToString() + "  -  login Account  - " + System.Text.Json.JsonSerializer.Serialize(loginAccount.Select(x=>new { Id = x.Id })));

                    if(loginAccount.Count>0)
                    {
                        foreach (var payMent in loginAccount.Where(x => supportedType.Contains(x.Type) && x.IsDeleted == false && x.ShowStatus == PayMentStatusEnum.Show))
                        {

                            var bankCheckerKey = new BankAccountKey() { AccountNumber = payMent.CardNumber, BankKey = convertPaymentTypeToBankKey(payMent.Type), PaymentId = payMent.Id, Priority = getBankPriority(payMent.Type) };
                            var loginSession = _bankStateHelper.GetSessionId(payMent.CardNumber, payMent.Type);
                            var isBlackListed = isAccountLoginBlackListed(payMent.CardNumber, loginSession);


                            if (payMent.UseStatus) // Meaning is enable from backend
                            {
                                if (!string.IsNullOrEmpty(loginSession) && !isBlackListed && !bankChecker.ContainsKey(bankCheckerKey))
                                {
                                    bankChecker.TryAdd(bankCheckerKey, new BankAccountValue(createChecker(payMent.Type, payMent.CardNumber, payMent.Phone), TimeSpan.FromMilliseconds(GetBankInterval(payMent.Type))));
                                }
                                else if (!string.IsNullOrEmpty(loginSession) && isBlackListed && bankChecker.ContainsKey(bankCheckerKey))
                                {
                                    bankChecker.TryRemove(bankCheckerKey, out var _checker);
                                }
                                else if (string.IsNullOrEmpty(loginSession) && bankChecker.ContainsKey(bankCheckerKey))
                                {
                                    bankChecker.TryRemove(bankCheckerKey, out var _checker);
                                }
                            }
                            else
                            {
                                // if backend disable , need stop it immediatry 
                                if (bankChecker.ContainsKey(bankCheckerKey))
                                {
                                    bankChecker.TryRemove(bankCheckerKey, out var _checker);
                                    _logger.LogInformation(DateTime.Now.ToString() + "  -  Manual Remove Disabled  Account   - " + System.Text.Json.JsonSerializer.Serialize(bankCheckerKey) + " - " + (_checker != null ? "Success" : "Failed"));
                                }
                            }
                        }
                    }
                    else
                    {
                        // remove all checker since no account can use 

                        foreach (var checkerKey in bankChecker.Keys)
                        {
                            bankChecker.TryRemove(checkerKey, out var _checker);
                        }
                        _logger.LogInformation("No Merchant Can Be Used , Clean Up All Checker");
                    }
                  
                }
                catch(Exception ex)
                {
                    _logger.LogError("SyncChecker - " + ex.Message);
                }
                _logger.LogInformation(DateTime.Now.ToString()+"  -  Existing Bank  - "+ System.Text.Json.JsonSerializer.Serialize(bankChecker.Keys));
                _logger.LogInformation(DateTime.Now.ToString() + "  -  Blacklisted Account  - " + System.Text.Json.JsonSerializer.Serialize(blackListedKey.Keys));

                Thread.Sleep(15000);
            }

            _logger.LogInformation("BankAccountService - End");
        }

        private long GetBankInterval(PayMentTypeEnum type)
        {
            long intervalMilliseconds = 0;
            switch(type)
            {
                case PayMentTypeEnum.TechcomBank:
                    {
                        intervalMilliseconds = 3000;
                        break;
                    }
                case PayMentTypeEnum.MBBank:
                    {
                        intervalMilliseconds = 5000;
                        break;
                    }
                case PayMentTypeEnum.BidvBank:
                    {
                        intervalMilliseconds = 5000;
                        break;
                    }
                case PayMentTypeEnum.VietinBank:
                    {
                        intervalMilliseconds = 5000;
                        break;
                    }
                case PayMentTypeEnum.MsbBank:
                    {
                        intervalMilliseconds = 5000;
                        break;
                    }
                case PayMentTypeEnum.SeaBank:
                    {
                        intervalMilliseconds = 5000;
                        break;
                    }
            }


            return intervalMilliseconds;
        }

        public void OnStop()
        {
            _cancellationTokenSource.Cancel();
            Task.WaitAll(checkerTask);
        }

        private  KeyValuePair< BankAccountKey, BankAccountValue>? GetNextAccount(int attempt)
        {
            // if attempt = 0 mean first time , if second time need pick MB as priority , first attempt priority is TCB
            KeyValuePair<BankAccountKey, BankAccountValue> returnKey = new KeyValuePair<BankAccountKey, BankAccountValue>();
            var cDate = DateTime.Now;

            if(bankChecker.Count == 0)
            {
                return null;
            }

            var searchQuery = bankChecker.Where(x =>( !x.Value.NextAllowed.HasValue || x.Value.NextAllowed.Value < cDate) &&(!x.Value.IntervalMilisecond.HasValue || !x.Value.LastUsed.HasValue || (x.Value.LastUsed.Value.Add(x.Value.IntervalMilisecond.Value)<cDate) ) && !IsAccountProcessLocked(x.Key));

            switch (attempt)
            {
                case 0:
                    {

                        returnKey = searchQuery.OrderBy(x => x.Key.Priority).ThenBy(x=>x.Value.LastUsed??DateTime.Now ).FirstOrDefault();
                        break;
                    }
                case 1:
                    {

                        returnKey = searchQuery.OrderByDescending(x => x.Key.Priority).ThenBy(x => x.Value.LastUsed ?? DateTime.Now).FirstOrDefault();
                        break;
                    }
            }

            return returnKey;

        }

        private T Clone<T>(T obj)
        {

            return System.Text.Json.JsonSerializer.Deserialize<T>(System.Text.Json.JsonSerializer.Serialize(obj));
        }


        public async Task<BankAccountDetail> GetBankAccountDetails(string BankKey, string AccountNumber , string ProcessId)
        {
            var returnModel = new BankAccountDetail() { BankName = BankKey , AccountNumber = AccountNumber };

            if (BankApp.BanksObject.ContainsKey(BankKey))
            {
                var binNumber = BankApp.BanksObject[BankKey].bin;
                var attemptCount = 0;
                string holderName = null;
                string returnMessage = null;
                bool haveBankProcess = false; // indicate have receive valid response from bank api
                bool isDone = false;

                while (attemptCount < _maxAttempt&& !isDone)
                {
                    if (attemptCount > 0)
                        await Task.Delay(1000);

                    var logPrefix = ("[" + ProcessId + "]-GetBankAccountDetails Attempt (" + attemptCount + ") ");

                    try
                    {
                        var matchedRecord = GetNextAccount(attemptCount);
                        _logger.LogInformation(logPrefix + " - Login Account (" + System.Text.Json.JsonSerializer.Serialize(matchedRecord) + ") ");

                        if(!matchedRecord.HasValue)
                        {
                            _logger.LogInformation(logPrefix + " - No Bank Checker");
                            isDone = true;
                        }
                        else if (matchedRecord.HasValue&&matchedRecord.Value.Key != null && GetProceedLock(matchedRecord.Value.Key, ProcessId))
                        {
                            var accountKey = Clone(matchedRecord.Value.Key);
                            haveBankProcess = true;
                            matchedRecord.Value.Value.LastUsed = DateTime.Now;
                            var result = await matchedRecord.Value.Value.CheckerObj.GetHolderName(AccountNumber, binNumber, matchedRecord.Value.Key.BankKey == BankKey);
                            returnModel.PaymentId = matchedRecord.Value.Key.PaymentId;
                            returnModel.BankKey = matchedRecord.Value.Key.BankKey;
                            _logger.LogInformation(logPrefix + "- Login Account Result (" + System.Text.Json.JsonSerializer.Serialize(result) + ") ");

                            if (result.IsSuccess)
                            {
                                holderName = result.HolderName;
                                returnModel.Success = true;
                                isDone = true;
                            }
                            else if (result.ApiError == enAPIResponseError.AccountInvalid)
                            {
                                returnMessage = "Account Invalid";
                                isDone = true;
                            }
                            else if (result.ApiError == enAPIResponseError.ApiCoolDown)
                            {
                                // MSB will need coolDown 15 second if hitting rate limit
                                matchedRecord.Value.Value.NextAllowed = DateTime.Now.AddMilliseconds(25000);
                                haveBankProcess = false; // Categories as Invalid Call
                            }
                            else if (result.ApiError == enAPIResponseError.SessionExpired)
                            {
                                blackListAccountSession(matchedRecord.Value.Key.AccountNumber, (result.SessionId ?? "session_empty"));
                                bankChecker.TryRemove(matchedRecord.Value.Key, out var _Checker);
                                _logger.LogInformation(logPrefix + " GetBankAccountDetails - Remove Checker " + System.Text.Json.JsonSerializer.Serialize(accountKey) + " - " + (_Checker != null ? "Success" : "Failed "));
                                haveBankProcess = false;// Categories as Invalid Call
                            }
                            else if (result.ApiError == enAPIResponseError.ApiTimeOut)
                            {
                                matchedRecord.Value.Value.NextAllowed = DateTime.Now.AddMilliseconds(5000); // cooldown 5 second when time out 
                                _logger.LogInformation(logPrefix + " GetBankAccountDetails - Request Timeout");
                                returnMessage = "Request Timeout ";
                                isDone = true;
                            }

                            ReleaseProceedLock(accountKey, ProcessId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(logPrefix + "GetBankAccountDetails - " + ex.Message);
                    }finally
                    {
                        attemptCount++;
                    }
                }

                if(haveBankProcess)
                {
                    returnModel.Success = true; // Indicate Ever Ever to get valid response from Bank API
                    returnModel.HolderName = holderName;
                    returnModel.ErrorMessage = (!string.IsNullOrEmpty(returnMessage)? returnMessage : "Success" ) ;

                }
                else
                {
                    returnModel.ErrorMessage = "No Inquiry Bank Avaliable";
                    returnModel.Success = null; // meaning no call api and can direct skip 
                }

            }
            else
            {
                returnModel.ErrorMessage = "Invalid Bank Key";
            }

            return returnModel;
        }

    }

    public class BankAccountValue
    {
        public DateTime? LastUsed { get; set; }

        public TimeSpan? IntervalMilisecond { get; set; }

        public DateTime? NextAllowed { get; set; } // mbBank Need cooldown if getting too many transaction 

        public IBankChecker CheckerObj { get; set; }

        public BankAccountValue(IBankChecker checkerObj , TimeSpan? intervalMilisecond)
        {
            CheckerObj = checkerObj;
            IntervalMilisecond = intervalMilisecond;
        }
    }

    public class BankAccountKey
    {
        public string BankKey { get; set; }

        public string AccountNumber { get; set; }

        public int PaymentId { get; set; } // From Payment Method use to keep track 

        public int Priority { get; set; }
    
        public override bool Equals(object obj)
        {
            // Check if the object is the same instance
            if (ReferenceEquals(this, obj)) return true;

            // Check if the object is of the same type
            if (obj == null || GetType() != obj.GetType()) return false;

            var other = (BankAccountKey)obj;

            // Compare properties for equality
            return BankKey == other.BankKey && AccountNumber == other.AccountNumber;
        }

        // Override GetHashCode method
        public override int GetHashCode()
        {
            return BankKey?.GetHashCode() ^ AccountNumber?.GetHashCode() ?? 0;
        }
    }



    public class BankAccountDetail
    {
        public string BankName { get; set; }
        public string AccountNumber { get; set; }

        public string HolderName { get; set; }

        public int? PaymentId { get; set; } // account which use to get holder name 

        public string? BankKey { get; set; } //BankKey

        public bool? Success { get; set; } // true if call api success 

        public string ErrorMessage { get; set; }    // message on failed reason

    }


}
