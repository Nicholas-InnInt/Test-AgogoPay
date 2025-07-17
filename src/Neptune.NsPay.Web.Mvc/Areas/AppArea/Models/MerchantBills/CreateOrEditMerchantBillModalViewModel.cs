using Neptune.NsPay.MerchantBills.Dtos;

using Abp.Extensions;
using Neptune.NsPay.Merchants.Dtos;
using System.Collections.Generic;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.MerchantBills
{
    public class CreateOrEditMerchantBillModalViewModel
    {
        public CreateOrEditMerchantBillDto MerchantBill { get; set; }

		public List<MerchantDto> Merchants { get; set; }

		public bool IsEditMode => MerchantBill.Id != null;
    }
}