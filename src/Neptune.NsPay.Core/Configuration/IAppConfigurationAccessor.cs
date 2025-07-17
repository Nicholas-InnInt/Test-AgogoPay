using Microsoft.Extensions.Configuration;

namespace Neptune.NsPay.Configuration
{
    public interface IAppConfigurationAccessor
    {
        IConfigurationRoot Configuration { get; }
    }
}
