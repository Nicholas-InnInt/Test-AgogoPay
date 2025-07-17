using Abp.Dependency;

namespace Neptune.NsPay.DashboardCustomization.Definitions.Cache
{
    public interface IDashboardDefinitionCacheManager : ITransientDependency
    {
        DashboardDefinition Get(string name);

        void Set(DashboardDefinition definition);
    }
}