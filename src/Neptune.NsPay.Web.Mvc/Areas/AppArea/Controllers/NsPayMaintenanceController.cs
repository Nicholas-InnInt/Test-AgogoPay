using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.NsPayMaintenance;
using Neptune.NsPay.Web.Areas.AppArea.Models.NsPayMaintenance;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_NsPayMaintenance)]
    public class NsPayMaintenanceController : NsPayControllerBase
    {
        private readonly INsPayMaintenanceAppService _nsPayMaintenanceAppService;

        public NsPayMaintenanceController(INsPayMaintenanceAppService nsPayMaintenanceAppService)
        {
            _nsPayMaintenanceAppService = nsPayMaintenanceAppService;

        }

        public ActionResult Index()
        {
            var model = new NsPayMaintenanceViewModel
            {
                Caches = _nsPayMaintenanceAppService.GetAllCaches().Items,
            };

            return View(model);
        }
    }
}