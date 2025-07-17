using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization.Roles.Dto;
using Neptune.NsPay.Authorization.Users.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Authorization.Users
{
    public interface IMerchantUserAppService
    {
        Task DeleteMerchantUser(EntityDto<long> input);
        Task CreateOrUpdateMerchantUser(CreateOrUpdateUserInput input);

        Task<GetUserForEditOutput> GetMerchantUserForEdit(NullableIdDto<long> input);

        Task<PagedResultDto<UserListDto>> GetMerchantUsers(GetUsersInput input);

        Task<List<RoleListDto>> GetMerchantUserRolesList();
        Task UnlockUser(EntityDto<long> input);
    }
}
