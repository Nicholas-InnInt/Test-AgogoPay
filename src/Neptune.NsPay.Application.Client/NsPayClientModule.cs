using Abp.Modules;
using Abp.Reflection.Extensions;

namespace Neptune.NsPay
{
    public class NsPayClientModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(NsPayClientModule).GetAssembly());
        }
    }
}
