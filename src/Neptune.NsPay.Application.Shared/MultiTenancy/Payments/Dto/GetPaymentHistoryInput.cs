using Abp.Runtime.Validation;
using Neptune.NsPay.Common;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.MultiTenancy.Payments.Dto
{
    public class GetPaymentHistoryInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public void Normalize()
        {
            if (string.IsNullOrEmpty(Sorting))
            {
                Sorting = "CreationTime";
            }

            Sorting = DtoSortingHelper.ReplaceSorting(Sorting, s =>
            {
                return s.Replace("editionDisplayName", "Edition.DisplayName");
            });
        }
    }
}
