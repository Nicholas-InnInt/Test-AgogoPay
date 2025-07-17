using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.Web.Areas.AppArea.Models.MerchantBills;
using Neptune.NsPay.Web.Controllers;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_MerchantBills)]
    public class MerchantBillsCryptoController : NsPayControllerBase
    {
        private readonly IMerchantBillsAppService _merchantBillsAppService;

        public MerchantBillsCryptoController(IMerchantBillsAppService merchantBillsAppService)
        {
            _merchantBillsAppService = merchantBillsAppService;
        }

        public IActionResult Index()
        {
            var model = new MerchantBillsViewModel
            {
                FilterText = "",
                IsShowMerchant = _merchantBillsAppService.IsShowMerchantFilter()
            };

            return View(model);
        }
    }
}