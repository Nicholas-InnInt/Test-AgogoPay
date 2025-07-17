using Neptune.NsPay.NsPayBackgroundJobs;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;

namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public class NsPayBackgroundJobsService : BaseService<NsPayBackgroundJob>, INsPayBackgroundJobsService, IDisposable
    {
        public NsPayBackgroundJobsService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Dispose()
        {
        }
    }
}
