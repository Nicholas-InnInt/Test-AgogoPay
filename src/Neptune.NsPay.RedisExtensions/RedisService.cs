using Abp.Extensions;
using Abp.Json;
﻿using Neptune.NsPay.Commons;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.WithdrawalDevices;
using NewLife.Caching;
using NewLife.Caching.Queues;
using NewLife.Redis.Core;

namespace Neptune.NsPay.RedisExtensions
{
    public class RedisService: IRedisService
    {
        private readonly INewLifeRedis _newLifeRedis;
        public RedisService(INewLifeRedis newLifeRedis)
        {
            _newLifeRedis = newLifeRedis;
        }

        public FullRedis GetFullRedis()
        {
            return _newLifeRedis.GetFullRedis();
        }

        #region 商户
        
        public List<MerchantRedisModel>? GetMerchantRedis()
        {
            List<MerchantRedisModel> merchantRedisModels = new List<MerchantRedisModel>();
            var redis = _newLifeRedis.GetFullRedis();
            var keys = redis.Execute(null, (r, k) => r.Execute<String[]>("KEYS", NsPayRedisKeyConst.MerchantKey+"*"));
            foreach (var key in keys)
            {
                var merchant = _newLifeRedis.Get<MerchantRedisModel>(key);
                if (merchant != null)
                {
                    merchant.MerchantRate = _newLifeRedis.Get<MerchantRateRedisModel>(key);
                    merchant.MerchantSetting = _newLifeRedis.Get<MerchantSettingRedisModel>(key);
					merchantRedisModels.Add(merchant);
				}
            }
            return merchantRedisModels;
		}

        public MerchantRedisModel? GetMerchantKeyValue(string merchantCode)
        {

            var merchant = _newLifeRedis.Get<MerchantRedisModel>(NsPayRedisKeyConst.MerchantKey + merchantCode);
            if (merchant != null)
            {
                merchant.MerchantRate= _newLifeRedis.Get<MerchantRateRedisModel>(NsPayRedisKeyConst.MerchantRateKey + merchantCode);
                merchant.MerchantSetting= _newLifeRedis.Get<MerchantSettingRedisModel>(NsPayRedisKeyConst.MerchantSetting + merchantCode);
            }
            return merchant;
        }
        public IDisposable? GetMerchantBalanceLock(string MerchantCode)
        {
            var redis = _newLifeRedis.GetFullRedis();
            return redis.AcquireLock("MerchantBalance:" + MerchantCode, 5000);
        }

        #endregion

        #region 支付组方式缓存

        public PayGroupMentRedisModel? GetPayGroupMentRedisValue(string cacheKey)
        {
            return _newLifeRedis.HashGetOne<PayGroupMentRedisModel?>(NsPayRedisKeyConst.PayGroupMent, cacheKey);
        }

        public PayGroupMentRedisModel? GetPayGroupMentByGroupName(string cacheKey)
        {
            var result = _newLifeRedis.HashGetAll<PayGroupMentRedisModel?>(NsPayRedisKeyConst.PayGroupMent).Where(r => r.Value.GroupName == cacheKey);
            if (result != null)
            {
                return result.FirstOrDefault().Value;
            }
            return null;
        }

        public PayGroupMentRedisModel? GetPayGroupMentByPayMentId(int cacheKey)
        {
            var result = _newLifeRedis.HashGetAll<PayGroupMentRedisModel?>(NsPayRedisKeyConst.PayGroupMent).Where(r => r.Value.PayMents.FirstOrDefault(r => r.Id == cacheKey) != null);
            if (result != null)
            {
                return result.FirstOrDefault().Value;
            }
            return null;
        }

        public  List<string> GetPayGroupMentIdByPayMentId(int paymentId)
        {
           return _newLifeRedis.HashGetAll<PayGroupMentRedisModel?>(NsPayRedisKeyConst.PayGroupMent).Where(r => r.Value.PayMents.FirstOrDefault(r => r.Id == paymentId) != null).Select(x=>x.Key).ToList();
        }

        public void AddPayGroupMentRedisValue(string cacheKey, PayGroupMentRedisModel payGroupMentRedisModel)
        {
            _newLifeRedis.HashAdd(NsPayRedisKeyConst.PayGroupMent, cacheKey, payGroupMentRedisModel);
        }

        public void DeletePayGroupMentRedisValue(string cacheKey)
        {
            _newLifeRedis.HashDel<PayGroupMentRedisModel>(NsPayRedisKeyConst.PayGroupMent, cacheKey);
        }

        #endregion

        #region ns系统配置

        public string GetNsPaySystemSettingKeyValue(string cacheKey)
        {
            var result = _newLifeRedis.Get<string>(NsPayRedisKeyConst.NsPaySystemKey + cacheKey);
            if (result == null)
            {
                return "";
            }
            return result;
        }

        public List<MerchantBankJobApiModel>? GetMerchantTcbBankJobApi()
        {
            return _newLifeRedis.Get<List<MerchantBankJobApiModel>>(NsPayRedisKeyConst.NsPaySystemKey + NsPaySystemSettingKeyConst.MerchantBankJobApi);
        }

        #endregion

        #region 基础

        public T GetRedisValue<T>(string cacheKey)
        {
            var value = _newLifeRedis.Get<T>(cacheKey);
            return value;
        }

        public void AddRedisValue<T>(string cacheKey, T value)
        {
            _newLifeRedis.Set(cacheKey, value);
        }

        public void RemoveRedisValue(string cacheKey)
        {
            _newLifeRedis.Remove(cacheKey);
        }

        public void RemoveRedisValueByPattern(string cacheKey)
        {
            _newLifeRedis.DelByPattern(cacheKey);
        }

        #endregion

        #region 银行组缓存
        public List<NaPasBankModel> GetTcbBankCodeCaches()
        {
            var value = _newLifeRedis.Get<List<NaPasBankModel>>(NsPayRedisKeyConst.TCBBankCode);
            return value;
        }

        public List<BankOrderNotifyModel> GetListBankOrderNotifyByAcb()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.ACBBankOrderNotify))
                {
                    var value = _newLifeRedis.ListGetAll<BankOrderNotifyModel>(NsPayRedisKeyConst.ACBBankOrderNotify);
                    if (value.Count() > 0)
                    {
                        return value;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void SetBankOrderNotifyByAcb(BankOrderNotifyModel bankOrderNotify)
        {
            List<BankOrderNotifyModel> list = new List<BankOrderNotifyModel>
            {
                bankOrderNotify
            };
            _newLifeRedis.ListRightPush<BankOrderNotifyModel>(NsPayRedisKeyConst.ACBBankOrderNotify, list);
        }
        public List<BankOrderNotifyModel>? GetListBankOrderNotifyByBidv()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.BIDVBankOrderNotify))
                {
                    var value = _newLifeRedis.ListGetAll<BankOrderNotifyModel>(NsPayRedisKeyConst.BIDVBankOrderNotify);
                    if (value.Count() > 0)
                    {
                        return value;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void SetBankOrderNotifyByBidv(BankOrderNotifyModel bankOrderNotify)
        {
            List<BankOrderNotifyModel> list = new List<BankOrderNotifyModel>
            {
                bankOrderNotify
            };
            _newLifeRedis.ListRightPush<BankOrderNotifyModel>(NsPayRedisKeyConst.BIDVBankOrderNotify, list);
        }
        public List<BankOrderNotifyModel>? GetListBankOrderNotifyByMB()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.MBBankOrderNotify))
                {
                    var value = _newLifeRedis.ListGetAll<BankOrderNotifyModel>(NsPayRedisKeyConst.MBBankOrderNotify);
                    if (value.Count() > 0)
                    {
                        return value;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void SetBankOrderNotifyByMB(BankOrderNotifyModel bankOrderNotify)
        {
            List<BankOrderNotifyModel> list = new List<BankOrderNotifyModel>
            {
                bankOrderNotify
            };
            _newLifeRedis.ListRightPush<BankOrderNotifyModel>(NsPayRedisKeyConst.MBBankOrderNotify, list);
        }
        public List<BankOrderNotifyModel>? GetListBankOrderNotifyByPVcom()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.PVcomBankOrderNotify))
                {
                    var value = _newLifeRedis.ListGetAll<BankOrderNotifyModel>(NsPayRedisKeyConst.PVcomBankOrderNotify);
                    if (value.Count() > 0)
                    {
                        return value;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void SetBankOrderNotifyByPVcom(BankOrderNotifyModel bankOrderNotify)
        {
            List<BankOrderNotifyModel> list = new List<BankOrderNotifyModel>
            {
                bankOrderNotify
            };
            _newLifeRedis.ListRightPush<BankOrderNotifyModel>(NsPayRedisKeyConst.PVcomBankOrderNotify, list);
        }
        public List<BankOrderNotifyModel>? GetListBankOrderNotifyByVcb()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.VCBBankOrderNotify))
                {
                    var value = _newLifeRedis.ListGetAll<BankOrderNotifyModel>(NsPayRedisKeyConst.VCBBankOrderNotify);
                    if (value.Count() > 0)
                    {
                        return value;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void SetBankOrderNotifyByVcb(BankOrderNotifyModel bankOrderNotify)
        {
            List<BankOrderNotifyModel> list = new List<BankOrderNotifyModel>
            {
                bankOrderNotify
            };
            _newLifeRedis.ListRightPush<BankOrderNotifyModel>(NsPayRedisKeyConst.VCBBankOrderNotify, list);
        }
        public List<BankOrderNotifyModel>? GetListBankOrderNotifyByTcb()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.TCBBankOrderNotify))
                {
                    var value = _newLifeRedis.ListGetAll<BankOrderNotifyModel>(NsPayRedisKeyConst.TCBBankOrderNotify);
                    if (value.Count() > 0)
                    {
                        return value;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void SetBankOrderNotifyByTcb(BankOrderNotifyModel bankOrderNotify)
        {
            List<BankOrderNotifyModel> list = new List<BankOrderNotifyModel>
            {
                bankOrderNotify
            };
            _newLifeRedis.ListRightPush<BankOrderNotifyModel>(NsPayRedisKeyConst.TCBBankOrderNotify, list);
        }
        public List<BankOrderNotifyModel>? GetListBankOrderNotifyByVtb()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.VTBBankOrderNotify))
                {
                    var value = _newLifeRedis.ListGetAll<BankOrderNotifyModel>(NsPayRedisKeyConst.VTBBankOrderNotify);
                    if (value.Count() > 0)
                    {
                        return value;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void SetBankOrderNotifyByVtb(BankOrderNotifyModel bankOrderNotify)
        {
            List<BankOrderNotifyModel> list = new List<BankOrderNotifyModel>
            {
                bankOrderNotify
            };
            _newLifeRedis.ListRightPush<BankOrderNotifyModel>(NsPayRedisKeyConst.VTBBankOrderNotify, list);
        }
        #endregion

        #region 缓存支付方式

        private List<PayMentRedisModel>? GetPayMentCaches()
        {
            var result = _newLifeRedis.HashGetAll<PayMentRedisModel>(NsPayRedisKeyConst.PayMents);
            if (result != null)
            {
                List<PayMentRedisModel> payMentRedisModels = result.Values.ToList();
                return payMentRedisModels;
            }
            return null;
        }

        public List<PayMentRedisModel> GetPayMents()
        {
            var payments = GetPayMentCaches();
            if (payments == null)
            {
                payments = new List<PayMentRedisModel>();
            }
            return payments;
        }

        public PayMentRedisModel? GetPayMentInfoById(int id)
        {
            var payments = GetPayMentCaches();
            if (payments == null)
            {
                return null;
            }
            var info = payments.FirstOrDefault(r => r.Id == id && r.IsDeleted == false);
            return info;
        }

        public PayMentRedisModel? GetPayMentInfo(string phone, string cardNumber, PayMentTypeEnum typeEnum)
        {
            var payments = GetPayMentCaches();
            if (payments == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(cardNumber))
            {
                var info = payments.FirstOrDefault(r => r.Phone == phone && r.Type == typeEnum && r.IsDeleted == false);
                return info;
            }
            else
            {
                var info = payments.FirstOrDefault(r => r.Phone == phone && r.CardNumber == cardNumber && r.Type == typeEnum && r.IsDeleted == false);
                return info;
            }
        }

        public PayMentRedisModel? GetPayMentTcbInfo(string phone,string cardNumber)
        {
            var payments = GetPayMentCaches();
            if (payments == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(cardNumber))
            {
                var info = payments.FirstOrDefault(r => r.Phone == phone && (r.Type == PayMentTypeEnum.BusinessTcbBank || r.Type == PayMentTypeEnum.TechcomBank) && r.IsDeleted == false);
                return info;
            }
            else
            {
                var info = payments.FirstOrDefault(r => r.Phone == phone && r.CardNumber == cardNumber && (r.Type == PayMentTypeEnum.BusinessTcbBank || r.Type == PayMentTypeEnum.TechcomBank) && r.IsDeleted == false);
                return info;
            }
        }

        public void SetPayMentCaches(PayMentRedisModel payMentRedis)
        {
            _newLifeRedis.HashAdd(NsPayRedisKeyConst.PayMents, payMentRedis.Id.ToString(), payMentRedis);
        }

        public void DeletePayMentCaches(string cacheKey)
        {
            _newLifeRedis.HashDel<PayMentRedisModel>(NsPayRedisKeyConst.PayMents, cacheKey);
        }
        #endregion

        #region 缓存成功订单
        public string GetSuccessOrder(string orderId)
        {
            return _newLifeRedis.Get<string>(NsPayRedisKeyConst.SuccessOrder + orderId);
        }
        public bool SetSuccessOrder(string orderId)
        {

            string insertUnique = @"if redis.call('exists', KEYS[1]) == 0 then
    redis.call('set', KEYS[1], ARGV[1])
    redis.call('expire', KEYS[1], tonumber(ARGV[2]))
    return 1
else
    return 0
end
";

            bool returnResult = false;

            try
            {
                int result = _newLifeRedis.GetFullRedis().Execute<int>(NsPayRedisKeyConst.SuccessOrder + orderId, (r, k) =>
                {
                    // keys count = 1
                    var keysCount = 1;

                    // Compose parameters for EVAL:
                    object[] parameters = new object[]
                    {
        insertUnique,
        keysCount,
        k,          // key
        orderId,
        ((int)(DateTime.Now.AddDays(2) - DateTime.Now).TotalSeconds).ToString()
                    };

                    return r.Execute<int>("EVAL", parameters);
                }, write: true);

                returnResult = result > 0;
            }
            catch (Exception ex)
            {
            }
            return returnResult;
        }

        public string GetSuccessBankOrder(string orderId, string type)
        {
            var value = _newLifeRedis.Get<string>(type + NsPayRedisKeyConst.SuccessBankOrder + orderId);
            return value;
        }

        public void SetSuccessBankOrder(string orderId, string type)
        {
            _newLifeRedis.Set(type + NsPayRedisKeyConst.SuccessBankOrder + orderId, orderId, DateTime.Now.AddDays(1) - DateTime.Now);
        }

        public string GetWithdrawalSuccessOrder(string orderId)
        {
            return _newLifeRedis.Get<string>(NsPayRedisKeyConst.WithdrawalSuccessOrder + orderId);
        }
        public void SetWithdrawalSuccessOrder(string orderId)
        {
            _newLifeRedis.Set(NsPayRedisKeyConst.WithdrawalSuccessOrder + orderId, orderId, DateTime.Now.AddDays(2) - DateTime.Now);
        }

        #endregion

        #region 后台作业服务

        #endregion

        #region 收款订单队列

        public void AddOrderQueueList(string channel, BankOrderPubModel bankOrder)
        {
            _newLifeRedis.AddReliableQueue(channel, bankOrder);
        }

        public RedisReliableQueue<BankOrderPubModel> GetOrderQueue(string channel)
        {
            try
            {
                var value = _newLifeRedis.GetRedisReliableQueue<BankOrderPubModel>(channel);
                return value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 提现订单队列

        public void AddWithdrawalOrderQueueList(string channel, WithdrawalOrderPubModel bankOrder)
        {
            _newLifeRedis.AddReliableQueue(channel, bankOrder);
        }

        public RedisReliableQueue<WithdrawalOrderPubModel> GetWithdrawalOrderQueue(string channel)
        {
            try
            {
                var value = _newLifeRedis.GetRedisReliableQueue<WithdrawalOrderPubModel>(channel);
                return value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 下发订单队列

        public void AddTransferOrderQueueList(BankOrderNotifyModel bankOrder)
        {
            _newLifeRedis.AddReliableQueue(NsPayRedisKeyConst.BankOrderNotify, bankOrder);
        }

        public RedisReliableQueue<BankOrderNotifyModel> GetTransferOrderQueue()
        {
            try
            {
                var value = _newLifeRedis.GetRedisReliableQueue<BankOrderNotifyModel>(NsPayRedisKeyConst.BankOrderNotify);
                return value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 交易流水
        public string GetMerchantBillOrder(string merchantCode, string payorderId)
        {
            return _newLifeRedis.Get<string>("MerchantBill:" + merchantCode + "BillNo_" + payorderId);
        }

        public void SetMerchantBillOrder(string merchantCode, string payorderId)
        {
            _newLifeRedis.Set("MerchantBill:" + merchantCode + "BillNo_" + payorderId, payorderId, DateTime.Now.AddHours(2) - DateTime.Now);
        }

        public string GetMerchantMqBillOrder(string merchantCode, string payorderId)
        {
            return _newLifeRedis.Get<string>("MerchantMQBill:" + merchantCode + "BillNo_" + payorderId);
        }

        public void SetMerchantMqBillOrder(string merchantCode, string payorderId)
        {
            _newLifeRedis.Set("MerchantMQBill:" + merchantCode + "BillNo_" + payorderId, payorderId, DateTime.Now.AddHours(2) - DateTime.Now);
        }

        //public void SetMerchantMqPublish(PayMerchantRedisMqDto redisMqDto)
        //{
        //    List<PayMerchantRedisMqDto> list = new List<PayMerchantRedisMqDto>
        //    {
        //        redisMqDto
        //    };
        //    _newLifeRedis.ListRightPush("MerchantBillRedisMq", list);
        //}
        #endregion

        #region 缓存回调订单
        public void AddCallBackOrderQueueList(string channel, string orderId)
        {
            _newLifeRedis.AddReliableQueue(channel, orderId);
        }

        public RedisReliableQueue<string> GetCallBackOrderQueue(string channel)
        {
            try
            {
                var value = _newLifeRedis.GetRedisReliableQueue<string>(channel);
                return value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 添加缓存队列

        public void SetPayOrderDepositSuccessCache(PayOrderDepositSuccessModel successModel)
        {
            List<PayOrderDepositSuccessModel> list = new List<PayOrderDepositSuccessModel>
            {
                successModel
            };
            _newLifeRedis.ListRightPush<PayOrderDepositSuccessModel>(NsPayRedisKeyConst.PayOrderDepositOrders, list);
        }

        public List<PayOrderDepositSuccessModel> GetPayOrderDepositSuccessCache()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.PayOrderDepositOrders))
                {
                    var value = _newLifeRedis.ListGetAll<PayOrderDepositSuccessModel>(NsPayRedisKeyConst.PayOrderDepositOrders);
                    if (value.Count() <= 0)
                    {
                        return null;
                    }
                    return value;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool PayOrderDepositSuccessCheckExist(string orderId)
        {
            bool isExisted = false;
            try
            {
                var lua = @"
local list = KEYS[1]
local target = ARGV[1]
local len = redis.call('LLEN', list)
for i = 0, len - 1 do
    local item = redis.call('LINDEX', list, i)
    if item and item:match('\""DepositsId\""%' .. 's*:' .. '%s*\""'
        .. target .. '\""') then
        return 1
    end
end
return 0
";
                var result = _newLifeRedis.GetFullRedis().Execute<string>(NsPayRedisKeyConst.PayOrderDepositOrders, (client, key) => client.Execute<string>("EVAL", lua, "1", NsPayRedisKeyConst.PayOrderDepositOrders, orderId));

                if (result != null)
                {
                    isExisted = result == "1";
                }

            }
            catch (Exception ex)
            {
                return false;
            }

            return isExisted;
        }

        #endregion

        #region 添加Mq缓存队列

        public RedisReliableQueue<PayMerchantRedisMqDto> GetMerchantMqPublish()
        {
            //try
            //{
            //    if (_newLifeRedis.ContainsKey("MerchantBillRedisMq"))
            //    {
            //        var value = _newLifeRedis.ListLeftPop<PayMerchantRedisMqDto>("MerchantBillRedisMq");
            //        return value;
            //    }
            //    return null;
            //}
            //catch (Exception ex)
            //{
            //    return null;
            //}
            try
            {
                var value = _newLifeRedis.GetRedisReliableQueue<PayMerchantRedisMqDto>(NsPayRedisKeyConst.BankOrderMqPublish);
                return value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void SetMerchantMqPublish(PayMerchantRedisMqDto redisMqDto)
        {
            //List<PayMerchantRedisMqDto> list = new List<PayMerchantRedisMqDto>
            //{
            //    redisMqDto
            //};
            //_newLifeRedis.ListRightPush("MerchantBillRedisMq", list);
            var newData = redisMqDto.MerchantCode + "_" + redisMqDto.PayOrderId + "_" + redisMqDto.WithdrawalOrderId + "_" + redisMqDto.MerchantWithdrawId;
            if (!GetWithExpiry(redisMqDto.PayMqSubType,newData))
            {
                AddWithExpiry(redisMqDto.PayMqSubType,newData, DateTime.Now.AddHours(2) - DateTime.Now);
                _newLifeRedis.AddReliableQueue(NsPayRedisKeyConst.BankOrderMqPublish, redisMqDto);
            }
        }

        void AddWithExpiry(string type, string key, TimeSpan expiry)
        {
            var hash = _newLifeRedis.GetFullRedis().GetDictionary<bool>("BankOrderMqPublishHash");
            hash[key] = true;
            _newLifeRedis.Set(type + ":" + $"{key}_expiry", "1", expiry); // 创建辅助键用于过期管理
        }

        bool GetWithExpiry(string type,string key)
        {
            var hash = _newLifeRedis.GetFullRedis().GetDictionary<bool>("BankOrderMqPublishHash");
            if (!hash.ContainsKey(key)) return false;

            if (!_newLifeRedis.ContainsKey(type + ":" + $"{key}_expiry"))
            {
                // 数据已过期，删除主哈希表中的字段
                hash.Remove(key);
                return false;
            }
            return hash[key];
        }

        public BankOrderNotifyModel GetAcbBankOrderNotifyMqPublish()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.ACBBankOrderNotify))
                {
                    var value = _newLifeRedis.ListLeftPop<BankOrderNotifyModel>(NsPayRedisKeyConst.ACBBankOrderNotify);
                    return value;
                    
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public BankOrderNotifyModel GetBidvBankOrderNotifyMqPublish()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.BIDVBankOrderNotify))
                {
                    var value = _newLifeRedis.ListLeftPop<BankOrderNotifyModel>(NsPayRedisKeyConst.BIDVBankOrderNotify);
                    return value;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public BankOrderNotifyModel GetMBBankOrderNotifyMqPublish()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.MBBankOrderNotify))
                {
                    var value = _newLifeRedis.ListLeftPop<BankOrderNotifyModel>(NsPayRedisKeyConst.MBBankOrderNotify);
                    return value;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public BankOrderNotifyModel GetPVcomBankOrderNotifyMqPublish()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.PVcomBankOrderNotify))
                {
                    var value = _newLifeRedis.ListLeftPop<BankOrderNotifyModel>(NsPayRedisKeyConst.PVcomBankOrderNotify);
                    return value;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public BankOrderNotifyModel GetVcbBankOrderNotifyMqPublish()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.VCBBankOrderNotify))
                {
                    var value = _newLifeRedis.ListLeftPop<BankOrderNotifyModel>(NsPayRedisKeyConst.VCBBankOrderNotify);
                    return value;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public BankOrderNotifyModel GetTcbBankOrderNotifyMqPublish()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.TCBBankOrderNotify))
                {
                    var value = _newLifeRedis.ListLeftPop<BankOrderNotifyModel>(NsPayRedisKeyConst.TCBBankOrderNotify);
                    return value;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public BankOrderNotifyModel GetVtbBankOrderNotifyMqPublish()
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.VTBBankOrderNotify))
                {
                    var value = _newLifeRedis.ListLeftPop<BankOrderNotifyModel>(NsPayRedisKeyConst.VTBBankOrderNotify);
                    return value;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 缓存支付方式余额

        public BankBalanceModel GetBalance(int paymentId, PayMentTypeEnum paytype)
        {
            var balance = Convert.ToDecimal(0);

            var cache = GetBalanceByPaymentId(paymentId);
            if (cache != null)
            {
                //if (paytype == PayMentTypeEnum.TechcomBank
                //    || paytype == PayMentTypeEnum.MBBank || paytype == PayMentTypeEnum.BusinessMbBank
                //    || paytype == PayMentTypeEnum.TechcomBank)
                //{
                //    balance = cache.Balance2;
                //    if (balance == 0)
                //    {
                //        balance = cache.Balance;
                //    }
                //}
                //else
                //{
                //balance = cache.Balance;
                //if (balance == 0)
                //{
                //    balance = cache.Balance2;
                //}
                //}
                return cache;
            }
            else
            {
                return new BankBalanceModel();
            }
            //return balance;
        }

        public BankBalanceModel? GetBalanceByPaymentId(int paymentId)
        {
            return _newLifeRedis.HashGetOne<BankBalanceModel?>(NsPayRedisKeyConst.PayMentBalance, paymentId.ToString());
        }

        public void SetBalance(int paymentId, BankBalanceModel bankBalanceModel)
        {
            _newLifeRedis.HashAdd(NsPayRedisKeyConst.PayMentBalance, paymentId.ToString(), bankBalanceModel);
        }


        #endregion

        #region 缓存正在使用支付方式

        public int? GetPayUseMent(int paymentId)
        {
            return _newLifeRedis.HashGetOne<int>(NsPayRedisKeyConst.PayUseMents, paymentId.ToString());
        }

        public void SetPayUseMent(int paymentId)
        {
            _newLifeRedis.HashAdd(NsPayRedisKeyConst.PayUseMents, paymentId.ToString(), paymentId);
        }

        public void RemovePayUseMent(int paymentId)
        {
            _newLifeRedis.HashDel<int>(NsPayRedisKeyConst.PayUseMents,paymentId.ToString());
        }

        #endregion

        #region 出款设备缓存

        public List<WithdrawalDeviceRedisModel> GetListRangeDevice(string merchantCode)
        {
            try
            {
                List<WithdrawalDeviceRedisModel> value = new List<WithdrawalDeviceRedisModel>();
                if (merchantCode == NsPayRedisKeyConst.NsPay)
                {
                    if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.WithdrawalDevices))
                    {
                        value = _newLifeRedis.ListGetAll<WithdrawalDeviceRedisModel>(NsPayRedisKeyConst.WithdrawalDevices);
                    }
                }
                else
                {
                    if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.WithdrawalDevices + "_" + merchantCode))
                    {
                        value = _newLifeRedis.ListGetAll<WithdrawalDeviceRedisModel>(NsPayRedisKeyConst.WithdrawalDevices + "_" + merchantCode);
                    }
                }
                if (value.Count() <= 0)
                {
                    return null;
                }
                return value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void SetRPushWithdrawDevice(string merchantCode, WithdrawalDeviceRedisModel withdrawalDevice)
        {
            List<WithdrawalDeviceRedisModel> list = new List<WithdrawalDeviceRedisModel>
            {
                withdrawalDevice
            };
            if (merchantCode == NsPayRedisKeyConst.NsPay)
            {
                _newLifeRedis.ListRightPush(NsPayRedisKeyConst.WithdrawalDevices, list);
            }
            else
            {
                _newLifeRedis.ListRightPush(NsPayRedisKeyConst.WithdrawalDevices + "_" + merchantCode, list);
            }
        }

        public void UpdateWithdrawDevice(string merchantCode,WithdrawalDeviceRedisModel withdrawalDevice)
        {
            List<WithdrawalDeviceRedisModel> list = new List<WithdrawalDeviceRedisModel>
            {
                withdrawalDevice
            };

            var cacheList = GetListRangeDevice(merchantCode);
            if (withdrawalDevice.MerchantCode== NsPayRedisKeyConst.NsPay)
            {
                if (cacheList != null)
                {
                    var info = cacheList.FirstOrDefault(r => r.Id == withdrawalDevice.Id);
                    if (info != null)
                    {
                        _newLifeRedis.ListRemove(NsPayRedisKeyConst.WithdrawalDevices, info);
                    }
                }
                _newLifeRedis.ListRightPush(NsPayRedisKeyConst.WithdrawalDevices, list);
            }
            else
            {
                if (cacheList != null)
                {
                    var info = cacheList.FirstOrDefault(r => r.Id == withdrawalDevice.Id);
                    if (info != null)
                    {
                        _newLifeRedis.ListRemove(NsPayRedisKeyConst.WithdrawalDevices + "_" + merchantCode, info);
                    }
                }
                _newLifeRedis.ListRightPush(NsPayRedisKeyConst.WithdrawalDevices + "_" + merchantCode, list);
            }
        }

        public void EditProcessWithdrawDevice(string merchantCode, WithdrawalDeviceRedisModel withdrawalDevice)
        {
            List<WithdrawalDeviceRedisModel> list = new List<WithdrawalDeviceRedisModel>
            {
                withdrawalDevice
            };

            var cacheList = GetListRangeDevice(merchantCode);
            if (withdrawalDevice.MerchantCode== NsPayRedisKeyConst.NsPay)
            {
                if (withdrawalDevice.Process == WithdrawalDevicesProcessTypeEnum.Process)
                {
                    if (cacheList != null)
                    {
                        var info = cacheList.FirstOrDefault(r => r.Id == withdrawalDevice.Id);
                        if (info == null)
                        {
                            SetRPushWithdrawDevice(merchantCode, withdrawalDevice);
                        }
                        else
                        {
                            _newLifeRedis.ListRemove(NsPayRedisKeyConst.WithdrawalDevices + "_" + merchantCode, info);
                            SetRPushWithdrawDevice(merchantCode, withdrawalDevice);
                        }
                    }
                    else
                    {
                        SetRPushWithdrawDevice(merchantCode, withdrawalDevice);
                    }
                }
                else
                {
                    if (cacheList != null)
                    {
                        var info = cacheList.FirstOrDefault(r => r.Id == withdrawalDevice.Id);
                        if (info != null)
                        {
                            _newLifeRedis.ListRemove(NsPayRedisKeyConst.WithdrawalDevices, info);
                        }
                    }
                }
            }
            else
            {
                if (withdrawalDevice.Process == WithdrawalDevicesProcessTypeEnum.Process)
                {
                    if (cacheList != null)
                    {
                        var info = cacheList.FirstOrDefault(r => r.Id == withdrawalDevice.Id);
                        if (info == null)
                        {
                            SetRPushWithdrawDevice(merchantCode, withdrawalDevice);
                        }
                        else
                        {
                            _newLifeRedis.ListRemove(NsPayRedisKeyConst.WithdrawalDevices + "_" + merchantCode, info);
                            SetRPushWithdrawDevice(merchantCode, withdrawalDevice);
                        }
                    }
                    else
                    {
                        SetRPushWithdrawDevice(merchantCode, withdrawalDevice);
                    }
                }
                else
                {
                    if (cacheList != null)
                    {
                        var info = cacheList.FirstOrDefault(r => r.Id == withdrawalDevice.Id);
                        if (info != null)
                        {
                            _newLifeRedis.ListRemove(NsPayRedisKeyConst.WithdrawalDevices + "_" + merchantCode, info);
                        }
                    }
                }
            }
        }

        public void DeleteWithdrawDevice(string merchantCode, WithdrawalDeviceRedisModel withdrawalDevice)
        {
            var cacheList = GetListRangeDevice(merchantCode);
            if (withdrawalDevice.MerchantCode == NsPayRedisKeyConst.NsPay)
            {
                if (cacheList != null)
                {
                    var info = cacheList.FirstOrDefault(r => r.Id == withdrawalDevice.Id);
                    if (info != null)
                    {
                        _newLifeRedis.ListRemove(NsPayRedisKeyConst.WithdrawalDevices, info);
                    }
                }
            }
            else
            {
                if (cacheList != null)
                {
                    var info = cacheList.FirstOrDefault(r => r.Id == withdrawalDevice.Id);
                    if (info != null)
                    {
                        _newLifeRedis.ListRemove(NsPayRedisKeyConst.WithdrawalDevices + "_" + merchantCode, info);
                    }
                }
            }
        }

        public WithdrawalDeviceRedisModel GetLPushWithdrawDevice(string merchantCode)
        {
            try
            {
                var internalWithdrawMerchant = GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.InternalWithdrawMerchant);
                if (internalWithdrawMerchant.Contains(merchantCode))
                {
                    if (_newLifeRedis.ContainsKey("WithdrawalDevice:Devices_" + merchantCode))
                    {
                        var value = _newLifeRedis.ListLeftPop<WithdrawalDeviceRedisModel>(NsPayRedisKeyConst.WithdrawalDevices + "_" + merchantCode);
                        return value;
                    }
                }
                else
                {
                    if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.WithdrawalDevices))
                    {
                        var value = _newLifeRedis.ListLeftPop<WithdrawalDeviceRedisModel>(NsPayRedisKeyConst.WithdrawalDevices);
                        return value;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<WithdrawalDeviceRedisModel> GethWithdrawDeviceAll()
        {
            List<WithdrawalDeviceRedisModel> value = new List<WithdrawalDeviceRedisModel>();
            if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.WithdrawalDevices))
            {
                var nspay = _newLifeRedis.ListGetAll<WithdrawalDeviceRedisModel>(NsPayRedisKeyConst.WithdrawalDevices);
                value.AddRange(nspay);
            }
            var internalWithdrawMerchant = GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.InternalWithdrawMerchant);
            if (internalWithdrawMerchant != null && !internalWithdrawMerchant.IsNullOrEmpty())
            {
                var strList = internalWithdrawMerchant.Split(",");
                foreach (var str in strList)
                {
                    if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.WithdrawalDevices + "_" + str))
                    {
                        var tempList = _newLifeRedis.ListGetAll<WithdrawalDeviceRedisModel>(NsPayRedisKeyConst.WithdrawalDevices + "_" + str);
                        value.AddRange(tempList);
                    }
                }
            }
            return value;
        }

        #endregion

        #region 出款订单

        public void SetRPushTransferOrder(string bankType, string phone, WithdrawalOrderRedisModel withdrawalOrderId)
        {
            List<WithdrawalOrderRedisModel> list = new List<WithdrawalOrderRedisModel>
            {
                withdrawalOrderId
            };
            _newLifeRedis.ListRightPush(bankType + "TransfserOrders:Devices_" + phone, list);
        }

        public bool RemoveTransferOrder(string bankType, string phone, WithdrawalOrderRedisModel withdrawalOrderId)
        {
            return  _newLifeRedis.ListRemove<WithdrawalOrderRedisModel>(bankType + "TransfserOrders:Devices_" + phone, withdrawalOrderId);
        }

        public void SetVcbTransferOrder(string devicePhone, VcbTransferOrder transferOrder)
        {
            _newLifeRedis.Set("VcbTransferOrder:Devices_" + devicePhone, transferOrder.ToJsonString());
        }

        public VcbTransferOrder GetVcbTransferOrder(string devicePhone)
        {
            var value = _newLifeRedis.Get<VcbTransferOrder>("VcbTransferOrder:Devices_" + devicePhone);
            return value;
        }

        public void RemoveVcbTransferOrder(string devicePhone)
        {
            _newLifeRedis.Remove("VcbTransferOrder:Devices_" + devicePhone);
        }

        public void SetTcbTransferOrder(string devicePhone, TcbTransferOrder transferOrder)
        {
            _newLifeRedis.Set("TcbTransferOrder:Devices_" + devicePhone, transferOrder.ToJsonString());
        }

        public TcbTransferOrder GeTcbTransferOrder(string devicePhone)
        {
            var value = _newLifeRedis.Get<TcbTransferOrder>("TcbTransferOrder:Devices_" + devicePhone);
            return value;
        }

        public void RemoveTcbTransferOrder(string devicePhone)
        {
            _newLifeRedis.Remove("TcbTransferOrder:Devices_" + devicePhone);
        }

        public WithdrawalOrderRedisModel GetLPushTransferOrder(string bankType, string phone)
        {
            try
            {
                if (_newLifeRedis.ContainsKey(bankType + "TransfserOrders:Devices_" + phone))
                {
                    var value = _newLifeRedis.ListLeftPop<WithdrawalOrderRedisModel>(bankType + "TransfserOrders:Devices_" + phone);
                    return value;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void CheckAndDeleteTransferOrder(string bankType, string phone,string orderId)
        {
            try
            {
                if (_newLifeRedis.ContainsKey(bankType + "TransfserOrders:Devices_" + phone))
                {
                    var lists = _newLifeRedis.ListGetAll<WithdrawalOrderRedisModel>(bankType + "TransfserOrders:Devices_" + phone);
                    if(lists.Count > 0)
                    {
                        var check = lists.FirstOrDefault(r => r.Id == orderId);
                        if(check != null)
                        {
                            check.OrderMoney = Convert.ToDecimal(check.OrderMoney.ToString("F2"));
                            _newLifeRedis.ListRemove(bankType + "TransfserOrders:Devices_" + phone, check);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion


        #region 出款账号登陆操作

        private string accountLoginActionRedisKey(string bankCard, PayMentTypeEnum payType, string merchantCode)
        {
            return "AccountLogin:" + payType.ToString() + "BankCard_" + bankCard + "_Merchant_" + merchantCode;
        }
        public void SetAccountLoginAction( string bankCard , PayMentTypeEnum payType , string merchantCode , AccountLoginActionRedisModel model )
        {
            _newLifeRedis.Set<AccountLoginActionRedisModel>(accountLoginActionRedisKey(bankCard, payType, merchantCode), model, DateTime.Now.AddMinutes(2) - DateTime.Now);

        }

        public AccountLoginActionRedisModel GetAccountLoginAction(string bankCard, PayMentTypeEnum payType, string merchantCode)
        {
            return _newLifeRedis.Get<AccountLoginActionRedisModel>(accountLoginActionRedisKey(bankCard, payType, merchantCode));
        }

        public void RemoveAccountLoginAction(string bankCard, PayMentTypeEnum payType, string merchantCode)
        {
             _newLifeRedis.Remove(accountLoginActionRedisKey(bankCard, payType, merchantCode));
        }


        #endregion

        #region 缓存出款流水编号

        public string GetMerchantBillTrasferOrder(string merchantCode, string payorderId)
        {
            return _newLifeRedis.Get<string>("MerchantBill:" + merchantCode + "TrasferBillNo_" + payorderId);
        }

        public void SetMerchantBillTrasferOrder(string merchantCode, string payorderId)
        {
            _newLifeRedis.Set("MerchantBill:" + merchantCode + "TrasferBillNo_" + payorderId, payorderId, DateTime.Now.AddHours(2) - DateTime.Now);
        }

        #endregion

        #region Telegram
        public void SetTelegramMessage(string merchantCode, BankOrderNotifyModel bankOrderNotify)
        {
            List<BankOrderNotifyModel> list = new List<BankOrderNotifyModel>
            {
                bankOrderNotify
            };
            _newLifeRedis.ListRightPush<BankOrderNotifyModel>(NsPayRedisKeyConst.TelegramMessage + merchantCode, list);
        }

        public List<BankOrderNotifyModel> GetTelegramMessage(string merchantCode)
        {
            try
            {
                if (_newLifeRedis.ContainsKey(NsPayRedisKeyConst.TelegramMessage + merchantCode))
                {
                    var value = _newLifeRedis.ListGetAll<BankOrderNotifyModel>(NsPayRedisKeyConst.TelegramMessage + merchantCode);
                    if (value.Count() <= 0)
                    {
                        return null;
                    }
                    return value;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        #region Excel
        public void SetWithdrawalOrderExcel(string username, string input)
        {
            _newLifeRedis.Set(NsPayRedisKeyConst.PayWithdrawalOrderExcel + username, input, DateTime.Now.AddMinutes(10) - DateTime.Now);
        }

        public string GetWithdrawalOrderExcel(string username)
        {
            return _newLifeRedis.Get<string>(NsPayRedisKeyConst.PayWithdrawalOrderExcel + username);
        }
        public void SetPayDepositExcel(string username, string input)
        {
            _newLifeRedis.Set(NsPayRedisKeyConst.PayDepositExcel + username, input, DateTime.Now.AddMinutes(10) - DateTime.Now);
        }

        public string GetPayDepositExcel(string username)
        {
            return _newLifeRedis.Get<string>(NsPayRedisKeyConst.PayDepositExcel + username);
        }

        public void SetPayOrderExcel(string username, string input)
        {
            _newLifeRedis.Set(NsPayRedisKeyConst.PayOrderExcel + username, input, DateTime.Now.AddMinutes(10) - DateTime.Now);
        }

        public string GetPayOrderExcel(string username)
        {
            return _newLifeRedis.Get<string>(NsPayRedisKeyConst.PayOrderExcel + username);
        }

        public void SetMerchantBillsExcel(string username, string input)
        {
            _newLifeRedis.Set(NsPayRedisKeyConst.MerchantBillsExcel + username, input, DateTime.Now.AddMinutes(1) - DateTime.Now);
        }

        public string GetMerchantBillsExcel(string username)
        {
            return _newLifeRedis.Get<string>(NsPayRedisKeyConst.MerchantBillsExcel + username);
        }
        #endregion

        #region 出款设备余额缓存

        public WithdrawBalanceModel? GetWitdrawDeviceBalance(int deivceId)
        {
            return _newLifeRedis.HashGetOne<WithdrawBalanceModel?>(NsPayRedisKeyConst.WithdrawDeviceBalance + deivceId, deivceId.ToString());
        }

        public void SetWitdrawDeviceBalance(int deivceId, WithdrawBalanceModel bankBalanceModel)
        {
            _newLifeRedis.HashAdd(NsPayRedisKeyConst.WithdrawDeviceBalance + deivceId, deivceId.ToString(), bankBalanceModel);
        }

        #endregion

        #region 出款订单otp更新

        public TransferOrderOtpModel? GetTransferOrder(string orderId)
        {
            return _newLifeRedis.HashGetOne<TransferOrderOtpModel?>(NsPayRedisKeyConst.TransferOrders + orderId, orderId.ToString());
        }

        public void SetGetTransferOrderOtp(string orderId, TransferOrderOtpModel transferOrder)
        {
            _newLifeRedis.HashAdd(NsPayRedisKeyConst.TransferOrders + orderId, orderId.ToString(), transferOrder);
        }

        public void RemoveGetTransferOrderOtp(string orderId)
        {
            _newLifeRedis.HashDel<TransferOrderOtpModel>(NsPayRedisKeyConst.TransferOrders + orderId, orderId.ToString());
        }

        public void SetMolibeSuccessTransferOrder(int deviceId, string orderId)
        {
            _newLifeRedis.Set(NsPayRedisKeyConst.MolibeSuccessTransferOrder + orderId, deviceId + "-" + orderId, DateTime.Now.AddDays(2) - DateTime.Now);
        }

        public string GetMolibeSuccessTransferOrder(string orderId)
        {
            return _newLifeRedis.Get<string>(NsPayRedisKeyConst.MolibeSuccessTransferOrder + orderId);
        }

        #endregion


        public bool? AddWithdrawalOrderProcessLock(string orderId)
        {
            bool? isSuccess = null;
            string uniqueCacheKey = $"process-withdraw:{orderId}";

            try
            {
                isSuccess = _newLifeRedis.Set<bool>(uniqueCacheKey, true, TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {

            }

            return isSuccess;
        }

        public bool IsWithdrawalOrderProcessing(string orderId)
        {
            bool isSuccess = false;
            string uniqueCacheKey = $"process-withdraw:{orderId}";

            try
            {
                isSuccess = _newLifeRedis.Get<bool>(uniqueCacheKey);
            }
            catch (Exception ex)
            {

            }

            return isSuccess;
        }

        public bool AddTelegramNotify(TelegramNotifyModel notifyModel)
        {
            bool isSuccess = false;
            try
            {
               var result = _newLifeRedis.GetFullRedis().GetStream<TelegramNotifyModel>( NsPayRedisKeyConst.TelegramNotify).Add( notifyModel);
                isSuccess = !result.IsNullOrEmpty();
            }
            catch (Exception ex)
            {
            }

            return isSuccess;

        }

        public void TrimRedisStreamIfNeeded(string streamKey , int maxLen = 100)
        {
            try
            {
                // 获取 Stream 长度
                var len = _newLifeRedis.GetFullRedis().Execute(streamKey, (c, k) =>
                {
                    var result = c.Execute("XLEN", streamKey);
                    return result?.ToLong() ?? 0;
                }, false);

                // 超过阈值则清理
                 int maxLength = maxLen;
                if ((long)len > maxLength)
                {
                    _newLifeRedis.GetFullRedis().Execute(streamKey, (c, k) =>
                        c.Execute("XTRIM", streamKey, "MAXLEN", maxLength.ToString()), true);

                    NlogLogger.Info($"[STREAM] Redis Stream {streamKey} 超长，已裁剪至 {maxLength}");
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Warn($"[STREAM] Redis Stream 自动裁剪异常: {ex.Message}");
            }
        }
    }
}
