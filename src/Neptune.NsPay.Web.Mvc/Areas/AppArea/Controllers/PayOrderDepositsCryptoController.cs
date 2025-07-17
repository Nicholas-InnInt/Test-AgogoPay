using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrderDeposits;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.Web.Areas.AppArea.Models.PayOrderDeposits;
using Neptune.NsPay.Web.Controllers;
using System;
using System.Threading.Tasks;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_PayOrderDeposits)]
    public class PayOrderDepositsCryptoController : NsPayControllerBase
    {
        private readonly IPayOrderDepositsAppService _payOrderDepositsAppService;

        public PayOrderDepositsCryptoController(IPayOrderDepositsAppService payOrderDepositsAppService)
        {
            _payOrderDepositsAppService = payOrderDepositsAppService;
        }

        public IActionResult Index()
        {
            var model = new PayOrderDepositsViewModel
            {
                FilterText = "",
                IsShowMerchant = _payOrderDepositsAppService.IsShowMerchantFilter()
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PayOrderDeposits_Edit)]
        public async Task<PartialViewResult> AssociatedOrderModal(string? id, string payType)
        {
            GetAssociatedDepositOrderOutput getAssociatedOrderOutput = new GetAssociatedDepositOrderOutput();

            if (!string.IsNullOrEmpty(id))
            {
                getAssociatedOrderOutput = await _payOrderDepositsAppService.GetAssociatedOrder(new EntityDto<string?> { Id = id }, (PayMentTypeEnum)Convert.ToInt32(payType));
            }

            var viewModel = new AssociatedDepositOrderViewModel()
            {
                AssociatedOrder = getAssociatedOrderOutput.AssociatedOrder
            };

            return PartialView("_AssociatedOrderModal", viewModel);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PayOrderDeposits_Edit)]
        public async Task<PartialViewResult> RejectOrderModal(string id, string payType)
        {
            var getRejectDepositOrderOutput = new GetRejectDepositOrderOutput();

            if (!string.IsNullOrEmpty(id))
            {
                getRejectDepositOrderOutput = await _payOrderDepositsAppService.GetRejectOrder(new EntityDto<string> { Id = id }, (PayMentTypeEnum)Convert.ToInt32(payType));
            }

            var viewModel = new RejectDepositOrderViewModel()
            {
                RejectOrder = getRejectDepositOrderOutput.RejectOrder
            };
            return PartialView("_RejectOrderModal", viewModel);
        }

        public async Task<PartialViewResult> ViewPayOrderDepositModal(string id)
        {
            var getPayOrderForViewDto = await _payOrderDepositsAppService.GetPayOrderDepositForView(id);

            var model = new PayOrderDepositViewModel()
            {
                PayOrderDeposit = getPayOrderForViewDto.PayOrderDeposit,
                PayOrder = getPayOrderForViewDto.PayOrder
            };

            return PartialView("_ViewPayOrderDepositsModal", model);
        }
    }
}