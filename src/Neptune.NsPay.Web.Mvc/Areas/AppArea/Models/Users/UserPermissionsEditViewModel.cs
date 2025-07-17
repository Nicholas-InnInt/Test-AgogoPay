using Abp.AutoMapper;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Authorization.Users.Dto;
using Neptune.NsPay.Web.Areas.AppArea.Models.Common;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Users
{
    [AutoMapFrom(typeof(GetUserPermissionsForEditOutput))]
    public class UserPermissionsEditViewModel : GetUserPermissionsForEditOutput, IPermissionsEditViewModel
    {
        public User User { get; set; }
    }
}