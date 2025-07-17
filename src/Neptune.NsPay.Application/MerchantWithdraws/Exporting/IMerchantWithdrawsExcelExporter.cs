using System.Collections.Generic;
using Neptune.NsPay.MerchantWithdraws.Dtos;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.MerchantWithdraws.Exporting
{
    public interface IMerchantWithdrawsExcelExporter
    {
        FileDto ExportToFile(List<GetMerchantWithdrawForViewDto> merchantWithdraws);
    }
}