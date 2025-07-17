using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;

namespace Neptune.NsPay.MongoDbExtensions.Services
{
    public class LogRecipientBankAccountMongoService : MongoBaseService<LogRecipientBankAccountsMongoEntity>, ILogRecipientBankAccountMongoService, IDisposable
    {

        public void Dispose()
        {
        }
    }
}
