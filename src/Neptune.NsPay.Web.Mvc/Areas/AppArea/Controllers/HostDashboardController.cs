using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.DashboardCustomization;
using System.Threading.Tasks;
using Neptune.NsPay.Web.Areas.AppArea.Startup;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_Host_Dashboard)]
    public class HostDashboardController : CustomizableDashboardControllerBase
    {
        public HostDashboardController(
            DashboardViewConfiguration dashboardViewConfiguration,
            IDashboardCustomizationAppService dashboardCustomizationAppService)
            : base(dashboardViewConfiguration, dashboardCustomizationAppService)
        {

        }

        public async Task<ActionResult> Index()
        {
            return await GetView(NsPayDashboardCustomizationConsts.DashboardNames.DefaultHostDashboard);
        }
    }
}