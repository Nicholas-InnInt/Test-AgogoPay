using Neptune.NsPay.PayMents;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public class PayMentService: BaseService<PayMent>, IPayMentService
    {
        public PayMentService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
