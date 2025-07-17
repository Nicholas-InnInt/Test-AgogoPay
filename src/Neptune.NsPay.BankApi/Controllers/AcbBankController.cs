using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.BankApi.Models;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using RestSharp;
using System.Net;
using Neptune.NsPay.Commons;
using Abp.Json;
using Abp.Extensions;
using Abp.Domain.Repositories;
using Neptune.NsPay.NsPayBackgroundJobs;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using PayPalCheckoutSdk.Orders;


namespace Neptune.NsPay.BankApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcbBankController : ControllerBase
    {
        private readonly IRedisService _redisService;
        private readonly IAcbBankHelper _acbBankHelper;
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        private readonly INsPayBackgroundJobsService _nsPayBackgroundJobsService;
        public AcbBankController(
            IRedisService redisService,
            IAcbBankHelper acbBankHelper,
            IPayGroupMentService payGroupMentService,
            IPayOrdersMongoService payOrdersMongoService,
            IPayOrderDepositsMongoService payOrderDepositsMongoService,
            INsPayBackgroundJobsService nsPayBackgroundJobsService
         )
        {
            _redisService = redisService;
            _acbBankHelper = acbBankHelper;
            _payGroupMentService = payGroupMentService;
            _payOrdersMongoService = payOrdersMongoService;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
            _nsPayBackgroundJobsService = nsPayBackgroundJobsService;
        }

        [Route("~/AcbBank/GetSocksIpTest")]
        [HttpPost]
        public async Task<JsonResult> GetSocksIpTest([FromBody] BankJobInputRequest input)
        {

            var credentials = new NetworkCredential("aduser", "45686131496");
            var client = new RestClient(new RestClientOptions("https://ipinfo.io/json")
            {
                Proxy = new WebProxy("socks5://59.153.218.20:1091", true, null, credentials)
            });

            var arrrequest = new RestRequest();
            var responseArr = await client.ExecuteGetAsync(arrrequest);
            return new JsonResult(responseArr.Content);
        }

        [Route("~/AcbBank/GetTest")]
        [HttpPost]
        public async Task<JsonResult> GetTest([FromBody] BankJobInputRequest input)
        {
            var payMentList = _redisService.GetPayMents().Where(r => r.Type == PayMentTypeEnum.ACBBank && r.IsDeleted == false && r.Phone == input.Phone);
            foreach (var payment in payMentList)
            {
                if (payment.Phone.IsNullOrEmpty() || payment.CardNumber.IsNullOrEmpty())
                    continue;
                int type = 0;
                if (payment.BusinessType && payment.Mail == "businessacb")
                {
                    type = 1;
                }
                var acbResult = await _acbBankHelper.GetHistoryAsync(input.Phone, payment.CardNumber, input.BankApi, type);
                return new JsonResult(acbResult);
            }
            return new JsonResult(null);
        }

        [Route("~/AcbBank/GetHistory")]
        [HttpPost]
        public async Task GetHistory([FromBody] BankJobInputRequest input)
        {
            if (input.BankApi.IsNullOrEmpty() || input.Phone.IsNullOrEmpty())
                return;
            var bankApi = input.BankApi;
            try
            {
                var payment = _redisService.GetPayMentInfo(input.Phone, input.CardNumber,PayMentTypeEnum.ACBBank);
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
                    int type = 0;
                    if (payment.BusinessType && payment.Mail == "businessacb")
                    {
                        type = 1;
                    }

                    List<AcbBankTransactionHistoryData> acbResult = null;
                    if (type == 0)
                    {
                        acbResult = await _acbBankHelper.GetHistoryAsync(input.Phone, payment.CardNumber, bankApi, type);
                    }
                    if (type == 1)
                    {
                        acbResult = await _acbBankHelper.GetBusinessHistoryAsync(input.Phone, payment.CardNumber, bankApi, type);
                    }

                    if (acbResult != null)
                    {
                        if (acbResult.Count > 0)
                        {
                            //获取最后一条记录
                            var lastrefno = _acbBankHelper.GetLastRefNoKey(payment.CardNumber);
                            if (lastrefno == null)
                            {
                                lastrefno = "";
                            }
                            lastrefno = lastrefno.Replace("\"", "");

                            var historyItems = new List<AcbBankTransactionHistoryDataDetail>();
                            var transactionLists = new List<AcbBankTransactionHistoryDataDetail>();
                            var acbList = acbResult.OrderByDescending(r => Convert.ToInt32(r.id)).Take(150);
                            foreach (var item in acbList)
                            {
                                var itemTime = new AcbBankTransactionHistoryDataDetail()
                                {
                                    id = item.id,
                                    desc = item.desc,
                                    money = item.money,
                                    balance = item.balance,
                                    TxnType = item.money.Contains("-") ? "-" : "+"
                                };
                                historyItems.Add(itemTime);

                                if (payment.DispenseType == PayMentDispensEnum.None)
                                {
                                    //加入取款缓存
                                    if (item.money.Contains("-"))
                                    {
                                        var bankOrderNotify = new BankOrderNotifyModel()
                                        {
                                            Type = PayMentTypeEnum.ACBBank,
                                            RefNo = item.id,
                                            PayMentId = payment.Id,
                                            //Phone = payment.Phone,
                                            //CardNo = payment.CardNumber,
                                            TransferTime = DateTime.Now,
                                            Remark = item.desc,
                                            Money = item.money.Replace(".", "").Replace("-", "").ParseToDecimal()
                                        };
                                        //_redisService.AddTransferOrderQueueList(bankOrderNotify);
                                    }
                                }
                            }
                            foreach (var item in historyItems)
                            {
                                //加入数据库
                                var info = await _payOrderDepositsMongoService.GetPayOrderByBankNoTime(item.id, payment.Id,item.desc);
                                if (info == null)
                                {
                                    var amount = item.money.Replace(".", "").Replace("-", "").ParseToDecimal();
                                    var balance = 0M;
                                    if (!string.IsNullOrEmpty(item.balance))
                                    {
                                        balance = item.balance.Replace(".", "").Replace("-", "").ParseToDecimal();
                                    }
                                    var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                    {
                                        PayType = PayMentTypeEnum.ACBBank.ToInt(),
                                        UserName = payment.Phone,
                                        AccountNo = payment.CardNumber,
                                        RefNo = item.id,
                                        Description = item.desc,
                                        CreditBank = "",
                                        CreditAcctNo = "",
                                        CreditAcctName = "",
                                        CreditAmount = amount,
                                        DebitBank = "",
                                        DebitAcctNo = "",
                                        DebitAcctName = "",
                                        DebitAmount = amount,
                                        Type = item.TxnType == "+" ? "CRDT" : "DBIT",
                                        AvailableBalance = balance,
                                        PayMentId = payment.Id,
                                    };
                                    await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                }

                                if (!item.money.Contains("-"))
                                {
                                    transactionLists.Add(item);
                                }

                                if (item.id == lastrefno) break;
                            }

                            //缓存最新记录
                            AcbBankTransactionHistoryDataDetail? lastInfo = null;
                            if (transactionLists.Count >=15)
                            {
                                lastInfo = transactionLists.OrderByDescending(r => r.id).Skip(14).FirstOrDefault();
                            }
                            if (payment.BusinessType && payment.Mail == "businessacb")
                            {
                                if (transactionLists.Count >= 15)
                                {
                                    lastInfo = transactionLists.OrderByDescending(r => r.id).Skip(14).FirstOrDefault();
                                }
                            }

                            if (lastInfo != null)
                            {
                                _acbBankHelper.SetLastRefNoKey(payment.CardNumber, lastInfo.id);
                            }

                            if (transactionLists.Count > 0)
                            {
                                foreach (var item in transactionLists)
                                {
                                    var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, payment.Type, item.desc, item.money.Replace(".", "").Replace("-", "").ParseToDecimal());
                                    if (payEntity != null)
                                    {
                                        //更新订单，同时增加流水表
                                        try
                                        {
                                            //判断2天内的订单是有成功，如果有的话不进行更新
                                            var successOrderCache = _redisService.GetSuccessOrder(payEntity.ID);
                                            if (successOrderCache.IsNullOrEmpty())
                                            {
                                                _redisService.SetSuccessOrder(payEntity.ID);

                                                var bankOrderPubModel = new BankOrderPubModel()
                                                {
                                                    MerchantCode = input.MerchantCode,
                                                    Type = PayMentTypeEnum.ACBBank,
                                                    PayMentId = payment.Id,
                                                    PayOrderId = payEntity.ID,
                                                    Id = item.id,
                                                    Money = item.money.Replace(".", "").Replace("-", "").ParseToDecimal(),
                                                    Desc = item.desc
                                                };
                                                _redisService.AddOrderQueueList(NsPayRedisKeyConst.ACBBankOrder, bankOrderPubModel);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            NlogLogger.Warn("ACB Bank定时任务更新订单流水表异常：" + ex.ToString());
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var cacheToken = _acbBankHelper.GetSessionId(payment.CardNumber);
                            if (cacheToken != null)
                            {
                                if (cacheToken.ErrorCount > 6)
                                {
                                    _acbBankHelper.RemoveSessionId(payment.CardNumber);
                                }
                            }
                        }
                    }
                    else
                    {
                        var cacheToken = _acbBankHelper.GetSessionId(payment.CardNumber);
                        if (cacheToken != null)
                        {
                            if (cacheToken.ErrorCount > 6)
                            {
                                _acbBankHelper.RemoveSessionId(payment.CardNumber);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("ACB Bank运行错误：" + ex);
            }
            await Task.CompletedTask;
        }
    }

}
