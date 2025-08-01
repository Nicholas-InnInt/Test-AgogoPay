﻿using System;
using System.Collections.Generic;
using Neptune.NsPay.Organizations.Dto;

namespace Neptune.NsPay.Authorization.Users.Dto
{
    public class GetUserForEditOutput
    {
        public Guid? ProfilePictureId { get; set; }

        public UserEditDto User { get; set; }

        public UserRoleDto[] Roles { get; set; }

        public UserMerchantDto[] Merechants { get; set; }

        public List<OrganizationUnitDto> AllOrganizationUnits { get; set; }

        public List<string> MemberedOrganizationUnits { get; set; }
        
        public string AllowedUserNameCharacters { get; set; }
        
        public bool IsSMTPSettingsProvided { get; set; }

        public List<UserTypeEnum> SupprtedUserType { get; set; }
    }
}