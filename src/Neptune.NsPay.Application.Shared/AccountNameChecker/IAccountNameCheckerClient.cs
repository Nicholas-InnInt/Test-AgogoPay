using Neptune.NsPay.AccountNameChecker.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.AccountNameChecker
{
    public interface IAccountNameCheckerClient
    {
        Task<CheckBankDetailResultDto> Get(string bankName, string accountNumber);
    }
}
