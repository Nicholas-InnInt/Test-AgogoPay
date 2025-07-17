using System.Collections.Generic;
using Neptune.NsPay.WithdrawalOrders.Dtos;
using Neptune.NsPay.Dto;
using Neptune.NsPay.Authorization.Users;

namespace Neptune.NsPay.WithdrawalOrders.Exporting
{
    public interface IWithdrawalOrdersExcelExporter
    {
        FileDto ExportToFile(List<GetWithdrawalOrderForViewDto> withdrawalOrders, UserTypeEnum userType);
    }
}