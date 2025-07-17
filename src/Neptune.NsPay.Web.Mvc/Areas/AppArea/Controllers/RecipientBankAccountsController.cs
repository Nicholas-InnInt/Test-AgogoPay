using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MerchantRates;
using Neptune.NsPay.MerchantRates.Dtos;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.Merchants.Dtos;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.RecipientBankAccounts;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.Web.Areas.AppArea.Models.Merchants;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.PayGroups;
using Neptune.NsPay.Web.Areas.AppArea.Models.RecipientBankAccounts;
using NUglify.Helpers;
using Abp.Collections.Extensions;
using Neptune.NsPay.PayOrders.Dtos;
using Neptune.NsPay.Web.Areas.AppArea.Models.PayOrders;
using System;
using Neptune.NsPay.RecipientBankAccounts.Dtos;
using Abp.Authorization;
using Neptune.NsPay.VietQR;
using Neptune.NsPay.BankInfo;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_RecipientBankAccounts)]
    public class RecipientBankAccountsController : NsPayControllerBase
    {

        private readonly IRecipientBankAccountsAppService _recipientBankAccountsAppService;

        public RecipientBankAccountsController(IRecipientBankAccountsAppService recipientBankAccountsAppService)
        {
            _recipientBankAccountsAppService = recipientBankAccountsAppService;

        }

        public ActionResult Index()
        {
            var model = new MerchantsViewModel
            {
                FilterText = ""

            };
            return View(model);
        }


        [AbpMvcAuthorize(AppPermissions.Pages_RecipientBankAccounts_Create, AppPermissions.Pages_RecipientBankAccounts_Edit)]
        public async Task<PartialViewResult> CreateOrEditRecipientBankAccountModal(string id)
        {
            GetRecipientBankAccountForEditOutput getRecipientBankAccount;

            if (!id.IsNullOrEmpty())
            {
                getRecipientBankAccount = await _recipientBankAccountsAppService.GetRecipientBankAccountById(new EntityDto<string> { Id = id });
            }
            else
            {
                getRecipientBankAccount = new GetRecipientBankAccountForEditOutput
                {
                    RecipientBankAccount = new CreateOrEditRecipientBankAccountsDto()
                };
            }

            var viewModel = new CreateOrEditRecipientBankAccountsViewModal()
            {
                RecipientBankAccounts = getRecipientBankAccount.RecipientBankAccount,
            };

            if (viewModel.RecipientBankAccounts.BankName != null)
                viewModel.RecipientBankAccounts.BankName = BankApp.BanksObject[viewModel.RecipientBankAccounts.BankKey].key; //BankKey will directy store as input in create and update

            if (!id.IsNullOrEmpty())
            {
                viewModel.IsEditMode = true;

            }
            else 
            {
                viewModel.IsEditMode = false;
            }

            return PartialView("_CreateOrEditModal", viewModel);
        }

        private string ConvertBankKey(string key) 
        {
            var bankCode = WithdrawalOrderBankMapper.FindBankByName(key);
            return bankCode;

        }

    }
}
