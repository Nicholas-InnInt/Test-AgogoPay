using MongoDB.Driver;
using MongoDB.Entities;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantFunds;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Stripe;

namespace Neptune.NsPay.MongoDbExtensions.Services
{
    public class MerchantFundsMongoService : MongoBaseService<MerchantFundsMongoEntity>, IMerchantFundsMongoService, IDisposable
    {
        public MerchantFundsMongoService() { }

        public async Task<List<MerchantFundsMongoEntity>> GetFundsAll()
        {
            var result = await DB.Find<MerchantFundsMongoEntity>()
                                 .ExecuteAsync();
            return result;
        }

        public async Task<MerchantFundsMongoEntity?> GetFundsByMerchantCode(string merchantCode)
        {
            var result = await DB.Find<MerchantFundsMongoEntity>()
                                 .Match(r => r.MerchantCode == merchantCode)
                                 .ExecuteSingleAsync();
            return result;
        }

        public async Task<List<MerchantFundsMongoEntity>> GetFundsByUserMerchant(List<int> merchantIds)
        {
            if (merchantIds.Count == 1)
            {
                var result = await DB.Find<MerchantFundsMongoEntity>()
                     .Match(f => f.Eq(a => a.MerchantId, merchantIds.FirstOrDefault()))
                     .ExecuteAsync();
                return result;
            }
            else
            {
                var result = await DB.Find<MerchantFundsMongoEntity>()
                                     .Match(f => f.In(a => a.MerchantId, merchantIds))
                                     .ExecuteAsync();
                return result;
            }
        }
        public async Task<bool> ResetMerchantFrozenBalance(string merchantCode , decimal frozenBalance)
        {
            var currentFundInfo = await DB.Find<MerchantFundsMongoEntity>().Match(r => r.MerchantCode == merchantCode).ExecuteFirstAsync();
            MerchantFundsMongoEntity latestFundInfo = null;
            var haveUpdated = false;
            try
            {
                if (currentFundInfo != null)
                {

                    var cDate = DateTime.Now;
                    var filter = Builders<MerchantFundsMongoEntity>.Filter.And(
                        Builders<MerchantFundsMongoEntity>.Filter.Eq(x => x.ID, currentFundInfo.ID),
                        Builders<MerchantFundsMongoEntity>.Filter.Eq(x => x.VersionNumber, currentFundInfo.VersionNumber)
                        );
                    var update = Builders<MerchantFundsMongoEntity>.Update
                  .Set(x => x.FrozenBalance, frozenBalance)
                  .Set(x => x.UpdateUnixTime, TimeHelper.GetUnixTimeStamp(cDate))
                  .Inc(x => x.VersionNumber, 1);


                    var options = new FindOneAndUpdateOptions<MerchantFundsMongoEntity>
                    {
                        ReturnDocument = ReturnDocument.After // return updated document
                    };

                    var updated = await DB.Collection<MerchantFundsMongoEntity>().FindOneAndUpdateAsync(filter, update, options);

                    if(updated!=null && updated.VersionNumber > currentFundInfo.VersionNumber)
                    {
                        haveUpdated = true;
                    }
                }
                   
            }
            catch (Exception ex)
            {
               
                NlogLogger.Error("ResetMerchantFrozenBalance Error ", ex);
            }

            return haveUpdated;
        }

        public void Dispose()
        {
        }
    }
}
