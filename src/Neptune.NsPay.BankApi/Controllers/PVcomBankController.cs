using Abp.Extensions;
using Abp.Json;
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
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;

namespace Neptune.NsPay.BankApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PVcomBankController : ControllerBase
    {
        private readonly IRedisService _redisService;
        private readonly IPVcomBankHelper _pvcomBankHelper;
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        public PVcomBankController(
            IRedisService redisService,
            IPVcomBankHelper pvcomBankHelper,
            IPayGroupMentService payGroupMentService,
            IPayOrdersMongoService payOrdersMongoService,
            IPayOrderDepositsMongoService payOrderDepositsMongoService
            )
        {
            _redisService = redisService;
            _pvcomBankHelper = pvcomBankHelper;
            _payGroupMentService = payGroupMentService;
            _payOrdersMongoService = payOrdersMongoService;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
        }

        [Route("~/PVcomBank/GetTest")]
        [HttpPost]
        public async Task<JsonResult> GetTest([FromBody] BankJobInputRequest input)
        {
            var payMentList = _redisService.GetPayMents().Where(r => r.Type == PayMentTypeEnum.TechcomBank && r.IsDeleted == false && r.Phone == input.Phone);
            foreach (var payment in payMentList)
            {
                if (payment.Phone.IsNullOrEmpty() || payment.CardNumber.IsNullOrEmpty())
                    continue;
                var acbResult = await _pvcomBankHelper.GetHistoryAsync(input.Phone, payment.CardNumber);
                return new JsonResult(acbResult);
            }
            return new JsonResult(null);
        }

        [Route("~/PVcomBank/GetHistory")]
        [HttpPost]
        public async Task GetHistory([FromBody] BankJobInputRequest input)
        {
            if (input.Phone.IsNullOrEmpty())
                return;
            try
            {
                var payment = _redisService.GetPayMentInfo(input.Phone, input.CardNumber, PayMentTypeEnum.PVcomBank);
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
                        NlogLogger.Fatal("账户：" + ",PVcom:" + payment.Phone + ",进入查询");
                    }
                    var isLogin = await _pvcomBankHelper.Verify(input.CardNumber);
                    if (isLogin)
                    {
                        var pvcomResult = await _pvcomBankHelper.GetHistoryAsync(input.Phone, payment.CardNumber);
                        if (input.Debbug == 1)
                        {
                            NlogLogger.Fatal("账户：" + ",PVcom:" + payment.Phone + ",获取数据：" + pvcomResult.Count);
                        }
                        if (pvcomResult != null)
                        {
                            var lastrefno = _pvcomBankHelper.GetLastRefNoKey(payment.CardNumber);
                            if (lastrefno == null)
                            {
                                lastrefno = "";
                            }
                            lastrefno = lastrefno.Replace("\"", "");
                            if (input.Debbug == 1)
                            {
                                NlogLogger.Fatal("账户：" + ",PVcom:" + payment.Phone + ",lastrefno:" + lastrefno);
                            }

                            var historyItems = new List<PVcomBankTransactionHistoryList>();
                            var transactionLists = new List<PVcomBankTransactionHistoryList>();
                            foreach (var item in pvcomResult)
                            {
                                historyItems.Add(item);

                                if (payment.DispenseType == PayMentDispensEnum.None)
                                {
                                    //加入取款缓存
                                    if (item.DEBITAMOUNT > 0)
                                    {
                                        var bankOrderNotify = new BankOrderNotifyModel()
                                        {
                                            Type = PayMentTypeEnum.PVcomBank,
                                            RefNo = item.SEQUENNO,
                                            PayMentId = payment.Id,
                                            //Phone = payment.Phone,
                                            //CardNo = payment.CardNumber,
                                            TransferTime = GetTransactionTime(item.T24DATE),
                                            Remark = item.CONTENT,
                                            Money = item.DEBITAMOUNT.ParseToDecimal(),
                                        };
                                        _redisService.SetBankOrderNotifyByPVcom(bankOrderNotify);
                                    }
                                }
                            }
                            //List<PayOrderDepositsMongoEntity> addlist = new List<PayOrderDepositsMongoEntity>();
                            foreach (var item in historyItems)
                            {

                                if (item.SEQUENNO == lastrefno) break;

                                var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(item.SEQUENNO, payment.Id,item.CONTENT);
                                if (info == null)
                                {
                                    var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                    {
                                        PayType = PayMentTypeEnum.PVcomBank.ToInt(),
                                        Type = item.CREATEAMOUNT > 0 ? "CRDT" : "DBIT",
                                        AvailableBalance = item.CURRENBALANCE.ParseToDecimal(),
                                        RefNo = item.SEQUENNO,
                                        CreditBank = "",
                                        CreditAcctNo = "",
                                        CreditAcctName = "",
                                        CreditAmount = item.CREATEAMOUNT,
                                        DebitBank = "",
                                        DebitAcctNo = "",
                                        DebitAcctName = "",
                                        DebitAmount = item.DEBITAMOUNT,
                                        Description = item.CONTENT,
                                        PayMentId = payment.Id,
                                        UserName = payment.Phone,
                                        AccountNo = payment.CardNumber,
                                        TransactionTime = GetTransactionTime(item.T24DATE)
                                    };
                                    //addlist.Add(payOrderDeposit);
                                    await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                }
                                else
                                {
                                    info.Description = item.CONTENT;
                                    await _payOrderDepositsMongoService.UpdateAsync(info);
                                }
                                if (item.CREATEAMOUNT > 0)
                                {
                                    //if (item.SEQUENNO == lastrefno) break;

                                    transactionLists.Add(item);
                                }
                            }

                            //if (addlist.Count > 0)
                            //{
                            //    await _payOrderDepositsMongoService.AddListAsync(addlist);
                            //}

                            if (input.Debbug == 1)
                            {
                                NlogLogger.Fatal("账户：" + ",PVcom:" + payment.Phone + ",获取等待匹配数据：" + transactionLists.Count);
                            }
                            //缓存最新记录
                            var lastInfo = new PVcomBankTransactionHistoryList();
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
                                _pvcomBankHelper.SetLastRefNoKey(payment.CardNumber, lastInfo.SEQUENNO);
                            }

                            if (transactionLists.Count > 0)
                            {
                                if (input.Debbug == 1)
                                {
                                    NlogLogger.Fatal("账户：" + ",PVcom:" + payment.Phone + ",进入匹配");
                                }
                                foreach (var item in transactionLists)
                                {
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, payment.Type, item.CONTENT, item.CREATEAMOUNT);
                                    if (payEntity != null)
                                    {
                                        try
                                        {
                                            var successOrderCache = _redisService.GetSuccessOrder(payEntity.ID);
                                            if (successOrderCache.IsNullOrEmpty())
                                            {
                                                _redisService.SetSuccessOrder(payEntity.ID);

                                                var bankOrderPubModel = new BankOrderPubModel()
                                                {
                                                    MerchantCode = input.MerchantCode,
                                                    Type = PayMentTypeEnum.PVcomBank,
                                                    PayMentId = payment.Id,
                                                    PayOrderId = payEntity.ID,
                                                    Id = item.SEQUENNO,
                                                    Money = item.CREATEAMOUNT,
                                                    Desc = item.CONTENT,
                      
                                                };

                                                _redisService.AddOrderQueueList(NsPayRedisKeyConst.PVcomBankOrder, bankOrderPubModel);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            NlogLogger.Warn("账户：" + ",PVcom Bank定时任务更新订单流水表异常：" + ex.ToString());
                                        }
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        var cacheToken = _pvcomBankHelper.GetSessionId(payment.CardNumber);
                        if (cacheToken != null)
                        {
                            _pvcomBankHelper.RemoveSessionId(payment.CardNumber);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户：" + input.Phone + "PV Bank运行错误：" + ex);
            }
            await Task.CompletedTask;
        }

        private DateTime GetTransactionTime(string txnDate)
        {
            if (!txnDate.IsNullOrEmpty())
            {
                if (txnDate.Length >= 14)
                {
                    var year = txnDate.Substring(0, 4);
                    var mm = txnDate.Substring(4, 2);
                    var dd = txnDate.Substring(6, 2);
                    var hh = txnDate.Substring(8, 2);
                    var tt = txnDate.Substring(10, 2);
                    var ss = txnDate.Substring(12, 2);
                    var temp = year + "-" + mm + "-" + dd + " " + hh + ":" + tt + ":" + ss;
                    var time = temp.ParseToDateTime();
                    return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(time, "China Standard Time");
                }
            }
            return DateTime.MinValue;
        }

    }

}
