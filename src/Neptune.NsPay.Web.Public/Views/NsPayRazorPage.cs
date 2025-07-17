using Abp.AspNetCore.Mvc.Views;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace Neptune.NsPay.Web.Public.Views
{
    public abstract class NsPayRazorPage<TModel> : AbpRazorPage<TModel>
    {
        [RazorInject]
        public IAbpSession AbpSession { get; set; }

        protected NsPayRazorPage()
        {
            LocalizationSourceName = NsPayConsts.LocalizationSourceName;
        }
    }
}
