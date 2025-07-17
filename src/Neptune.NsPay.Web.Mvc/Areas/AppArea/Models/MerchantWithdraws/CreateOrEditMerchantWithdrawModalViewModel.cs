using Neptune.NsPay.MerchantWithdraws.Dtos;

using Abp.Extensions;
using Neptune.NsPay.MerchantWithdrawBanks.Dtos;
using System.Collections.Generic;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.MerchantWithdraws
{
    public class CreateOrEditMerchantWithdrawModalViewModel
    {
        public CreateOrEditMerchantWithdrawDto MerchantWithdraw { get; set; }
        public List<MerchantWithdrawBankDto> MerchantBanks { get; set; }
        public decimal? Balance { get; set; }

        public decimal PendingWithdrawalOrderAmount { get; set; }

        public decimal PendingMerchantWithdrawalAmount { get; set; }

        public decimal BalanceInit { get; set; }
        public bool IsEditMode => MerchantWithdraw.Id.HasValue;
    }
}