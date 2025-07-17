using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.MerchantWithdraws;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.MerchantWithdraws.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;
using Stripe;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_MerchantWithdraws)]
    public class MerchantWithdrawsController : NsPayControllerBase
    {
        private readonly IMerchantWithdrawsAppService _merchantWithdrawsAppService;

        public MerchantWithdrawsController(IMerchantWithdrawsAppService merchantWithdrawsAppService)
        {
            _merchantWithdrawsAppService = merchantWithdrawsAppService;

        }

        public ActionResult Index()
        {
            var model = new MerchantWithdrawsViewModel
            {
                FilterText = ""
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_MerchantWithdraws_Create, AppPermissions.Pages_MerchantWithdraws_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(long? id)
        {
            GetMerchantWithdrawForEditOutput getMerchantWithdrawForEditOutput;

            if (id.HasValue)
            {
                getMerchantWithdrawForEditOutput = await _merchantWithdrawsAppService.GetMerchantWithdrawForEdit(new EntityDto<long> { Id = (long)id });
            }
            else
            {
                getMerchantWithdrawForEditOutput = await _merchantWithdrawsAppService.GetMerchantWithdrawForCreate();
                //getMerchantWithdrawForEditOutput.MerchantWithdraw.ReviewTime = DateTime.Now;
            }

            var viewModel = new CreateOrEditMerchantWithdrawModalViewModel()
            {
                MerchantWithdraw = getMerchantWithdrawForEditOutput.MerchantWithdraw,
                MerchantBanks = getMerchantWithdrawForEditOutput.MerchantBanks,
                Balance = getMerchantWithdrawForEditOutput.Balance,
                BalanceInit = getMerchantWithdrawForEditOutput.BalanceInit??0,
                PendingWithdrawalOrderAmount = getMerchantWithdrawForEditOutput.PendingWithdrawalOrderAmount,
                PendingMerchantWithdrawalAmount = getMerchantWithdrawForEditOutput.PendingMerchantWithdrawalAmount,

            };

            return PartialView("_CreateOrEditModal", viewModel);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_MerchantWithdraws_AuditTurndown)]
        public async Task<PartialViewResult> TurndownOrPassModal(long? id)
        {
            GetMerchantWithdrawForTurndownOutput getMerchantWithdrawForTurndownOutput = new GetMerchantWithdrawForTurndownOutput();

            if (id.HasValue)
            {
                getMerchantWithdrawForTurndownOutput = await _merchantWithdrawsAppService.GetMerchantWithdrawForTurndown(new EntityDto<long> { Id = id.Value });
            }

            var viewModel = new TurndownOrPassMerchantWithdrawModalViewModel()
            {
                MerchantWithdraw = getMerchantWithdrawForTurndownOutput.MerchantWithdraw
            };

            return PartialView("_TurndownOrPassModal", viewModel);
        }

        public async Task<PartialViewResult> ViewMerchantWithdrawModal(long id)
        {
            var getMerchantWithdrawForViewDto = await _merchantWithdrawsAppService.GetMerchantWithdrawForView(id);

            var model = new MerchantWithdrawViewModel()
            {
                MerchantWithdraw = getMerchantWithdrawForViewDto.MerchantWithdraw
            };

            return PartialView("_ViewMerchantWithdrawModal", model);
        }

    }
}