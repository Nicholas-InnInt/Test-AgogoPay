﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Abp;
using Abp.Extensions;
using Abp.Runtime.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Identity;
using Neptune.NsPay.Web.OpenIddict.Claims;
using OpenIddict.Abstractions;

namespace Neptune.NsPay.Web.OpenIddict.Controllers
{
    [Route("connect/token")]
    [IgnoreAntiforgeryToken]
    [ApiExplorerSettings(IgnoreApi = true)]
    public partial class TokenController : AbpOpenIdDictControllerBase
    {
        public TokenController(
            SignInManager signInManager,
            UserManager userManager,
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictAuthorizationManager authorizationManager,
            IOpenIddictScopeManager scopeManager,
            IOpenIddictTokenManager tokenManager,
            AbpOpenIddictClaimsPrincipalManager openIddictClaimsPrincipalManager) :
            base(
                signInManager,
                userManager,
                applicationManager,
                authorizationManager,
                scopeManager,
                tokenManager,
                openIddictClaimsPrincipalManager
            )
        {
        }

        [HttpGet, HttpPost, Produces("application/json")]
        public async Task<IActionResult> HandleAsync()
        {
            var request = HttpContext.GetOpenIddictServerRequest();

            if (request == null)
            {
                throw new InvalidOperationException("The OpenIDConnect request cannot retrieved!");
            }

            if (request.IsPasswordGrantType())
            {
                return await HandlePasswordAsync(request);
            }

            if (request.IsAuthorizationCodeGrantType())
            {
                return await HandleAuthorizationCodeAsync(request);
            }

            throw new AbpException($"The specified grant type {request.GrantType} is not implemented!");
        }

        private int? FindTenantId(ClaimsPrincipal? principal)
        {
            Check.NotNull(principal, nameof(principal));

            var tenantIdOrNull = principal.Claims?.FirstOrDefault(c => c.Type == AbpClaimTypes.TenantId);
            if (tenantIdOrNull == null || tenantIdOrNull.Value.IsNullOrWhiteSpace())
            {
                return null;
            }

            if (Int32.TryParse(tenantIdOrNull.Value, out var guid))
            {
                return guid;
            }

            return null;
        }
    }
}