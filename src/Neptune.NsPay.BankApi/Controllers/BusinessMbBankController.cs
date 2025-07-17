using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.BankApi.Models;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.Commons;
using Abp.Json;
using System.Globalization;
using Neptune.NsPay.MongoDbExtensions.Models;
using Abp.Extensions;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.HttpExtensions.Bank.Helpers;

namespace Neptune.NsPay.BankApi.Controllers
{
    [ApiController]
    public class BusinessMbBankController : ControllerBase
    {
        private readonly IRedisService _redisService;
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IBusinessMBBankHelper _businessMBBankHelper;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;

        public BusinessMbBankController(
            IRedisService redisService,
            IPayGroupMentService payGroupMentService,
            IBusinessMBBankHelper businessMBBankHelper,
            IPayOrdersMongoService payOrdersMongoService,
            IPayOrderDepositsMongoService payOrderDepositsMongoService
            )
        {
            _redisService = redisService;
            _payGroupMentService = payGroupMentService;
            _businessMBBankHelper = businessMBBankHelper;
            _payOrdersMongoService = payOrdersMongoService;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
        }

        [Route("~/BusinessMbBank/GetTest")]
        [HttpPost]
        public async Task<JsonResult> GetTest([FromBody] BankJobInputRequest input)
        {
            var payMentList = _redisService.GetPayMents().Where(r => r.Type == PayMentTypeEnum.BusinessMbBank && r.IsDeleted == false && r.Phone == input.Phone);
            foreach (var payment in payMentList)
            {
                if (payment.Phone.IsNullOrEmpty() || payment.CardNumber.IsNullOrEmpty())
                    continue;
                var acbResult = await _businessMBBankHelper.GetTransactionHistory(input.Phone, payment.CardNumber, payment.FullName);
                return new JsonResult(acbResult);
            }
            return new JsonResult(null);
        }


        [Route("~/BusinessMbBank/GetHistory")]
        [HttpPost]
        public async Task GetHistory([FromBody] BankJobInputRequest input)
        {
            if (input.Phone.IsNullOrEmpty())
                return;
            try
            {
                var payment = _redisService.GetPayMentInfo(input.Phone, input.CardNumber, PayMentTypeEnum.BusinessMbBank);
                if (payment != null)
                {
                    if (payment.Phone.IsNullOrEmpty() || payment.CardNumber.IsNullOrEmpty())
                        return;

                    if (payment.ShowStatus == PayMentStatusEnum.Hide || payment.IsDeleted == true)
                        return;
                    var isuse = _redisService.GetPayUseMent(payment.Id);
                    if (isuse == null) return;
                    //if (isuse <= 0) return;

                    //var payInfo = await _payGroupMentService.GetFirstAsync(r => r.PayMentId == payment.Id && r.IsDeleted == false);
                    //if (payInfo != null)
                    //{
                    //    if (payInfo.Status == false)
                    //    {
                    //        return;
                    //    }
                    //}

                    if (input.Debbug == 1)
                    {
                        NlogLogger.Fatal("Business MB Bank:" + payment.Phone + ",进入查询");
                    }
                    var mbResult = await _businessMBBankHelper.GetTransactionHistory(input.Phone, payment.CardNumber, payment.FullName);
                    if (mbResult != null)
                    {
                        if (mbResult.Count > 0)
                        {
                            //获取最后一条记录
                            var lastrefno = _businessMBBankHelper.GetLastRefNoKey(payment.CardNumber);
                            if (lastrefno == null)
                            {
                                lastrefno = "";
                            }
                            lastrefno = lastrefno.Replace("\"", "");
                            if (input.Debbug == 1)
                            {
                                NlogLogger.Fatal("Business MB Bank:" + payment.Phone + ",lastrefno:" + lastrefno);
                            }

                            var historyItems = new List<BusinessMBBankTransactionHistoryList>();
                            var transactionLists = new List<BusinessMBBankTransactionHistoryList>();
                            foreach (var item in mbResult)
                            {
                                if (item.transactionRefNo == lastrefno) break;

                                var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(item.transactionRefNo, payment.Id, item.description);
                                if (info == null)
                                {
                                    var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                    {
                                        PayType = PayMentTypeEnum.BusinessMbBank.ToInt(),
                                        Type = item.creditAmount > 0 ? "CRDT" : "DBIT",
                                        RefNo = item.transactionRefNo,
                                        AccountNo = item.accountNo,
                                        CreditBank = "",
                                        CreditAcctNo = "",
                                        CreditAcctName = "",
                                        CreditAmount = item.creditAmount,
                                        DebitBank = "",
                                        DebitAcctNo = "",
                                        DebitAcctName = "",
                                        DebitAmount = item.debitAmount,
                                        Description = item.description,
                                        AvailableBalance = item.availableBalance,
                                        PayMentId = payment.Id,
                                        UserName = payment.Phone,
                                        TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(item.transactionDate, new CultureInfo("de-DE")), "China Standard Time")
                                    };
                                    await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                }
                                item.TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(item.transactionDate, new CultureInfo("de-DE")), "China Standard Time");
                                historyItems.Add(item);
                            }


                            foreach (var item in historyItems)
                            {
                                if (item.creditAmount > 0)
                                {
                                    transactionLists.Add(item);
                                }
                                if (item.transactionRefNo == lastrefno) break;
                            }
                            if (input.Debbug == 1)
                            {
                                NlogLogger.Fatal("Business MB Bank:" + payment.Phone + ",获取等待匹配数据：" + transactionLists.Count);
                            }

                            //缓存最新记录
                            var lastInfo = new BusinessMBBankTransactionHistoryList();
                            if (transactionLists.Count > 10)
                            {
                                lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).Skip(10).FirstOrDefault();
                            }

                            if (lastInfo != null)
                            {
                                _businessMBBankHelper.SetLastRefNoKey(payment.CardNumber, lastInfo.transactionRefNo);
                            }
                            if (transactionLists.Count > 0)
                            {
                                if (input.Debbug == 1)
                                {
                                    NlogLogger.Fatal("Business MB Bank:" + payment.Phone + ",进入匹配");
                                }
                                foreach (var item in transactionLists)
                                {
                                    if (input.Debbug == 1)
                                    {
                                        NlogLogger.Fatal("Business MB Bank:" + payment.Phone + ",订单：" + item.ToJsonString());
                                    }
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, payment.Type, item.description, item.creditAmount.ParseToDecimal());

                                    if (payEntity != null)
                                    {
                                        if (input.Debbug == 1)
                                        {
                                            NlogLogger.Fatal("Business MB Bank:" + payment.Phone + ",匹配订单：" + payEntity.ToJsonString());
                                        }
                                        try
                                        {
                                            var successOrderCache = _redisService.GetSuccessOrder(payEntity.ID);
                                            if (successOrderCache.IsNullOrEmpty())
                                            {
                                                _redisService.SetSuccessOrder(payEntity.ID);

                                                if (input.Debbug == 1)
                                                {
                                                    NlogLogger.Fatal("Techcom:" + payment.Phone + ",加入订阅：" + payEntity.ToJsonString());
                                                }
                                                var bankOrderPubModel = new BankOrderPubModel()
                                                {
                                                    MerchantCode = input.MerchantCode,
                                                    Type = PayMentTypeEnum.BusinessMbBank,
                                                    PayMentId = payment.Id,
                                                    PayOrderId = payEntity.ID,
                                                    Id = item.transactionRefNo,
                                                    Money = Convert.ToDecimal(item.creditAmount),
                                                    Desc = item.description,
                                                };
                                                _redisService.AddOrderQueueList(NsPayRedisKeyConst.BusinessMbBankOrder, bankOrderPubModel);

                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            NlogLogger.Warn("Business MB Bank定时任务更新订单流水表异常：" + ex.ToString());
                                        }
                                    }
                                    else
                                    {
                                        if (input.Debbug == 1)
                                        {
                                            NlogLogger.Fatal("Business MB Bank:" + payment.Phone + ",没有匹配订单，" + payEntity.ToJsonString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var cacheToken = _businessMBBankHelper.GetSessionId(payment.CardNumber);
                        if (cacheToken != null)
                        {
                            _businessMBBankHelper.RemoveToken(payment.CardNumber);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户：" + input.Phone + "Business MB Bank运行错误：" + ex);
            }
            await Task.CompletedTask;
        }
    }

}
