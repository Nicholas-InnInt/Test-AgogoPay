using System.Threading.Tasks;
using Abp.Domain.Uow;

namespace Neptune.NsPay.OpenIddict
{
    public interface IOpenIddictDbConcurrencyExceptionHandler
    {
        Task HandleAsync(AbpDbConcurrencyException exception);
    }
}