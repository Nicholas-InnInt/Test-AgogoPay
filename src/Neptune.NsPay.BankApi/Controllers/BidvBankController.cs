using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.BankApi.Models;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Abp.Json;
using Abp.Extensions;
using Abp.Domain.Repositories;
using Neptune.NsPay.NsPayBackgroundJobs;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;

namespace Neptune.NsPay.BankApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidvBankController : ControllerBase
    {
        private readonly IRedisService _redisService;
        private readonly IBidvBankHelper _bidvBankHelper;
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        private readonly INsPayBackgroundJobsService _nsPayBackgroundJobsService;
        public BidvBankController(
            IRedisService redisService,
            IBidvBankHelper bidvBankHelper,
            IPayGroupMentService payGroupMentService,
            IPayOrdersMongoService payOrdersMongoService,
            IPayOrderDepositsMongoService payOrderDepositsMongoService,
            INsPayBackgroundJobsService nsPayBackgroundJobsService
            )
        {
            _redisService = redisService;
            _bidvBankHelper = bidvBankHelper;
            _payGroupMentService = payGroupMentService;
            _payOrdersMongoService = payOrdersMongoService;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
            _nsPayBackgroundJobsService = nsPayBackgroundJobsService;
        }

        [Route("~/BidvBank/GetTest")]
        [HttpPost]
        public async Task<JsonResult> GetTest([FromBody] BankJobInputRequest input)
        {
            var payMentList = _redisService.GetPayMents().Where(r => r.Type == PayMentTypeEnum.BidvBank && r.IsDeleted == false && r.Phone == input.Phone);
            foreach (var payment in payMentList)
            {
                if (payment.Phone.IsNullOrEmpty() || payment.CardNumber.IsNullOrEmpty())
                    continue;
                var acbResult = await _bidvBankHelper.GetHistoryAsync(input.Phone, payment.CardNumber, input.BankApi);
                return new JsonResult(acbResult);
            }
            return new JsonResult(null);
        }

        [Route("~/BidvBank/GetHistory")]
        [HttpPost]
        public async Task GetHistory([FromBody] BankJobInputRequest input)
        {
            if (input.BankApi.IsNullOrEmpty() || input.Phone.IsNullOrEmpty())
                return;
            var bankApi = input.BankApi;
            try
            {
                var payment = _redisService.GetPayMentInfo(input.Phone, input.CardNumber, PayMentTypeEnum.BidvBank);
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

                    var bidvResult = await _bidvBankHelper.GetHistoryAsync(input.Phone, payment.CardNumber, bankApi);
                    if (bidvResult != null)
                    {
                        if (bidvResult.code == 0)
                        {
                            //获取最后一条记录
                            var lastrefno = _bidvBankHelper.GetLastRefNoKey(payment.CardNumber);
                            if (lastrefno == null)
                            {
                                lastrefno = "";
                            }
                            lastrefno = lastrefno.Replace("\"", "");

                            var historyItems = new List<BidvBankTransactionHistoryItemTime>();
                            var transactionLists = new List<BidvBankTransactionHistoryItemTime>();
                            foreach (var item in bidvResult.data)
                            {
                                if (item.refNo == lastrefno) break;
  
                                var itemTime = new BidvBankTransactionHistoryItemTime()
                                {
                                    txnDate = item.txnDate,
                                    txnTime = item.txnTime,
                                    txnRemark = item.txnRemark,
                                    txnType = item.txnType,
                                    amount = item.amount,
                                    refNo = item.refNo,
                                    balance = item.balance,
                                    TransactionTime = GetTransactionTime(item.txnDate, item.txnTime),
                                };
                                historyItems.Add(itemTime);

                                if (payment.DispenseType == PayMentDispensEnum.None)
                                {
                                    //加入取款缓存
                                    if (item.txnType != "+")
                                    {
                                        var bankOrderNotify = new BankOrderNotifyModel()
                                        {
                                            Type = PayMentTypeEnum.BidvBank,
                                            RefNo = item.refNo,
                                            PayMentId = payment.Id,
                                            //Phone = payment.Phone,
                                            //CardNo = payment.CardNumber,
                                            TransferTime = GetTransactionTime(item.txnDate, item.txnTime),
                                            Remark = item.txnRemark,
                                            Money = item.amount.ParseToDecimal(),
                                        };
                                        _redisService.SetBankOrderNotifyByBidv(bankOrderNotify);
                                    }
                                }
                            }

                            List<PayOrderDepositsMongoEntity> addlist = new List<PayOrderDepositsMongoEntity>();
                            foreach (var item in historyItems)
                            {
                                //加入数据库
                                var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(item.refNo, payment.Id,item.txnRemark);
                                if (info == null)
                                {
                                    var amount = item.amount.ParseToDecimal();
                                    var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                    {
                                        PayType = PayMentTypeEnum.BidvBank.ToInt(),
                                        Type = item.txnType == "+" ? "CRDT" : "DBIT",
                                        RefNo = item.refNo,
                                        Description = item.txnRemark,
                                        UserName = payment.Phone,
                                        AccountNo = payment.CardNumber,
                                        CreditBank = "",
                                        CreditAcctNo = "",
                                        CreditAcctName = "",
                                        CreditAmount = amount,
                                        DebitBank = "",
                                        DebitAcctNo = "",
                                        DebitAcctName = "",
                                        DebitAmount = amount,
                                        AvailableBalance = item.balance.ParseToDecimal(),
                                        TransactionTime = GetTransactionTime(item.txnDate, item.txnTime),
                                        PayMentId = payment.Id,
                                    };
                                    addlist.Add(payOrderDeposit);
                                }
                                if (item.txnType == "+")
                                {
                                    transactionLists.Add(item);
                                }

                                if (item.refNo == lastrefno) break;
                            }

                            if (addlist.Count > 0)
                            {
                                await _payOrderDepositsMongoService.AddListAsync(addlist);
                            }

                            //缓存最新记录
                            var lastInfo = new BidvBankTransactionHistoryItemTime();
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
                                _bidvBankHelper.SetLastRefNoKey(payment.CardNumber, lastInfo.refNo);
                            }

                            if (transactionLists.Count > 0)
                            {
                                foreach (var item in transactionLists)
                                {
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, payment.Type, item.txnRemark, Convert.ToDecimal(item.amount.Replace(",", "")));
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
                                                    Type = PayMentTypeEnum.BidvBank,
                                                    PayMentId = payment.Id,
                                                    PayOrderId = payEntity.ID,
                                                    Id = item.refNo,
                                                    Money = Convert.ToDecimal(item.amount.Replace(",", "")),
                                                    Desc = item.txnRemark,
                                                };
                                                _redisService.AddOrderQueueList(NsPayRedisKeyConst.BIDVBankOrder, bankOrderPubModel);

                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            NlogLogger.Warn("bidv Bank定时任务更新订单流水表异常：" + ex.ToString());
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            var cacheToken = _bidvBankHelper.GetSessionId(payment.CardNumber);
                            if (cacheToken != null && !cacheToken.token.IsNullOrEmpty())
                            {
                                TimeSpan timeSpan = DateTime.Now - cacheToken.Createtime;
                                if (timeSpan.TotalMinutes > 10)
                                {
                                    _bidvBankHelper.RemoveSessionId(payment.CardNumber);
                                }
                            }
                        }
                    }
                    else
                    {
                        var cacheToken = _bidvBankHelper.GetSessionId(payment.CardNumber);
                        if (cacheToken != null && !cacheToken.token.IsNullOrEmpty())
                        {
                            _bidvBankHelper.RemoveSessionId(payment.CardNumber);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Bidv Bank运行错误：" + ex);
            }

            await Task.CompletedTask;
        }

        private DateTime GetTransactionTime(string txnDate, string txnTime)
        {
            var date = txnDate.Split('/');
            if (date.Length >= 3)
            {
                var tempstr = date[2] + "-" + date[1] + "-" + date[0] + " " + txnTime;
                var time = tempstr.ParseToDateTime();
                return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(time, "China Standard Time");
            }
            return DateTime.MinValue;
        }

    }

}
