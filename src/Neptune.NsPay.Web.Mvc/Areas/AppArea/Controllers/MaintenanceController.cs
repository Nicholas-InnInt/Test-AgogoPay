using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Caching;
using Neptune.NsPay.Web.Areas.AppArea.Models.Maintenance;
using Neptune.NsPay.Web.Controllers;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_Host_Maintenance)]
    public class MaintenanceController : NsPayControllerBase
    {
        private readonly ICachingAppService _cachingAppService;

        public MaintenanceController(ICachingAppService cachingAppService)
        {
            _cachingAppService = cachingAppService;
        }

        public ActionResult Index()
        {
            var model = new MaintenanceViewModel
            {
                Caches = _cachingAppService.GetAllCaches().Items,
                CanClearAllCaches = _cachingAppService.CanClearAllCaches()
            };

            return View(model);
        }
    }
}