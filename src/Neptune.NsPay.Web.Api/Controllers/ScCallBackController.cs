using Abp.Extensions;
using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.HttpExtensions.PayOnline;
using Neptune.NsPay.HttpExtensions.PayOnline.Models;
using Neptune.NsPay.HttpExtensions.ScratchCard;
using Neptune.NsPay.HttpExtensions.ScratchCard.Models;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.Web.Api.Helpers;
using Neptune.NsPay.Web.Api.Models;

namespace Neptune.NsPay.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScCallBackController : BaseController
    {
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IScDoiTheCaoHelper _scDoiTheCaoHelper;
        private readonly IRedisService _redisService;
        private readonly ICallBackService _callBackService;
        private readonly IPayOnlineHelper _payOnlineHelper;
        private readonly IKafkaProducer _kafkaProducer;

        public ScCallBackController(IPayOrdersMongoService payOrdersMongoService,
            IScDoiTheCaoHelper scDoiTheCaoHelper,
            IRedisService redisService,
            ICallBackService callBackService,
			IPayOnlineHelper payOnlineHelper,
            IKafkaProducer kafkaProducer)
        {
            _payOrdersMongoService = payOrdersMongoService;
            _scDoiTheCaoHelper = scDoiTheCaoHelper;
            _redisService = redisService;
            _callBackService = callBackService;
			_payOnlineHelper = payOnlineHelper;
            _kafkaProducer = kafkaProducer;

        }

        [Route("~/CallBack/Doithecaoonline")]
        [HttpPost]
        public async Task<JsonResult> Doithecaoonline([FromBody] DoithecaoonlineCallbackModel request)
        {
            NlogLogger.Warn("Doithecaoonline call back result：" + request.ToJsonString());
            try
            {
                //检验参数
                if (request == null)
                    return toResponseError(StatusCodeType.ParameterError, "param error");
                if (string.IsNullOrEmpty(request.card_series) || string.IsNullOrEmpty(request.card_transaction_id))
                    return toResponseError(StatusCodeType.ParameterError, "param card_transaction_id or card_series is null");
                if (request.card_status == 2)
                {
                    if (!request.card_transaction_id.IsNullOrEmpty())
                    {
                        if (request.card_transaction_id.Contains('_'))
                        {
                            var arr = request.card_transaction_id.Split('_');
                            var merchantId = Convert.ToInt32(arr[0]);
                            var orderNumber = arr[1];
                            if (arr.Count() > 2)
                            {
                                orderNumber += "_" + arr[2];
                            }

                            var payorder = await _payOrdersMongoService.GetPayOrderByOrderNumber(merchantId, orderNumber);
                            if (payorder != null)
                            {
                                if (payorder.OrderMoney != request.card_real_amount)
                                {
                                    var errorMsg = "Số tiền đơn hàng không khớp";
                                    await _payOrdersMongoService.UpdateOrderStatusByOrderId(payorder.ID, PayOrderOrderStatusEnum.Failed, request.card_real_amount, errorMsg);
                                }
                                else
                                {
                                    var payment = _redisService.GetPayMentInfoById(payorder.PayMentId);
                                    if (payment != null)
                                    {
                                        //手动再次查询
                                        var transactionid = OrderHelper.GetScTransactionid(payorder.MerchantId, payorder.OrderNumber);
                                        ScratchCardRequest scratchCardRequest = new ScratchCardRequest()
                                        {
                                            Amount = payorder.OrderMoney,
                                            Code = payorder.ScCode,
                                            Seri = payorder.ScSeri,
                                            Transactionid = transactionid
                                        };
                                        var response = await _scDoiTheCaoHelper.CheckCard(payment, scratchCardRequest);
                                        if (response.success && response.code == 200 && response.data.FirstOrDefault()?.card_status == 2)
                                        {
                                            //更新订单
                                            await _payOrdersMongoService.UpdateSuccesByOrderId(payorder.ID, payorder.OrderMoney);

                                            //加入mq回调
                                            var checkOrder = _redisService.GetMerchantBillOrder(payorder.MerchantCode,payorder.OrderNumber);
                                            if (checkOrder.IsNullOrEmpty())
                                            {
                                                _redisService.SetMerchantBillOrder(payorder.MerchantCode, payorder.OrderNumber);
                                                //var redisMqDto = new PayMerchantRedisMqDto()
                                                //{
                                                //    PayMqSubType = MQSubscribeStaticConsts.MerchantBillAddBalance,
                                                //    MerchantCode = payorder.MerchantCode,
                                                //    PayOrderId = payorder.ID,
                                                //};
                                                //_redisService.SetMerchantMqPublish(redisMqDto);

                                                await _kafkaProducer.ProduceAsync<PayOrderPublishDto>(KafkaTopics.PayOrder, payorder.ID, new PayOrderPublishDto()
                                                {
                                                    MerchantCode = payorder.MerchantCode,
                                                    PayOrderId = payorder.ID,
                                                    TriggerDate = DateTime.Now,
                                                    ProcessId = Guid.NewGuid().ToString()
                                                });

                                            }
                                            //回调
                                            await _callBackService.CallBackPost(payorder.ID);
                                        }
                                        else
                                        {
                                            string errorMsg = "Code:" + request.card_code + ",Seri:" + request.card_series + ",ErrorMsg:" + response.data.FirstOrDefault()?.card_content;
                                            await _payOrdersMongoService.UpdateOrderStatusByOrderId(payorder.ID, PayOrderOrderStatusEnum.Failed, request.card_real_amount, errorMsg);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (request.card_status == 3)
                {
                    //更新订单
                    if (!request.card_transaction_id.IsNullOrEmpty())
                    {
                        if (request.card_transaction_id.Contains('_'))
                        {
                            var arr = request.card_transaction_id.Split('_');
                            var merchantId = Convert.ToInt32(arr[0]);
                            var orderNumber = arr[1];
                            var payorder = await _payOrdersMongoService.GetPayOrderByOrderNumber(merchantId, orderNumber);
                            if (payorder != null)
                            {
                                string errorMsg = "Code:" + request.card_code + ",Seri:" + request.card_series + ",ErrorMsg:" + request.card_content;
                                await _payOrdersMongoService.UpdateOrderStatusByOrderId(payorder.ID, PayOrderOrderStatusEnum.Failed, request.card_real_amount, errorMsg);
                            }
                        }
                    }
                }
                return toResponseError(StatusCodeType.Success, "");
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Sc异常信息：" + ex.ToString());
                return toResponseError(StatusCodeType.ParameterError, "param error");
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
