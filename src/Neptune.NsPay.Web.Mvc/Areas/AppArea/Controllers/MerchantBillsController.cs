using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.MerchantBills;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MerchantBills.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_MerchantBills)]
    public class MerchantBillsController : NsPayControllerBase
    {
        private readonly IMerchantBillsAppService _merchantBillsAppService;

        public MerchantBillsController(IMerchantBillsAppService merchantBillsAppService)
        {
            _merchantBillsAppService = merchantBillsAppService;

        }

        public ActionResult Index()
        {
            var model = new MerchantBillsViewModel
            {
                FilterText = "",
                IsShowMerchant= _merchantBillsAppService.IsShowMerchantFilter()
            };

            return View(model);
        }

   //     [AbpMvcAuthorize(AppPermissions.Pages_MerchantBills_Create)]
   //     public async Task<PartialViewResult> CreateOrEditModal(string id)
   //     {
   //         GetMerchantBillForEditOutput getMerchantBillForEditOutput;

   //         if (!id.IsNullOrEmpty())
   //         {
   //             getMerchantBillForEditOutput = await _merchantBillsAppService.GetMerchantBillForEdit(new EntityDto<string> { Id = id });
   //         }
   //         else
   //         {
   //             getMerchantBillForEditOutput = new GetMerchantBillForEditOutput
   //             {
   //                 MerchantBill = new CreateOrEditMerchantBillDto()
   //             };
   //             getMerchantBillForEditOutput.MerchantBill.CreationTime = DateTime.Now;
   //         }

   //         var viewModel = new CreateOrEditMerchantBillModalViewModel()
   //         {
   //             MerchantBill = getMerchantBillForEditOutput.MerchantBill,
			//	Merchants = await _merchantBillsAppService.GetMerchants()
			//};

   //         return PartialView("_CreateOrEditModal", viewModel);
   //     }

        public async Task<PartialViewResult> ViewMerchantBillModal(string id)
        {
            var getMerchantBillForViewDto = await _merchantBillsAppService.GetMerchantBillForView(id);

            var model = new MerchantBillViewModel()
            {
                MerchantBill = getMerchantBillForViewDto.MerchantBill
            };

            return PartialView("_ViewMerchantBillModal", model);
        }

    }
}