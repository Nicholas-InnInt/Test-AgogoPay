using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.NsPaySystemSettings;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.NsPaySystemSettings;
using Neptune.NsPay.NsPaySystemSettings.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_NsPaySystemSettings)]
    public class NsPaySystemSettingsController : NsPayControllerBase
    {
        private readonly INsPaySystemSettingsAppService _nsPaySystemSettingsAppService;

        public NsPaySystemSettingsController(INsPaySystemSettingsAppService nsPaySystemSettingsAppService)
        {
            _nsPaySystemSettingsAppService = nsPaySystemSettingsAppService;

        }

        public ActionResult Index()
        {
            var model = new NsPaySystemSettingsViewModel
            {
                FilterText = ""
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_NsPaySystemSettings_Create, AppPermissions.Pages_NsPaySystemSettings_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            GetNsPaySystemSettingForEditOutput getNsPaySystemSettingForEditOutput;

            if (id.HasValue)
            {
                getNsPaySystemSettingForEditOutput = await _nsPaySystemSettingsAppService.GetNsPaySystemSettingForEdit(new EntityDto { Id = (int)id });
            }
            else
            {
                getNsPaySystemSettingForEditOutput = new GetNsPaySystemSettingForEditOutput
                {
                    NsPaySystemSetting = new CreateOrEditNsPaySystemSettingDto()
                };
            }

            var viewModel = new CreateOrEditNsPaySystemSettingModalViewModel()
            {
                NsPaySystemSetting = getNsPaySystemSettingForEditOutput.NsPaySystemSetting,

            };

            return PartialView("_CreateOrEditModal", viewModel);
        }

        public async Task<PartialViewResult> ViewNsPaySystemSettingModal(int id)
        {
            var getNsPaySystemSettingForViewDto = await _nsPaySystemSettingsAppService.GetNsPaySystemSettingForView(id);

            var model = new NsPaySystemSettingViewModel()
            {
                NsPaySystemSetting = getNsPaySystemSettingForViewDto.NsPaySystemSetting
            };

            return PartialView("_ViewNsPaySystemSettingModal", model);
        }

    }
}