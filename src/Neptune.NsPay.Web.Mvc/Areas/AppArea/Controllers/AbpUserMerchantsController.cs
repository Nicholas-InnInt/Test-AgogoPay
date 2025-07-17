using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.AbpUserMerchants;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.AbpUserMerchants;
using Neptune.NsPay.AbpUserMerchants.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_Users)]
    public class AbpUserMerchantsController : NsPayControllerBase
    {
        private readonly IAbpUserMerchantsAppService _abpUserMerchantsAppService;

        public AbpUserMerchantsController(IAbpUserMerchantsAppService abpUserMerchantsAppService)
        {
            _abpUserMerchantsAppService = abpUserMerchantsAppService;

        }

        public ActionResult Index()
        {
            var model = new AbpUserMerchantsViewModel
            {
                FilterText = ""
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Administration_Users_Create, AppPermissions.Pages_Administration_Users_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            GetAbpUserMerchantForEditOutput getAbpUserMerchantForEditOutput;

            if (id.HasValue)
            {
                getAbpUserMerchantForEditOutput = await _abpUserMerchantsAppService.GetAbpUserMerchantForEdit(new EntityDto { Id = (int)id });
            }
            else
            {
                getAbpUserMerchantForEditOutput = new GetAbpUserMerchantForEditOutput
                {
                    AbpUserMerchant = new CreateOrEditAbpUserMerchantDto()
                };
            }

            var viewModel = new CreateOrEditAbpUserMerchantModalViewModel()
            {
                AbpUserMerchant = getAbpUserMerchantForEditOutput.AbpUserMerchant,

            };

            return PartialView("_CreateOrEditModal", viewModel);
        }

        public async Task<PartialViewResult> ViewAbpUserMerchantModal(int id)
        {
            var getAbpUserMerchantForViewDto = await _abpUserMerchantsAppService.GetAbpUserMerchantForView(id);

            var model = new AbpUserMerchantViewModel()
            {
                AbpUserMerchant = getAbpUserMerchantForViewDto.AbpUserMerchant
            };

            return PartialView("_ViewAbpUserMerchantModal", model);
        }

    }
}