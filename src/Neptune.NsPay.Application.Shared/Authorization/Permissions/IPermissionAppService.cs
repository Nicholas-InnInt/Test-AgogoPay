using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization.Permissions.Dto;

namespace Neptune.NsPay.Authorization.Permissions
{
    public interface IPermissionAppService : IApplicationService
    {
        ListResultDto<FlatPermissionWithLevelDto> GetAllPermissions();
    }
}
