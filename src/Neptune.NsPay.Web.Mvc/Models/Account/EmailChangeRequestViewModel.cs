﻿using System;
using System.Web;
using Abp.Runtime.Security;
using Abp.Runtime.Validation;
using Neptune.NsPay.Authorization.Accounts.Dto;

namespace Neptune.NsPay.Web.Models.Account
{
    public class EmailChangeRequestViewModel : ChangeEmailInput
    {
        /// <summary>
        /// Tenant id.
        /// </summary>
        public int? TenantId { get; set; }

        protected override void ResolveParameters()
        {
            base.ResolveParameters();

            if (!string.IsNullOrEmpty(c))
            {
                var parameters = SimpleStringCipher.Instance.Decrypt(c);
                var query = HttpUtility.ParseQueryString(parameters);

                if (query["tenantId"] != null)
                {
                    TenantId = Convert.ToInt32(query["tenantId"]);
                }
            }
        }
    }
}