using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Users
{
    public class UserLoginAttemptsViewModel
    {
        public List<ComboboxItemDto> LoginAttemptResults { get; set; }
    }
}