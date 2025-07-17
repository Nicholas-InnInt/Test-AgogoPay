using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers
{
    public class BankStateHelper : IBankStateHelper
    {
        private readonly IRedisService _redisService;

        public BankStateHelper(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public int GetPayState(string account, PayMentTypeEnum paytype)
        {
            try
            {
                var bankCacheKey = CommonHelper.GetBankCacheBankKey(paytype, account);

                if (paytype is PayMentTypeEnum.MBBank)
                {
                    var sessionid = _redisService.GetRedisValue<string>(bankCacheKey);
                    return string.IsNullOrEmpty(sessionid) ? 0 : 1;
                }

                object cacheObject = paytype switch
                {
                    // Special login model for specific banks
                    PayMentTypeEnum.VietinBank => _redisService.GetRedisValue<VtbBankLoginModel>(bankCacheKey),
                    PayMentTypeEnum.TechcomBank => _redisService.GetRedisValue<TechcomBankLoginToken>(bankCacheKey),
                    PayMentTypeEnum.VietcomBank => _redisService.GetRedisValue<VietcomBankLoginResponse>(bankCacheKey),
                    PayMentTypeEnum.BidvBank => _redisService.GetRedisValue<BidvBankLoginResponse>(bankCacheKey),
                    PayMentTypeEnum.ACBBank => _redisService.GetRedisValue<AcbBankLoginModel>(bankCacheKey),
                    PayMentTypeEnum.PVcomBank => _redisService.GetRedisValue<PVcomBankLoginModel>(bankCacheKey),
                    PayMentTypeEnum.BusinessMbBank => _redisService.GetRedisValue<BusinessMBBankLoginModel>(bankCacheKey),
                    PayMentTypeEnum.BusinessTcbBank => _redisService.GetRedisValue<BusinessTechcomBankLoginToken>(bankCacheKey),
                    PayMentTypeEnum.BusinessVtbBank => _redisService.GetRedisValue<BusinessVtbBankLoginModel>(bankCacheKey),

                    // Common login model for other banks
                    _ => _redisService.GetRedisValue<BankLoginModel>(bankCacheKey)
                };

                return cacheObject is not null ? 1 : 0;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Redis异常：" + ex.ToString());
                return 0;
            }
        }

        public string GetSessionId(string account, PayMentTypeEnum paytype)
        {
            try
            {
                var bankCacheKey = CommonHelper.GetBankCacheBankKey(paytype, account);

                var sessionId = paytype switch
                {
                    PayMentTypeEnum.MBBank => _redisService.GetRedisValue<MbBankLoginModel>(bankCacheKey)?.SessionId,
                    PayMentTypeEnum.TechcomBank => _redisService.GetRedisValue<TechcomBankLoginToken>(bankCacheKey)?.session_state,
                    PayMentTypeEnum.BidvBank => _redisService.GetRedisValue<BidvBankLoginResponse>(bankCacheKey)?.paySessionModel?.Token,
                    PayMentTypeEnum.VietinBank => _redisService.GetRedisValue<VtbBankLoginModel>(bankCacheKey)?.sessionid,
                    PayMentTypeEnum.SeaBank => _redisService.GetRedisValue<BankLoginModel>(bankCacheKey)?.Token,
                    PayMentTypeEnum.MsbBank => _redisService.GetRedisValue<BankLoginModel>(bankCacheKey)?.Token,
                    _ => null
                };

                return sessionId ?? "";
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Redis异常：" + ex.ToString());
            }

            return "";
        }


        public string GetAccount(string cardNumber, PayMentTypeEnum paytype)
        {
            var bankCacheKey = CommonHelper.GetBankCacheBankKey(paytype, cardNumber);
            return paytype switch
            {
                // Special login model for specific banks
                PayMentTypeEnum.VietinBank => _redisService.GetRedisValue<VtbBankLoginModel>(bankCacheKey)?.Account,
                PayMentTypeEnum.BidvBank => _redisService.GetRedisValue<BidvBankLoginResponse>(bankCacheKey)?.Account,
                PayMentTypeEnum.VietcomBank => _redisService.GetRedisValue<VietcomBankLoginResponse>(bankCacheKey)?.Account,
                PayMentTypeEnum.TechcomBank => _redisService.GetRedisValue<TechcomBankLoginToken>(bankCacheKey)?.Account,
                PayMentTypeEnum.BusinessTcbBank => _redisService.GetRedisValue<BusinessTechcomBankLoginToken>(bankCacheKey)?.Account,

                // Common login model for other banks
                _ => _redisService.GetRedisValue<BankLoginModel>(bankCacheKey)?.Account
            } ?? "";
        }
    }
}