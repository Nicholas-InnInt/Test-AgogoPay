using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.Authorization.Users.Dto
{
    public class CreateOrUpdateUserInput
    {
        [Required]
        public UserEditDto User { get; set; }

        [Required]
        public string[] AssignedRoleNames { get; set; }

        public string[] AssignedMerchantNames { get; set; }

        public bool SendActivationEmail { get; set; } = false;

        public bool SetRandomPassword { get; set; }

        public List<long> OrganizationUnits { get; set; }

        public CreateOrUpdateUserInput()
        {
            OrganizationUnits = new List<long>();
        }
    }
}