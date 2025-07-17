using Neptune.NsPay.MerchantWithdraws.Dtos;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.MerchantWithdraws
{
    public class TurndownOrPassMerchantWithdrawModalViewModel
    {
        public TurndownOrPassMerchantWithdrawDto MerchantWithdraw { get; set; }
        public bool IsEditMode => MerchantWithdraw.Id.HasValue;
    }
}
