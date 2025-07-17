using System.Threading.Tasks;
using Abp.Application.Services;
using Neptune.NsPay.Sessions.Dto;

namespace Neptune.NsPay.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();

        Task<UpdateUserSignInTokenOutput> UpdateUserSignInToken();
    }
}
