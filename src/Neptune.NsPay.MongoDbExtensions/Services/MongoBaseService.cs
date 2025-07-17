using System.Linq.Expressions;
using MongoDB.Entities;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using System.Linq.Expressions;

namespace Neptune.NsPay.MongoDbExtensions.Services
{
    public class MongoBaseService<T> : IMongoBaseService<T> where T : Entity
    {

        public async Task<string> AddAsync(T list)
        {
            await DB.InsertAsync(list);
            return list.ID;
        }

        public async Task AddListAsync(List<T> list)
        {
            await list.SaveAsync();
        }

        public async Task<T> GetById(string id)
        {
            return await DB.Find<T>().OneAsync(id);
        }

        public async Task<List<T>> GetListByIds(List<string> ids)
        {
            return await DB.Find<T>().ManyAsync(x => ids.Contains(x.ID));
        }

        public async Task<bool> UpdateAsync(T list)
        {

            var result = await DB.Update<T>()
                    .MatchID(list.ID)
                    .ModifyWith(list)
                    .ExecuteAsync();
            return result.IsModifiedCountAvailable;
        }

        public async Task DeleteAsync(string id)
        {
            var data = await DB.Find<T>().OneAsync(id);

            if (data != null)
                await DB.DeleteAsync<T>(id);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await DB.Find<T>().Match(predicate).ExecuteAnyAsync();
        }
    }
}
