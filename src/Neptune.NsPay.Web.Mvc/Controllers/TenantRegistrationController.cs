﻿using System.Collections.Generic;
using System.Security.Claims;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.Identity;
using Neptune.NsPay.MultiTenancy;
using Neptune.NsPay.MultiTenancy.Dto;
using Neptune.NsPay.MultiTenancy.Payments;
using Neptune.NsPay.Security;
using Neptune.NsPay.Url;
using Neptune.NsPay.Web.Security.Recaptcha;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Microsoft.AspNetCore.Identity;
using Neptune.NsPay.Editions;
using Neptune.NsPay.ExtraProperties;
using Neptune.NsPay.MultiTenancy.Payments.Dto;
using Neptune.NsPay.Web.Models.TenantRegistration;

namespace Neptune.NsPay.Web.Controllers
{
    public class TenantRegistrationController : NsPayControllerBase
    {
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly UserManager _userManager;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly LogInManager _logInManager;
        private readonly SignInManager _signInManager;
        private readonly IWebUrlService _webUrlService;
        private readonly ITenantRegistrationAppService _tenantRegistrationAppService;
        private readonly IPasswordComplexitySettingStore _passwordComplexitySettingStore;
        private readonly IPaymentAppService _paymentAppService;
        private readonly EditionManager _editionManager;
        private readonly IUserClaimsPrincipalFactory<User> _userClaimsPrincipalFactory;

        public TenantRegistrationController(
            IMultiTenancyConfig multiTenancyConfig,
            UserManager userManager,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            LogInManager logInManager,
            SignInManager signInManager,
            IWebUrlService webUrlService,
            ITenantRegistrationAppService tenantRegistrationAppService,
            IPasswordComplexitySettingStore passwordComplexitySettingStore,
            IPaymentAppService paymentAppService,
            EditionManager editionManager,
            IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory)
        {
            _multiTenancyConfig = multiTenancyConfig;
            _userManager = userManager;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _logInManager = logInManager;
            _signInManager = signInManager;
            _webUrlService = webUrlService;
            _tenantRegistrationAppService = tenantRegistrationAppService;
            _passwordComplexitySettingStore = passwordComplexitySettingStore;
            _paymentAppService = paymentAppService;
            _editionManager = editionManager;
            _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        }

        public async Task<ActionResult> SelectEdition()
        {
            CheckTenantRegistrationIsEnabled();

            var output = await _tenantRegistrationAppService.GetEditionsForSelect();
            if (!AbpSession.UserId.HasValue && output.EditionsWithFeatures.IsNullOrEmpty())
            {
                return RedirectToAction("Register", "TenantRegistration");
            }

            var model = ObjectMapper.Map<EditionsSelectViewModel>(output);

            return View(model);
        }

        public async Task<ActionResult> Register(
            int? editionId,
            SubscriptionStartType? subscriptionStartType = null,
            PaymentPeriodType? paymentPeriodType = null)
        {
            CheckTenantRegistrationIsEnabled();

            var model = new TenantRegisterViewModel
            {
                PasswordComplexitySetting = await _passwordComplexitySettingStore.GetSettingsAsync(),
                SubscriptionStartType = subscriptionStartType,
                EditionPaymentType = EditionPaymentType.NewRegistration,
                PaymentPeriodType = paymentPeriodType,
                SuccessUrl = _webUrlService.GetSiteRootAddress().EnsureEndsWith('/') +
                             "TenantRegistration/BuyNowSucceed",
                ErrorUrl = _webUrlService.GetSiteRootAddress().EnsureEndsWith('/') + "Payment/PaymentFailed",
            };

            if (editionId.HasValue)
            {
                model.EditionId = editionId.Value;
                model.Edition = await _tenantRegistrationAppService.GetEdition(editionId.Value);
            }

            ViewBag.UseCaptcha = UseCaptchaOnRegistration();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<ActionResult> Register(RegisterTenantInput model)
        {
            try
            {
                if (UseCaptchaOnRegistration())
                {
                    model.CaptchaResponse = HttpContext.Request.Form[RecaptchaValidator.RecaptchaResponseKey];
                }

                var result = await _tenantRegistrationAppService.RegisterTenant(model);

                CurrentUnitOfWork.SetTenantId(result.TenantId);

                var user = await _userManager.GetAdminAsync();

                // Directly login if possible
                if (result.IsTenantActive &&
                    result.IsActive &&
                    !result.IsEmailConfirmationRequired &&
                    !_webUrlService.SupportsTenancyNameInUrl)
                {
                    var loginResult = await GetLoginResultAsync(user.UserName, model.AdminPassword, model.TenancyName);

                    if (loginResult.Result == AbpLoginResultType.Success)
                    {
                        await _signInManager.SignOutAsync();
                        await _signInManager.SignInAsync(loginResult.Identity, false);

                        SetTenantIdCookie(result.TenantId);

                        return Redirect(Url.Action("Index", "Home", new {area = "AppArea"}));
                    }

                    Logger.Warn("New registered user could not be login. This should not be normally. login result: " +
                                loginResult.Result);
                }

                //Show result page
                var resultModel = ObjectMapper.Map<TenantRegisterResultViewModel>(result);

                resultModel.TenantLoginAddress = _webUrlService.SupportsTenancyNameInUrl
                    ? _webUrlService.GetSiteRootAddress(model.TenancyName).EnsureEndsWith('/') + "Account/Login"
                    : "";

                if (result.PaymentId.HasValue)
                {
                    return RedirectToAction("GatewaySelection", "Payment", new
                    {
                        paymentId = result.PaymentId.Value
                    });
                }

                return View("RegisterResult", resultModel);
            }
            catch (UserFriendlyException ex)
            {
                ViewBag.UseCaptcha = UseCaptchaOnRegistration();
                ViewBag.ErrorMessage = ex.Message;

                var viewModel = new TenantRegisterViewModel
                {
                    PasswordComplexitySetting = await _passwordComplexitySettingStore.GetSettingsAsync(),
                    EditionId = model.EditionId,
                    SubscriptionStartType = model.SubscriptionStartType,
                    EditionPaymentType = EditionPaymentType.NewRegistration
                };

                if (model.EditionId.HasValue)
                {
                    viewModel.Edition = await _tenantRegistrationAppService.GetEdition(model.EditionId.Value);
                    viewModel.EditionId = model.EditionId.Value;
                }

                return View("Register", viewModel);
            }
        }

        public async Task<IActionResult> BuyNowSucceed(long paymentId)
        {
            await _tenantRegistrationAppService.BuyNowSucceed(paymentId);
            await LoginAdminAsync();
            return RedirectToAction("Index", "SubscriptionManagement", new {area = "AppArea"});
        }

        public async Task NewRegistrationSucceed(long paymentId)
        {
            await _tenantRegistrationAppService.NewRegistrationSucceed(paymentId);
        }

        public async Task<IActionResult> ExtendSucceed(long paymentId)
        {
            await _tenantRegistrationAppService.ExtendSucceed(paymentId);
            return RedirectToAction("Index", "SubscriptionManagement", new {area = "AppArea"});
        }

        public async Task<IActionResult> UpgradeSucceed(long paymentId)
        {
            await _tenantRegistrationAppService.UpgradeSucceed(paymentId);
            return RedirectToAction("Index", "SubscriptionManagement", new {area = "AppArea"});
        }

        private async Task LoginAdminAsync()
        {
            var user = await _userManager.GetAdminAsync();
            var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(principal.Identity as ClaimsIdentity, false);
        }

        private bool IsSelfRegistrationEnabled()
        {
            return SettingManager.GetSettingValueForApplication<bool>(
                AppSettings.TenantManagement.AllowSelfRegistration);
        }

        private void CheckTenantRegistrationIsEnabled()
        {
            if (!IsSelfRegistrationEnabled())
            {
                throw new UserFriendlyException(L("SelfTenantRegistrationIsDisabledMessage_Detail"));
            }

            if (!_multiTenancyConfig.IsEnabled)
            {
                throw new UserFriendlyException(L("MultiTenancyIsNotEnabled"));
            }
        }

        private bool UseCaptchaOnRegistration()
        {
            return SettingManager.GetSettingValueForApplication<bool>(AppSettings.TenantManagement
                .UseCaptchaOnRegistration);
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress,
            string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result,
                        usernameOrEmailAddress, tenancyName);
            }
        }
    }
}