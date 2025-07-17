using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Web.Areas.AppArea.Models.WithdrawlDashboard;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_MerchantDashboard)]
    public class MerchantDashboardController : NsPayControllerBase
    {

        public MerchantDashboardController()
        {

        }

        public ActionResult Index()
        {
            var model = new MerchantDashboardViewModel
            {

            };

            return View(model);
        }

    }
}