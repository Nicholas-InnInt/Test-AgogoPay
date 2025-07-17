using Abp.Zero.Ldap.Authentication;
using Abp.Zero.Ldap.Configuration;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.MultiTenancy;

namespace Neptune.NsPay.Authorization.Ldap
{
    public class AppLdapAuthenticationSource : LdapAuthenticationSource<Tenant, User>
    {
        public AppLdapAuthenticationSource(ILdapSettings settings, IAbpZeroLdapModuleConfig ldapModuleConfig)
            : base(settings, ldapModuleConfig)
        {
        }
    }
}