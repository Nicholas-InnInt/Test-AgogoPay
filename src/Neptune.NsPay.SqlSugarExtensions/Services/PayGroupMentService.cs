using Neptune.NsPay.PayGroupMents;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;

namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public class PayGroupMentService : BaseService<PayGroupMent>, IPayGroupMentService, IDisposable
    {
        public PayGroupMentService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Dispose()
        {
        }
    }
}
