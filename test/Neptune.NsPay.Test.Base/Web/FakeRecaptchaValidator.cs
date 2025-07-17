using System.Threading.Tasks;
using Neptune.NsPay.Security.Recaptcha;

namespace Neptune.NsPay.Test.Base.Web
{
    public class FakeRecaptchaValidator : IRecaptchaValidator
    {
        public Task ValidateAsync(string captchaResponse)
        {
            return Task.CompletedTask;
        }
    }
}
