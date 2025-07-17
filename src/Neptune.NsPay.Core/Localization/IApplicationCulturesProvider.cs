using System.Globalization;

namespace Neptune.NsPay.Localization
{
    public interface IApplicationCulturesProvider
    {
        CultureInfo[] GetAllCultures();
    }
}