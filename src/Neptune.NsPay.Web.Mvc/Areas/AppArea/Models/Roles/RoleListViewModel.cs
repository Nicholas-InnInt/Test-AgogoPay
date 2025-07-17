using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Authorization.Permissions.Dto;
using Neptune.NsPay.Web.Areas.AppArea.Models.Common;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Roles
{
    public class RoleListViewModel : IPermissionsEditViewModel
    {
        public List<FlatPermissionDto> Permissions { get; set; }

        public List<string> GrantedPermissionNames { get; set; }
    }
}