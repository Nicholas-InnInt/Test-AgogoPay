using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.Authorization.Accounts.Dto
{
    public class SendEmailActivationLinkInput
    {
        [Required]
        public string EmailAddress { get; set; }
    }
}