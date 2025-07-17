using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.Gdpr
{
    public interface IUserCollectedDataProvider
    {
        Task<List<FileDto>> GetFiles(UserIdentifier user);
    }
}
