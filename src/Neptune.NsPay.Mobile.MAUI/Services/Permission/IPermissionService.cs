﻿namespace Neptune.NsPay.Services.Permission
{
    public interface IPermissionService
    {
        bool HasPermission(string key);
    }
}