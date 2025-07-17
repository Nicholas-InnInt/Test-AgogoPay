using Neptune.NsPay.AbpUserMerchants.Dtos;

using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.AbpUserMerchants
{
    public class CreateOrEditAbpUserMerchantModalViewModel
    {
        public CreateOrEditAbpUserMerchantDto AbpUserMerchant { get; set; }

        public bool IsEditMode => AbpUserMerchant.Id.HasValue;
    }
}