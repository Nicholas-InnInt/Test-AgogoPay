using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public class BinaryObjectManager: BaseService<BinaryObject>, IBinaryObjectManagerService, IDisposable
    {
        public BinaryObjectManager(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        public void Dispose()
        {
        }
    }
}
