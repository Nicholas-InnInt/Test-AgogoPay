using Abp;
using Abp.Domain.Services;
using System.Threading.Tasks;

namespace Neptune.NsPay.Authorization.Users.Profile
{
    public interface IProfileImageService : IDomainService
    {
        Task<string> GetProfilePictureContentForUser(UserIdentifier userIdentifier);
    }
}