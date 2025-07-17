using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.BankApi.Models;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.NsPayBackgroundJobs;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;

namespace Neptune.NsPay.BankApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VietcomBankController : ControllerBase
    {
        private readonly IRedisService _redisService;
        private readonly IVietcomBankHelper _vietcomBankHelper;
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        private readonly INsPayBackgroundJobsService _nsPayBackgroundJobsService;
        public VietcomBankController(
            IRedisService redisService,
            IVietcomBankHelper vietcomBankHelper,
            IPayGroupMentService payGroupMentService,
            IPayOrdersMongoService payOrdersMongoService,
            IPayOrderDepositsMongoService payOrderDepositsMongoService,
            INsPayBackgroundJobsService nsPayBackgroundJobsService
            )
        {
            _redisService = redisService;
            _vietcomBankHelper = vietcomBankHelper;
            _payGroupMentService = payGroupMentService;
            _payOrdersMongoService = payOrdersMongoService;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
            _nsPayBackgroundJobsService = nsPayBackgroundJobsService;
        }

        [Route("~/VietcomBank/GetTest")]
        [HttpPost]
        public async Task<JsonResult> GetTest([FromBody] BankJobInputRequest input)
        {
            var payMentList = _redisService.GetPayMents().Where(r => r.Type == PayMentTypeEnum.VietcomBank && r.IsDeleted == false && r.Phone == input.Phone);
            foreach (var payment in payMentList)
            {
                if (payment.Phone.IsNullOrEmpty() || payment.CardNumber.IsNullOrEmpty())
                    continue;
                var acbResult = await _vietcomBankHelper.GetTransactionHistory(input.Phone, payment.CardNumber, input.BankApi);
                return new JsonResult(acbResult);
            }
            return new JsonResult(null);
        }


        [Route("~/VietcomBank/GetHistory")]
        [HttpPost]
        public async Task GetHistory([FromBody] BankJobInputRequest input)
        {
            if (input.BankApi.IsNullOrEmpty() || input.Phone.IsNullOrEmpty())
                return;
            var bankApi = input.BankApi;

            try
            {
                var payment = _redisService.GetPayMentInfo(input.Phone, input.CardNumber, PayMentTypeEnum.VietcomBank);
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

                    var isLogin = _vietcomBankHelper.Verify(input.CardNumber);
                    if (isLogin)
                    {
                        var vietcomResult = await _vietcomBankHelper.GetTransactionHistory(input.Phone, payment.CardNumber, bankApi);
                        if (vietcomResult != null)
                        {
                            if (vietcomResult.code == 0)
                            {
                                //获取最后一条记录
                                var lastrefno = _vietcomBankHelper.GetLastRefNoKey(payment.CardNumber);
                                if (lastrefno == null)
                                {
                                    lastrefno = "";
                                }
                                lastrefno = lastrefno.Replace("\"", "");

                                var historyItems = new List<ViecomBankTransactionHistoryItemTime>();
                                var transactionLists = new List<ViecomBankTransactionHistoryItemTime>();
                                foreach (var item in vietcomResult.data)
                                {
                                    //如果返回手机号的订单说明订单有错误直接跳过这个订单
                                    if (item.Reference.Contains(payment.Phone) || string.IsNullOrEmpty(item.Teller)) continue;
                             
                                    //if (item.Reference == lastrefno) break;
                                   
                                    var timeItem = new ViecomBankTransactionHistoryItemTime()
                                    {
                                        Reference = item.Reference,
                                        Amount = item.Amount,
                                        Description = item.Description,
                                        DorCCode = item.DorCCode,
                                        PostingDate = item.PostingDate,
                                        PostingTime = item.PostingTime,
                                        Teller = item.Teller,
                                    };
                                    timeItem.TransactionTime = GetTransactionTime(item?.PostingDate, item?.PostingTime);
                                    historyItems.Add(timeItem);

                                    if (payment.DispenseType == PayMentDispensEnum.None)
                                    {
                                        //加入取款缓存
                                        if (item?.DorCCode != "C")
                                        {
                                            var bankOrderNotify = new BankOrderNotifyModel()
                                            {
                                                Type = PayMentTypeEnum.VietcomBank,
                                                RefNo = item?.Reference,
                                                PayMentId = payment.Id,
                                                //Phone = payment.Phone,
                                                //CardNo = payment.CardNumber,
                                                TransferTime = GetTransactionTime(item?.PostingDate, item?.PostingTime),
                                                Remark = item?.Description,
                                                Money = Convert.ToDecimal(item?.Amount.Replace(",", "")),
                                            };
                                            _redisService.SetBankOrderNotifyByVcb(bankOrderNotify);
                                        }
                                    }
                                }

                                foreach (var item in historyItems)
                                {
                                    //加入数据库,vcb会重复返回，不能批量添加
                                    var info = await _payOrderDepositsMongoService.GetPayOrderFirstAsync(item.Reference, item.Description, payment.Id);
                                    if (info == null)
                                    {
                                        var amount = Convert.ToDecimal(item?.Amount.Replace(",", ""));
                                        var payOrderDeposit = new PayOrderDepositsMongoEntity()
                                        {
                                            PayType = PayMentTypeEnum.VietcomBank.ToInt(),
                                            Type = item?.DorCCode == "C" ? "CRDT" : "DBIT",
                                            UserName = payment.Phone,
                                            AccountNo = payment.CardNumber,
                                            RefNo = item?.Reference,
                                            AvailableBalance = 0,
                                            CreditBank = "",
                                            CreditAcctNo = "",
                                            CreditAcctName = "",
                                            CreditAmount = amount,
                                            DebitBank = "",
                                            DebitAcctNo = "",
                                            DebitAcctName = "",
                                            DebitAmount = amount,
                                            Description = item?.Description,
                                            TransactionTime = GetTransactionTime(item?.PostingDate, item?.PostingTime),
                                            PayMentId = payment.Id
                                        };
                                        await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                    }
                                    if (item?.DorCCode == "C")
                                    {                         
                                        transactionLists.Add(item);
                                    }

                                    if (item.Reference == lastrefno) break;
                                }

                                //缓存最新记录
                                var lastInfo = new ViecomBankTransactionHistoryItemTime();
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
                                    _vietcomBankHelper.SetLastRefNoKey(payment.CardNumber, lastInfo.Reference);
                                }

                                if (transactionLists.Count > 0)
                                {
                                    foreach (var item in transactionLists)
                                    {
                                        var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(input.MerchantCode, payment.Type, item.Description, Convert.ToDecimal(item.Amount.Replace(",", "")));
                                        if (payEntity != null)
                                        {
                                            if (payEntity.OrderMoney != Convert.ToDecimal(item.Amount.Replace(",", "")))
                                            {
                                                NlogLogger.Fatal("订单金额不匹配，支付id：" + payment.Id + "，订单信息：" + payEntity.ToJsonString());
                                                continue;
                                            }
                                            if (payEntity.OrderStatus == PayOrderOrderStatusEnum.Completed)
                                            {
                                                NlogLogger.Fatal("订单已经完成，支付id：" + payment.Id + "，订单信息：" + payEntity.ToJsonString());
                                                continue;
                                            }


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
                                                        Type = PayMentTypeEnum.VietcomBank,
                                                        PayMentId = payment.Id,
                                                        PayOrderId = payEntity.ID,
                                                        Id = item.Reference,
                                                        Money = Convert.ToDecimal(item.Amount.Replace(",", "")),
                                                        Desc = item.Description,
                                                    };

                                                    _redisService.AddOrderQueueList(NsPayRedisKeyConst.VCBBankOrder, bankOrderPubModel);

                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                NlogLogger.Warn("Vietcom Bank定时任务更新订单流水表异常：" + ex.ToString());
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                var cacheToken = _vietcomBankHelper.GetSessionId(payment.CardNumber);
                                if (cacheToken != null)
                                {
                                    _vietcomBankHelper.RemoveSessionId(payment.CardNumber);
                                }
                            }
                        }
                        else
                        {
                            var cacheToken = _vietcomBankHelper.GetSessionId(payment.CardNumber);
                            if (cacheToken != null)
                            {
                                _vietcomBankHelper.RemoveSessionId(payment.CardNumber);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Vietcom Bank运行错误：" + ex);
            }
            await Task.CompletedTask;
        }

        private DateTime GetTransactionTime(string? PostingDate, string? PostingTime)
        {
            if (PostingDate == null || PostingTime == null) { return DateTime.MinValue; }
            //posttingtime 拆分 155053
            if (PostingTime?.Length >= 6)
            {
                var hh = PostingTime.Substring(0, 2);
                var mm = PostingTime.Substring(2, 2);
                var ss = PostingTime.Substring(PostingTime.Length - 2);
                var tempStr = PostingDate + " " + hh + ":" + mm + ":" + ss;
                var time = tempStr.ParseToDateTime();
                return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(time, "China Standard Time");
            }
            else
            {
                var posttingdateStr = TimeHelper.ConvertDateTime(Convert.ToDateTime(PostingDate)).ToString();
                posttingdateStr = posttingdateStr.Substring(0, posttingdateStr.Length - 2);
                var tempStr = posttingdateStr + PostingTime?.Substring(0, PostingTime.Length - 1);
                var time = TimeHelper.ToLocalTimeDateByMilliseconds(tempStr);
                return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(time, "China Standard Time");
            }
        }

    }

}
