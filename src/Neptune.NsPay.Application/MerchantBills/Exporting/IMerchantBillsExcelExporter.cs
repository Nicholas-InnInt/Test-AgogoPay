using System.Collections.Generic;
using Neptune.NsPay.MerchantBills.Dtos;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.MerchantBills.Exporting
{
    public interface IMerchantBillsExcelExporter
    {
        FileDto ExportToFile(List<GetMerchantBillForViewDto> merchantBills, int pageNumber);
    }
}