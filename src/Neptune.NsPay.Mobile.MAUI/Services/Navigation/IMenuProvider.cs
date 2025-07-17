using Neptune.NsPay.Models.NavigationMenu;

namespace Neptune.NsPay.Services.Navigation
{
    public interface IMenuProvider
    {
        List<NavigationMenuItem> GetAuthorizedMenuItems(Dictionary<string, string> grantedPermissions);
    }
}