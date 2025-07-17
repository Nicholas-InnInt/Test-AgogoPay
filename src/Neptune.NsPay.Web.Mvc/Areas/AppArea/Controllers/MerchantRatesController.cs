using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.MerchantRates;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.MerchantRates;
using Neptune.NsPay.MerchantRates.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_MerchantRates)]
    public class MerchantRatesController : NsPayControllerBase
    {
        private readonly IMerchantRatesAppService _merchantRatesAppService;

        public MerchantRatesController(IMerchantRatesAppService merchantRatesAppService)
        {
            _merchantRatesAppService = merchantRatesAppService;

        }

        public ActionResult Index()
        {
            var model = new MerchantRatesViewModel
            {
                FilterText = ""
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_MerchantRates_Create, AppPermissions.Pages_MerchantRates_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            GetMerchantRateForEditOutput getMerchantRateForEditOutput;

            if (id.HasValue)
            {
                getMerchantRateForEditOutput = await _merchantRatesAppService.GetMerchantRateForEdit(new EntityDto { Id = (int)id });
            }
            else
            {
                getMerchantRateForEditOutput = new GetMerchantRateForEditOutput
                {
                    MerchantRate = new CreateOrEditMerchantRateDto()
                };
            }

            var viewModel = new CreateOrEditMerchantRateModalViewModel()
            {
                MerchantRate = getMerchantRateForEditOutput.MerchantRate,

            };

            return PartialView("_CreateOrEditModal", viewModel);
        }

        public async Task<PartialViewResult> ViewMerchantRateModal(int id)
        {
            var getMerchantRateForViewDto = await _merchantRatesAppService.GetMerchantRateForView(id);

            var model = new MerchantRateViewModel()
            {
                MerchantRate = getMerchantRateForViewDto.MerchantRate
            };

            return PartialView("_ViewMerchantRateModal", model);
        }

    }
}