using System.Collections.Generic;
using System.Linq;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Authorization.Users.Dto;
using Neptune.NsPay.Security;
using Neptune.NsPay.Web.Areas.AppArea.Models.Common;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Users
{
    [AutoMapFrom(typeof(GetUserForEditOutput))]
    public class CreateOrEditUserModalViewModel : GetUserForEditOutput, IOrganizationUnitsEditViewModel
    {
        public bool CanChangeUserName => User.UserName != AbpUserBase.AdminUserName;

        public int AssignedRoleCount
        {
            get { return Roles.Count(r => r.IsAssigned); }
        }

        public int AssignedMerchantCount
        {
            get { return Merechants.Count(r => r.IsAssigned); }
        }

        public int AssignedOrganizationUnitCount => MemberedOrganizationUnits.Count;

        public bool IsEditMode => User.Id.HasValue;

        public PasswordComplexitySetting PasswordComplexitySetting { get; set; }

        public List<UserTypeEnum> SupprtedUserType { get; set; }
    }
}