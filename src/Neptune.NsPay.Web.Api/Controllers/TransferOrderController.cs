using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.Web.Api.Models;
using Neptune.NsPay.WithdrawalOrders;

namespace Neptune.NsPay.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferOrderController : BaseController
    {
        private readonly IRedisService _redisService;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;
        public TransferOrderController(IRedisService redisService,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            IMerchantFundsMongoService merchantFundsMongoService)
        {
            _redisService = redisService;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _merchantFundsMongoService = merchantFundsMongoService;
        }

        [HttpPost]
        [Route("~/Transfer/GetTransferOrder")]
        public async Task<JsonResult> GetTransferOrder([FromBody] TransferGetOrderRequest transferGetOrderRequest)
        {
            try
            {
                string siteType = AppSettings.Configuration["WebSite:Type"];
                if (siteType != "APITransfer")
                {
                    return toResponseError(StatusCodeType.ParameterError, "请求参数错误!");
                }
                if (string.IsNullOrEmpty(transferGetOrderRequest.MerchNo) || string.IsNullOrEmpty(transferGetOrderRequest.OrderNo))
                {
                    NlogLogger.Error("查询出款订单:" + transferGetOrderRequest.ToJsonString() + "参数错误");
                    return toResponseError(StatusCodeType.ParameterError, "参数错误");
                }
                var withdrawalOrder = await _withdrawalOrdersMongoService.GetWithdrawOrderByOrderNumber(transferGetOrderRequest.MerchNo,transferGetOrderRequest.OrderNo);
                if (withdrawalOrder != null)
                {
                    if (withdrawalOrder.OrderStatus == WithdrawalOrderStatusEnum.Pending)
                    {
                        withdrawalOrder.OrderStatus = WithdrawalOrderStatusEnum.Pending;
                    }
                    else
                    {
                        if (withdrawalOrder.OrderStatus != WithdrawalOrderStatusEnum.Success)
                        {
                            withdrawalOrder.OrderStatus = WithdrawalOrderStatusEnum.Pending;
                        }
                    }
                    TransferGetOrderResult transferGetOrderResult = new TransferGetOrderResult()
                    {
                        OrderNo = withdrawalOrder.OrderNumber,
                        TradeMoney = withdrawalOrder.OrderMoney.ToString(),
                        Status = (int)withdrawalOrder.OrderStatus,
                        TradeNo = withdrawalOrder.WithdrawNo
                    };
                    NlogLogger.Info("查询出款订单:" + transferGetOrderResult.ToJsonString() + "完成");
                    return toResponse<TransferGetOrderResult>(transferGetOrderResult);
                }
                else
                {
                    return toResponseError(StatusCodeType.ParameterError, "查无订单");
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("查询出款订单错误", ex);
                return toResponseError(StatusCodeType.ParameterError, "请求出错");
            }
        }

        [HttpPost]
        [Route("~/Transfer/GetBalance")]
        public async Task<JsonResult> GetBalance([FromBody] MerchantBalanceRequest merchantBalanceRequest)
        {
            try
            {
                string siteType = AppSettings.Configuration["WebSite:Type"];
                if (siteType != "APITransfer")
                {
                    return toResponseError(StatusCodeType.ParameterError, "请求参数错误!");
                }
                if (string.IsNullOrEmpty(merchantBalanceRequest.MerchNo))
                {
                    NlogLogger.Error("查询商户余额:" + merchantBalanceRequest.ToJsonString() + "参数错误");
                    return toResponseError(StatusCodeType.ParameterError, "参数错误");
                }
                var funds = await _merchantFundsMongoService.GetFundsByMerchantCode(merchantBalanceRequest.MerchNo);
                if (funds == null)
                {
                    return toResponseError(StatusCodeType.ParameterError, "商户号错误");
                }
                MerchantBalanceResult merchantBalanceResult = new MerchantBalanceResult()
                {
                    MerchNo = merchantBalanceRequest.MerchNo,
                    Balance = funds == null ? 0 : funds.Balance,
                    Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                return toResponse<MerchantBalanceResult>(merchantBalanceResult);
            }
            catch (Exception ex)
            {
                NlogLogger.Error("查询商户余额错误", ex);
                return toResponseError(StatusCodeType.ParameterError, "请求出错");
            }
        }
    }
}
