﻿using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.Authorization.Roles.Dto
{
    public class RoleEditDto
    {
        public int? Id { get; set; }

        [Required]
        public string DisplayName { get; set; }
        
        public bool IsDefault { get; set; }
    }
}