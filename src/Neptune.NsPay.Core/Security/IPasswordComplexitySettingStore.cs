using System.Threading.Tasks;

namespace Neptune.NsPay.Security
{
    public interface IPasswordComplexitySettingStore
    {
        Task<PasswordComplexitySetting> GetSettingsAsync();
    }
}
