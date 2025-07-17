using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Neptune.NsPay.Authorization;

namespace Neptune.NsPay
{
    /// <summary>
    /// Application layer module of the application.
    /// </summary>
    [DependsOn(
        typeof(NsPayApplicationSharedModule),
        typeof(NsPayCoreModule)
        )]
    public class NsPayApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            //Adding authorization providers
            Configuration.Authorization.Providers.Add<AppAuthorizationProvider>();

            //Adding custom AutoMapper configuration
            Configuration.Modules.AbpAutoMapper().Configurators.Add(CustomDtoMapper.CreateMappings);
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(NsPayApplicationModule).GetAssembly());
        }
    }
}