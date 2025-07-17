using Abp.AspNetCore.Mvc.ViewComponents;

namespace Neptune.NsPay.Web.Public.Views
{
    public abstract class NsPayViewComponent : AbpViewComponent
    {
        protected NsPayViewComponent()
        {
            LocalizationSourceName = NsPayConsts.LocalizationSourceName;
        }
    }
}