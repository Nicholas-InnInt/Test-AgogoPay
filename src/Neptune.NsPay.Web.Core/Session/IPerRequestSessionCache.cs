using System.Threading.Tasks;
using Neptune.NsPay.Sessions.Dto;

namespace Neptune.NsPay.Web.Session
{
    public interface IPerRequestSessionCache
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformationsAsync();
    }
}
