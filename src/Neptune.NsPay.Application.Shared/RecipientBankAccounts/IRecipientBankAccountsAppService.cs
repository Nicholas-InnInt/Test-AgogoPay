using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Common.Dto;
using Neptune.NsPay.MerchantBills.Dtos;
using Neptune.NsPay.Merchants.Dtos;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PayOrders.Dtos;
using Neptune.NsPay.RecipientBankAccounts.Dtos;

namespace Neptune.NsPay.RecipientBankAccounts
{
    public interface IRecipientBankAccountsAppService : IApplicationService
    {
        Task<PagedResultDto<GetRecipientBankAccountsDtos>> GetAll(GetAllRecipientBankAccountsInput input);

        Task<GetRecipientBankAccountForEditOutput> GetRecipientBankAccountById(EntityDto<string> input);


    }
}