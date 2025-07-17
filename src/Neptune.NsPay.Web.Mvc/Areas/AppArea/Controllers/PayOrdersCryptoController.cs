using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.Web.Areas.AppArea.Models.PayOrders;
using Neptune.NsPay.Web.Controllers;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_PayOrders)]
    public class PayOrdersCryptoController : NsPayControllerBase
    {
        private readonly IPayOrdersAppService _payOrdersAppService;

        public PayOrdersCryptoController(IPayOrdersAppService payOrdersAppService)
        {
            _payOrdersAppService = payOrdersAppService;
        }

        public IActionResult Index()
        {
            var model = new PayOrdersViewModel
            {
                FilterText = "",
                IsShowMerchant = _payOrdersAppService.IsShowMerchantFilter()
            };

            return View(model);
        }
    }
}
