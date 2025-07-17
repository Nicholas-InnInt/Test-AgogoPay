using Abp.Dependency;
using Abp.Reflection.Extensions;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Configuration;

namespace Neptune.NsPay.Test.Base
{
    public class TestAppConfigurationAccessor : IAppConfigurationAccessor, ISingletonDependency
    {
        public IConfigurationRoot Configuration { get; }

        public TestAppConfigurationAccessor()
        {
            Configuration = AppConfigurations.Get(
                typeof(NsPayTestBaseModule).GetAssembly().GetDirectoryPathOrNull()
            );
        }
    }
}
