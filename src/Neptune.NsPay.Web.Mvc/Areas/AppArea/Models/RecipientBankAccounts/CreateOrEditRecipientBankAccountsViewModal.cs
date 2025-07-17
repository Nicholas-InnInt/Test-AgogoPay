using Neptune.NsPay.PayOrders.Dtos;
using Neptune.NsPay.RecipientBankAccounts.Dtos;
using NUglify.Helpers;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.RecipientBankAccounts
{
    public class CreateOrEditRecipientBankAccountsViewModal
    {
        
        public CreateOrEditRecipientBankAccountsDto RecipientBankAccounts { get; set; }

        public bool IsEditMode { get; set; }
    }
}
