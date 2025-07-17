using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.Authorization.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}
