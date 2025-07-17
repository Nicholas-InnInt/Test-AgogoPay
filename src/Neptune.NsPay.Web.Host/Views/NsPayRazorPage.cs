using Abp.AspNetCore.Mvc.Views;

namespace Neptune.NsPay.Web.Views
{
    public abstract class NsPayRazorPage<TModel> : AbpRazorPage<TModel>
    {
        protected NsPayRazorPage()
        {
            LocalizationSourceName = NsPayConsts.LocalizationSourceName;
        }
    }
}
