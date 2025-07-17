using Neptune.NsPay.WithdrawalOrders.Dtos;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.WithdrawalOrders
{
    public class WithdrawalOrderViewPayoutDetailsModel : GetWithdrawalOrderForViewDto
    {
        public string vietQRURL { get; set; }
    }
}