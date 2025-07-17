using System.Collections.Generic;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.PayOrderDeposits.Exporting
{
    public interface IPayOrderDepositsExcelExporter
    {
        FileDto ExportToFile(List<GetPayOrderDepositForViewDto> payOrderDeposits, int pageNumber);
    }
}