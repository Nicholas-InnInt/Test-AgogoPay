using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;

namespace Neptune.NsPay.Startup
{
    [DependsOn(typeof(NsPayCoreModule))]
    public class NsPayGraphQLModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(NsPayGraphQLModule).GetAssembly());
        }

        public override void PreInitialize()
        {
            base.PreInitialize();

            //Adding custom AutoMapper configuration
            Configuration.Modules.AbpAutoMapper().Configurators.Add(CustomDtoMapper.CreateMappings);
        }
    }
}