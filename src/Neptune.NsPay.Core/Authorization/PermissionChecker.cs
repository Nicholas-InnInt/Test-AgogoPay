using Abp.Authorization;
using Neptune.NsPay.Authorization.Roles;
using Neptune.NsPay.Authorization.Users;

namespace Neptune.NsPay.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {

        }
    }
}
