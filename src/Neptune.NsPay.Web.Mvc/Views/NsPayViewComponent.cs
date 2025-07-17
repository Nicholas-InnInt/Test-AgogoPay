using Abp.AspNetCore.Mvc.ViewComponents;

namespace Neptune.NsPay.Web.Views
{
    public abstract class NsPayViewComponent : AbpViewComponent
    {
        protected NsPayViewComponent()
        {
            LocalizationSourceName = NsPayConsts.LocalizationSourceName;
        }
    }
}