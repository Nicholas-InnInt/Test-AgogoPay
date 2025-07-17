using Neptune.NsPay.WithdrawalOrders.Dtos;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.WithdrawalOrders
{
    public class WithdrawalOrderDisplayProofModel 
    {
        public GetDisplayProofDto Content { get; set; }
        public string OrderId { get; set; }

        public string OrderNumber { get; set; }
    }
}
