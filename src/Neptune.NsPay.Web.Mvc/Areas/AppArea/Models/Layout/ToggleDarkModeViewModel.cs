namespace Neptune.NsPay.Web.Areas.AppArea.Models.Layout;

public class ToggleDarkModeViewModel
{
    public string CssClass { get; }
    public bool IsDarkModeActive { get; }

    public ToggleDarkModeViewModel(string cssClass, bool isDarkModeActive)
    {
        CssClass = cssClass;
        IsDarkModeActive = isDarkModeActive;
    }
}