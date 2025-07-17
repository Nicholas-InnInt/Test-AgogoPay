using Neptune.NsPay.MongoDbExtensions.Models;

namespace Neptune.NsPay.MongoDbExtensions.Services.Interfaces
{
    public interface IMerchantFundsMongoService : IMongoBaseService<MerchantFundsMongoEntity>
    {

        Task<List<MerchantFundsMongoEntity>> GetFundsAll();

        Task<MerchantFundsMongoEntity?> GetFundsByMerchantCode(string merchantCode);
        Task<List<MerchantFundsMongoEntity>> GetFundsByUserMerchant(List<int> merchantIds);
        Task<bool> ResetMerchantFrozenBalance(string merchantCode, decimal frozenBalance);
    }
}
