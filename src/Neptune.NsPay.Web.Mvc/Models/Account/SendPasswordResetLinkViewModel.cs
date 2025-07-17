using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.Web.Models.Account
{
    public class SendPasswordResetLinkViewModel
    {
        [Required]
        public string EmailAddress { get; set; }
    }
}