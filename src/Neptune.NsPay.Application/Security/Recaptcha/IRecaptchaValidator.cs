using System.Threading.Tasks;

namespace Neptune.NsPay.Security.Recaptcha
{
    public interface IRecaptchaValidator
    {
        Task ValidateAsync(string captchaResponse);
    }
}