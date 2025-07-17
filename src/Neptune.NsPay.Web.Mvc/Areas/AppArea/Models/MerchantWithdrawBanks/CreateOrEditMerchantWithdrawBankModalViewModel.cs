using Neptune.NsPay.MerchantWithdrawBanks.Dtos;

using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.MerchantWithdrawBanks
{
    public class CreateOrEditMerchantWithdrawBankModalViewModel
    {
        public CreateOrEditMerchantWithdrawBankDto MerchantWithdrawBank { get; set; }

        public bool IsEditMode => MerchantWithdrawBank.Id.HasValue;
    }
}