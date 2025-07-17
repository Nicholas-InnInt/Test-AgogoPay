using Abp.Collections.Extensions;
using MongoDB.Driver.Linq;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Web.Api.Services.Interfaces;
using Stripe;
using Twilio.TwiML.Voice;

namespace Neptune.NsPay.Web.Api.Services
{
    public class PayMentManageService : IPayMentManageService
    {
        private readonly IRedisService _redisService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IBankStateHelper _bankStateHelper;
        private readonly IBankBalanceService _bankBalanceService;

        public PayMentManageService(
           IRedisService redisService,
           IPayOrdersMongoService payOrdersMongoService,
           IPayGroupMentService payGroupMentService,
           IBankStateHelper bankStateHelper,
           IBankBalanceService bankBalanceService)
        {
            _redisService = redisService;
            _payOrdersMongoService = payOrdersMongoService;
            _payGroupMentService = payGroupMentService;
            _bankStateHelper = bankStateHelper;
            _bankBalanceService = bankBalanceService;
        }

        //根据订单金额过滤支付方式
        public async Task<List<PayMentRedisModel>> CheckBankPayMents(List<PayMentRedisModel> payMents, decimal orderMoney)
        {
            List<PayMentRedisModel> payMentRedis = new List<PayMentRedisModel>();

            payMents = payMents.Where(r => r.Type != PayMentTypeEnum.ScratchCards).ToList();

            var removeList = new List<PayMentRedisModel>();
            var addList = new List<PayMentRedisModel>();

            foreach (var item in payMents)
            {
                var flag = true;
                //金额设置
                if (item.MinMoney > 0)
                {
                    if (orderMoney < item.MinMoney)
                    {
                        removeList.Add(item);
                        flag = false;
                    }
                }
                if(item.MaxMoney > 0)
                {
                    if(orderMoney > item.MaxMoney)
                    {
                        removeList.Add(item);
                        flag = false;
                    }
                }

                //限额设置
                //获取12小时内的订单数据
                //var hourPayMentMoney = await _payOrdersMongoService.GetPayOrderByPayMentId(item.Id, PayOrderOrderStatusEnum.Completed, DateTime.Now.AddHours(-12));
                //if (item.LimitMoney > 0)
                //{
                //    if ((hourPayMentMoney + orderMoney) > item.LimitMoney)
                //    {
                //        removeList.Add(item);
                //        flag = false;
                //        //更新状态
                //        var paygroupment = await _payGroupMentService.GetFirstAsync(r => r.PayMentId == item.Id && r.IsDeleted == false);
                //        paygroupment.Status = false;
                //        await _payGroupMentService.UpdateAsync(paygroupment);
                //        //更新缓存
                //        var cacheInfo = _redisService.GetPayGroupMentByPayMentId(item.Id);
                //        if (cacheInfo != null)
                //        {
                //            var info = cacheInfo.PayMents.FirstOrDefault(r => r.Id == item.Id);
                //            if (info != null)
                //            {
                //                cacheInfo.PayMents.Remove(info);
                //                info.UseStatus = false;
                //                cacheInfo.PayMents.Add(info);
                //                _redisService.AddPayGroupMentRedisValue(cacheInfo.GroupId.ToString(), cacheInfo);
                //            }
                //        }
                //    }
                //}

                //余额设置
                //if (item.BalanceLimitMoney > 0)
                //{
                //    var islogin = _bankStateHelper.GetPayState(item.Phone, item.Type);
                //    var payMentBalance = _redisService.GetBalance(item.Id, item.Type);
                //    var balance = await _bankBalanceService.GetBalance(item.Id, payMentBalance.Balance, payMentBalance.Balance2, islogin);
                //    if (balance > item.BalanceLimitMoney)
                //    {
                //        removeList.Add(item);
                //        flag = false;
                //        //更新状态
                //        var paygroupment = await _payGroupMentService.GetFirstAsync(r => r.PayMentId == item.Id && r.IsDeleted == false);
                //        paygroupment.Status = false;
                //        await _payGroupMentService.UpdateAsync(paygroupment);
                //        //更新缓存
                //        var cacheInfo = _redisService.GetPayGroupMentByPayMentId(item.Id);
                //        if (cacheInfo != null)
                //        {
                //            var info = cacheInfo.PayMents.FirstOrDefault(r => r.Id == item.Id);
                //            if (info != null)
                //            {
                //                cacheInfo.PayMents.Remove(info);
                //                info.UseStatus = false;
                //                cacheInfo.PayMents.Add(info);
                //                _redisService.AddPayGroupMentRedisValue(cacheInfo.GroupId.ToString(), cacheInfo);
                //            }
                //        }
                //    }
                //}

                if (flag)
                {
                    addList.Add(item);
                }
            }
            payMentRedis = addList;
            return payMentRedis;
        }

        public string GetColorByPayType(PayMentTypeEnum paytype)
        {
            if (paytype == PayMentTypeEnum.MBBank || paytype == PayMentTypeEnum.BusinessMbBank)
            {
                return "#141FD1";
            }
            if (paytype == PayMentTypeEnum.VietinBank || paytype== PayMentTypeEnum.BusinessVtbBank)
            {
                return "#008ED3";
            }
            if (paytype == PayMentTypeEnum.TechcomBank || paytype == PayMentTypeEnum.BusinessTcbBank)
            {
                return "#EC1C24";
            }
            if (paytype == PayMentTypeEnum.VietcomBank)
            {
                return "#004A2C";
            }
            if (paytype == PayMentTypeEnum.BidvBank)
            {
                return "#006B68";
            }
            if (paytype == PayMentTypeEnum.ACBBank)
            {
                return "#1F419B";
            }
            if (paytype == PayMentTypeEnum.PVcomBank)
            {
                return "#ED8500";
            }
            return "#141FD1";
        }
    }
}
