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
    public class AbpUserService: BaseService<AbpUsers>, IAbpUserService, IDisposable
    {
        public AbpUserService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        public void Dispose()
        {
        }
    }
}
