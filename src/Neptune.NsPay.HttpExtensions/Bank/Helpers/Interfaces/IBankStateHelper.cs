using Neptune.NsPay.PayMents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces
{
    public interface IBankStateHelper
    {
        int GetPayState(string account, PayMentTypeEnum paytype);

        string GetAccount(string cardNumber, PayMentTypeEnum paytype);
        string GetSessionId(string account, PayMentTypeEnum paytype);
    }
}
