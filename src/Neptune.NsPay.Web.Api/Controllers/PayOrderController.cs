using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.Web.Api.Models;

namespace Neptune.NsPay.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayOrderController : BaseController
    {
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        public PayOrderController(IPayOrdersMongoService payOrdersMongoService)
        {
            _payOrdersMongoService = payOrdersMongoService;
        }


        [Route("~/PayOrder/GetOrder")]
        [HttpPost]
        public async Task<JsonResult> GetOrder([FromBody] PayGetOrderRequest payGetOrderRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(payGetOrderRequest.MerchNo) || string.IsNullOrEmpty(payGetOrderRequest.OrderNo))
                {
                    NlogLogger.Error("查询订单:" + payGetOrderRequest.ToJsonString() + "参数错误");
                    return toResponseError(StatusCodeType.ParameterError, "参数错误");
                }
                var payorder = await _payOrdersMongoService.GetPayOrderByOrderNumber(payGetOrderRequest.MerchNo, payGetOrderRequest.OrderNo);
                if (payorder != null)
                {
                    int status = 0;
                    if (payorder.OrderStatus == PayOrderOrderStatusEnum.Completed)
                    {
                        status = 1;
                    }
                    PayGetOrderResult payGetOrderResult = new PayGetOrderResult()
                    {
                        OrderNo = payorder.OrderNumber,
                        Money = payorder.OrderMoney.ToString(),
                        TradeMoney = payorder.TradeMoney.ToString(),
                        Stauts = status
                    };
                    return toResponse<PayGetOrderResult>(payGetOrderResult);
                }
                else
                {
                    return toResponseError(StatusCodeType.ParameterError, "查无订单");
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("查询订单错误", ex);
                return toResponseError(StatusCodeType.ParameterError, "请求出错");
            }
        }


    }
}
