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
using System.Globalization;

namespace Neptune.NsPay.BankApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VietinBankController : ControllerBase
    {
        private readonly IRedisService _redisService;
        private readonly IVietinBankHelper _vietinBankHelper;
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        public VietinBankController(
            IRedisService redisService,
            IVietinBankHelper vietinBankHelper,
            IPayGroupMentService payGroupMentService,
            IPayOrdersMongoService payOrdersMongoService,
            IPayOrderDepositsMongoService payOrderDepositsMongoService
            )
        {
            _redisService = redisService;
            _vietinBankHelper = vietinBankHelper;
            _payGroupMentService = payGroupMentService;
            _payOrdersMongoService = payOrdersMongoService;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
        }

        [Route("~/VietinBank/GetTest")]
        [HttpPost]
        public async Task<JsonResult> GetTest([FromBody] BankJobInputRequest input)
        {
            var payMentList = _redisService.GetPayMents().Where(r => r.Type == PayMentTypeEnum.VietinBank && r.IsDeleted == false && r.Phone == input.Phone);
            foreach (var payment in payMentList)
            {
                if (payment.Phone.IsNullOrEmpty() || payment.CardNumber.IsNullOrEmpty())
                    continue;
                var acbResult = await _vietinBankHelper.GetHistTransactions(payment.Phone, payment.CardNumber);
                return new JsonResult(acbResult);
            }
            return new JsonResult(null);
        }

        [Route("~/VietinBank/GetHistory")]
        [HttpPost]
        public async Task GetHistory([FromBody] BankJobInputRequest input)
        {
            if (input.Phone.IsNullOrEmpty())
                return;
            try
            {
                var payment = _redisService.GetPayMentInfo(input.Phone, input.CardNumber, PayMentTypeEnum.VietinBank);
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

                    var hisTransList = await _vietinBankHelper.GetHistTransactions(payment.Phone, payment.CardNumber);

                    if (input.Debbug == 1)
                    {
                        NlogLogger.Fatal("VietinBank:" + payment.Phone + ",查询结果：" + hisTransList);
                    }

                    if (hisTransList!=null && hisTransList.Count > 0)
                    {
                        //获取最后一条记录
                        var lastrefno = _vietinBankHelper.GetLastRefNoKey(payment.CardNumber);
                        if (lastrefno == null)
                        {
                            lastrefno = "";
                        }
                        lastrefno = lastrefno.Replace("\"", "");

                        var historyItems = new List<HisTransactionItem>();
                        var transactionLists = new List<HisTransactionItem>();
                        //过滤订单，获取存款订单
                        var histortList = hisTransList;
                        if (input.Debbug == 1)
                        {
                            NlogLogger.Fatal("VietinBank:" + payment.Phone + ",查询数量：" + histortList.Count + ",最后订单：" + lastrefno);
                        }
                        foreach (var item in histortList)
                        {
                            //if (item.trxId == lastrefno) break;
       
                            item.TransactionTime = DateTime.Parse(item.processDate, new CultureInfo("de-DE"));
                            historyItems.Add(item);

                            if (payment.DispenseType == PayMentDispensEnum.None)
                            {
                                //加入取款缓存
                                if (item.dorC != "C")
                                {
                                    var bankOrderNotify = new BankOrderNotifyModel()
                                    {
                                        Type = PayMentTypeEnum.VietinBank,
                                        RefNo = item.trxId,
                                        PayMentId = payment.Id,
                                        //Phone = payment.Phone,
                                        //CardNo = payment.CardNumber,
                                        TransferTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(item.processDate, new CultureInfo("de-DE")), "China Standard Time"),
                                        Remark = item.remark,
                                        Money = item.amount
                                    };
                                    _redisService.SetBankOrderNotifyByVtb(bankOrderNotify);
                                }
                            }
                        }
                        //List<PayOrderDepositsMongoEntity> addlist = new List<PayOrderDepositsMongoEntity>();
                        foreach (var item in historyItems)
                        {
                            if (item.trxId == lastrefno) break;

                            var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(item.trxId, payment.Id, item.remark);
                            if (info == null)
                            {
                                var amount = item.amount;
                                var corresponsiveAccount = item.corresponsiveAccount;
                                var corresponsiveName = item.corresponsiveName;
                                var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                {
                                    PayType = PayMentTypeEnum.VietinBank.ToInt(),
                                    Type = item.dorC == "C" ? "CRDT" : "DBIT" ,
                                    UserName = payment.Phone,
                                    AccountNo = payment.CardNumber,
                                    RefNo = item.trxId,
                                    AvailableBalance = item.balance,
                                    TransactionTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse(item.processDate, new CultureInfo("de-DE")), "China Standard Time"),
                                    CreditBank = "",
                                    CreditAcctNo = corresponsiveAccount,
                                    CreditAcctName = corresponsiveName,
                                    CreditAmount = amount,
                                    DebitBank = "",
                                    DebitAcctNo = corresponsiveAccount,
                                    DebitAcctName = corresponsiveName,
                                    DebitAmount = amount,
                                    Description = item.remark,
                                    PayMentId = payment.Id
                                };
                                //addlist.Add(payOrderDeposit);
                                await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                            }

                            var dorc = item.dorC;
                            if (dorc == "C")//存款数据
                            {
                                //if (item.trxId == lastrefno) break;
      
                                transactionLists.Add(item);
                            }
                        }
                        
                        //if (addlist.Count > 0)
                        //{
                        //    await _payOrderDepositsMongoService.AddListAsync(addlist);
                        //}

                        //缓存最新的记录
                        var lastInfo = new HisTransactionItem();
                        if (transactionLists.Count > 10)
                        {
                            lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).Skip(10).FirstOrDefault();
                        }
                        //else
                        //{
                        //    lastInfo = transactionLists.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                        //}
                        if (lastInfo != null)
                        {
                            _vietinBankHelper.SetLastRefNoKey(payment.CardNumber, lastInfo.trxId);
                        }

                        if (input.Debbug == 1)
                        {
                            NlogLogger.Fatal("VietinBank:" + payment.Phone + ",匹配数据：" + transactionLists.Count);
                        }

                        if (transactionLists.Count > 0)
                        {
                            foreach (var item in transactionLists)
                            {
                                var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, payment.Type, item.remark, item.amount);
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
                                                Type = PayMentTypeEnum.VietinBank,
                                                PayMentId = payment.Id,
                                                PayOrderId = payEntity.ID,
                                                Id = item.trxId,
                                                Money = item.amount,
                                                Desc = item.remark,
                                            };

                                            _redisService.AddOrderQueueList(NsPayRedisKeyConst.VTBBankOrder, bankOrderPubModel);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        NlogLogger.Warn("VietinBank定时任务更新订单流水表异常：" + ex.ToString());
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Vietin Bank运行错误：" + ex);
            }
        }
    }

}
