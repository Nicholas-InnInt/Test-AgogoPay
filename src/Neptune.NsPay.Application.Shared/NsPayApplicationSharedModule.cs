using Abp.Modules;
using Abp.Reflection.Extensions;

namespace Neptune.NsPay
{
    [DependsOn(typeof(NsPayCoreSharedModule))]
    public class NsPayApplicationSharedModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(NsPayApplicationSharedModule).GetAssembly());
        }
    }
}