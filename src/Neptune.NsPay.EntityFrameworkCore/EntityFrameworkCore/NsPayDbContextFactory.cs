using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.Web;

namespace Neptune.NsPay.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class NsPayDbContextFactory : IDesignTimeDbContextFactory<NsPayDbContext>
    {
        public NsPayDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<NsPayDbContext>();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            /*
             You can provide an environmentName parameter to the AppConfigurations.Get method. 
             In this case, AppConfigurations will try to read appsettings.{environmentName}.json.
             Use Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") method or from string[] args to get environment if necessary.
             https://docs.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli#args
             */
            var configuration = AppConfigurations.Get(
                WebContentDirectoryFinder.CalculateContentRootFolder(),
                addUserSecrets: true
            );

            NsPayDbContextConfigurer.Configure(builder, serviceProvider, configuration.GetConnectionString(NsPayConsts.ConnectionStringName));

            return new NsPayDbContext(builder.Options);
        }
    }
}
