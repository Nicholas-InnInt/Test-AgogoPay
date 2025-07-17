using Neptune.NsPay.Merchants;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;

namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public class MerchantService : BaseService<Merchant>, IMerchantService, IDisposable
    {
        public MerchantService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Dispose()
        {
        }
    }
}
