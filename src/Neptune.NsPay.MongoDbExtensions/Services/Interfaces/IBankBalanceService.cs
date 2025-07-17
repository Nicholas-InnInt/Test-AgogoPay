using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.MongoDbExtensions.Services.Interfaces
{
    public interface IBankBalanceService
    {
        Task<decimal> GetBalance(int payMentId, decimal balance, decimal balance2, int islogin);
    }
}
