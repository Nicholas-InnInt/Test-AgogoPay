using Neptune.NsPay.AbpUserMerchants;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public class AbpUserMerchantService: BaseService<AbpUserMerchant>, IAbpUserMerchantService, IDisposable
    {
        public AbpUserMerchantService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Dispose()
        {
        }
    }
}
