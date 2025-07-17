using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.Layout;
using Neptune.NsPay.Web.Views;

namespace Neptune.NsPay.Web.Areas.AppArea.Views.Shared.Components.
    AppAreaQuickThemeSelect
{
    public class AppAreaQuickThemeSelectViewComponent : NsPayViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(string cssClass, string iconClass = "flaticon-interface-7 fs-2")
        {
            return Task.FromResult<IViewComponentResult>(View(new QuickThemeSelectionViewModel
            {
                CssClass = cssClass,
                IconClass = iconClass
            }));
        }
    }
}
