using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.PayMents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces
{
    public interface IBankHelper
    {
        string GetLastRefNoKey(PayMentTypeEnum type, string account);
        void SetLastRefNoKey(PayMentTypeEnum type, string account, string refno);

        void SetBankSessionId(PayMentTypeEnum type, string account, BankLoginModel loginModel);

        void RemoveBankSessionId(PayMentTypeEnum type, string account);
        BankLoginModel GetBankSessionId(PayMentTypeEnum type, string account);

    }
}
