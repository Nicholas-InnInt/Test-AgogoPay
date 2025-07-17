using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.MerchantSettings;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.MerchantSettings;
using Neptune.NsPay.MerchantSettings.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_MerchantSettings)]
    public class MerchantSettingsController : NsPayControllerBase
    {
        private readonly IMerchantSettingsAppService _merchantSettingsAppService;

        public MerchantSettingsController(IMerchantSettingsAppService merchantSettingsAppService)
        {
            _merchantSettingsAppService = merchantSettingsAppService;

        }

        public ActionResult Index()
        {
            var model = new MerchantSettingsViewModel
            {
                FilterText = ""
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_MerchantSettings_Create, AppPermissions.Pages_MerchantSettings_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            GetMerchantSettingForEditOutput getMerchantSettingForEditOutput;

            if (id.HasValue)
            {
                getMerchantSettingForEditOutput = await _merchantSettingsAppService.GetMerchantSettingForEdit(new EntityDto { Id = (int)id });
            }
            else
            {
                getMerchantSettingForEditOutput = new GetMerchantSettingForEditOutput
                {
                    MerchantSetting = new CreateOrEditMerchantSettingDto()
                };
            }

            var viewModel = new CreateOrEditMerchantSettingModalViewModel()
            {
                MerchantSetting = getMerchantSettingForEditOutput.MerchantSetting,

            };

            return PartialView("_CreateOrEditModal", viewModel);
        }

        public async Task<PartialViewResult> ViewMerchantSettingModal(int id)
        {
            var getMerchantSettingForViewDto = await _merchantSettingsAppService.GetMerchantSettingForView(id);

            var model = new MerchantSettingViewModel()
            {
                MerchantSetting = getMerchantSettingForViewDto.MerchantSetting
            };

            return PartialView("_ViewMerchantSettingModal", model);
        }

    }
}