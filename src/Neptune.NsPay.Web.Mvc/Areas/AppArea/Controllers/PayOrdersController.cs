using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.PayOrders;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PayOrders.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using System.Collections.Generic;
using Neptune.NsPay.Web.Areas.AppArea.Models.WithdrawalOrders;
using Abp.Authorization;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_PayOrders)]
    public class PayOrdersController : NsPayControllerBase
    {
        private readonly IPayOrdersAppService _payOrdersAppService;

        public PayOrdersController(IPayOrdersAppService payOrdersAppService)
        {
            _payOrdersAppService = payOrdersAppService;

        }

        public ActionResult Index()
        {
            var model = new PayOrdersViewModel
            {
                FilterText = "",
                IsShowMerchant = _payOrdersAppService.IsShowMerchantFilter()
            };

            return View(model);
        }

        //[AbpMvcAuthorize(AppPermissions.Pages_PayOrders_Edit)]
        //public async Task<PartialViewResult> CreateOrEditModal(string id)
        //{
        //    GetPayOrderForEditOutput getPayOrderForEditOutput;

        //    if (!id.IsNullOrEmpty())
        //    {
        //        getPayOrderForEditOutput = await _payOrdersAppService.GetPayOrderForEdit(new EntityDto<string> { Id = id });
        //    }
        //    else
        //    {
        //        getPayOrderForEditOutput = new GetPayOrderForEditOutput
        //        {
        //            PayOrder = new CreateOrEditPayOrderDto()
        //        };
        //        getPayOrderForEditOutput.PayOrder.TransactionTime = DateTime.Now;
        //        getPayOrderForEditOutput.PayOrder.CreationTime = DateTime.Now;
        //    }

        //    var viewModel = new CreateOrEditPayOrderModalViewModel()
        //    {
        //        PayOrder = getPayOrderForEditOutput.PayOrder,
        //    };

        //    return PartialView("_CreateOrEditModal", viewModel);
        //}

        public async Task<PartialViewResult> ViewPayOrderModal(string id)
        {
            var getPayOrderForViewDto = await _payOrdersAppService.GetPayOrderForView(id);

            var model = new PayOrderViewModel()
            {
                PayOrder = getPayOrderForViewDto.PayOrder
            };

            return PartialView("_ViewPayOrderModal", model);
        }


        [AbpMvcAuthorize(AppPermissions.Pages_PayOrders_EnforceCallBcak)]
        public async Task<PartialViewResult> EditPayOrderModel(List<PayOrderBatchNotificationInput> input)
        {
            var model = new PayOrderModel();
            model.OrderTotalCount = 0;
            if (input != null)
            {

                foreach (var payorder in input)
                {
                    model.ListEditPayOrder.Add(new EditPayOrder { OrderId = payorder.PayOrderID });
                    model.ListEditPayOrder.Add(new EditPayOrder { OrderNumber = payorder.PayOrderNumber });

                }

                model.OrderTotalCount = input.Count;
            }
            return PartialView("_EditPayOrderCallback", model);
        }

    }
}