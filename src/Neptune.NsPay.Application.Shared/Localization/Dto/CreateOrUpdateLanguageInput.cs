using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.Localization.Dto
{
    public class CreateOrUpdateLanguageInput
    {
        [Required]
        public ApplicationLanguageEditDto Language { get; set; }
    }
}