using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.Layout;
using Neptune.NsPay.Web.Session;
using Neptune.NsPay.Web.Views;

namespace Neptune.NsPay.Web.Areas.AppArea.Views.Shared.Themes.Theme2.Components.AppAreaTheme2Brand
{
    public class AppAreaTheme2BrandViewComponent : NsPayViewComponent
    {
        private readonly IPerRequestSessionCache _sessionCache;

        public AppAreaTheme2BrandViewComponent(IPerRequestSessionCache sessionCache)
        {
            _sessionCache = sessionCache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var headerModel = new HeaderViewModel
            {
                LoginInformations = await _sessionCache.GetCurrentLoginInformationsAsync()
            };

            return View(headerModel);
        }
    }
}
