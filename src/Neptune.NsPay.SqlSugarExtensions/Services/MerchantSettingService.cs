using Neptune.NsPay.MerchantSettings;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;

namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public class MerchantSettingService : BaseService<MerchantSetting>, IMerchantSettingService, IDisposable
    {
        public MerchantSettingService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Dispose()
        {
        }
    }
}
