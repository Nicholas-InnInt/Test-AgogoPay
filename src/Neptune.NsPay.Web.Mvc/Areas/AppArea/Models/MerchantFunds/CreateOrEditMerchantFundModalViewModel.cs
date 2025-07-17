using Neptune.NsPay.MerchantFunds.Dtos;

using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.MerchantFunds
{
    public class CreateOrEditMerchantFundModalViewModel
    {
        public CreateOrEditMerchantFundDto MerchantFund { get; set; }

        public bool IsEditMode => MerchantFund.Id.HasValue;
    }
}