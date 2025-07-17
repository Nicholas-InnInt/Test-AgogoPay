using Abp.MultiTenancy;
using Neptune.NsPay.Url;

namespace Neptune.NsPay.Web.Url
{
    public class MvcAppUrlService : AppUrlServiceBase
    {
        public override string EmailActivationRoute => "Account/EmailConfirmation";
        
        public override string EmailChangeRequestRoute => "Account/EmailChangeRequest";

        public override string PasswordResetRoute => "Account/ResetPassword";

        public MvcAppUrlService(
                IWebUrlService webUrlService,
                ITenantCache tenantCache
            ) : base(
                webUrlService,
                tenantCache
            )
        {

        }
    }
}