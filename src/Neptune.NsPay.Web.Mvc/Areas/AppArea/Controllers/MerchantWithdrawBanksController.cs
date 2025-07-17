using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.MerchantWithdrawBanks;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.MerchantWithdrawBanks;
using Neptune.NsPay.MerchantWithdrawBanks.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_MerchantWithdrawBanks)]
    public class MerchantWithdrawBanksController : NsPayControllerBase
    {
        private readonly IMerchantWithdrawBanksAppService _merchantWithdrawBanksAppService;

        public MerchantWithdrawBanksController(IMerchantWithdrawBanksAppService merchantWithdrawBanksAppService)
        {
            _merchantWithdrawBanksAppService = merchantWithdrawBanksAppService;

        }

        public ActionResult Index()
        {
            var model = new MerchantWithdrawBanksViewModel
            {
                FilterText = ""
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_MerchantWithdrawBanks_Create, AppPermissions.Pages_MerchantWithdrawBanks_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            GetMerchantWithdrawBankForEditOutput getMerchantWithdrawBankForEditOutput;

            if (id.HasValue)
            {
                getMerchantWithdrawBankForEditOutput = await _merchantWithdrawBanksAppService.GetMerchantWithdrawBankForEdit(new EntityDto { Id = (int)id });
            }
            else
            {
                getMerchantWithdrawBankForEditOutput = new GetMerchantWithdrawBankForEditOutput
                {
                    MerchantWithdrawBank = new CreateOrEditMerchantWithdrawBankDto()
                };
            }

            var viewModel = new CreateOrEditMerchantWithdrawBankModalViewModel()
            {
                MerchantWithdrawBank = getMerchantWithdrawBankForEditOutput.MerchantWithdrawBank,

            };

            return PartialView("_CreateOrEditModal", viewModel);
        }

        public async Task<PartialViewResult> ViewMerchantWithdrawBankModal(int id)
        {
            var getMerchantWithdrawBankForViewDto = await _merchantWithdrawBanksAppService.GetMerchantWithdrawBankForView(id);

            var model = new MerchantWithdrawBankViewModel()
            {
                MerchantWithdrawBank = getMerchantWithdrawBankForViewDto.MerchantWithdrawBank
            };

            return PartialView("_ViewMerchantWithdrawBankModal", model);
        }

    }
}