using System.Threading.Tasks;
using Abp.Domain.Policies;

namespace Neptune.NsPay.Authorization.Users
{
    public interface IUserPolicy : IPolicy
    {
        Task CheckMaxUserCountAsync(int tenantId);
    }
}
