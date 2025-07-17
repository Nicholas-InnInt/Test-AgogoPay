using System.Linq.Expressions;
using Abp.Application.Services.Dto;
using Neptune.NsPay.MerchantBills.Dtos;
using Neptune.NsPay.MerchantDashboard.Dtos;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PayOrders.Dtos;
using Neptune.NsPay.RecipientBankAccounts.Dtos;

namespace Neptune.NsPay.MongoDbExtensions.Services.Interfaces
{
    public interface IRecipientBankAccountMongoService : IMongoBaseService<RecipientBankAccountMongoEntity>
    {
        Task<PagedResultDto<RecipientBankAccountMongoEntity>> GetAllWithPagination(GetAllRecipientBankAccountsInput input);

        Task<List<RecipientBankAccountMongoEntity>> GetByAccountDetails(string bankCode , string accountNumber);
        Task<RecipientBankAccountMongoEntity> GetAsync(Expression<Func<RecipientBankAccountMongoEntity, bool>> predicate);
    }
}
