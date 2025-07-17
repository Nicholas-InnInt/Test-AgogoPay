using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Neptune.NsPay.ApiClient;
using Neptune.NsPay.Mobile.MAUI.Core.ApiClient;

namespace Neptune.NsPay
{
    [DependsOn(typeof(NsPayClientModule), typeof(AbpAutoMapperModule))]

    public class NsPayMobileMAUIModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Localization.IsEnabled = false;
            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;

            Configuration.ReplaceService<IApplicationContext, MAUIApplicationContext>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(NsPayMobileMAUIModule).GetAssembly());
        }
    }
}