using System.Collections.Generic;
using Neptune.NsPay.Authorization.Permissions.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Common
{
    public interface IPermissionsEditViewModel
    {
        List<FlatPermissionDto> Permissions { get; set; }

        List<string> GrantedPermissionNames { get; set; }
    }
}