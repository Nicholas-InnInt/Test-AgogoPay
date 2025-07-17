using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.Runtime.Validation;

namespace Neptune.NsPay.Authorization.Users.Dto
{
    public interface IGetUsersToExcelInput : IGetUsersInput
    {
        List<string> SelectedColumns { get; set; }
    }
}
