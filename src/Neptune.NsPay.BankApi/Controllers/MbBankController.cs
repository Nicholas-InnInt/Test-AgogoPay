using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.BankApi.Models;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using System.Globalization;
using Neptune.NsPay.Commons;
using Abp.Json;
using Abp.Extensions;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.HttpExtensions.Bank.Helpers;

namespace Neptune.NsPay.BankApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MbBankController : ControllerBase
    {
        private readonly IRedisService _redisService;
        private readonly IMBBankHelper _mbBankHelper;
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;

        public MbBankController(
            IRedisService redisService,
            IMBBankHelper mbBankHelper,
            IPayGroupMentService payGroupMentService,
            IPayOrdersMongoService payOrdersMongoService,
            IPayOrderDepositsMongoService payOrderDepositsMongoService
            )
        {
            _redisService = redisService;
            _mbBankHelper = mbBankHelper;
            _payGroupMentService = payGroupMentService;
            _payOrdersMongoService = payOrdersMongoService;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
        }

        [Route("~/MbBank/GetTest")]
        [HttpPost]
        public async Task<JsonResult> GetTest([FromBody] BankJobInputRequest input)
        {
            var payMentList = _redisService.GetPayMents().Where(r => r.Type == PayMentTypeEnum.MBBank && r.IsDeleted == false && r.Phone == input.Phone);
            foreach (var payment in payMentList)
            {
                if (payment.Phone.IsNullOrEmpty() || payment.CardNumber.IsNullOrEmpty())
                    continue;
                var acbResult = await _mbBankHelper.GetTransactionHistory(payment.Phone, payment.CardNumber);
                return new JsonResult(acbResult);
            }
            return new JsonResult(null);
        }

        [Route("~/MbBank/GetHistory")]
        [HttpPost]
        public async Task GetHistory([FromBody] BankJobInputRequest input)
        {
            if (input.Phone.IsNullOrEmpty())
                return;
            try
            {
                var payment = _redisService.GetPayMentInfo(input.Phone.Trim(), input.CardNumber, PayMentTypeEnum.MBBank);
                if (payment != null)
                {
                    if (payment.Phone.IsNullOrEmpty() || payment.CardNumber.IsNullOrEmpty())
                    {
                        return;
                    }

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
                        NlogLogger.Fatal("MB:" + payment.Phone + ",进入查询");
                    }

                    var mbResult = await _mbBankHelper.GetTransactionHistory(payment.Phone, payment.CardNumber);
                    if (mbResult !=null && mbResult.Count > 0)
                    {
                        //获取最后一条记录
                        var lastrefno = _mbBankHelper.GetLastRefNoKey(payment.CardNumber);
                        if (lastrefno == null)
                        {
                            lastrefno = "";
                        }
                        lastrefno = lastrefno.Replace("\"", "");
                        if (input.Debbug == 1)
                        {
                            NlogLogger.Fatal("MB:" + payment.Phone + ",获取数据：" + mbResult.Count() + ",lastrefno:" + lastrefno);
                        }

                        var historyItems = new List<TransactionHistoryDetail>();
                        var transactionLists = new List<TransactionHistoryDetail>();
                        //过滤订单，获取存款订单
                        foreach (var item in mbResult)
                        {
                            var transactionHistoryDetail = new TransactionHistoryDetail
                            {
                                transactionDate = item.transactionDate,
                                accountNo = item.accountNo,
                                creditAmount = item.creditAmount,
                                debitAmount = item.debitAmount,
                                description = item.description, 
                                availableBalance = item.availableBalance,
                                refNo = item.refNo,
                                benAccountName = item.benAccountName,
                                bankName = item.bankName,
                                benAccountNo = item.benAccountNo,
                                TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(item.transactionDate, new CultureInfo("de-DE")), "China Standard Time")
                            };
                            historyItems.Add(transactionHistoryDetail);
                        }

                        historyItems = historyItems.OrderByDescending(r => r.TransactionTime).Take(350).ToList();
                        foreach (var item in historyItems)
                        {
                            var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(item.refNo, payment.Id,item.description);
                            if (input.Debbug == 1)
                            {
                                if (info == null)
                                {
                                    NlogLogger.Fatal("MB:" + payment.Phone + ",银行订单：" + item.refNo + "，等待添加");
                                }
                                else
                                {
                                    NlogLogger.Fatal("MB:" + payment.Phone + ",银行订单：" + item.refNo + "，已经添加");
                                }
                            }
                            if (info == null)
                            {
                                if (input.Debbug == 1)
                                {
                                    NlogLogger.Fatal("MB:" + payment.Phone + ",添加订单：" + item.refNo);
                                }
                                var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                {
                                    PayType = PayMentTypeEnum.MBBank.ToInt(),
                                    RefNo = item.refNo,
                                    Type = item.creditAmount.ParseToDecimal() > 0 ? "CRDT" : "DBIT",
                                    AccountNo = item.accountNo,
                                    UserName = payment.Phone,
                                    AvailableBalance = item.availableBalance.ParseToDecimal(),
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(item.transactionDate, new CultureInfo("de-DE")), "China Standard Time"),
                                    Description = item.description,
                                    CreditBank = item.bankName,
                                    CreditAcctNo = item.benAccountNo,
                                    CreditAcctName = item.benAccountName,
                                    CreditAmount = item.creditAmount.ParseToDecimal(),
                                    DebitBank = item.bankName,
                                    DebitAcctNo = item.benAccountNo,
                                    DebitAcctName = item.benAccountName,
                                    DebitAmount = item.debitAmount.ParseToDecimal(),
                                    PayMentId = payment.Id,
                                };
                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            }
                            var creditAmount = item.creditAmount.ParseToDecimal();
                            if (creditAmount > 0)
                            {
                                transactionLists.Add(item);
                            }

                            if (payment.DispenseType == PayMentDispensEnum.None)
                            {
                                //加入取款缓存
                                if (item.debitAmount.ParseToDecimal() > 0)
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel()
                                    {
                                        Type = PayMentTypeEnum.MBBank,
                                        RefNo = item.refNo,
                                        PayMentId = payment.Id,
                                        //Phone = payment.Phone,
                                        //CardNo = payment.CardNumber,
                                        TransferTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(item.transactionDate, new CultureInfo("de-DE")), "China Standard Time"),
                                        Remark = item.description,
                                        Money = item.debitAmount.ParseToDecimal(),
                                    };
                                    //_redisService.AddTransferOrderQueueList(bankOrderNotify);
                                }
                            }
                            if (item.refNo == lastrefno) break;
                        }

                        if (input.Debbug == 1)
                        {
                            NlogLogger.Fatal("MB:" + payment.Phone + ",获取等待匹配数据：" + transactionLists.Count);
                        }
                        //缓存最新的记录
                        TransactionHistoryDetail? lastInfo = null;
                        if (transactionLists.Count > 20)
                        {
                            lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).Skip(20).FirstOrDefault();
                        }
                        //else
                        //{
                        //    lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        //}

                        if (lastInfo != null)
                        {
                            _mbBankHelper.SetLastRefNoKey(payment.CardNumber, lastInfo.refNo);
                        }

                        if (transactionLists.Count > 0)
                        {
                            foreach (var item in transactionLists)
                            {
                                if (input.Debbug == 1)
                                {
                                    NlogLogger.Fatal("MB:" + payment.Phone + ",进入匹配");
                                }
                                var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, payment.Type, item.description, item.creditAmount.ParseToDecimal());
                                if (payEntity != null)
                                {
                                    if (input.Debbug == 1)
                                    {
                                        NlogLogger.Fatal("MB:" + payment.Phone + ",匹配订单：" + payEntity.ToJsonString());
                                    }
                                    try
                                    {
                                        var successOrderCache = _redisService.GetSuccessOrder(payEntity.ID);
                                        if (successOrderCache.IsNullOrEmpty())
                                        {
                                            _redisService.SetSuccessOrder(payEntity.ID);

                                            if (input.Debbug == 1)
                                            {
                                                NlogLogger.Fatal("MB:" + payment.Phone + ",加入订阅：" + payEntity.ToJsonString());
                                            }

                                            var bankOrderPubModel = new BankOrderPubModel()
                                            {
                                                MerchantCode = input.MerchantCode,
                                                Type = PayMentTypeEnum.MBBank,
                                                PayMentId = payment.Id,
                                                PayOrderId = payEntity.ID,
                                                Id = item.refNo,
                                                Money = Convert.ToDecimal(item.creditAmount),
                                                Desc = item.description,
                                            };
                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.MBBankOrder, bankOrderPubModel);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        NlogLogger.Warn("MB Bank定时任务更新订单流水表异常：" + ex.ToString());
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        var cacheToken = _mbBankHelper.GetSessionId(payment.CardNumber);
                        if (cacheToken != null)
                        {
                            if (cacheToken.ErrorCount > 2)
                            {
                                _mbBankHelper.RemoveSessionId(payment.CardNumber);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户：" + input.Phone + ",MB Bank运行错误：" + ex);
            }
            await Task.CompletedTask;
        }
    }

}
