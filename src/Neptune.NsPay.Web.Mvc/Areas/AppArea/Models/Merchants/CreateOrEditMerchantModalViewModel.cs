using Neptune.NsPay.Merchants.Dtos;

using Abp.Extensions;
using Neptune.NsPay.MerchantRates.Dtos;
using System.Collections.Generic;
using Neptune.NsPay.PayGroups.Dtos;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Merchants
{
    public class CreateOrEditMerchantModalViewModel
    {
        public CreateOrEditMerchantDto Merchant { get; set; }

		public CreateOrEditMerchantRateDto MerchantRate { get; set; }

		public List<PayGroupDto> PayGroups { get; set; }

		public List<string> PlatformCode { get; set; }

		public List<string> Countries { get; set; }

		public bool IsEditMode => Merchant.Id.HasValue;
    }
}