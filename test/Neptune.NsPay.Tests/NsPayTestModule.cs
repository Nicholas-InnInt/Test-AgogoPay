using Abp.Modules;
using Neptune.NsPay.Test.Base;

namespace Neptune.NsPay.Tests
{
    [DependsOn(typeof(NsPayTestBaseModule))]
    public class NsPayTestModule : AbpModule
    {
       
    }
}
