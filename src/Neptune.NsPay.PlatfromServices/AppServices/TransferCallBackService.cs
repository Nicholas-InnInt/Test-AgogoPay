using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.WithdrawalOrders;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.PlatfromServices.AppServices
{
    public class TransferCallBackService: ITransferCallBackService
    {
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IRedisService _redisService;

        public TransferCallBackService(IWithdrawalOrdersMongoService withdrawalOrdersMongoService, IRedisService redisService)
        {
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _redisService = redisService;
        }

        public async Task<bool> TransferCallBackPost(string orderId, string remark = "")
        {
            var order = await _withdrawalOrdersMongoService.GetById(orderId);
            if (order != null)
            {
                if (order.NotifyStatus == WithdrawalNotifyStatusEnum.Success || order.NotifyStatus == WithdrawalNotifyStatusEnum.Fail)
                {
                    return true;
                }
                NlogLogger.Fatal("订单：" + order.ToJsonString());
                var flag = false;

                if (order.OrderStatus >= WithdrawalOrderStatusEnum.Success && order.OrderStatus != WithdrawalOrderStatusEnum.PendingProcess)
                {
                    flag = true;
                }

                if (flag)
                {
                var notifyurl = order.NotifyUrl;
                if (!string.IsNullOrEmpty(notifyurl))
                {
                    var status = order.OrderStatus;
                    if (order.OrderStatus != WithdrawalOrderStatusEnum.Success)
                    {
                        status = WithdrawalOrderStatusEnum.Fail;
                    }
                    var merchant = _redisService.GetMerchantKeyValue(order.MerchantCode);
                    IDictionary<string, string> parameters = new Dictionary<string, string>
                        {
                            { "MerchNo".ToLower(), order.MerchantCode.ToLower() },
                            { "OrderNo".ToLower(), order.OrderNumber.ToLower() },
                            { "Money".ToLower(), order.OrderMoney.ToString("F0").ToLower() },
                            { "BankAccNo".ToLower(), order.BenAccountNo.ToString().ToLower() },
                            { "BankAccName".ToLower(), order.BenAccountName.ToString().ToLower() },
                            { "BankName".ToLower(), order.BenBankName.ToString().ToLower() },
                            { "Status".ToLower(), ((int)status).ToString().ToLower() }
                        };
                        var paradic = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
                        string sign = SignHelper.GetSignContent(paradic).ToLower() + "&secret=" + merchant.MerchantSecret.ToLower();
                        NlogLogger.Info("出款加密前参数：" + sign);
                        sign = MD5Helper.MD5Encrypt32(sign).ToLower();
                        var param = new
                        {
                            MerchNo = merchant.MerchantCode,
                            OrderNo = order.OrderNumber,
                            Money = order.OrderMoney.ToString("F0"),
                            BankAccNo = order.BenAccountNo,
                            BankAccName = order.BenAccountName,
                            BankName = order.BenBankName,
                            Status = status,
                            Sign = sign
                        };
                        try
                        {
                            NlogLogger.Info("出款回调接口：" + notifyurl + ",回调参数：" + param.ToJsonString());
                            var client = new RestClient(notifyurl);
                            var request = new RestRequest()
                                .AddParameter("application/json", param.ToJsonString(), ParameterType.RequestBody);
                            var response = await client.ExecutePostAsync(request);
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                NlogLogger.Info("出款平台返回：" + response.Content);
                                if (!string.IsNullOrEmpty(response.Content))
                                {
                                    if (response.Content.Trim().ToLower() == "success")
                                    {
                                        //更新订单表，修改订单上分状态
                                        order.NotifyNumber = order.NotifyNumber + 1;
                                        order.NotifyStatus = WithdrawalNotifyStatusEnum.Success;
                                        if (!string.IsNullOrEmpty(remark))
                                        {
                                            order.Remark = remark;
                                        }
                                        var result = await _withdrawalOrdersMongoService.UpdateNotifyStatus(order.ID, order.NotifyNumber);
                                        if (!result)
                                        {
                                            for (int i = 0; i < 3; i++)
                                            {
                                                var tempresult = await _withdrawalOrdersMongoService.UpdateNotifyStatus(order.ID, order.NotifyNumber);
                                                if (tempresult)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        order.NotifyNumber = order.NotifyNumber + 1;
                                        if (order.NotifyNumber >= 8)
                                        {
                                            order.NotifyStatus = WithdrawalNotifyStatusEnum.Fail;
                                        }
                                        await _withdrawalOrdersMongoService.UpdateNotifyStatus(order.ID, order.NotifyNumber, order.NotifyStatus);
                                    }
                                }
                            }
                            else
                            {
                                order.NotifyNumber = order.NotifyNumber + 1;
                                if (order.NotifyNumber >= 8)
                                {
                                    order.NotifyStatus = WithdrawalNotifyStatusEnum.Fail;
                                }
                                await _withdrawalOrdersMongoService.UpdateNotifyStatus(order.ID, order.NotifyNumber, order.NotifyStatus);
                            }
                        }
                        catch (Exception ex)
                        {
                            NlogLogger.Error("回调异常：" + ex);
                            order.NotifyNumber = order.NotifyNumber + 1;
                            if (order.NotifyNumber >= 8)
                            {
                                order.NotifyStatus = WithdrawalNotifyStatusEnum.Fail;
                            }
                            await _withdrawalOrdersMongoService.UpdateNotifyStatus(order.ID, order.NotifyNumber, order.NotifyStatus);
                        }
                    }
                }
            }
            return false;
        }
    }
}
