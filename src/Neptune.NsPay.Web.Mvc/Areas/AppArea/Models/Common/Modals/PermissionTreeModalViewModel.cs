﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neptune.NsPay.Authorization.Permissions.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Common.Modals
{
    public class PermissionTreeModalViewModel : IPermissionsEditViewModel
    {
        public List<FlatPermissionDto> Permissions { get; set; }
        public List<string> GrantedPermissionNames { get; set; }
    }
}
