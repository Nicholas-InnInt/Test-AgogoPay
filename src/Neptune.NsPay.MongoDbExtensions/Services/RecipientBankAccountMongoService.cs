using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Entities;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.MerchantBills;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Common;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantBills.Dtos;
using Twilio.Rest.Video.V1.Room.Participant;
using Neptune.NsPay.RecipientBankAccounts.Dtos;
using System.Linq.Expressions;

namespace Neptune.NsPay.MongoDbExtensions.Services
{
    public class RecipientBankAccountMongoService : MongoBaseService<RecipientBankAccountMongoEntity>, IRecipientBankAccountMongoService, IDisposable
    {
        public RecipientBankAccountMongoService() { }

        public async Task<PagedResultDto<RecipientBankAccountMongoEntity>> GetAllWithPagination(GetAllRecipientBankAccountsInput input)
        {
            int pageNumber = input.SkipCount == 0 ? 1 : (input.SkipCount / input.MaxResultCount) + 1;
            int limit = input.MaxResultCount;
            int skip = (pageNumber - 1) * limit;

            var filters = new List<FilterDefinition<RecipientBankAccountMongoEntity>>();

            if (!string.IsNullOrEmpty(input.Filter))
            {
                var regexFilter = new BsonRegularExpression(input.Filter, "i"); // Case-insensitive regex

                filters.Add(
                    Builders<RecipientBankAccountMongoEntity>.Filter.Or(
                        Builders<RecipientBankAccountMongoEntity>.Filter.Regex(x => x.HolderName, regexFilter),
                        Builders<RecipientBankAccountMongoEntity>.Filter.Regex(x => x.AccountNumber, regexFilter),
                        Builders<RecipientBankAccountMongoEntity>.Filter.Regex(x => x.BankCode, regexFilter)
                    )
                );
            }

            if (!string.IsNullOrEmpty(input.HolderName))
            {
                filters.Add(Builders<RecipientBankAccountMongoEntity>.Filter.Regex(x => x.HolderName, new BsonRegularExpression(input.HolderName, "i")));
            }

            if (!string.IsNullOrEmpty(input.AccountNumber))
            {
                filters.Add(Builders<RecipientBankAccountMongoEntity>.Filter.Regex(x => x.AccountNumber, new BsonRegularExpression(input.AccountNumber, "i")));
            }

            if (!string.IsNullOrEmpty(input.BankCode))
            {
                filters.Add(Builders<RecipientBankAccountMongoEntity>.Filter.Regex(x => x.BankCode, new BsonRegularExpression(input.BankCode, "i")));
            }

            var combinedFilter = filters.Count > 0 ? Builders<RecipientBankAccountMongoEntity>.Filter.And(filters) : Builders<RecipientBankAccountMongoEntity>.Filter.Empty;

            int totalCount = (int)await DB.Collection<RecipientBankAccountMongoEntity>()
                                  .CountDocumentsAsync(combinedFilter);

            var recordSorting = Builders<RecipientBankAccountMongoEntity>.Sort.Descending(x => x.CreationUnixTime);

            var response = await DB.Collection<RecipientBankAccountMongoEntity>()
                           .Find(combinedFilter)
                           .Sort(recordSorting)
                           .Skip(skip)
                           .Limit(limit)
                           .ToListAsync();

            return new PagedResultDto<RecipientBankAccountMongoEntity>
            {
                Items = response,
                TotalCount = totalCount
            };
        }


        public async Task<List<RecipientBankAccountMongoEntity>> GetByAccountDetails(string bankCode, string accountNumber)
        {
            var filters = new List<FilterDefinition<RecipientBankAccountMongoEntity>>();

            filters.Add(Builders<RecipientBankAccountMongoEntity>.Filter.Eq(x => x.BankCode, bankCode));
            filters.Add(Builders<RecipientBankAccountMongoEntity>.Filter.Eq(x => x.AccountNumber, accountNumber));
            var combinedFilter = Builders<RecipientBankAccountMongoEntity>.Filter.And(filters);

            var response = await DB.Collection<RecipientBankAccountMongoEntity>()
                       .Find(combinedFilter)
                       .ToListAsync();


            return response;
        }

        public async Task<RecipientBankAccountMongoEntity> GetAsync(Expression<Func<RecipientBankAccountMongoEntity, bool>> predicate)
        {
            return await DB.Collection<RecipientBankAccountMongoEntity>().Find(predicate).FirstOrDefaultAsync();
        }



        public void Dispose()
        {
        }
    }
}
