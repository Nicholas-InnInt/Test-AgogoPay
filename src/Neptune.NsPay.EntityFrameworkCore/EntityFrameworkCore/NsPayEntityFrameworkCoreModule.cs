using Abp;
using Abp.Dependency;
using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.DataEvent;
using Neptune.NsPay.EntityHistory;
using Neptune.NsPay.Migrations.Seed;

namespace Neptune.NsPay.EntityFrameworkCore
{
    [DependsOn(
        typeof(AbpZeroCoreEntityFrameworkCoreModule),
        typeof(NsPayCoreModule)
    )]
    public class NsPayEntityFrameworkCoreModule : AbpModule
    {
        /* Used it tests to skip DbContext registration, in order to use in-memory database of EF Core */
        public bool SkipDbContextRegistration { get; set; }

        public bool SkipDbSeed { get; set; }

        public override void PreInitialize()
        {
            if (!SkipDbContextRegistration)
            {
                var datachangedEvent =   IocManager.Resolve<IDataChangedEvent>();
                Configuration.Modules.AbpEfCore().AddDbContext<NsPayDbContext>(options =>
                {
                    if (options.ExistingConnection != null)
                    {
                        NsPayDbContextConfigurer.Configure(options.DbContextOptions, datachangedEvent,
                            options.ExistingConnection);
                    }
                    else
                    {
                        NsPayDbContextConfigurer.Configure(options.DbContextOptions, datachangedEvent,
                            options.ConnectionString);
                    }
                });
            }

            // Set this setting to true for enabling entity history.
            Configuration.EntityHistory.IsEnabled = false;

            // Uncomment below line to write change logs for the entities below:
            // Configuration.EntityHistory.Selectors.Add("NsPayEntities", EntityHistoryHelper.TrackedTypes);
            // Configuration.CustomConfigProviders.Add(new EntityHistoryConfigProvider(Configuration));
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(NsPayEntityFrameworkCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            var configurationAccessor = IocManager.Resolve<IAppConfigurationAccessor>();

            using (var scope = IocManager.CreateScope())
            {
                if (!SkipDbSeed && scope.Resolve<DatabaseCheckHelper>()
                        .Exist(configurationAccessor.Configuration["ConnectionStrings:Default"]))
                {
                    SeedHelper.SeedHostDb(IocManager);
                }
            }
        }
    }
}