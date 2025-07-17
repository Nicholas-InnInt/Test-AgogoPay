using Abp.Collections.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Neptune.NsPay.AccountCheckerClientExtension;
using Neptune.NsPay.AccountNameChecker.Dto;
using Neptune.NsPay.BankInfo;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.ELKLogExtension;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Utils;
using Neptune.NsPay.VietQR;
using Neptune.NsPay.Web.TransferApi.Models;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.WithdrawalOrders;
using NewLife.Caching;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X9;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;


namespace Neptune.NsPay.Web.TransferApi.SignalR
{
    public class OrderUpdateQueueItem
    {
        public string OrderId { get; set; }

        public string OrderNumber { get; set; }

        public int DeviceId { get; set; }

        public bool IsUpdate { get; set; }
        public WithdrawalOrderStatusEnum OrderStatus { get; set; }

        public DateTime CreatedDate { get; set; }

    }
    public class PushOrderWorkerService: BackgroundService
    {
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly PushOrderService _pushOrderService;
        public static bool isStop = false;
        private Task dbSyncTask;
        public PushOrderWorkerService(PushOrderService pushOrderService)
        {
            _pushOrderService = pushOrderService;
            dbSyncTask = Task.Run(async () => await _pushOrderService.ConsumeDeviceStatusRequestQueue(cancellationTokenSource.Token));
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("PushOrderWorkerService is starting...");
            return base.StartAsync(cancellationToken); // must call base
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            long cycleCounter = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _pushOrderService.RunJob(cycleCounter);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    cycleCounter++;
                }

                await Task.Delay(1500, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Background service is stopping.");
            isStop = true;
            cancellationTokenSource.Cancel();
            await base.StopAsync(cancellationToken);  // Ensure base cleanup is called
            Console.WriteLine("Background service has stopped.");
        }
    }

    public class WithdrawalQueueCacheItem
    {
        public string OrderId { get; set; }
        public int MerchantId { get; set; }
        public string MerchantCode { get; set; }
        public string OrderNo { get; set; }
        public decimal Amount { get; set; }
        public string RecipientAccountNumber { get; set; }
        public string RecipientAccountHolderName { get; set; }
        public string RecipientAccountBankName { get; set; }
        public int DeviceId { get; set; }
        public bool IsAccountNameVerified { get; set; } // push Date in TimeStamp
        public DateTime OrderCreationDate { get; set; }
        public string QRImageBase64 { get; set; }
        public string QRContent { get; set; } // push Date in TimeStamp
        public long timeStamp { get; set; } // push Date in TimeStamp

    }


    public class PushOrderService 
    {
        private readonly IHubContext<OrderHub, IOrderHubClient> _hub;
        private static readonly ConcurrentDictionary<int , Tuple<string, DateTime>> deviceOrderDict = new ConcurrentDictionary<int, Tuple<string, DateTime>>();
        private static readonly ConcurrentDictionary<int, DateTime?> deviceList = new ConcurrentDictionary<int,DateTime?>();
        private static readonly ConcurrentDictionary<string, DateTime> currentProcessOrder = new ConcurrentDictionary<string, DateTime>();
        private static  ConcurrentDictionary<int , string > activeDevices = new ConcurrentDictionary<int, string>();
        private static ConcurrentDictionary<int, WithdrawalDevicesBankTypeEnum> deviceTypeDict = new ConcurrentDictionary<int, WithdrawalDevicesBankTypeEnum>();
        private static readonly ConcurrentDictionary<int, DateTime> balanceRequestList = new ConcurrentDictionary<int, DateTime>();
        private static readonly ConcurrentDictionary<int, DateTime> deviceLastSendBalanceTime = new ConcurrentDictionary<int, DateTime>();
        private static readonly ConcurrentDictionary<int, DateTime> deviceLastUpdateBalanceTime = new ConcurrentDictionary<int, DateTime>();
        private static readonly ConcurrentDictionary<int, DateTime> deviceLastCompletedOrderTime = new ConcurrentDictionary<int, DateTime>();
        private static readonly ConcurrentDictionary<Tuple<int , WithdrawalQueueCacheItem>, DateTime> recoverOrderList = new ConcurrentDictionary<Tuple<int, WithdrawalQueueCacheItem>, DateTime>();

        public static IMemoryCache _memoryCache;
        private readonly static double deviceCheckIntervalMsec = 90000;
        private readonly IRedisService _redisService;
        private readonly IMerchantService _merchantService;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersService;
        private readonly IWithdrawalDevicesService _withdrawalDevicesService;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;
        private readonly IMerchantBillsMongoService _merchantBillsMongoService;
        private readonly AccountCheckerService _accountCheckerService;
        private static bool _openCheckAccountName;
        private static DateTime lastRefreshCache;
        private static readonly object _lockObject = new object();
        private readonly DeviceProcessLock _deviceProcessLock;
        private readonly ITransferCallBackService _transferCallBackService;
        private static string CurrentProcessId;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly LogOrderService _logOrderService;
        private readonly WithdrawalOrderHelper _withdrawalOrderHelper;
        private readonly IMapper _objectMapper;

        // when order completed and avaliable to send order again , will become null , else 
        public PushOrderService(IHubContext<OrderHub, IOrderHubClient> hub, IMemoryCache memoryCache , IRedisService redisService
            , IMerchantService merchantService, IWithdrawalOrdersMongoService withdrawalOrdersService , IWithdrawalDevicesService withdrawalDevicesService
            , IMerchantFundsMongoService merchantFundsMongoService , AccountCheckerService accountCheckerService , IConfiguration configuration
           , DeviceProcessLock deviceProcessLock,   IMerchantBillsMongoService merchantBillsMongoService , ITransferCallBackService transferCallBackService
            , IKafkaProducer kafkaProducer , LogOrderService logOrderService, WithdrawalOrderHelper withdrawalOrderHelper)
        {
            _hub = hub;
            _memoryCache = memoryCache;
            _redisService = redisService;
            _merchantService = merchantService;
            _withdrawalOrdersService = withdrawalOrdersService;
            _withdrawalDevicesService = withdrawalDevicesService;
            _merchantFundsMongoService = merchantFundsMongoService;
            _accountCheckerService = accountCheckerService;
            _deviceProcessLock = deviceProcessLock;
            _merchantBillsMongoService = merchantBillsMongoService;
            _transferCallBackService = transferCallBackService;
            _kafkaProducer = kafkaProducer;
            _logOrderService = logOrderService;
            _withdrawalOrderHelper = withdrawalOrderHelper;

            var enabledCheck = configuration["AccountNameChecker:Enabled"];

            if ( !string.IsNullOrEmpty(enabledCheck) && Boolean.TryParse( enabledCheck , out var _enable))
            {
                _openCheckAccountName = _enable;
            }

            var config = new MapperConfiguration(cfg => {

                cfg.CreateMap<WithdrawalQueueCacheItem, SignalROrderInfo>();
            });

            // Step 2: Create the Mapper instance using the config
            _objectMapper = config.CreateMapper();

            NlogLogger.Warn("PushOrderService - contructor call");
        }


        private void refreshCache()
        {
            try
            {
                var accountOnOff = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.AccountNameCheckerOnOff);

                if (!string.IsNullOrEmpty(accountOnOff) && Boolean.TryParse(accountOnOff , out bool _onOff))
                {
                    _openCheckAccountName = _onOff;
                }

                NlogLogger.Info("Server Cache On Off Updated New Value " + accountOnOff);
            }
            catch (Exception ex)
            {

                NlogLogger.Error("PushOrderWorker  - refreshCache On Off", ex);
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

        public async Task<bool> saveDisconnectedDeviceOrder(int deviceId)
        {
            var currentDeviceOrderKey = GetDeviceExpirationOrderLock(deviceId);
            var resendOrder = _memoryCache.Get(currentDeviceOrderKey);
            bool haveOrder = false;

            if (resendOrder != null)
            {
                var signalRContent = (WithdrawalQueueCacheItem)resendOrder;
                if (!currentProcessOrder.ContainsKey(signalRContent.OrderId))
                {
                    recoverOrderList.TryAdd(Tuple.Create(deviceId, signalRContent), DateTime.Now.AddMilliseconds(5000));
                    _memoryCache.Remove(currentDeviceOrderKey);
                    NlogLogger.Info("SaveDisconnectedDeviceOrder - Removed Order From Cache (" + signalRContent.OrderId + ") Device - " + deviceId);
                }
            }

            return haveOrder;
        }

        public async Task<bool> ForceCompleteDeviceOrder(int deviceId , string orderId)
        {
            var currentDeviceOrderKey = GetDeviceExpirationOrderLock(deviceId);
            var resendOrder = _memoryCache.Get(currentDeviceOrderKey);
            bool haveOrder = false;
            bool orderInProgress = false;

            if (resendOrder != null)
            {
                var orderInfo = (WithdrawalQueueCacheItem)resendOrder;

                if(orderInfo.OrderId == orderId)
                {
                    haveOrder = true;
                    _memoryCache.Remove(currentDeviceOrderKey);
                }
            }

            if (currentProcessOrder.ContainsKey(orderId))
            {
                orderInProgress = true;
                currentProcessOrder.Remove(orderId);
            }

            deviceLastCompletedOrderTime.AddOrUpdate(deviceId, DateTime.Now, (key, oldValue) => DateTime.Now);
            _memoryCache.Remove(GetDeviceWaitingBalanceUpdateLock(deviceId)); // release balance request waiting lock of device  after order completed 
            balanceRequestList.Remove(deviceId);// clean previous balance request 
            deviceList[deviceId] = null; // free update the device order

            NlogLogger.Info("PushOrderWorker - ForceCompleteDeviceOrder " + deviceId.ToString() + " Order ID - " + orderId.ToString()+" Is Removed From Cache ("+ haveOrder + ") Is Removed From In Progress Order ("+ orderInProgress + ") ");

            return haveOrder|| orderInProgress;
        }

        public async Task<string> ConvertImageToBase64(string imageUrl)
        {
            string base64String = string.Empty;

            try
            {
                NlogLogger.Info("Getting QR URL - " + imageUrl);
                using (HttpClient client = new HttpClient())
                {
                    // Download the image as a byte array
                    byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);

                    // Convert the byte array to a Base64 string
                    base64String =  Convert.ToBase64String(imageBytes);
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("ConvertImageToBase64 error - " + ex.Message);
            }

            return base64String;
        }

        private WithdrawalQueueCacheItem convertToSignalrModel(WithdrawalOrderRedisModel redisModel , int deviceId)
        {
            var returnModel = new WithdrawalQueueCacheItem()
            {
                OrderId = redisModel?.Id,
                OrderNo = redisModel?.OrderNo,
                Amount = redisModel?.OrderMoney??0,
                DeviceId = deviceId,
                RecipientAccountNumber = redisModel?.BenAccountNo,
                RecipientAccountHolderName = redisModel?.BenAccountName,
                RecipientAccountBankName = redisModel?.BenBankName,
                MerchantId = redisModel?.MerchantId??0,
                MerchantCode = redisModel?.MerchantCode,
                IsAccountNameVerified = (redisModel?.IsAccountNameVerified??false)
            };

            return returnModel;
        }


        private string ConvertToAlphaNumberic(string rawString)
        {
            string pattern = @"[^a-zA-Z0-9]";
            return Regex.Replace(rawString, pattern, "");
        }

        private void setDeviceNewOrder(int deviceId  , WithdrawalQueueCacheItem cacheOrder)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(deviceCheckIntervalMsec)
            };

            options.RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                try
                {
                    NlogLogger.Info("Device Order Cache - " + reason.ToString() + " Key " + key.ToString());
                    var valueObj = (WithdrawalQueueCacheItem)value;
                    if (reason == EvictionReason.Replaced)
                    {
                        recoverOrderList.TryAdd(Tuple.Create(valueObj.DeviceId, valueObj), DateTime.Now.AddMilliseconds(5000));
                        NlogLogger.Info("Device Order Replaced  - Device Id : " + key + " Order Id - " + valueObj.OrderId + " Order Number - " + valueObj.OrderNo);
                    }
                    else if (reason == EvictionReason.Expired)
                    {
                        recoverOrderList.TryAdd(Tuple.Create(valueObj.DeviceId, valueObj), DateTime.Now.AddMilliseconds(5000));
                        NlogLogger.Info("Device Order Expired  - Device Id : " + key + " Order Id - " + valueObj.OrderId + " Order Number - " + valueObj.OrderNo);
                    }

                }
                catch (Exception ex)
                {
                    NlogLogger.Error("DeviceOrderCache", ex);
                }
            });

            var orderKey = GetDeviceExpirationOrderLock(deviceId);
            _memoryCache.Set(orderKey, cacheOrder, options);
        }

        public  async Task<string> ConvertImageToBase64New(WithdrawalOrdersMongoEntity orderInfo)
        {
            string base64String = string.Empty;

            try
            {
               
                NlogLogger.Info("Generating QR  - " + orderInfo.ID);
                var bankKeyStr = WithdrawalOrderBankMapper.FindBankByName(orderInfo.BenBankName);
                var vietNameInEnglish = UtilsHelper.VietnameseToEnglish(orderInfo.BenAccountName);
                var qrPay = QRPay.InitVietQR(
                                  bankBin: BankApp.BanksObject[bankKeyStr].bin,
                                  bankNumber: orderInfo.BenAccountNo,
                                  orderInfo.OrderMoney.ToString("F0"),
                                  ConvertToAlphaNumberic(orderInfo.OrderNumber)
                                );
                var qRBankInfo = qrPay.Build();

                if(deviceTypeDict.TryGetValue(orderInfo.DeviceId ,out var  _type) && (_type == WithdrawalDevicesBankTypeEnum.BIDV))
                {
                    base64String = QrCodeHelper.GetQrCodeAsBase64New(qRBankInfo);
                }
                //else if(_type == WithdrawalDevicesBankTypeEnum.BVB)
                //{
                //    base64String = QrCodeHelper.GetQrCodeAsBase64Custom(qRBankInfo , 20);
                //}
                else
                {
                base64String = QrCodeHelper.GetQrCodeAsBase64(qRBankInfo);
                }

            }
            catch (Exception ex)
            {
                NlogLogger.Error("Generating QR  error - " + ex.Message);
            }

            return base64String;
        }

        public async Task<string> GetQRContent(WithdrawalOrdersMongoEntity orderInfo)
        {
            string qrContent = string.Empty;

            try
            {

                var bankKeyStr = WithdrawalOrderBankMapper.FindBankByName(orderInfo.BenBankName);
                var vietNameInEnglish = UtilsHelper.VietnameseToEnglish(orderInfo.BenAccountName);
                var qrPay = QRPay.InitVietQR(
                                  bankBin: BankApp.BanksObject[bankKeyStr].bin,
                                  bankNumber: orderInfo.BenAccountNo,
                                  orderInfo.OrderMoney.ToString("F0"),
                                  ConvertToAlphaNumberic(orderInfo.OrderNumber)
                                );
                qrContent = qrPay.Build();

            }
            catch (Exception ex)
            {
                NlogLogger.Error("Get QR Content error - " + ex.Message);
            }

            return qrContent;
        }


        #region using signalrModel v2


        public async Task<string> ConvertImageToBase64NewV2 (WithdrawalQueueCacheItem orderInfo)
        {
            string base64String = string.Empty;

            try
            {

                NlogLogger.Info("Generating QR  - " + orderInfo.OrderId);
                var bankKeyStr = WithdrawalOrderBankMapper.FindBankByName(orderInfo.RecipientAccountBankName);
                var vietNameInEnglish = UtilsHelper.VietnameseToEnglish(orderInfo.RecipientAccountHolderName);
                var qrPay = QRPay.InitVietQR(
                                  bankBin: BankApp.BanksObject[bankKeyStr].bin,
                                  bankNumber: orderInfo.RecipientAccountNumber,
                                  orderInfo.Amount.ToString("F0"),
                                  ConvertToAlphaNumberic(orderInfo.OrderNo)
                                );
                var qRBankInfo = qrPay.Build();

                if (deviceTypeDict.TryGetValue(orderInfo.DeviceId, out var _type) && (_type == WithdrawalDevicesBankTypeEnum.BIDV))   
                {
                    base64String = QrCodeHelper.GetQrCodeAsBase64New(qRBankInfo);
                }
                else
                {
                    base64String = QrCodeHelper.GetQrCodeAsBase64(qRBankInfo);
                }

            }
            catch (Exception ex)
            {
                NlogLogger.Error("Generating QR  error - " + ex.Message);
            }

            return base64String;
        }

        public async Task<string> GetQRContentV2(WithdrawalQueueCacheItem orderInfo)
        {
            string qrContent = string.Empty;

            try
            {

                var bankKeyStr = WithdrawalOrderBankMapper.FindBankByName(orderInfo.RecipientAccountBankName);
                var vietNameInEnglish = UtilsHelper.VietnameseToEnglish(orderInfo.RecipientAccountHolderName);
                var qrPay = QRPay.InitVietQR(
                                  bankBin: BankApp.BanksObject[bankKeyStr].bin,
                                  bankNumber: orderInfo.RecipientAccountNumber,
                                  orderInfo.Amount.ToString("F0"),
                                  ConvertToAlphaNumberic(orderInfo.OrderNo)
                                );
                qrContent = qrPay.Build();

            }
            catch (Exception ex)
            {
                NlogLogger.Error("Get QR Content error - " + ex.Message);
            }

            return qrContent;
        }

        #endregion


        public async Task ConsumeDeviceStatusRequestQueue(CancellationToken cancellationToken)
        {

            NlogLogger.Info("ConsumeDeviceStatusRequestQueue Started ");
            try
            {
                var counter = 0;
                // Consume items indefinitely(until cancellation or completion)
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        getLatestCacheValue();
                        if (counter % 5 == 0)
                        {
                            // sync active device

                            var allActiveDevice = _withdrawalDevicesService.GetWhere(x => !x.IsDeleted && x.Status && x.Process == 0);

                            if (allActiveDevice != null)
                            {
                                var newDictionary = new ConcurrentDictionary<int, string>(allActiveDevice.Select(x => x.Id).Distinct().ToDictionary(x => x, x => string.Empty));
                                Interlocked.Exchange(ref activeDevices, newDictionary);
                            }

                            NlogLogger.Debug("ConsumeDeviceStatusRequestQueue - Active Device List (" + JsonConvert.SerializeObject(activeDevices) + ")");
                        }

                        // house keep in progress order if expired and still in Pending Order Status then update it to 
                        foreach (var expiredOrder in currentProcessOrder.Where(x => x.Value < DateTime.Now.AddSeconds(-120)))
                        {
                            var withdrawalOrder = await _withdrawalOrdersService.GetById(expiredOrder.Key);
                            var processSuccess = true; // if is pending and update fail , then no need remove the order from Queue 
                            if (withdrawalOrder != null && withdrawalOrder.OrderStatus == WithdrawalOrderStatusEnum.Pending)
                            {
                                try
                                {
                                    withdrawalOrder.OrderStatus = WithdrawalOrderStatusEnum.PendingProcess;
                                    withdrawalOrder.TransactionTime = DateTime.Now;
                                    withdrawalOrder.TransactionUnixTime = TimeHelper.GetUnixTimeStamp(withdrawalOrder.TransactionTime);
                                    withdrawalOrder.Remark = "设备处理超时";
                                    await _withdrawalOrdersService.UpdateAsync(withdrawalOrder);
                                    NlogLogger.Debug("ConsumeDeviceStatusRequestQueue - Order Expired (" + withdrawalOrder.ID + " | " + withdrawalOrder.OrderNumber + ")");
                                }
                                catch (Exception ex)
                                {
                                    processSuccess = false;
                                    NlogLogger.Error("Update expired Pending Process Withdrawal Order - " + ex.Message);
                                }
                            }

                            if (processSuccess)
                            {
                                currentProcessOrder.TryRemove(expiredOrder);
                            }

                        }

                        // when sendBalance will set after 15 sec from DateTime.Now
                        foreach (var expiredUpdateBalance in balanceRequestList.Where(x => x.Value < DateTime.Now))
                        {
                            balanceRequestList.TryRemove(expiredUpdateBalance);
                        }

                    }
                    catch(Exception ex)
                    {
                        NlogLogger.Error("ConsumeDeviceStatusRequestQueue Execute Error " , ex);
                    }

                    counter++;
                    await Task.Delay(5000, cancellationToken);
                }
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("Consumer task has been canceled.");
                NlogLogger.Error("ConsumeDeviceStatusRequestQueue Error " , ex);
            }
            finally
            {
                Console.WriteLine("Consumer has completed.");
            }

            NlogLogger.Info("ConsumeDeviceStatusRequestQueue Ended ");
        }

        private string GetDeviceWaitingOrderLock(int deviceId )
        {
            return string.Format("WaitingOrderUpdate:{0}", deviceId.ToString());
        }

        private  string GetDeviceWaitingBalanceUpdateLock(int deviceId)
        {
            return string.Format("WaitingBalanceUpdate:{0}", deviceId.ToString());
        }

        private string GetDeviceRetryBalanceUpdateLock(int deviceId)
        {
            return string.Format("RetryBalanceUpdate:{0}", deviceId.ToString());
        }

        private string GetDeviceExpirationOrderLock(int deviceId)
        {
            return string.Format("WaitingOrderExpiration:{0}", deviceId.ToString());
        }

        private bool GetStatusUpdateLock(int deviceId , string processId)
        {
            var cacheKey = string.Format("WaitingStatusUpdate:{0}", deviceId.ToString());

            Func<ICacheEntry, string> factory = entry =>
            {
                // Optionally, set cache entry properties like expiration
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15);

                // Return the value to cache
                return processId;
            };


            var getProcessLock = _memoryCache.GetOrCreate<string>(cacheKey, factory); // make sure device balance  send in 30 second interval 

            return getProcessLock == processId;
        }

        public async Task RunJob(long cycleCounter)
        {

            var processId = Guid.NewGuid().ToString();
            CurrentProcessId = processId;
            var logPrefix = "[" + processId + "] - ";

            syncConnectedDevice();

            // HouseKeep Expired Balance
            foreach (var expiredUpdateBalance in balanceRequestList.Where(x => x.Value < DateTime.Now))
            {
                balanceRequestList.TryRemove(expiredUpdateBalance);
            }


            var validDeviceList = deviceList.Where(x => activeDevices.ContainsKey(x.Key)).ToDictionary();
            var processedDevice = new List<int>();

            // cool down device also need prevent to send order or sending Balance ; 
            processedDevice.AddRange(validDeviceList.Where(x => (deviceLastCompletedOrderTime.TryGetValue(x.Key, out var lastOrderCompletedTime) | true) && (lastOrderCompletedTime.AddSeconds(3) > DateTime.Now)).Select(x => x.Key));

            if (cycleCounter % 2 == 0)
            {
                // exclude locked device since need to send balance request first 
                var avaliableDevice = validDeviceList.Where(x => !x.Value.HasValue && (!processedDevice.Contains(x.Key))).ToDictionary(); //  && _deviceProcessLock.IsDeviceCoolDownOrderProcess(x.Key)

                if (avaliableDevice.Count > 0) // Have Avaliable Device Only Check Withdrawal
                {
                    var deviceOrderDict = await GetPendingWithdrawalList(avaliableDevice.Keys.ToList(), processId);

                    var timeStampUnix = TimeHelper.GetUnixTimeStamp(DateTime.Now);
                    NlogLogger.Info(logPrefix + "PushOrderWorker - New Device Order " + JsonConvert.SerializeObject(deviceOrderDict));

                    foreach (var deviceOrder in deviceOrderDict)
                    {
                        var cOrder = deviceOrder.Value.Item1;
                        var bankKeyStr = WithdrawalOrderBankMapper.FindBankByName(cOrder.RecipientAccountBankName);
                        var vietQRbase64 = string.Empty;
                        var vietNameInEnglish = UtilsHelper.VietnameseToEnglish(cOrder.RecipientAccountHolderName).ToUpper();

                        if (!bankKeyStr.IsNullOrEmpty())
                        {
                            var vietQRUrl = "https://img.vietqr.io/image/" + BankApp.BanksObject[bankKeyStr].bin + "-" + cOrder.RecipientAccountNumber + "-qr_only.png?amount=" + cOrder.Amount.ToString("F0") + "&accountName=" + HttpUtility.UrlEncode(vietNameInEnglish) + "&addInfo=" + ConvertToAlphaNumberic(cOrder.OrderNo);

                            if (deviceTypeDict.TryGetValue(cOrder.DeviceId, out var _type) && (_type == WithdrawalDevicesBankTypeEnum.BIDV))
                            {
                                // bidv get from vietqr only go for self generated
                                vietQRbase64 = await ConvertImageToBase64(vietQRUrl);

                                if (vietQRbase64 == string.Empty)
                                {
                                    //vietQRbase64 = await ConvertImageToBase64New(cOrder);
                                    vietQRbase64 = await ConvertImageToBase64NewV2(cOrder);
                                }

                            }
                            else
                            {
                                //vietQRbase64 = await ConvertImageToBase64New(cOrder);
                                vietQRbase64 = await ConvertImageToBase64NewV2(cOrder);

                                // if Call failed then self generated
                                if (vietQRbase64 == string.Empty)
                                {
                                    //vietQRbase64 = await ConvertImageToBase64(vietQRUrl);
                                }
                            }


                        }
                        else
                        {
                            NlogLogger.Error(logPrefix + "PushOrderWorker - BankKey Not Found " + cOrder.RecipientAccountBankName);
                        }

                        cOrder.RecipientAccountHolderName = vietNameInEnglish;
                        cOrder.timeStamp = timeStampUnix;
                        cOrder.QRImageBase64 = vietQRbase64;
                        cOrder.QRContent = await GetQRContentV2(cOrder);


                        _redisService.AddWithdrawalOrderProcessLock(cOrder.OrderId);
                        deviceList[deviceOrder.Key] = DateTime.Now.AddMilliseconds(deviceCheckIntervalMsec);


                        // Add into ELK and run it async , handle by singleton injection
                        _logOrderService.SubmitLog(new OrderLogModel()
                        {
                            ActionName = ActionNameList.SendWithdrawalOrderDevice,
                            DeviceId = deviceOrder.Key,
                            LogDate = DateTime.Now,
                            OrderId = cOrder.OrderId,
                            OrderNumber = cOrder.OrderNo,
                            User = "PushOrderService",
                            ProceedId = CurrentProcessId,
                            ProcessingTimeMs = 0,
                            OrderCreationDate = cOrder.OrderCreationDate,
                            Desc = "Delay  " + (DateTime.Now - cOrder.OrderCreationDate).TotalMilliseconds + " ms "
                        }).FireAndForgetSafeAsync(ex => { NlogLogger.Error("[" + CurrentProcessId + "] PushDeviceOrder Error ", ex); });

                        // set new device Order
                        setDeviceNewOrder(deviceOrder.Key, cOrder);

                        //_memoryCache.Set(GetDeviceExpirationOrderLock(deviceOrder.Key), signalROrderContent, TimeSpan.FromMilliseconds(deviceCheckIntervalMsec));
                        sendDeviceOrder(deviceOrder.Key, cOrder);
                        processedDevice.Add(deviceOrder.Key);
                    }
                }
            }


            //if(cycleCounter%3 ==0)
            //{

            foreach (var waitingStatusAndExpiredDevice in validDeviceList.Where(x => !processedDevice.Contains(x.Key) && x.Value.HasValue && (x.Value == DateTime.MinValue || x.Value < DateTime.Now)))
            {
                // Update those expired device to waiting Status Update
                if (waitingStatusAndExpiredDevice.Value != DateTime.MinValue)
                {
                    deviceList.TryUpdate(waitingStatusAndExpiredDevice.Key, DateTime.MinValue, waitingStatusAndExpiredDevice.Value);
                }
                sendDeviceStatusRequest(waitingStatusAndExpiredDevice.Key, processId);
                processedDevice.Add(waitingStatusAndExpiredDevice.Key);
            }

            // After 15 second send if withdrawalorder still not process will resend again
            foreach (var waitingStatusAndExpiredDevice in validDeviceList.Where(x => !processedDevice.Contains(x.Key) && x.Value.HasValue && x.Value != DateTime.MinValue && x.Value.Value > DateTime.Now)) // && (DateTime.Now.AddMilliseconds(deviceCheckIntervalMsec) - x.Value.Value).TotalSeconds>20
            {
                // this cache will removed if order status updated
                var resendOrder = _memoryCache.Get(GetDeviceExpirationOrderLock(waitingStatusAndExpiredDevice.Key));

                if (resendOrder != null)
                {
                    var withdarwalCache = (WithdrawalQueueCacheItem)resendOrder;
                    if (!currentProcessOrder.ContainsKey(withdarwalCache.OrderId))
                    {
                        sendDeviceOrder(waitingStatusAndExpiredDevice.Key, withdarwalCache, true);
                        processedDevice.Add(waitingStatusAndExpiredDevice.Key);
                    }
                }
            }
            //}

            if (cycleCounter % 2 == 0)
            {
                // Active Device no Order and No ever Processed This Cycle
                // after last order completed 15s only send balance request 
                var latestDeviceStatus = deviceList.Where(x => activeDevices.ContainsKey(x.Key) && !x.Value.HasValue && !processedDevice.Contains(x.Key)
                && (deviceLastCompletedOrderTime.TryGetValue(x.Key, out var lastOrderCompletedTime) | true) && (lastOrderCompletedTime.AddSeconds(15) < DateTime.Now) // last Order Completed Time not less than 15 seconds
                && (deviceLastUpdateBalanceTime.TryGetValue(x.Key, out var lastUpdateBalanceTime) | true) && (lastUpdateBalanceTime.AddSeconds(60) < DateTime.Now) // last update balance not less than 60 seconds
                ).ToDictionary();

                foreach (var device in latestDeviceStatus.Where(x => (deviceLastSendBalanceTime.TryGetValue(x.Key, out var _lastSend) | true) && (_lastSend == DateTime.MinValue || _lastSend.AddSeconds(60) < DateTime.Now)))
                {
                    if (!balanceRequestList.ContainsKey(device.Key) && sendBalanceUpdateRequest(device.Key)) // if lock then false else true , balance will using cache to lock interval 30 second
                    {
                        balanceRequestList.TryAdd(device.Key, DateTime.Now.AddSeconds(60)); // 60 second interval
                        deviceLastSendBalanceTime.AddOrUpdate(device.Key, DateTime.Now, (key, oldValue) => DateTime.Now);
                    }
                }
            }

            NlogLogger.Info(logPrefix + "PushOrderWorker - Device Status " + JsonConvert.SerializeObject(deviceList));
            NlogLogger.Info(logPrefix + "PushOrderWorker - Valid Device List " + JsonConvert.SerializeObject(validDeviceList));
            NlogLogger.Info(logPrefix + "PushOrderWorker - Current Process Order " + JsonConvert.SerializeObject(currentProcessOrder));
            NlogLogger.Info(logPrefix + "PushOrderWorker - balance Update Request " + JsonConvert.SerializeObject(balanceRequestList));
            NlogLogger.Info(logPrefix + "PushOrderWorker - Last Completed Order  Time " + JsonConvert.SerializeObject(deviceLastCompletedOrderTime));
            NlogLogger.Info(logPrefix + "PushOrderWorker - Last Send Balance Time " + JsonConvert.SerializeObject(deviceLastSendBalanceTime));

        }

        private async Task<Tuple<WithdrawalQueueCacheItem?, bool>> validateOrderV2(int deviceId, WithdrawalQueueCacheItem orderInfo)
        {
            var dateNow = TimeHelper.GetUnixTimeStamp(DateTime.Now.AddMinutes(-60));//TimeHelper.GetUnixTimeStamp(DateTime.Now.AddMinutes(-30));
            bool isNameVerified = true;
            bool isValid = true;
            var orderInDetails = await _withdrawalOrdersService.GetById(orderInfo.OrderId);

            if (dateNow >= orderInDetails.CreationUnixTime)
            {
                NlogLogger.Debug("GetPendingWithdrawalList - Order Expired (" + orderInDetails.ID + "), ([Order Number]" + orderInDetails.OrderNumber + ")");
                isValid = false;
            }
            else if (orderInDetails.DeviceId != deviceId)
            {
                isValid = false;
                NlogLogger.Debug("GetPendingWithdrawalList - Device Error  (" + orderInDetails.ID + ") ([Order Number]" + orderInDetails.OrderNumber + ") Order Device (" + orderInDetails.DeviceId + ")");
            }
            else if (orderInDetails.OrderStatus is WithdrawalOrderStatusEnum.Success)
            {
                isValid = false;
                NlogLogger.Debug("GetPendingWithdrawalList - Order Success Status  (" + orderInDetails.ID + ") ([Order Number]" + orderInDetails.OrderNumber + ")");
            }
            else if (orderInDetails.OrderStatus is not WithdrawalOrderStatusEnum.Wait or WithdrawalOrderStatusEnum.Pending)
            {
                isValid = false;
                NlogLogger.Debug("GetPendingWithdrawalList - Order Not Pending  Status  (" + orderInDetails.ID + ") ([Order Number]" + orderInDetails.OrderNumber + ")");
            }
            else
            {
                if(orderInfo.OrderCreationDate == DateTime.MinValue)
                {
                    orderInfo.OrderCreationDate = orderInDetails.CreationTime;
                }

            #region 转账银行映射失败

            var bankKeyStr = WithdrawalOrderBankMapper.FindBankByName(orderInfo.RecipientAccountBankName);

            if (bankKeyStr.IsNullOrEmpty() || !BankApp.BanksObject.ContainsKey(bankKeyStr))
            {
                var orderDetails = await _withdrawalOrdersService.GetById(orderInfo.OrderId);
                orderDetails.OrderStatus = WithdrawalOrderStatusEnum.Fail;
                orderDetails.TransactionTime = DateTime.Now;
                orderDetails.TransactionUnixTime = TimeHelper.GetUnixTimeStamp(orderDetails.TransactionTime);
                orderDetails.Remark = "转账银行不支持";
                await _withdrawalOrdersService.UpdateAsync(orderDetails);
                    _redisService.AddTelegramNotify(new TelegramNotifyModel()
                    {
                        OrderId = orderDetails.ID,
                        MerchantCode = orderDetails.MerchantCode,
                        OrderNumber = orderDetails.OrderNumber,
                        OrderAmount = orderDetails.OrderMoney,
                        OrderTimeUnix = orderDetails.CreationUnixTime,
                        TriggerDate = DateTime.UtcNow,
                        Type = NotifyTypeEnum.BankNotSupported
                    });

                await _withdrawalOrderHelper.HandleWithdrawalOrderComplete(CurrentProcessId, orderDetails.ID, orderDetails.OrderMoney, WithdrawalOrderStatusEnum.Fail, orderDetails.MerchantCode, true);
                isValid = false;
                NlogLogger.Debug("GetPendingWithdrawalList - Bank Mapping Not Supported (" + orderInfo.OrderId + ") ([Order Number]" + orderInfo.OrderNo + ")");
            }

            if (!bankKeyStr.IsNullOrEmpty() && _openCheckAccountName && !orderInfo.IsAccountNameVerified)// 检查收款人信息
            {
                var isMatch = await _accountCheckerService.CheckAccountName(bankKeyStr, orderInfo.RecipientAccountNumber, orderInfo.RecipientAccountHolderName, orderInfo.MerchantId);
                NlogLogger.Debug("GetPendingWithdrawalList - Account Name Checking Result  (" + isMatch + ") ([Order Number]" + orderInfo.OrderNo + ")");

                if ((isMatch.HasValue && !isMatch.Value))
                {
                    isValid = false;
                    var orderDetails = await _withdrawalOrdersService.GetById(orderInfo.OrderId);
                    // meaning not match
                    orderDetails.OrderStatus = WithdrawalOrderStatusEnum.Fail;
                    orderDetails.Remark = "收款人姓名不匹配";
                    orderDetails.TransactionTime = DateTime.Now;
                    orderDetails.TransactionUnixTime = TimeHelper.GetUnixTimeStamp(orderDetails.TransactionTime);
                    await _withdrawalOrdersService.UpdateAsync(orderDetails);
                        await _withdrawalOrdersService.UpdateAsync(orderDetails);
                        _redisService.AddTelegramNotify(new TelegramNotifyModel()
                        {
                            OrderId = orderDetails.ID,
                            MerchantCode = orderDetails.MerchantCode,
                            OrderNumber = orderDetails.OrderNumber,
                            OrderAmount = orderDetails.OrderMoney,
                            OrderTimeUnix = orderDetails.CreationUnixTime,
                            TriggerDate = DateTime.UtcNow,
                            Type = NotifyTypeEnum.ErrorHolderName
                        });
                        await _withdrawalOrderHelper.HandleWithdrawalOrderComplete(CurrentProcessId, orderDetails.ID , orderDetails.OrderMoney, WithdrawalOrderStatusEnum.Fail, orderDetails.MerchantCode ,true);

                    // Add into ELK and run it async , handle by singleton injection
                    _logOrderService.SubmitLog(new OrderLogModel()
                    {
                        ActionName = ActionNameList.WithdrawalOrderUpdateStatus,
                        DeviceId = deviceId,
                        LogDate = DateTime.Now,
                        OrderId = orderInfo.OrderId,
                        OrderNumber = orderInfo.OrderNo,
                        User = "PushOrderService",
                        ProceedId = CurrentProcessId,
                        ProcessingTimeMs = 0,
                        OrderCreationDate = orderInfo.OrderCreationDate
                    }).FireAndForgetSafeAsync(ex => { NlogLogger.Error("[" + CurrentProcessId + "] PushDeviceOrder Status Update Error ", ex); });

                    NlogLogger.Debug("GetPendingWithdrawalList - Recipient Account Name Not Match ([Order Number]" + orderInfo.OrderNo + ")");
                }
                else if (!isMatch.HasValue)
                {
                    isNameVerified = false;
                }
                }
                else if (orderInfo.IsAccountNameVerified)
            {
                isNameVerified = true;
            }
            #endregion
            }

            orderInfo.IsAccountNameVerified = isNameVerified;
            return Tuple.Create((isValid ? orderInfo : null), isNameVerified);
        }


        private async Task<Tuple<WithdrawalQueueCacheItem?, bool>> getDeviceOrderWithRetry(int deviceId)
        {
            int maxAttempt = 5;
            bool isNameVerified = true;
            bool isFromVolQueue = false;
            WithdrawalQueueCacheItem? returnOrderInfo = null;
            var deviceInfo = await _withdrawalDevicesService.GetFirstAsync(r => r.Id == deviceId && r.IsDeleted == false);

            if (deviceInfo == null)
            {
                NlogLogger.Debug("GetPendingWithdrawalList - Device Not Valid (" + deviceId + ")");
                return null;
            }
            else
            {
                deviceTypeDict.TryAdd(deviceId, deviceInfo.BankType);
            }

            for (int i = 0; i < maxAttempt; i++)
            {
                isNameVerified = true;
                isFromVolQueue = false;

                // check device recovered List
                var firstOrder = recoverOrderList.FirstOrDefault(x => x.Key.Item1 == deviceId && x.Value < DateTime.Now);
                WithdrawalQueueCacheItem? orderInfo = null;

                if (firstOrder.Key != null)
                {
                    if (!currentProcessOrder.ContainsKey(firstOrder.Key.Item2.OrderId))
                    {
                        orderInfo = firstOrder.Key.Item2;
                    }

                    recoverOrderList.TryRemove(firstOrder);

                    if (orderInfo == null)
                    {
                        continue;
                    }
                }

                if (orderInfo == null)
                {
                    var withdrawalOrder = _redisService.GetLPushTransferOrder(deviceInfo.BankType.ToString(), deviceInfo.Phone);
                    if (withdrawalOrder == null)
                    {
                        break;
                    }
                    else
                    {
                        orderInfo = convertToSignalrModel(withdrawalOrder, deviceId);
                        isFromVolQueue = true;
                    }
                }

                // will return order info if  order is valid  to process
                var getValidOrder = await validateOrderV2(deviceId, orderInfo);

                if (getValidOrder.Item1 != null)
                {
                    isNameVerified = getValidOrder.Item2;
                    var newOrderInfo = getValidOrder.Item1;
 
                    returnOrderInfo = newOrderInfo;
                    break;
                }
                else
                {
                    continue;
                }
            }

            if (returnOrderInfo != null && isFromVolQueue)
            {
            }

            return Tuple.Create(returnOrderInfo, isNameVerified);
        }

        private async Task< Dictionary<int,Tuple<WithdrawalQueueCacheItem, bool>>> GetPendingWithdrawalList(List<int> avaliableDevices , string eventId)
        {
          
            NlogLogger.Debug("["+ eventId + "]GetPendingWithdrawalList - Start (" + DateTime.Now.ToString() + ") DeviceId ("+ JsonConvert.SerializeObject(avaliableDevices) +")");
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
            SemaphoreSlim semaphore = new SemaphoreSlim(3); // Limit to 3 concurrent tasks.
            ConcurrentDictionary<int, Tuple<WithdrawalQueueCacheItem, bool>> returnData = new ConcurrentDictionary<int, Tuple<WithdrawalQueueCacheItem, bool>>();
            var tasks = new List<Task>();
            foreach (int deviceId in avaliableDevices)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await semaphore.WaitAsync();
                        var orderInfo = await getDeviceOrderWithRetry(deviceId);
            
                        if (orderInfo != null && orderInfo.Item1 != null)
                        {
                            returnData.TryAdd(deviceId, orderInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        NlogLogger.Error("[" + eventId + "]GetPendingWithdrawalList - Error Concurrent " + deviceId.ToString(), ex);
                }
                    finally { semaphore.Release(); }


                }));
            }

            await Task.WhenAll(tasks);

            sw.Stop();
            NlogLogger.Debug("[" + eventId + "]GetPendingWithdrawalList - Time Taken (" + sw.ElapsedMilliseconds + "ms)");
       
            return returnData.ToDictionary();
        }

        public void deviceInactiveTracked(int deviceId)
        {
            activeDevices.TryRemove(deviceId, out var device);
        }
        public bool deviceStatusUpdate(int deviceId , DeviceProcessStatusEnum status)
        {
            if (status == DeviceProcessStatusEnum.Idle)
            {
                // reset device for next 
                deviceList.TryUpdate(deviceId , null , DateTime.MinValue);
            }
            else if(status == DeviceProcessStatusEnum.ProcessPayout)
            {
                // if payout will check again next ms 
                deviceList.TryUpdate(deviceId, DateTime.Now.AddMilliseconds(deviceCheckIntervalMsec), DateTime.MinValue);
            }

            NlogLogger.Info("PushOrderWorker - deviceStatusUpdate " + deviceId.ToString() + " Status - " + status.ToString());

            return true;
        }

        public bool sendDeviceCurrentOrder(int deviceId)
        {
            _memoryCache.Remove(GetDeviceWaitingOrderLock(deviceId));
            NlogLogger.Info("PushOrderWorker - sendDeviceCurrentOrder (Remove Waiting Cache)" + deviceId.ToString());
            return true;
        }

        public bool deviceOrderInProcess(string orderId , int deviceId)
        {
            bool addIntoInProcessList = currentProcessOrder.TryAdd(orderId, DateTime.Now);

            var deviceOrderKey = GetDeviceExpirationOrderLock(deviceId);
            var currentDeviceOrder = _memoryCache.Get<WithdrawalQueueCacheItem>(deviceOrderKey);

            if(currentDeviceOrder!=null && currentDeviceOrder.OrderId  == orderId)
            {
                // remove resend if device current push order same  with in progress order 
                _memoryCache.Remove(deviceOrderKey);
            }

            NlogLogger.Info("PushOrderWorker - order In Progess  Order ID - " + orderId.ToString()+" Queue Add - "+ addIntoInProcessList);
            return addIntoInProcessList;
        }


        public bool deviceBalanceUpdated(int deviceId)
        {
            bool removedFromRequestList = balanceRequestList.TryRemove(deviceId, out DateTime nextDateTime);
            NlogLogger.Info("PushOrderWorker - deviceBalanceUpdated  Device ID - " + deviceId.ToString() + " IsRemoved - " + removedFromRequestList);
            deviceLastUpdateBalanceTime.AddOrUpdate(deviceId, DateTime.Now, (key, oldValue) => DateTime.Now);
            return removedFromRequestList;
        }
        public async Task<bool> deviceOrderCompleted(int deviceId , string orderId)
        {
            bool result = false;
            NlogLogger.Info("PushOrderWorker - deviceOrderCompleted " + deviceId.ToString() + " Order ID - " + orderId.ToString());
            if (currentProcessOrder.ContainsKey(orderId))
            {
                deviceList[deviceId] = null; // free update the device order
                // Remove From Order If Found
                currentProcessOrder.Remove(orderId);
                // only current in process order will update last completed Date
                deviceLastCompletedOrderTime.AddOrUpdate(deviceId, DateTime.Now, (key, oldValue) => DateTime.Now);
                _memoryCache.Remove(GetDeviceWaitingBalanceUpdateLock(deviceId)); // release balance request waiting lock of device  after order completed 
                balanceRequestList.Remove(deviceId);// clean previous balance request 
            }

            return result;
        }
        private void sendDeviceStatusRequest(int deviceId , string processId)
        {
            var connection = OrderHub.GetDeviceConnection(deviceId);
           
            if (!connection.IsNullOrEmpty())
            {
                if (GetStatusUpdateLock(deviceId, processId)) // no send last
                {
                    NlogLogger.Info("PushOrderWorker - ("+ processId + ") sendDeviceStatusRequest " + deviceId.ToString());
                _hub.Clients.Client(connection).RequestDeviceStatus(deviceId);
            }
                else
                {

                    NlogLogger.Info("PushOrderWorker - (" + processId + ") Cooldown sendDeviceStatusRequest " + deviceId.ToString());
                }
            }
        }

        private bool sendBalanceUpdateRequest(int deviceId)
        {
            var connection = OrderHub.GetDeviceConnection(deviceId);
            var isSendSuccess = true;

            if (!connection.IsNullOrEmpty())
            {
                var getBalanceLock = _memoryCache.Get(GetDeviceWaitingBalanceUpdateLock(deviceId)); // make sure device balance  send in 30 second interval 

                if (getBalanceLock == null) // no send last
                {
                    NlogLogger.Info("PushOrderWorker - sendBalanceUpdateRequest " + deviceId.ToString());
                    var oldValue = _memoryCache.Set(GetDeviceWaitingBalanceUpdateLock(deviceId), string.Empty, TimeSpan.FromSeconds(60));
                    _hub.Clients.Client(connection).RequestUpdateBalance(deviceId);
                }
                else
                {
                    isSendSuccess = false; // cache Lock
                }
            
            }

            return isSendSuccess;
        }

        private void sendDeviceOrder(int deviceId , WithdrawalQueueCacheItem orderInfo , bool isRetry = false)
        {
            var connection = OrderHub.GetDeviceConnection(deviceId);

            if (!connection.IsNullOrEmpty())
            {
                var getOrderLock = _memoryCache.Get(GetDeviceWaitingOrderLock(deviceId)); // make sure same content send in 15 second interval

                if(getOrderLock == null || getOrderLock.ToString() != orderInfo.OrderId)
                {
                    var signalrOrder = _objectMapper.Map<SignalROrderInfo>(orderInfo);
                    NlogLogger.Info("PushOrderWorker - sendDeviceOrder " + deviceId.ToString() + "Order Id "+ orderInfo.OrderId + "Order Number " + orderInfo.RecipientAccountNumber + "Cache Value - " +(getOrderLock??string.Empty) +(isRetry ?" (Retry) " : string.Empty));
                    var oldValue = _memoryCache.Set(GetDeviceWaitingOrderLock(deviceId), orderInfo.OrderId, TimeSpan.FromSeconds(15));
                    _hub.Clients.Client(connection).ProcessOrder(signalrOrder);
                }
            }
        }

        private void  syncConnectedDevice()
        {
            var latestConnectedDevice = OrderHub.GetConnectedDevice();
            var newAddedDevice = new List<int>();
            var processId = Guid.NewGuid().ToString();
           
            foreach (var newDevice in latestConnectedDevice.GroupJoin(deviceList, t1 => t1, t2 => t2.Key, (t1, t2) => new {  t1 , t2}).Where(x=>x.t2.Count()==0))
            {
                if (deviceList.TryAdd(newDevice.t1, DateTime.MinValue))
                {
                    newAddedDevice.Add(newDevice.t1);
                }
            }

            foreach (var deletedDevice in deviceList.GroupJoin(latestConnectedDevice, t1 => t1.Key, t2 => t2, (t1, t2) => new { t1 , t2 }).Where(x => x.t2.Count()==0))
            {
                deviceList.TryRemove(deletedDevice.t1.Key, out var value);
            }

            foreach (var newAddedDeviceItem in newAddedDevice)
            {
                // send signalR To Comfirm the status
                sendDeviceStatusRequest(newAddedDeviceItem, processId);
            }
        }

    }

    internal static class ExtensionClass
    {
        internal static void FireAndForgetSafeAsync(this Task task, Action<Exception>? errorHandler = null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    errorHandler?.Invoke(ex);
                    // Optional: log the exception
                }
            });
        }

        public static void FireAndForgetSafeAsyncV2<T>(this Task<T> task, Action<Exception>? errorHandler = null, Action<T>? successHandler = null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    T result = await task;
                    successHandler?.Invoke(result);
                }
                catch (Exception ex)
                {
                    errorHandler?.Invoke(ex);
                    // Optional: log the exception
                }
            });
        }
    }
}
