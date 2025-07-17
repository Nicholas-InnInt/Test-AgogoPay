using MongoDB.Entities;
using System.Linq.Expressions;

namespace Neptune.NsPay.MongoDbExtensions.Services.Interfaces
{
    public interface IMongoBaseService<T> where T : Entity
    {
        Task<string> AddAsync(T list);
        Task AddListAsync(List<T> list);
        Task<T> GetById(string id);
        Task<List<T>> GetListByIds(List<string> ids);
        Task<bool> UpdateAsync(T list);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }
}