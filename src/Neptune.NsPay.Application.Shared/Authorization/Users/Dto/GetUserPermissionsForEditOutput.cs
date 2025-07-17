using System.Collections.Generic;
using Neptune.NsPay.Authorization.Permissions.Dto;

namespace Neptune.NsPay.Authorization.Users.Dto
{
    public class GetUserPermissionsForEditOutput
    {
        public List<FlatPermissionDto> Permissions { get; set; }

        public List<string> GrantedPermissionNames { get; set; }
    }
}