using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers
{
    public class BankHelper : IBankHelper
    {
        private readonly IRedisService _redisService;

        public BankHelper(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public void SetBankSessionId(PayMentTypeEnum type, string account, BankLoginModel loginModel)
        {
            _redisService.AddRedisValue<BankLoginModel>(CommonHelper.GetBankCacheBankKey(type, account), loginModel);
        }

        public void RemoveBankSessionId(PayMentTypeEnum type, string account)
        {
            _redisService.RemoveRedisValue(CommonHelper.GetBankCacheBankKey(type, account));
        }

        public BankLoginModel GetBankSessionId(PayMentTypeEnum type, string account)
        {
            return _redisService.GetRedisValue<BankLoginModel>(CommonHelper.GetBankCacheBankKey(type, account));
        }

        public string GetLastRefNoKey(PayMentTypeEnum type, string account)
        {
            return _redisService.GetRedisValue<string>(CommonHelper.GetBankLastRefNoKey(type, account));
        }
        public void SetLastRefNoKey(PayMentTypeEnum type, string account, string refno)
        {
            _redisService.AddRedisValue<string>(CommonHelper.GetBankLastRefNoKey(type, account), refno);
        }
    }
}