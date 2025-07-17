using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Neptune.NsPay.EntityFrameworkCore;

namespace Neptune.NsPay.HealthChecks
{
    public class NsPayDbContextHealthCheck : IHealthCheck
    {
        private readonly DatabaseCheckHelper _checkHelper;

        public NsPayDbContextHealthCheck(DatabaseCheckHelper checkHelper)
        {
            _checkHelper = checkHelper;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            if (_checkHelper.Exist("db"))
            {
                return Task.FromResult(HealthCheckResult.Healthy("NsPayDbContext connected to database."));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("NsPayDbContext could not connect to database"));
        }
    }
}
