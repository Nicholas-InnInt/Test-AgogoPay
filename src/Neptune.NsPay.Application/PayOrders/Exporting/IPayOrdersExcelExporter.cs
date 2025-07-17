using System.Collections.Generic;
using Neptune.NsPay.PayOrders.Dtos;
using Neptune.NsPay.Dto;
using Neptune.NsPay.Authorization.Users;

namespace Neptune.NsPay.PayOrders.Exporting
{
    public interface IPayOrdersExcelExporter
    {
        FileDto ExportToFile(List<GetPayOrderForViewDto> payOrders, int pageNumber, UserTypeEnum userType);
    }
}