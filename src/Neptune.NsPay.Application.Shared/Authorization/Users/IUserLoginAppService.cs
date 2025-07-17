using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization.Users.Dto;

namespace Neptune.NsPay.Authorization.Users
{
    public interface IUserLoginAppService : IApplicationService
    {
        Task<PagedResultDto<UserLoginAttemptDto>> GetUserLoginAttempts(GetLoginAttemptsInput input);
    }
}
