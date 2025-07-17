using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.MerchantFunds;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.MerchantFunds;
using Neptune.NsPay.MerchantFunds.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_MerchantFunds)]
    public class MerchantFundsController : NsPayControllerBase
    {
        private readonly IMerchantFundsAppService _merchantFundsAppService;

        public MerchantFundsController(IMerchantFundsAppService merchantFundsAppService)
        {
            _merchantFundsAppService = merchantFundsAppService;

        }

        public ActionResult Index()
        {
            var model = new MerchantFundsViewModel
            {
                FilterText = ""
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_MerchantFunds_Create, AppPermissions.Pages_MerchantFunds_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            GetMerchantFundForEditOutput getMerchantFundForEditOutput;

            if (id.HasValue)
            {
                getMerchantFundForEditOutput = await _merchantFundsAppService.GetMerchantFundForEdit(new EntityDto { Id = (int)id });
            }
            else
            {
                getMerchantFundForEditOutput = new GetMerchantFundForEditOutput
                {
                    MerchantFund = new CreateOrEditMerchantFundDto()
                };
                getMerchantFundForEditOutput.MerchantFund.CreationTime = DateTime.Now;
                getMerchantFundForEditOutput.MerchantFund.UpdateTime = DateTime.Now;
            }

            var viewModel = new CreateOrEditMerchantFundModalViewModel()
            {
                MerchantFund = getMerchantFundForEditOutput.MerchantFund,

            };

            return PartialView("_CreateOrEditModal", viewModel);
        }

        public async Task<PartialViewResult> ViewMerchantFundModal(int id)
        {
            var getMerchantFundForViewDto = await _merchantFundsAppService.GetMerchantFundForView(id);

            var model = new MerchantFundViewModel()
            {
                MerchantFund = getMerchantFundForViewDto.MerchantFund
            };

            return PartialView("_ViewMerchantFundModal", model);
        }

    }
}