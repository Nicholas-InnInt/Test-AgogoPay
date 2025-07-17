using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantFunds;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.RedisExtensions;

namespace Neptune.NsPay.BillingExtensions
{
    public class MerchantBalanceHelper : IMerchantBalanceHelper
    {
        public MerchantBalanceHelper()
        {
        }

        public async Task<bool> AddMerchantFundsFrozenBalance(string merchantCode, decimal orderMoney)
        {
            var result = await DB.Update<MerchantFundsMongoEntity>()
                .Match(r => r.MerchantCode == merchantCode)
                .Modify(b => b
                    .Inc(x => x.FrozenBalance, +orderMoney)
                    .Inc(x => x.VersionNumber, +1)
                    .Set(x => x.UpdateTime, DateTime.Now)
                    .Set(x => x.UpdateUnixTime, TimeHelper.GetUnixTimeStamp(DateTime.Now)))
                .ExecuteAsync();

            return result.ModifiedCount > 0;
        }

        public async Task<bool> UnlockFrozenBalanceAsync(string merchantCode, decimal amount)
        {
            var result = await DB.Update<MerchantFundsMongoEntity>()
                .Match(x => x.MerchantCode == merchantCode && x.FrozenBalance >= amount)
                .Modify(b => b
                    .Inc(x => x.FrozenBalance, -amount)
                    .Set(x => x.UpdateTime, DateTime.Now)
                    .Inc(x => x.VersionNumber, +1)
                    .Set(x => x.UpdateUnixTime, TimeHelper.GetUnixTimeStamp(DateTime.Now)))
                .ExecuteAsync();

            if (result.ModifiedCount == 0)
            {
                NlogLogger.Warn($"[WARN] 解冻失败，冻结余额不足，商户：{merchantCode}，尝试解冻：{amount}");
            }

            return result.ModifiedCount > 0;
        }
    }
}