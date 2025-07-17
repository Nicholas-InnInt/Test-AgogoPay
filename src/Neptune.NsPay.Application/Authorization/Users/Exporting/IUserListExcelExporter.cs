using System.Collections.Generic;
using Neptune.NsPay.Authorization.Users.Dto;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.Authorization.Users.Exporting
{
    public interface IUserListExcelExporter
    {
        FileDto ExportToFile(List<UserListDto> userListDtos, List<string> selectedColumns);
    }
}