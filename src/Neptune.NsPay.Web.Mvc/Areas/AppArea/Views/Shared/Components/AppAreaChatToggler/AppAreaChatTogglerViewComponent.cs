using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.Layout;
using Neptune.NsPay.Web.Views;

namespace Neptune.NsPay.Web.Areas.AppArea.Views.Shared.Components.AppAreaChatToggler
{
    public class AppAreaChatTogglerViewComponent : NsPayViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(string cssClass, string iconClass = "flaticon-chat-2 fs-4")
        {
            return Task.FromResult<IViewComponentResult>(View(new ChatTogglerViewModel
            {
                CssClass = cssClass,
                IconClass = iconClass
            }));
        }
    }
}
