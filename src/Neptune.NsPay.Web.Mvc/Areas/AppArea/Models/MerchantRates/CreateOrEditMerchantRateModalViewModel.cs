using Neptune.NsPay.MerchantRates.Dtos;

using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.MerchantRates
{
    public class CreateOrEditMerchantRateModalViewModel
    {
        public CreateOrEditMerchantRateDto MerchantRate { get; set; }

        public bool IsEditMode => MerchantRate.Id.HasValue;
    }
}