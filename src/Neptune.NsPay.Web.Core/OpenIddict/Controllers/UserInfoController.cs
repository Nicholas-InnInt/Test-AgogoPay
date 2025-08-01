﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Runtime.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Identity;
using Neptune.NsPay.Web.OpenIddict.Claims;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Neptune.NsPay.Web.OpenIddict.Controllers
{
    [Route("connect/userinfo")]
    [IgnoreAntiforgeryToken]
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserInfoController : AbpOpenIdDictControllerBase
    {
        public UserInfoController(
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

        [HttpGet]
        [HttpPost]
        [Produces("application/json")]
        public virtual async Task<IActionResult> Userinfo()
        {
            var claims = await GetUserInfoClaims();
            if (claims == null)
            {
                return Challenge(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidToken,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The specified access token is bound to an account that no longer exists."
                    }));
            }

            return Ok(claims);
        }

        protected virtual async Task<Dictionary<string, object>> GetUserInfoClaims()
        {
            var user = await UserManager.GetUserAsync(User);
            if (user == null)
            {
                return null;
            }

            var claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
                [OpenIddictConstants.Claims.Subject] = await UserManager.GetUserIdAsync(user)
            };

            if (User.HasScope(OpenIddictConstants.Scopes.Profile))
            {
                claims[AbpClaimTypes.TenantId] = user.TenantId;
                claims[OpenIddictConstants.Claims.PreferredUsername] = user.UserName;
                claims[OpenIddictConstants.Claims.FamilyName] = user.Surname;
                claims[OpenIddictConstants.Claims.GivenName] = user.Name;
            }

            if (User.HasScope(OpenIddictConstants.Scopes.Email))
            {
                claims[OpenIddictConstants.Claims.Email] = await UserManager.GetEmailAsync(user);
                claims[OpenIddictConstants.Claims.EmailVerified] = await UserManager.IsEmailConfirmedAsync(user);
            }

            if (User.HasScope(OpenIddictConstants.Scopes.Phone))
            {
                claims[OpenIddictConstants.Claims.PhoneNumber] = await UserManager.GetPhoneNumberAsync(user);
                claims[OpenIddictConstants.Claims.PhoneNumberVerified] =
                    await UserManager.IsPhoneNumberConfirmedAsync(user);
            }

            if (User.HasScope(OpenIddictConstants.Scopes.Roles))
            {
                claims[OpenIddictConstants.Claims.Role] = await UserManager.GetRolesAsync(user);
            }

            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

            return claims;
        }
    }
}