using Abp.Extensions;
using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Commons;
using Neptune.NsPay.ELKLogExtension;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Web.TransferApi.Models;
using Neptune.NsPay.Web.TransferApi.SignalR;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.WithdrawalOrders;
using System.Diagnostics;

namespace Neptune.NsPay.Web.TransferApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TransferV2Controller : ControllerBase
    {
        private readonly IRedisService _redisService;
        private readonly IMerchantService _merchantService;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersService;
        private readonly IWithdrawalDevicesService _withdrawalDevicesService;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;
        private readonly IBinaryObjectManagerService _binaryObjectManagerService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly PushOrderHelper _pushOrderHelper;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly LogOrderService _logOrderService;
        private readonly WithdrawalOrderHelper _withdrawalOrderHelper;
        private readonly IMerchantBillsMongoService _merchantBillsMongoService;

        public TransferV2Controller(IRedisService redisService,
           IMerchantService merchantService,
           IWithdrawalOrdersMongoService withdrawalOrdersService,
           IWithdrawalDevicesService withdrawalDevicesService,
           IMerchantFundsMongoService merchantFundsMongoService,
           IBinaryObjectManagerService binaryObjectManagerService,
           IPushNotificationService pushNotificationService,
           PushOrderHelper pushOrderHelper,
           IKafkaProducer kafkaProducer,
           LogOrderService logOrderService,
           WithdrawalOrderHelper withdrawalOrderHelper,
           IMerchantBillsMongoService merchantBillsMongoService)
        {
            _redisService = redisService;
            _merchantService = merchantService;
            _withdrawalOrdersService = withdrawalOrdersService;
            _withdrawalDevicesService = withdrawalDevicesService;
            _merchantFundsMongoService = merchantFundsMongoService;
            _binaryObjectManagerService = binaryObjectManagerService;
            _pushNotificationService = pushNotificationService;
            _pushOrderHelper = pushOrderHelper;
            _kafkaProducer = kafkaProducer;
            _logOrderService = logOrderService;
            _withdrawalOrderHelper = withdrawalOrderHelper;
            _merchantBillsMongoService = merchantBillsMongoService;
        }

        /// <summary>
        /// 查询出款设备
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetDevice")]
        public async Task<JsonResult> GetDevice([FromBody] GetDeviceInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.MerchantCode.IsNullOrEmpty() || input.Phone.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            var merchantCode = "";
            //检测商户是否正常
            if (input.MerchantCode != "NsPay")
            {
                var merchant = await _merchantService.GetFirstAsync(r => r.MerchantCode == input.MerchantCode);
                if (merchant != null)
                {
                    merchantCode = merchant.MerchantCode;
                }
            }
            else
            {
                merchantCode = input.MerchantCode;
            }
            if (merchantCode.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "商户错误" });
            }

            //检测设备是否正常
            var device = await _withdrawalDevicesService.GetFirstAsync(r => r.Phone == input.Phone && r.BankType == input.BankType && r.MerchantCode == merchantCode && r.IsDeleted == false);
            if (device == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "查无设备" });
            }
            GetDeviceReponse reponse = new GetDeviceReponse()
            {
                DeviceId = device.Id,
                Phone = device.Phone,
                CardNumber = device.CardNumber,
                Name = device.Name,
                Otp = device.BankOtp,
                Status = device.Status,
                Process = (int)device.Process,
                LoginPassWord = device.LoginPassWord,
            };

            return new JsonResult(new ApiResult<GetDeviceReponse> { Code = StatusCodeEnum.OK, Message = "", Data = reponse });
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        [HttpPost]
        [Route("UpdateWithdrawOrder")]
        public async Task<JsonResult> UpdateWithdrawOrder(UpdateWithdrawOrderInput input)
        {
            var processId = Guid.NewGuid().ToString();
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.OrderId.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }

            //查询订单
            var orderInfo = await _withdrawalOrdersService.GetById(input.OrderId);
            if (orderInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单错误" });
            }

            if (input.DeviceId.HasValue && orderInfo.DeviceId != input.DeviceId.Value)
            {
                NlogLogger.Info("UpdateWithdrawOrder： Device Not Matched " + input.ToJsonString());
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备错误" });
            }

            var orderDeviceId = orderInfo.DeviceId;

            if (input.OrderStatus == WithdrawalOrderStatusEnum.Success)
            {
                var updateOrderInfo = orderInfo.DeepClone();
                NlogLogger.Info("[" + processId + "]订单回调成功：" + input.ToJsonString());
                updateOrderInfo.TransactionNo = input.TransactionNo;
                updateOrderInfo.OrderStatus = WithdrawalOrderStatusEnum.Success;
                updateOrderInfo.TransactionTime = DateTime.Now;
                updateOrderInfo.TransactionUnixTime = TimeHelper.GetUnixTimeStamp(updateOrderInfo.TransactionTime);
                var result = await _withdrawalOrdersService.UpdateOrderStatusAsync(updateOrderInfo);
                var flag = result;

                //if (!result)
                //{
                //    flag = await _withdrawalOrdersService.UpdateAsync(orderInfo);
                //}
                if (flag)
                {
                    var successOrderCache = _redisService.GetWithdrawalSuccessOrder(orderInfo.ID);
                    if (successOrderCache.IsNullOrEmpty())
                    {
                        _redisService.SetWithdrawalSuccessOrder(orderInfo.ID);
                    }

                    var checkOrderInfo = _redisService.GetMerchantBillTrasferOrder(orderInfo.MerchantCode, orderInfo.OrderNumber);
                    if (string.IsNullOrEmpty(checkOrderInfo))
                    {
                        _redisService.SetMerchantBillTrasferOrder(orderInfo.MerchantCode, orderInfo.OrderNumber);

                        await _withdrawalOrderHelper.HandleWithdrawalOrderComplete(processId, orderInfo.ID, orderInfo.OrderMoney, WithdrawalOrderStatusEnum.Success, orderInfo.MerchantCode);

                        _logOrderService.SubmitLog(new OrderLogModel()
                        {
                            ActionName = ActionNameList.WithdrawalDeviceProcessCompleted,
                            DeviceId = updateOrderInfo.DeviceId,
                            LogDate = DateTime.Now,
                            OrderId = updateOrderInfo.ID,
                            OrderNumber = updateOrderInfo.OrderNumber,
                            User = "TranferV2",
                            ProceedId = processId,
                            OrderStatus = updateOrderInfo.OrderStatus.ToString(),
                            ProcessingTimeMs = 0,
                            OrderCreationDate = orderInfo.CreationTime
                        }).FireAndForgetSafeAsync(ex => { NlogLogger.Error("[" + processId + "] Transfer v2 Update Status Error ", ex); });

                        NlogLogger.Fatal("[" + processId + "] 出款订单：" + orderInfo.OrderNumber + "，添加完成");
                    }
                }
            }
            else
            {
                var successOrderCache = _redisService.GetWithdrawalSuccessOrder(orderInfo.ID);
                if (successOrderCache.IsNullOrEmpty())
                {
                    NlogLogger.Info("[" + processId + "]订单回调失败：" + input.ToJsonString());
                    orderInfo.OrderStatus = input.OrderStatus;
                    orderInfo.TransactionTime = DateTime.Now;
                    orderInfo.TransactionUnixTime = TimeHelper.GetUnixTimeStamp(orderInfo.TransactionTime);
                    orderInfo.Remark = input.Remark;
                    await _withdrawalOrdersService.UpdateOrderStatusAsync(orderInfo);

                    _logOrderService.SubmitLog(new OrderLogModel()
                    {
                        ActionName = ActionNameList.WithdrawalOrderUpdateStatus,
                        DeviceId = orderInfo.DeviceId,
                        LogDate = DateTime.Now,
                        OrderId = orderInfo.ID,
                        OrderNumber = orderInfo.OrderNumber,
                        User = "TranferV2",
                        ProceedId = processId,
                        OrderStatus = orderInfo.OrderStatus.ToString(),
                        ProcessingTimeMs = 0,
                        OrderCreationDate = orderInfo.CreationTime
                    }).FireAndForgetSafeAsync(ex => { NlogLogger.Error("[" + processId + "] Transfer v2 Update Status Error ", ex); });
                }

                if(input.OrderStatus == WithdrawalOrderStatusEnum.Fail && !input.Remark.IsNullOrEmpty())
                {
                    var notifyType = _withdrawalOrderHelper.GetNotifyTypeByRemark(input.Remark);

                    if(notifyType.HasValue)
                    {
                        _redisService.AddTelegramNotify(new TelegramNotifyModel()
                        {
                            OrderId = orderInfo.ID,
                            MerchantCode = orderInfo.MerchantCode,
                            OrderNumber = orderInfo.OrderNumber,
                            OrderAmount = orderInfo.OrderMoney,
                            OrderTimeUnix = orderInfo.CreationUnixTime,
                            TriggerDate = DateTime.UtcNow,
                            Type = notifyType.Value
                        });

                        await _withdrawalOrderHelper.HandleWithdrawalOrderComplete(processId, orderInfo.ID, orderInfo.OrderMoney, WithdrawalOrderStatusEnum.Fail, orderInfo.MerchantCode, isConfirmed: (notifyType.Value != NotifyTypeEnum.BankMaintenance));

                    }
                }
            }

            if (input.OrderStatus == WithdrawalOrderStatusEnum.Pending)
            {
                _pushOrderHelper.DeviceOrderInProcess(orderInfo.ID, orderInfo.DeviceId);
            }
            else if (input.OrderStatus > WithdrawalOrderStatusEnum.Pending)
            {
                await _pushOrderHelper.DeviceOrderCompleted(orderInfo.ID, orderInfo.DeviceId);
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        [HttpPost]
        [Route("UpdateWithdrawalOrderRemark")]
        public async Task<JsonResult> UpdateWithdrawalOrderRemark(UpdateWithdrawalOrderRemarkInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.OrderId.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.Remark.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }

            var orderInfo = await _withdrawalOrdersService.GetById(input.OrderId);
            NlogLogger.Info("订单回调失败[UpdateWithdrawalOrderRemark]：" + input.ToJsonString());
            orderInfo.Remark = input.Remark;
            try
            {
                var result = await _withdrawalOrdersService.UpdateAsync(orderInfo);
                if (!result)
                {
                    return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "更新失败" });
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error($"订单更新失败[UpdateWithdrawalOrderRemark]：: {ex.Message}", ex);
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "系统错误" });
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 上传订单收据
        /// </summary>
        [HttpPost]
        [Route("UploadReceipt")]
        public async Task<JsonResult> UploadReceipt(UploadReceiptInput input)
        {
            var eventId = Guid.NewGuid().ToString();
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.OrderId.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }

            if (input.FileContentBase64.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //查询订单
            var orderInfo = await _withdrawalOrdersService.GetById(input.OrderId);
            if (orderInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单错误" });
            }

            #region 储存订单收据

            // store proof content directly to DB

            var splitstring = input.FileContentBase64.Split(',');
            var base64WithoutHeader = splitstring.Length > 1 ? splitstring[1] : splitstring[0];
            var storageInfo = new Storage.BinaryObject() { TenantId = 1, Bytes = Convert.FromBase64String(base64WithoutHeader), Description = "Upload Receipt (" + input.OrderId + ")" };
            var haveInsert = await _binaryObjectManagerService.AddAsync(storageInfo);

            if (haveInsert > 0)
            {
                var orderInfoLatest = await _withdrawalOrdersService.GetById(input.OrderId);
                orderInfoLatest.BinaryContentId = storageInfo.Id;
                orderInfoLatest.ContentMIMEType = DetectMimeType(storageInfo.Bytes);
                var updateResult = await _withdrawalOrdersService.UpdateReceipt(orderInfoLatest);
            }

            stopwatch.Stop();
            NlogLogger.Fatal("[" + eventId + "]出款订单：" + orderInfo.OrderNumber + "，收据更新  " + stopwatch.ElapsedMilliseconds + "ms");

            #endregion 储存订单收据

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 提交服务器当前设备状态
        /// 1 - Idle (闲置) 2- ProcessPayout (处理出款)
        /// </summary>
        [HttpPost]
        [Route("UpdateDeviceStatus")]
        public async Task<JsonResult> UpdateProcessStatus(UpdateProcessDeviceStatusInput input)
        {
            // when order completed or signalr request to update the status , please update the device status throught this API
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.DeviceId <= 0)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 查看设备状态与订单状态
        /// </summary>
        [HttpPost]
        [Route("CheckWithdrawalOrderStatus")]
        public async Task<JsonResult> CheckWithdrawalOrderStatus(CheckDeviceWithdrawalOrderInput input)
        {            // When Device Getting The Withdrawal Order from signalR , call this endpoint to check and verify the device and order status
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            else if (input.DeviceId <= 0)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            else if (input.OrderId.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }

            #region check device status

            var processId = Guid.NewGuid().ToString();
            input.AutoCompleteOrder = true;
            var deviceInfo = await _withdrawalDevicesService.GetFirstAsync(r => r.Id == input.DeviceId && r.IsDeleted == false);
            if (deviceInfo == null)
            {
                _pushOrderHelper.DeviceInactiveTracked(input.DeviceId);
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备错误" });
            }
            else if (deviceInfo.Process != WithdrawalDevicesProcessTypeEnum.Process)
            {
                _pushOrderHelper.DeviceInactiveTracked(deviceInfo.Id);
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备错误" });
            }

            #endregion check device status

            #region check withdrawal status

            var withdrawalExpiredDate = TimeHelper.GetUnixTimeStamp(DateTime.Now.AddMinutes(-60));
            var orderInfo = await _withdrawalOrdersService.GetById(input.OrderId);

            if (orderInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单错误" });
            }
            else if (orderInfo.DeviceId != input.DeviceId)
            {
                NlogLogger.Debug("GetPendingWithdrawalList - Device Error  (" + orderInfo.ID + ") ([Order Number]" + orderInfo.OrderNumber + ") Order Device (" + orderInfo.DeviceId + ")");
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "出款设备错误" });
            }
            else if (withdrawalExpiredDate >= orderInfo.CreationUnixTime)
            {
                await autoCompleteOrder(input.DeviceId, input.OrderId, input.AutoCompleteOrder);
                NlogLogger.Debug("GetPendingWithdrawalList - Order Expired (" + orderInfo.ID + "), ([Order Number]" + orderInfo.OrderNumber + ")");
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单超时" });
            }
            else if (orderInfo.OrderStatus is WithdrawalOrderStatusEnum.Success)
            {
                await autoCompleteOrder(input.DeviceId, input.OrderId, input.AutoCompleteOrder);
                NlogLogger.Debug("GetPendingWithdrawalList - Order Success Status  (" + orderInfo.ID + ") ([Order Number]" + orderInfo.OrderNumber + ")");
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单已成功" });
            }
            else if (orderInfo.OrderStatus is not WithdrawalOrderStatusEnum.Wait or WithdrawalOrderStatusEnum.Pending)
            {
                await autoCompleteOrder(input.DeviceId, input.OrderId, input.AutoCompleteOrder);
                NlogLogger.Debug("GetPendingWithdrawalList - Order Not Pending  Status  (" + orderInfo.ID + ") ([Order Number]" + orderInfo.OrderNumber + ")");
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单超时" });
            }
            //判断商户余额是否足够
            var funds = await _merchantFundsMongoService.GetFundsByMerchantCode(orderInfo.MerchantCode);
            if (funds == null)
            {
                await autoCompleteOrder(input.DeviceId, input.OrderId, input.AutoCompleteOrder);
                NlogLogger.Debug("GetPendingWithdrawalList - Merchant Order Not Enough Fund (" + orderInfo.ID + ") , (" + orderInfo.MerchantId + ") , (" + orderInfo.OrderNumber + ")  ");
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "商户余额不足" });
            }

            var lastMerchantTransaction = await _merchantBillsMongoService.GetLastMerchantBillByMerchantId(orderInfo.MerchantId);
            var smallerValue = lastMerchantTransaction != null ? Math.Min(lastMerchantTransaction.BalanceAfter, funds.Balance) : funds.Balance;

            if ((smallerValue - orderInfo.OrderMoney) <= 0)
            {
                //更新订单状态为失败
                orderInfo.OrderStatus = WithdrawalOrderStatusEnum.Fail;
                orderInfo.TransactionTime = DateTime.Now;
                orderInfo.TransactionUnixTime = TimeHelper.GetUnixTimeStamp(orderInfo.TransactionTime);
                orderInfo.Remark = "商户余额不足";
                await _withdrawalOrdersService.UpdateAsync(orderInfo);
                await _withdrawalOrderHelper.HandleWithdrawalOrderComplete(processId, orderInfo.ID, orderInfo.OrderMoney, WithdrawalOrderStatusEnum.Fail, orderInfo.MerchantCode, true);
                NlogLogger.Debug("GetPendingWithdrawalList - Merchant Order Not Enough Fund (" + orderInfo.ID + ") , (" + orderInfo.MerchantId + ") , (" + orderInfo.OrderNumber + ")  ");
                _logOrderService.SubmitLog(new OrderLogModel()
                {
                    ActionName = ActionNameList.WithdrawalOrderUpdateStatus,
                    DeviceId = input.DeviceId,
                    LogDate = DateTime.Now,
                    OrderId = orderInfo.ID,
                    OrderNumber = orderInfo.OrderNumber,
                    User = "PushOrderService",
                    ProceedId = processId,
                    ProcessingTimeMs = 0,
                    OrderCreationDate = orderInfo.CreationTime
                }).FireAndForgetSafeAsync(ex => { NlogLogger.Error("[" + processId + "] PushDeviceOrder Status Update Error ", ex); });
                await autoCompleteOrder(input.DeviceId, input.OrderId, input.AutoCompleteOrder);
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "商户余额不足" });
            }

            #endregion check withdrawal status

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        private async Task autoCompleteOrder(int deviceId, string orderId, bool? autoComplete)
        {
            if (autoComplete.HasValue && autoComplete.Value)
            {
                await _pushOrderHelper.ForceCompleteOrder(deviceId, orderId);
            }
        }

        /// <summary>
        /// 获取出款订单，队列取出
        /// </summary>
        [HttpPost]
        [Route("GetWithdrawOrder")]
        public async Task<JsonResult> GetWithdrawOrder(GetWithdrawOrderInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.MerchantCode.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            var merchantCode = "";
            //检测商户是否正常
            if (input.MerchantCode != "NsPay")
            {
                var merchant = await _merchantService.GetFirstAsync(r => r.MerchantCode == input.MerchantCode);
                if (merchant != null)
                {
                    merchantCode = merchant.MerchantCode;
                }
            }
            else
            {
                merchantCode = input.MerchantCode;
            }
            if (merchantCode.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "商户错误" });
            }
            var deviceInfo = await _withdrawalDevicesService.GetFirstAsync(r => r.Phone == input.Phone && r.BankType == input.BankType && r.IsDeleted == false);
            if (deviceInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备异常" });
            }
            var withdrawalOrder = _redisService.GetLPushTransferOrder(deviceInfo.BankType.ToString(), deviceInfo.Phone);
            if (withdrawalOrder == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "暂无订单" });
            }
            var dateNow = TimeHelper.GetUnixTimeStamp(DateTime.Now.AddMinutes(-30));
            var orderInfo = await _withdrawalOrdersService.GetById(withdrawalOrder.Id);
            if (dateNow >= orderInfo.CreationUnixTime)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单超时" });
            }
            if (orderInfo.OrderStatus is WithdrawalOrderStatusEnum.Success)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单已成功" });
            }
            if (orderInfo.OrderStatus is not WithdrawalOrderStatusEnum.Wait or WithdrawalOrderStatusEnum.Pending)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单超时" });
            }
            //判断商户余额是否足够
            var funds = await _merchantFundsMongoService.GetFundsByMerchantCode(orderInfo.MerchantCode);
            if (funds == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "商户余额不足" });
            }
            if ((funds.Balance - orderInfo.OrderMoney) <= 0)
            {
                //更新订单状态为失败
                orderInfo.OrderStatus = WithdrawalOrderStatusEnum.Fail;
                orderInfo.TransactionTime = DateTime.Now;
                orderInfo.TransactionUnixTime = TimeHelper.GetUnixTimeStamp(orderInfo.TransactionTime);
                orderInfo.Remark = "商户余额不足";
                await _withdrawalOrdersService.UpdateAsync(orderInfo);
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "商户余额不足" });
            }
            GetWithdrawOrderResponse response = new GetWithdrawOrderResponse()
            {
                Id = withdrawalOrder.Id,
                MerchantCode = withdrawalOrder.MerchantCode,
                OrderNo = withdrawalOrder.OrderNo,
                BenBankName = withdrawalOrder.BenBankName,
                BenAccountNo = withdrawalOrder.BenAccountNo,
                BenAccountName = withdrawalOrder.BenAccountName,
                OrderMoney = withdrawalOrder.OrderMoney,
                DeviceId = deviceInfo.Id
            };
            return new JsonResult(new ApiResult<GetWithdrawOrderResponse> { Code = StatusCodeEnum.OK, Message = "", Data = response });
        }

        /// <summary>
        /// 更新设备余额
        /// </summary>
        [HttpPost]
        [Route("UpdateBalance")]
        public async Task<JsonResult> UpdateBalance(UpdateBalanceInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.DeviceId == 0)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            var deviceInfo = await _withdrawalDevicesService.GetFirstAsync(r => r.Id == input.DeviceId);
            if (deviceInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备错误" });
            }
            //更新余额
            var balance = _redisService.GetWitdrawDeviceBalance(deviceInfo.Id);
            if (balance == null)
            {
                //重新添加缓存
                WithdrawBalanceModel model = new WithdrawBalanceModel()
                {
                    DeviceId = deviceInfo.Id,
                    Balance = input.Balance,
                    Phone = deviceInfo.Phone,
                    BankType = deviceInfo.BankType,
                    UpdateTime = DateTime.Now,
                };
                _redisService.SetWitdrawDeviceBalance(deviceInfo.Id, model);
            }
            else
            {
                //更新缓存
                balance.Balance = input.Balance;
                balance.UpdateTime = DateTime.Now;
                _redisService.SetWitdrawDeviceBalance(deviceInfo.Id, balance);
            }

            var deviceMinBalance = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.WithdrawDeviceMinBalance);

            if (!deviceMinBalance.IsNullOrEmpty() && int.TryParse(deviceMinBalance, out int minBalance) && minBalance > 0 && input.Balance <= minBalance)
            {
                // set device to stop process
                deviceInfo.Process = WithdrawalDevicesProcessTypeEnum.Stop; // make device offline
                await _withdrawalDevicesService.UpdateAsync(deviceInfo);
                var redisValue = _redisService.GetListRangeDevice(deviceInfo.MerchantCode);
                if (redisValue != null && redisValue.Any(x => x.Id == deviceInfo.Id))
                {
                    var targetDevice = redisValue.First(x => x.Id == deviceInfo.Id);
                    targetDevice.Process = deviceInfo.Process;
                    _redisService.UpdateWithdrawDevice(deviceInfo.MerchantCode, targetDevice);
                }

                _pushOrderHelper.DeviceInactiveTracked(deviceInfo.Id);
            }

            _pushOrderHelper.DeviceBalanceUpdated(deviceInfo.Id);

            await _pushNotificationService.BalanceChanged(deviceInfo.Id, input.Balance);

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        public static string DetectMimeType(byte[] data)
        {
            // Check for PNG (starts with 89 50 4E 47)
            if (data.Length >= 4 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            {
                return "image/png";
            }
            // Check for JPEG (starts with FF D8 FF)
            else if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            {
                return "image/jpeg";
            }
            // Check for GIF (starts with 47 49 46 38)
            else if (data.Length >= 4 && data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && (data[3] == 0x38 || data[3] == 0x39))
            {
                return "image/gif";
            }
            // Check for WebP (starts with 52 49 46 46)
            else if (data.Length >= 4 && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46)
            {
                return "image/webp";
            }
            // Default MIME type if not recognized
            else
            {
                return "application/octet-stream";
            }
        }
    }
}