using Abp.AutoMapper;
using Neptune.NsPay.Authorization.Users.Dto;

namespace Neptune.NsPay.Mobile.MAUI.Models.User
{
    [AutoMapFrom(typeof(CreateOrUpdateUserInput))]
    public class UserCreateOrUpdateModel : CreateOrUpdateUserInput
    {

    }
}
