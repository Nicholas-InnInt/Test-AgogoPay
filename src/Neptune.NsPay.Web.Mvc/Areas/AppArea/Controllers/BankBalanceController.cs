using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.BankBalance;
using Neptune.NsPay.BankBalance.Dto;
using Neptune.NsPay.Web.Areas.AppArea.Models.BankBalance;
using Neptune.NsPay.Web.Controllers;
using System;
using System.Threading.Tasks;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_BankBalances)]
    public class BankBalanceController : NsPayControllerBase
    {
        private readonly IBankBalancesAppService _bankBalancesAppService;
        public BankBalanceController(IBankBalancesAppService bankBalancesAppService)
        {
            _bankBalancesAppService = bankBalancesAppService;
        }

        public async Task<IActionResult> Index(BankBalanceInput input)
        {
            var datetime = DateTime.Now;
            var balanceInput = new GetAllBankBalanceInput()
            {
                MaxTransactionTimeFilter = datetime.Date.AddDays(1),
                MinTransactionTimeFilter = datetime.Date.AddHours(-12),
                Filter = input.FilterText
                //UtcTimeFilter= "GMT7+"
            };
            var viewModel = await _bankBalancesAppService.GetBankBalance(balanceInput);
            var model = new BankBalancesViewModel
            {
                FilterText = input.FilterText,
                BankBalances = viewModel
            };
            return View(model);
        }
    }
}
