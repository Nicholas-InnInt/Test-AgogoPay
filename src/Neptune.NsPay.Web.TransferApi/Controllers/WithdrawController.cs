using Abp.Extensions;
using Abp.Json;
using AutoMapper.Internal.Mappers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Web.TransferApi.Models;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.WithdrawalOrders;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using PayPalCheckoutSdk.Orders;
using SqlSugar.Extensions;
using Stripe;

namespace Neptune.NsPay.Web.TransferApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WithdrawController : ControllerBase
    {
        private readonly IRedisService _redisService;
        private readonly IMerchantService _merchantService;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersService;
        private readonly IWithdrawalDevicesService _withdrawalDevicesService;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly WithdrawalOrderHelper _withdrawalOrderHelper;
        public WithdrawController(IRedisService redisService,
            IMerchantService merchantService,
            IWithdrawalOrdersMongoService withdrawalOrdersService,
            IWithdrawalDevicesService withdrawalDevicesService,
            IMerchantFundsMongoService merchantFundsMongoService,
            IKafkaProducer kafkaProducer,
            WithdrawalOrderHelper withdrawalOrderHelper)
        {
            _redisService = redisService;
            _merchantService = merchantService;
            _withdrawalOrdersService = withdrawalOrdersService;
            _withdrawalDevicesService = withdrawalDevicesService;
            _merchantFundsMongoService = merchantFundsMongoService;
            _kafkaProducer = kafkaProducer;
            _withdrawalOrderHelper = withdrawalOrderHelper;
        }

        /// <summary>
        /// 查询出款设备
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Withdraw/GetDevice")]
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
        /// 获取出款订单，队列取出
        /// </summary>
        [HttpPost]
        [Route("~/Withdraw/GetWithdrawOrder")]
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
            if(funds == null)
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
        /// 检查出款中的订单
        /// </summary>
        [HttpPost]
        [Route("~/Withdraw/CheckWithdrawOrder")]
        public async Task<JsonResult> CheckWithdrawOrder(CheckWithdrawOrderInput input)
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

            //查询设备
            var deviceInfo = await _withdrawalDevicesService.GetFirstAsync(r => r.Phone == input.Phone && r.BankType == input.BankType && r.IsDeleted == false);
            if (deviceInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备异常" });
            }

            WithdrawalOrdersMongoEntity? checkOrder = null;
            var startTime = DateTime.Now.AddMinutes(-10);
            if (input.MerchantCode != "NsPay")
            {
                checkOrder = await _withdrawalOrdersService.GetWithdrawOrderByDevice(deviceInfo.MerchantCode, deviceInfo.Id, startTime);
            }
            else
            {
                var nspaySetting = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.InternalWithdrawMerchant);
                List<string> merchants = new List<string>();
                if (nspaySetting != null)
                {
                    merchants = nspaySetting.Split(',').ToList();
                }
                checkOrder = await _withdrawalOrdersService.GetWithdrawOrderByDevice(merchants, deviceInfo.Id, startTime);
            }
            if (checkOrder != null)
            {
                return new JsonResult(new ApiResult<WithdrawalOrdersMongoEntity> { Code = StatusCodeEnum.OK, Message = "", Data = checkOrder });
            }
            else
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "暂无代付中订单" });
            }
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        [HttpPost]
        [Route("~/Withdraw/UpdateWithdrawOrder")]
        public async Task<JsonResult> UpdateWithdrawOrder(UpdateWithdrawOrderInput input)
        {
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
            var eventId = Guid.NewGuid().ToString();
            if (orderInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单错误" });
            }

            if (input.OrderStatus == WithdrawalOrderStatusEnum.Success)
            {
                NlogLogger.Info("订单回调成功：" + input.ToJsonString());
                orderInfo.TransactionNo = input.TransactionNo;
                orderInfo.OrderStatus = WithdrawalOrderStatusEnum.Success;
                orderInfo.TransactionTime = DateTime.Now;
                orderInfo.TransactionUnixTime = TimeHelper.GetUnixTimeStamp(orderInfo.TransactionTime);
                var result = await _withdrawalOrdersService.UpdateAsync(orderInfo);
                var flag = result;
                if (!result)
                {
                    flag = await _withdrawalOrdersService.UpdateAsync(orderInfo);
                }
                if (flag)
                {
                    var successOrderCache = _redisService.GetWithdrawalSuccessOrder(orderInfo.ID);
                    if (successOrderCache.IsNullOrEmpty())
                    {
                        _redisService.SetWithdrawalSuccessOrder(orderInfo.ID);

                        var bankOrderPubModel = new WithdrawalOrderPubModel()
                        {
                            MerchantCode = orderInfo.MerchantCode,
                            OrderId = orderInfo.ID,
                        };
                        _redisService.AddWithdrawalOrderQueueList(NsPayRedisKeyConst.WithdrawalOrder, bankOrderPubModel);
                    }

                    //如果成功加入缓存
                    var checkOrderInfo = _redisService.GetMerchantBillTrasferOrder(orderInfo.MerchantCode, orderInfo.OrderNumber);
                    if (string.IsNullOrEmpty(checkOrderInfo))
                    {
                        _redisService.SetMerchantBillTrasferOrder(orderInfo.MerchantCode, orderInfo.OrderNumber);
                        //PayMerchantRedisMqDto redisMqDto = new PayMerchantRedisMqDto()
                        //{
                        //    PayMqSubType = MQSubscribeStaticConsts.MerchantBillReduceBalance,
                        //    MerchantCode = orderInfo.MerchantCode,
                        //    WithdrawalOrderId = orderInfo.ID,
                        //};
                        //_redisService.SetMerchantMqPublish(redisMqDto);
                        //var transferOrder = new TransferOrderPublishDto()
                        //{
                        //    MerchantCode = orderInfo.MerchantCode,
                        //    WithdrawalOrderId = orderInfo.ID,
                        //    TriggerDate = DateTime.Now,
                        //    OrderStatus = (int)WithdrawalOrderStatusEnum.Success,
                        //    IsCallbackOnly= false
                        //};
                        //await _kafkaProducer.ProduceAsync<TransferOrderPublishDto>(KafkaTopics.TransferOrder, orderInfo.ID, transferOrder);

                        await _withdrawalOrderHelper.HandleWithdrawalOrderComplete(eventId, orderInfo.ID , orderInfo.OrderMoney, WithdrawalOrderStatusEnum.Success, orderInfo.MerchantCode);


                        NlogLogger.Fatal("出款订单：" + orderInfo.OrderNumber + "，添加完成");
                    }
                }
            }
            else
            {
                var successOrderCache = _redisService.GetWithdrawalSuccessOrder(orderInfo.ID);
                if (successOrderCache.IsNullOrEmpty())
                {
                    NlogLogger.Info("订单回调失败：" + input.ToJsonString());
                    orderInfo.OrderStatus = input.OrderStatus;
                    orderInfo.TransactionTime = DateTime.Now;
                    orderInfo.TransactionUnixTime = TimeHelper.GetUnixTimeStamp(orderInfo.TransactionTime);
                    orderInfo.Remark = input.Remark;
                    await _withdrawalOrdersService.UpdateAsync(orderInfo);
                }
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }


        /// <summary>
        /// 获取当前出款订单
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Withdraw/GetCurrentOrder")]
        public async Task<JsonResult> GetCurrentOrder(GetOrderOtpInput input)
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

            //获取当前出款订单
            var order = await _withdrawalOrdersService.GetWithdrawOrderByDevice(input.DeviceId, DateTime.Now.AddMinutes(-30));
            if (order == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "暂无订单" });
            }
            else
            {
                GetWithdrawOrderResponse transferOrderOtpModel = new GetWithdrawOrderResponse()
                {
                    Id = order.ID,
                    DeviceId = order.DeviceId,
                    MerchantCode = order.MerchantCode,
                    OrderNo = order.OrderNumber,
                    BenBankName = order.BenBankName,
                    BenAccountNo = order.BenAccountNo,
                    BenAccountName = order.BenAccountName,
                    OrderMoney = order.OrderMoney,
                };
                return new JsonResult(new ApiResult<GetWithdrawOrderResponse> { Code = StatusCodeEnum.OK, Message = "", Data = transferOrderOtpModel });
            }
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Withdraw/GetOrderInfo")]
        public async Task<JsonResult> GetOrderInfo(GetOrderInfoInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (string.IsNullOrEmpty(input.OrderId))
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            //获取当前出款订单
            var order = await _withdrawalOrdersService.GetById(input.OrderId);
            if (order == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单错误" });
            }
            else
            {
                GetWithdrawOrderResponse response = new GetWithdrawOrderResponse()
                {
                    Id = order.ID,
                    MerchantCode = order.MerchantCode,
                    OrderNo = order.OrderNumber,
                    BenBankName = order.BenBankName,
                    BenAccountNo = order.BenAccountNo,
                    BenAccountName = order.BenAccountName,
                    OrderMoney = order.OrderMoney,
                    DeviceId = order.DeviceId
                };
                return new JsonResult(new ApiResult<GetWithdrawOrderResponse> { Code = StatusCodeEnum.OK, Message = "", Data = response });
            }
        }

        /// <summary>
        /// 更新设备余额
        /// </summary>
        [HttpPost]
        [Route("~/Withdraw/UpdateBalance")]
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

            if (!deviceMinBalance.IsNullOrEmpty() && int.TryParse(deviceMinBalance, out int minBalance)&& minBalance>0&& input.Balance<= minBalance)
            {
                // set device to stop process
                deviceInfo.Process = WithdrawalDevicesProcessTypeEnum.Stop; // make device offline
                await _withdrawalDevicesService.UpdateAsync(deviceInfo);
                var redisValue = _redisService.GetListRangeDevice(deviceInfo.MerchantCode);
                if (redisValue != null && redisValue.Any(x=>x.Id == deviceInfo.Id))
                {
                    var targetDevice = redisValue.First(x => x.Id == deviceInfo.Id);
                    targetDevice.Process = deviceInfo.Process;
                    _redisService.UpdateWithdrawDevice(deviceInfo.MerchantCode, targetDevice);
                }

            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取订单otp
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Withdraw/GetOrderOtp")]
        public async Task<JsonResult> GetOrderOtp(GetOrderOtpInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.DeviceId == 0 || string.IsNullOrEmpty(input.OrderId))
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            var deviceInfo = await _withdrawalDevicesService.GetFirstAsync(r => r.Id == input.DeviceId);
            if (deviceInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备错误" });
            }

            //获取当前出款订单
            var order = _redisService.GetTransferOrder(input.OrderId);
            if (order == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单错误" });
            }
            else
            {
                return new JsonResult(new ApiResult<TransferOrderOtpModel> { Code = StatusCodeEnum.OK, Message = "", Data = order });
            }
        }

        /// <summary>
        /// 获取订单otp
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Withdraw/GetOrderOtpforApk")]
        public async Task<JsonResult> GetOrderOtpforApk(GetOrderOtpforApk input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (string.IsNullOrEmpty(input.Phone) && string.IsNullOrEmpty(input.BankType))
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            WithdrawalDevicesBankTypeEnum bankTypeEnum = (WithdrawalDevicesBankTypeEnum)Enum.Parse(typeof(WithdrawalDevicesBankTypeEnum), input.BankType);
            var deviceInfo = await _withdrawalDevicesService.GetFirstAsync(r => r.Phone == input.Phone && r.BankType == bankTypeEnum);
            if (deviceInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备错误" });
            }

            var orderInfo = await _withdrawalOrdersService.GetWithdrawOrderByDevice(deviceInfo.Id, DateTime.Now.AddMinutes(-30));
            if (orderInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "暂无订单" });
            }

            //获取当前出款订单
            var order = _redisService.GetTransferOrder(orderInfo.ID);
            if (order == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单错误" });
            }
            else
            {
                return new JsonResult(new ApiResult<TransferOrderOtpModel> { Code = StatusCodeEnum.OK, Message = "", Data = order });
            }
        }

        /// <summary>
        /// 更新出款状态
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Withdraw/UpdateOtptatus")]
        public async Task<JsonResult> UpdateOtptatus(UpdateOrderStatusInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.DeviceId == 0 || string.IsNullOrEmpty(input.OrderId))
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            var deviceInfo = await _withdrawalDevicesService.GetFirstAsync(r => r.Id == input.DeviceId);
            if (deviceInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备错误" });
            }

            //获取当前出款订单
            var order = _redisService.GetTransferOrder(input.OrderId);
            if (order == null)
            {
                //if (input.OrderStatus == 0)
                //{
                //    TransferOrderOtpModel transfer = new TransferOrderOtpModel()
                //    {
                //        DeviceId = input.DeviceId,
                //        OrderStatus = input.OrderStatus,
                //        OrderId = input.OrderId,
                //        BankType = deviceInfo.BankType,
                //        CreateTime = DateTime.Now,
                //        Otp = "",
                //        Phone = deviceInfo.Phone,
                //    };
                //    _redisService.SetGetTransferOrderOtp(input.OrderId, transfer); 
                //    return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
                //}
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单错误" });
            }
            else
            {
                if (input.OrderStatus == 1)
                {
                    order.OrderStatus = input.OrderStatus;
                    order.UpdateTime = DateTime.Now;
                    _redisService.SetGetTransferOrderOtp(input.OrderId, order);
                }
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
            }
        }

        /// <summary>
        /// 清楚订单OTP
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Withdraw/RemoveOrderOtp")]
        public async Task<JsonResult> RemoveOrderOtp(GetOrderOtpInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.DeviceId == 0 || input.OrderId.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            var deviceInfo = await _withdrawalDevicesService.GetFirstAsync(r => r.Id == input.DeviceId);
            if (deviceInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备错误" });
            }

            //获取当前出款订单
            var order = _redisService.GetTransferOrder(input.OrderId);
            if (order == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单错误" });
            }
            else
            {
                _redisService.RemoveGetTransferOrderOtp(input.OrderId);
                return new JsonResult(new ApiResult<TransferOrderOtpModel> { Code = StatusCodeEnum.OK, Message = "" });
            }
        }

        /// <summary>
        /// 更新otp
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Withdraw/CreateOrderOtp")]
        public async Task<JsonResult> CreateOrderOtp(CreateOrderOtpInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.DeviceId == 0 || input.OrderId.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            var deviceInfo = await _withdrawalDevicesService.GetFirstAsync(r => r.Id == input.DeviceId);
            if (deviceInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备错误" });
            }

            //获取当前出款订单
            var order = _redisService.GetTransferOrder(input.OrderId);
            if (order == null)
            {
                TransferOrderOtpModel transfer = new TransferOrderOtpModel()
                {
                    DeviceId = input.DeviceId,
                    TransferOtp = input.TransferOtp,
                    OrderId = input.OrderId,
                    BankType = deviceInfo.BankType,
                    CreateTime = DateTime.Now,
                    Otp = "",
                    OrderStatus = 0,
                    Phone = deviceInfo.Phone,
                };
                _redisService.SetGetTransferOrderOtp(input.OrderId, transfer);
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
            }
            else
            {
                var timespan = DateTime.Now - order.CreateTime;
                if (timespan.TotalSeconds > 120)
                {
                    _redisService.RemoveGetTransferOrderOtp(input.OrderId);
                }
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单未匹配" });
        }

        /// <summary>
        /// 更新otp
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Withdraw/UpdateOrderOtp")]
        public async Task<JsonResult> UpdateOrderOtp(UpdateOrderOtpInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.DeviceId == 0 || input.OrderId.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            var deviceInfo = await _withdrawalDevicesService.GetFirstAsync(r => r.Id == input.DeviceId);
            if (deviceInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备错误" });
            }

            //获取当前出款订单
            var order = _redisService.GetTransferOrder(input.OrderId);
            if (order == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "订单错误" });
            }
            else
            {
                order.Otp = input.Otp;
                order.UpdateTime = DateTime.Now;
                _redisService.SetGetTransferOrderOtp(input.OrderId, order);
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
            }
        }


        /// <summary>
        /// 缓存手机已成功订单
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Withdraw/CreateSuccessOrder")]
        public async Task<JsonResult> CreateSuccessOrder(GetOrderOtpInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.DeviceId == 0 || input.OrderId.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            var deviceInfo = await _withdrawalDevicesService.GetFirstAsync(r => r.Id == input.DeviceId);
            if (deviceInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备错误" });
            }

            _redisService.SetMolibeSuccessTransferOrder(input.DeviceId, input.OrderId);

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 缓存手机已成功订单
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Withdraw/GetSuccessOrder")]
        public async Task<JsonResult> GetSuccessOrder(GetOrderOtpInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.DeviceId == 0 || input.OrderId.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            var deviceInfo = await _withdrawalDevicesService.GetFirstAsync(r => r.Id == input.DeviceId);
            if (deviceInfo == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "设备错误" });
            }

            var result = _redisService.GetMolibeSuccessTransferOrder(input.OrderId);
            if (string.IsNullOrEmpty(result))
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "查无订单" });
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }
    }
}
