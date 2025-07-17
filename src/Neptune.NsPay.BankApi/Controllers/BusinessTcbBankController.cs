using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.BankApi.Models;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Models;
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
    public class BusinessTcbBankController : ControllerBase
    {
        private readonly IRedisService _redisService;
        private readonly ITechcomBankHelper _techcomBankHelper;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;

        public BusinessTcbBankController(
            IRedisService redisService, 
            ITechcomBankHelper techcomBankHelper,
            IPayOrdersMongoService payOrdersMongoService,
            IPayOrderDepositsMongoService payOrderDepositsMongoService
            )
        {
            _redisService = redisService;
            _techcomBankHelper = techcomBankHelper;
            _payOrdersMongoService = payOrdersMongoService;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
        }

        [Route("~/BusinessTcbBank/GetTest")]
        [HttpPost]
        public async Task<JsonResult> GetTest([FromBody] BankJobInputRequest input)
        {
            var payMentList = _redisService.GetPayMents().Where(r => r.Type == PayMentTypeEnum.BusinessTcbBank && r.IsDeleted == false && r.Phone == input.Phone);
            foreach (var payment in payMentList)
            {
                if (payment.Phone.IsNullOrEmpty() || payment.CardNumber.IsNullOrEmpty())
                    continue;
                var acbResult = await _techcomBankHelper.GetTransactionHistory(input.Phone);
                return new JsonResult(acbResult);
            }
            return new JsonResult(null);
        }


        [Route("~/BusinessTcbBank/GetHistory")]
        [HttpPost]
        public async Task GetHistory([FromBody] BankJobInputRequest input)
        {
            if (input.Phone.IsNullOrEmpty())
                return;
            try
            {
                var payment = _redisService.GetPayMentInfo(input.Phone, input.CardNumber, PayMentTypeEnum.BusinessTcbBank);
                if (payment != null)
                {
                    if (payment.Phone.IsNullOrEmpty() || payment.CardNumber.IsNullOrEmpty())
                        return;

                    if (payment.ShowStatus == PayMentStatusEnum.Hide || payment.IsDeleted == true)
                        return;
                    var isuse = _redisService.GetPayUseMent(payment.Id);
                    if (isuse == null) return;
                    //if (isuse <= 0) return;

                    if (input.Debbug == 1)
                    {
                        NlogLogger.Fatal("Techcom:" + payment.Phone + ",进入查询");
                    }

                    //检查卡号是否在收款
                    //var payInfo = await _payGroupMentService.GetFirstAsync(r => r.PayMentId == payment.Id && r.IsDeleted == false);
                    //if(payInfo != null)
                    //{
                    //    if (payInfo.Status == false)
                    //    {
                    //        return;
                    //    }
                    //}

                    if (input.Debbug == 1)
                    {
                        NlogLogger.Fatal("Techcom:" + payment.Phone + ",登录验证通过");
                    }

                    var techcomResult = await _techcomBankHelper.GetBusinessTransactionHistory(payment.CardNumber, input.Phone);
                    if (input.Debbug == 1)
                    {
                        NlogLogger.Fatal("Techcom:" + payment.Phone + ",获取数据：" + techcomResult.Count);
                    }
                    if (techcomResult != null)
                    {
                        //获取最后一条记录
                        var lastrefno = _techcomBankHelper.GetBusinessLastRefNoKey(payment.CardNumber);
                        if (lastrefno == null)
                        {
                            lastrefno = "";
                        }
                        lastrefno = lastrefno.Replace("\"", "");
                        if (input.Debbug == 1)
                        {
                            NlogLogger.Fatal("Techcom:" + payment.Phone + ",lastrefno:" + lastrefno);
                        }

                        var historyItems = new List<TechcomBankTransactionHistoryResponse>();
                        var transactionLists = new List<TechcomBankTransactionHistoryResponse>();
                        foreach (var item in techcomResult)
                        {
                            if (item.id == lastrefno) break;

                            //加入数据库
                            var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(item.reference, payment.Id, item.description);
                            if (info == null)
                            {
                                if (item.type == "Credit")
                                {
                                    item.type = "CRDT";
                                }
                                if (item.type == "Debit")
                                {
                                    item.type = "DBIT";
                                }
                                var amount = item.transactionAmountCurrency.amount.ParseToDecimal();
                                var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                {
                                    PayType = PayMentTypeEnum.BusinessTcbBank.ToInt(),
                                    UserName = payment.Phone,
                                    AccountNo = payment.CardNumber,
                                    RefNo = item.reference,
                                    Description = item.description,
                                    Type = item.type,
                                    AvailableBalance = item.runningBalance.ParseToDecimal(),
                                    CreditBank = item.additions.creditBank,
                                    CreditAcctNo = item.additions.creditAcctNo,
                                    CreditAcctName = item.additions.creditAcctName,
                                    CreditAmount = amount,
                                    DebitBank = item.additions.debitBank,
                                    DebitAcctNo = item.additions.debitAcctNo,
                                    DebitAcctName = item.additions.debitAcctName,
                                    DebitAmount = amount,
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(item.creationTime.ParseToDateTime(), "China Standard Time"),
                                    PayMentId = payment.Id
                                };
                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            }
                            item.TransactionTime = Convert.ToDateTime(item.creationTime);
                            historyItems.Add(item);

                            if (payment.Type == PayMentTypeEnum.BusinessTcbBank)
                            {
                                if (payment.DispenseType == PayMentDispensEnum.None)
                                {
                                    //加入取款缓存
                                    if (item.type != "CRDT")
                                    {
                                        var bankOrderNotify = new BankOrderNotifyModel()
                                        {
                                            Type = PayMentTypeEnum.BusinessTcbBank,
                                            RefNo = item.id,
                                            PayMentId = payment.Id,
                                            //Phone = payment.Phone,
                                            //CardNo = payment.CardNumber,
                                            TransferTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(item.creationTime.ParseToDateTime(), "China Standard Time"),
                                            Remark = item.description,
                                            Money = item.transactionAmountCurrency.amount.ParseToDecimal(),
                                        };
                                        _redisService.SetBankOrderNotifyByTcb(bankOrderNotify);
                                    }
                                }
                            }
                        }
                        foreach (var item in historyItems)
                        {
                            if (item.type == "Credit" || item.type == "CRDT")
                            {
                                transactionLists.Add(item);
                            }

                            if (item.id == lastrefno) break;
                        }


                        if (input.Debbug == 1)
                        {
                            NlogLogger.Fatal("Techcom:" + payment.Phone + ",获取等待匹配数据：" + transactionLists.Count);
                        }
                        //缓存最新记录
                        TechcomBankTransactionHistoryResponse? lastInfo = null;
                        if (transactionLists.Count > 20)
                        {
                            lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).Skip(20).FirstOrDefault();
                        }
                        else
                        {

                            lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        }

                        if (lastInfo != null)
                        {
                            _techcomBankHelper.SetBusinessLastRefNoKey(payment.CardNumber, lastInfo.id);
                        }

                        if (transactionLists.Count > 0)
                        {
                            if (input.Debbug == 1)
                            {
                                NlogLogger.Fatal("Techcom:" + payment.Phone + ",进入匹配");
                            }
                            foreach (var item in transactionLists)
                            {
                                if (input.Debbug == 1)
                                {
                                    NlogLogger.Fatal("Techcom:" + payment.Phone + ",订单：" + item.ToJsonString());
                                }
                                var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, payment.Type, item.description, item.transactionAmountCurrency.amount.ParseToDecimal());

                                if (payEntity != null)
                                {
                                    if (input.Debbug == 1)
                                    {
                                        NlogLogger.Fatal("Techcom:" + payment.Phone + ",匹配订单：" + payEntity.ToJsonString());
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
                                                Type = PayMentTypeEnum.BusinessTcbBank,
                                                PayMentId = payment.Id,
                                                PayOrderId = payEntity.ID,
                                                Id = item.reference,
                                                Money = item.transactionAmountCurrency.amount.ParseToDecimal(),
                                                Desc = item.description,
                                            };
                                            bankOrderPubModel.Money = decimal.Parse(bankOrderPubModel.Money.ToString("F2"));
                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.BusinessTcbBankOrder, bankOrderPubModel);

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        NlogLogger.Warn("Techcom Bank定时任务更新订单流水表异常：" + ex.ToString());
                                    }
                                }
                                else
                                {
                                    if (input.Debbug == 1)
                                    {
                                        NlogLogger.Fatal("Techcom:" + payment.Phone + ",没有匹配订单，" + payEntity.ToJsonString());
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var cacheToken = _techcomBankHelper.GetBusinessToken(payment.CardNumber);
                        if (cacheToken != null)
                        {
                            _techcomBankHelper.RemoveBusinessToken(payment.CardNumber);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户：" + input.Phone + "Techcom Bank运行错误：" + ex);
            }

            await Task.CompletedTask;
        }
    }

}
