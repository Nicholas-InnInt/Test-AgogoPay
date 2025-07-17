using Abp.Extensions;
using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.BankApi.Models;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using System.Globalization;

namespace Neptune.NsPay.BankApi.Controllers
{
    public class BusinessVtbBankController : ControllerBase
    {
        private readonly IRedisService _redisService;
        private readonly IBusinessVtbBankHelper _businessVtbBankHelper;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        public BusinessVtbBankController(
            IRedisService redisService,
            IBusinessVtbBankHelper businessVtbBankHelper,
            IPayOrdersMongoService payOrdersMongoService,
            IPayOrderDepositsMongoService payOrderDepositsMongoService
            )
        {
            _redisService = redisService;
            _businessVtbBankHelper = businessVtbBankHelper;
            _payOrdersMongoService = payOrdersMongoService;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
        }

        [Route("~/BusinessVtbBank/GetTest")]
        [HttpPost]
        public async Task<JsonResult> GetTest([FromBody] BankJobInputRequest input)
        {
            var payMentList = _redisService.GetPayMents().Where(r => r.Type == PayMentTypeEnum.BusinessVtbBank && r.IsDeleted == false && r.Phone == input.Phone);
            foreach (var payment in payMentList)
            {
                if (payment.Phone.IsNullOrEmpty() || payment.CardNumber.IsNullOrEmpty())
                    continue;
                var acbResult = await _businessVtbBankHelper.GetTransactionHistory(input.Phone, payment.CardNumber);
                return new JsonResult(acbResult);
            }
            return new JsonResult(null);
        }


        [Route("~/BusinessVtbBank/GetHistory")]
        [HttpPost]
        public async Task GetHistory([FromBody] BankJobInputRequest input)
        {
            if (input.Phone.IsNullOrEmpty())
                return;
            try
            {
                var payment = _redisService.GetPayMentInfo(input.Phone, input.CardNumber, PayMentTypeEnum.BusinessVtbBank);
                if (payment != null)
                {
                    if (payment.Phone.IsNullOrEmpty() || payment.CardNumber.IsNullOrEmpty())
                        return;

                    if (payment.ShowStatus == PayMentStatusEnum.Hide || payment.IsDeleted == true)
                        return;
                    var isuse = _redisService.GetPayUseMent(payment.Id);
                    if (isuse == null) return;
                    //if (isuse <= 0) return;

                    var hisTransList = await _businessVtbBankHelper.GetTransactionHistory(payment.Phone, payment.CardNumber);

                    if (input.Debbug == 1)
                    {
                        NlogLogger.Fatal("BusinessVtb:" + payment.Phone + ",查询结果：" + hisTransList);
                    }

                    if (hisTransList!=null)
                    {
                        //获取最后一条记录
                        var lastrefno = _businessVtbBankHelper.GetLastRefNoKey(payment.CardNumber);
                        if (lastrefno == null)
                        {
                            lastrefno = "";
                        }
                        lastrefno = lastrefno.Replace("\"", "");

                        var historyItems = new List<BusinessVtbBankTransactionsItem>();
                        var transactionLists = new List<BusinessVtbBankTransactionsItem>();
                        //过滤订单，获取存款订单
                        var histortList = hisTransList;
                        if (input.Debbug == 1)
                        {
                            NlogLogger.Fatal("BusinessVtbBank:" + payment.Phone + ",查询数量：" + histortList.Count + ",最后订单：" + lastrefno);
                        }
                        foreach (var item in histortList)
                        {
                            item.TransactionTime = DateTime.Parse(item.tranDate, new CultureInfo("de-DE"));
                            historyItems.Add(item);

                            if (payment.DispenseType == PayMentDispensEnum.None)
                            {
                                //加入取款缓存
                                if (item.dorc != "C")
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel()
                                    {
                                        Type = PayMentTypeEnum.BusinessVtbBank,
                                        RefNo = item.trxId,
                                        PayMentId = payment.Id,
                                        //Phone = payment.Phone,
                                        //CardNo = payment.CardNumber,
                                        TransferTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(item.tranDate, new CultureInfo("de-DE")), "China Standard Time"),
                                        Remark = item.remark,
                                        Money = item.amount.ParseToDecimal(),
                                    };
                                    _redisService.SetBankOrderNotifyByVtb(bankOrderNotify);
                                }
                            }
                        }
                        foreach (var item in historyItems)
                        {

                            var amount = item.amount.Replace("-", "").Replace("+", "");
                            double outMoney;
                            double.TryParse(amount, out outMoney);
                            var tempMoney = (decimal)outMoney;
                            var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(item.trxId, payment.Id, item.remark);
                            if (info == null)
                            {
                                var corresponsiveAccount = item.corresponsiveAccount;
                                var corresponsiveName = item.corresponsiveName;
                                var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                {
                                    PayType = PayMentTypeEnum.BusinessVtbBank.ToInt(),
                                    Type = item.dorc == "C" ? "CRDT" : "DBIT",
                                    UserName = payment.Phone,
                                    AccountNo = payment.CardNumber,
                                    RefNo = item.trxId,
                                    AvailableBalance = item.balance.ParseToDecimal(),
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(item.tranDate, new CultureInfo("de-DE")), "China Standard Time"),
                                    CreditBank = "",
                                    CreditAcctNo = corresponsiveAccount,
                                    CreditAcctName = corresponsiveName,
                                    CreditAmount = amount.ParseToDecimal(),
                                    DebitBank = "",
                                    DebitAcctNo = corresponsiveAccount,
                                    DebitAcctName = corresponsiveName,
                                    DebitAmount = amount.ParseToDecimal(),
                                    Description = item.remark,
                                    PayMentId = payment.Id
                                };
                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            }

                            var dorc = item.dorc;
                            if (dorc == "C")//存款数据
                            {
                                transactionLists.Add(item);
                            }
                            if (item.trxId == lastrefno) break;
                        }

                        //缓存最新的记录
                        var lastInfo = new BusinessVtbBankTransactionsItem();
                        if (transactionLists.Count > 10)
                        {
                            lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).Skip(10).FirstOrDefault();
                        }
                        else
                        {
                            lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        }
                        if (lastInfo != null)
                        {
                            _businessVtbBankHelper.SetLastRefNoKey(payment.CardNumber, lastInfo.trxId);
                        }

                        if (input.Debbug == 1)
                        {
                            NlogLogger.Fatal("BusinessVtbBank:" + payment.Phone + ",匹配数据：" + transactionLists.Count);
                        }

                        if (transactionLists.Count > 0)
                        {
                            foreach (var item in transactionLists)
                            {
                                var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, payment.Type, item.remark, item.amount.ParseToDecimal());
                                if (payEntity != null)
                                {
                                    //更新订单，同时增加流水表
                                    try
                                    {
                                        var successOrderCache = _redisService.GetSuccessOrder(payEntity.ID);
                                        if (successOrderCache.IsNullOrEmpty())
                                        {
                                            _redisService.SetSuccessOrder(payEntity.ID);

                                            var bankOrderPubModel = new BankOrderPubModel()
                                            {
                                                MerchantCode = input.MerchantCode,
                                                Type = PayMentTypeEnum.BusinessVtbBank,
                                                PayMentId = payment.Id,
                                                PayOrderId = payEntity.ID,
                                                Id = item.trxId,
                                                Money = item.amount.ParseToDecimal(),
                                                Desc = item.remark,
                                            };

                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.BusinessVtbBankOrder, bankOrderPubModel);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        NlogLogger.Warn("BusinessVtbBank定时任务更新订单流水表异常：" + ex.ToString());
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var cacheToken = _businessVtbBankHelper.GetSessionId(payment.CardNumber);
                        if (cacheToken != null)
                        {
                            _businessVtbBankHelper.RemoveToken(payment.CardNumber);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error(input.Phone + ",BusinessVtbBank Bank运行错误：" + ex);
            }
        }

    }
}
