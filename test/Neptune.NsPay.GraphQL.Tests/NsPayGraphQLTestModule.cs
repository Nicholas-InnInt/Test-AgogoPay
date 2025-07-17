using Abp.Modules;
using Abp.Reflection.Extensions;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Neptune.NsPay.Configure;
using Neptune.NsPay.Startup;
using Neptune.NsPay.Test.Base;

namespace Neptune.NsPay.GraphQL.Tests
{
    [DependsOn(
        typeof(NsPayGraphQLModule),
        typeof(NsPayTestBaseModule))]
    public class NsPayGraphQLTestModule : AbpModule
    {
        public override void PreInitialize()
        {
            IServiceCollection services = new ServiceCollection();
            
            services.AddAndConfigureGraphQL();

            WindsorRegistrationHelper.CreateServiceProvider(IocManager.IocContainer, services);
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(NsPayGraphQLTestModule).GetAssembly());
        }
    }
}