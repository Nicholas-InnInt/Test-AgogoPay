using Abp.Domain.Services;

namespace Neptune.NsPay
{
    public abstract class NsPayDomainServiceBase : DomainService
    {
        /* Add your common members for all your domain services. */

        protected NsPayDomainServiceBase()
        {
            LocalizationSourceName = NsPayConsts.LocalizationSourceName;
        }
    }
}
