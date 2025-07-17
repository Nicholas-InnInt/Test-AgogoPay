using System.Collections.Generic;
using Neptune.NsPay.Authorization.Delegation;
using Neptune.NsPay.Authorization.Users.Delegation.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Layout
{
    public class ActiveUserDelegationsComboboxViewModel
    {
        public IUserDelegationConfiguration UserDelegationConfiguration { get; set; }

        public List<UserDelegationDto> UserDelegations { get; set; }

        public string CssClass { get; set; }
    }
}
