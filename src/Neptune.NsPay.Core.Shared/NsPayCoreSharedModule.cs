using Abp.Modules;
using Abp.Reflection.Extensions;

namespace Neptune.NsPay
{
    public class NsPayCoreSharedModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(NsPayCoreSharedModule).GetAssembly());
        }
    }
}