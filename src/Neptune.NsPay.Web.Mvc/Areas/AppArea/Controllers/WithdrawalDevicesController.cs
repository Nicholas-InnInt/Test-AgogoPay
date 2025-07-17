using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Web.Areas.AppArea.Models.WithdrawalDevices;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.WithdrawalDevices.Dtos;
using System.Threading.Tasks;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_WithdrawalDevices)]
    public class WithdrawalDevicesController : NsPayControllerBase
    {
        private readonly IWithdrawalDevicesAppService _withdrawalDevicesAppService;

        public WithdrawalDevicesController(IWithdrawalDevicesAppService withdrawalDevicesAppService)
        {
            _withdrawalDevicesAppService = withdrawalDevicesAppService;
        }

        public ActionResult Index()
        {
            var model = new WithdrawalDevicesViewModel
            {
                FilterText = ""
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_WithdrawalDevices_Create, AppPermissions.Pages_WithdrawalDevices_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            GetWithdrawalDeviceForEditOutput getWithdrawalDeviceForEditOutput;

            if (id.HasValue)
            {
                getWithdrawalDeviceForEditOutput = await _withdrawalDevicesAppService.GetWithdrawalDeviceForEdit(new EntityDto { Id = (int)id });
            }
            else
            {
                getWithdrawalDeviceForEditOutput = new GetWithdrawalDeviceForEditOutput
                {
                    WithdrawalDevice = new CreateOrEditWithdrawalDeviceDto()
                };
            }

            var viewModel = new CreateOrEditWithdrawalDeviceModalViewModel()
            {
                WithdrawalDevice = getWithdrawalDeviceForEditOutput.WithdrawalDevice,
                Merchants = await _withdrawalDevicesAppService.GetMerchants(),
                IsInternalMerchant = await _withdrawalDevicesAppService.GetIsInternalMerchant()
            };

            return PartialView("_CreateOrEditModal", viewModel);
        }

        public async Task<PartialViewResult> ViewWithdrawalDeviceModal(int id)
        {
            var getWithdrawalDeviceForViewDto = await _withdrawalDevicesAppService.GetWithdrawalDeviceForView(id);

            var model = new WithdrawalDeviceViewModel()
            {
                WithdrawalDevice = getWithdrawalDeviceForViewDto.WithdrawalDevice
            };

            return PartialView("_ViewWithdrawalDeviceModal", model);
        }

        public async Task<PartialViewResult> BatchPauseWithdrawalDeviceModal()
        {
            var getWithdrawalDeviceActiveBankList = await _withdrawalDevicesAppService.GetWithdrawalDeviceActiveBankList();

            return PartialView("_BatchPauseWithdrawalDeviceModal", getWithdrawalDeviceActiveBankList);
        }
    }
}