﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Localization;
using Abp.Net.Mail;
using Neptune.NsPay.Chat;
using Neptune.NsPay.Editions;
using Neptune.NsPay.Localization;
using Neptune.NsPay.MultiTenancy;
using System.Net.Mail;
using System.Web;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Abp.Timing;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.Net.Emailing;

namespace Neptune.NsPay.Authorization.Users
{
    /// <summary>
    /// Used to send email to users.
    /// </summary>
    public class UserEmailer : NsPayServiceBase, IUserEmailer, ITransientDependency
    {
        private readonly IEmailTemplateProvider _emailTemplateProvider;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly ICurrentUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ISettingManager _settingManager;
        private readonly EditionManager _editionManager;
        private readonly UserManager _userManager;
        private readonly IAbpSession _abpSession;

        // used for styling action links on email messages.
        private string _emailButtonStyle =
            "padding-left: 30px; padding-right: 30px; padding-top: 12px; padding-bottom: 12px; color: #ffffff; background-color: #00bb77; font-size: 14pt; text-decoration: none;";

        private string _emailButtonColor = "#00bb77";

        public UserEmailer(
            IEmailTemplateProvider emailTemplateProvider,
            IEmailSender emailSender,
            IRepository<Tenant> tenantRepository,
            ICurrentUnitOfWorkProvider unitOfWorkProvider,
            IUnitOfWorkManager unitOfWorkManager,
            ISettingManager settingManager,
            EditionManager editionManager,
            UserManager userManager,
            IAbpSession abpSession)
        {
            _emailTemplateProvider = emailTemplateProvider;
            _emailSender = emailSender;
            _tenantRepository = tenantRepository;
            _unitOfWorkProvider = unitOfWorkProvider;
            _unitOfWorkManager = unitOfWorkManager;
            _settingManager = settingManager;
            _editionManager = editionManager;
            _userManager = userManager;
            _abpSession = abpSession;
        }

        /// <summary>
        /// Send email activation link to user's email address.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="link">Email activation link</param>
        /// <param name="plainPassword">
        /// Can be set to user's plain password to include it in the email.
        /// </param>
        public virtual async Task SendEmailActivationLinkAsync(User user, string link, string plainPassword = null)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                if (user.EmailConfirmationCode.IsNullOrEmpty())
                {
                    throw new Exception("EmailConfirmationCode should be set in order to send email activation link.");
                }

                link = link.Replace("{userId}", user.Id.ToString());
                link = link.Replace("{confirmationCode}", Uri.EscapeDataString(user.EmailConfirmationCode));

                if (user.TenantId.HasValue)
                {
                    link = link.Replace("{tenantId}", user.TenantId.ToString());
                }

                link = EncryptQueryParameters(link);

                var tenancyName = GetTenancyNameOrNull(user.TenantId);
                var emailTemplate = GetTitleAndSubTitle(user.TenantId, L("EmailActivation_Title"),
                    L("EmailActivation_SubTitle"));
                var mailMessage = new StringBuilder();

                mailMessage.AppendLine("<b>" + L("NameSurname") + "</b>: " + user.Name + " " + user.Surname + "<br />");

                if (!tenancyName.IsNullOrEmpty())
                {
                    mailMessage.AppendLine("<b>" + L("TenancyName") + "</b>: " + tenancyName + "<br />");
                }

                mailMessage.AppendLine("<b>" + L("UserName") + "</b>: " + user.UserName + "<br />");

                if (!plainPassword.IsNullOrEmpty())
                {
                    mailMessage.AppendLine("<b>" + L("Password") + "</b>: " + plainPassword + "<br />");
                }

                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine(L("EmailActivation_ClickTheLinkBelowToVerifyYourEmail") + "<br /><br />");
                mailMessage.AppendLine("<a style=\"" + _emailButtonStyle + "\" bg-color=\"" + _emailButtonColor +
                                       "\" href=\"" + link + "\">" + L("Verify") + "</a>");
                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine("<span style=\"font-size: 9pt;\">" +
                                       L("EmailMessage_CopyTheLinkBelowToYourBrowser") + "</span><br />");
                mailMessage.AppendLine("<span style=\"font-size: 8pt;\">" + link + "</span>");

                await ReplaceBodyAndSendAsync(user.EmailAddress, L("EmailActivation_Subject"), emailTemplate,
                    mailMessage);
            });
        }

        /// <summary>
        /// Sends a password reset link to user's email.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="link">Reset link</param>
        public async Task SendPasswordResetLinkAsync(User user, string link = null)
        {
            var expirationHours = await _settingManager.GetSettingValueAsync<int>(
                AppSettings.UserManagement.Password.PasswordResetCodeExpirationHours
            );

            if (user.PasswordResetCode.IsNullOrEmpty())
            {
                throw new Exception("PasswordResetCode should be set in order to send password reset link.");
            }

            var tenancyName = GetTenancyNameOrNull(user.TenantId);
            var emailTemplate = GetTitleAndSubTitle(user.TenantId, L("PasswordResetEmail_Title"),
                L("PasswordResetEmail_SubTitle"));
            var mailMessage = new StringBuilder();

            mailMessage.AppendLine("<b>" + L("NameSurname") + "</b>: " + user.Name + " " + user.Surname + "<br />");

            if (!tenancyName.IsNullOrEmpty())
            {
                mailMessage.AppendLine("<b>" + L("TenancyName") + "</b>: " + tenancyName + "<br />");
            }

            mailMessage.AppendLine("<b>" + L("UserName") + "</b>: " + user.UserName + "<br />");
            mailMessage.AppendLine("<b>" + L("ResetCode") + "</b>: " + user.PasswordResetCode + "<br />");

            if (!link.IsNullOrEmpty())
            {
                link = link.Replace("{userId}", user.Id.ToString());
                link = link.Replace("{resetCode}", Uri.EscapeDataString(user.PasswordResetCode));

                var expireDate = Uri.EscapeDataString(Clock.Now.AddHours(expirationHours)
                    .ToString(NsPayConsts.DateTimeOffsetFormat, CultureInfo.InvariantCulture));

                link = link.Replace("{expireDate}", expireDate);

                if (user.TenantId.HasValue)
                {
                    link = link.Replace("{tenantId}", user.TenantId.ToString());
                }

                link = EncryptQueryParameters(link);

                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine(L("PasswordResetEmail_ClickTheLinkBelowToResetYourPassword") + "<br /><br />");
                mailMessage.AppendLine("<a style=\"" + _emailButtonStyle + "\" bg-color=\"" + _emailButtonColor +
                                       "\" href=\"" + link + "\">" + L("Reset") + "</a>");
                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine("<span style=\"font-size: 9pt;\">" +
                                       L("EmailMessage_CopyTheLinkBelowToYourBrowser") + "</span><br />");
                mailMessage.AppendLine("<span style=\"font-size: 8pt;\">" + link + "</span>");
            }

            await ReplaceBodyAndSendAsync(user.EmailAddress, L("PasswordResetEmail_Subject"), emailTemplate,
                mailMessage);
        }

        public async Task TryToSendChatMessageMail(User user, string senderUsername, string senderTenancyName,
            ChatMessage chatMessage)
        {
            try
            {
                var emailTemplate = GetTitleAndSubTitle(user.TenantId, L("NewChatMessageEmail_Title"),
                    L("NewChatMessageEmail_SubTitle"));
                var mailMessage = new StringBuilder();

                mailMessage.AppendLine("<b>" + L("Sender") + "</b>: " + senderTenancyName + "/" + senderUsername +
                                       "<br />");
                mailMessage.AppendLine("<b>" + L("Time") + "</b>: " +
                                       chatMessage.CreationTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") +
                                       " UTC<br />");
                mailMessage.AppendLine("<b>" + L("Message") + "</b>: " + chatMessage.Message + "<br />");
                mailMessage.AppendLine("<br />");

                await ReplaceBodyAndSendAsync(user.EmailAddress, L("NewChatMessageEmail_Subject"), emailTemplate,
                    mailMessage);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        public async Task SendEmailChangeRequestLinkAsync(User user, string emailAddress, string link)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                link = link.Replace("{userId}", user.Id.ToString());
                link = link.Replace("{emailAddress}", Uri.EscapeDataString(emailAddress));
                link = link.Replace("{oldMailAddress}", Uri.EscapeDataString(user.EmailAddress));

                if (user.TenantId.HasValue)
                {
                    link = link.Replace("{tenantId}", user.TenantId.ToString());
                }

                link = EncryptQueryParameters(link);

                var tenancyName = GetTenancyNameOrNull(user.TenantId);
                var emailTemplate = GetTitleAndSubTitle(user.TenantId, L("EmailChangeRequest_Title"),
                    L("EmailChangeRequest_SubTitle"));
                var mailMessage = new StringBuilder();

                mailMessage.AppendLine("<b>" + L("NameSurname") + "</b>: " + user.Name + " " + user.Surname + "<br />");

                if (!tenancyName.IsNullOrEmpty())
                {
                    mailMessage.AppendLine("<b>" + L("TenancyName") + "</b>: " + tenancyName + "<br />");
                }

                mailMessage.AppendLine("<b>" + L("UserName") + "</b>: " + user.UserName + "<br />");
                
                mailMessage.AppendLine("<b>" + L("NewEmailAddress") + "</b>: " + emailAddress + "<br />");

                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine(L("EmailChangeRequest_ClickTheLinkBelowToChangeYourEmail") + "<br /><br />");
                mailMessage.AppendLine("<a style=\"" + _emailButtonStyle + "\" bg-color=\"" + _emailButtonColor +
                                       "\" href=\"" + link + "\">" + L("Verify") + "</a>");
                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine("<span style=\"font-size: 9pt;\">" +
                                       L("EmailMessage_CopyTheLinkBelowToYourBrowser") + "</span><br />");
                mailMessage.AppendLine("<span style=\"font-size: 8pt;\">" + link + "</span>");

                await ReplaceBodyAndSendAsync(user.EmailAddress, L("EmailChangeRequest_Subject"), emailTemplate,
                    mailMessage);
            });
        }

        public async Task TryToSendSubscriptionExpireEmail(int tenantId, DateTime utcNow)
        {
            try
            {
                using (_unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                    {
                        var tenantAdmin = await _userManager.GetAdminAsync();
                        if (tenantAdmin == null || string.IsNullOrEmpty(tenantAdmin.EmailAddress))
                        {
                            return;
                        }

                        var hostAdminLanguage = await _settingManager.GetSettingValueForUserAsync(
                            LocalizationSettingNames.DefaultLanguage, tenantAdmin.TenantId, tenantAdmin.Id);
                        var culture = CultureHelper.GetCultureInfoByChecking(hostAdminLanguage);
                        var emailTemplate = GetTitleAndSubTitle(tenantId, L("SubscriptionExpire_Title"),
                            L("SubscriptionExpire_SubTitle"));
                        var mailMessage = new StringBuilder();

                        mailMessage.AppendLine("<b>" + L("Message") + "</b>: " + L("SubscriptionExpire_Email_Body",
                            culture, utcNow.ToString("yyyy-MM-dd") + " UTC") + "<br />");
                        mailMessage.AppendLine("<br />");

                        await ReplaceBodyAndSendAsync(tenantAdmin.EmailAddress, L("SubscriptionExpire_Email_Subject"),
                            emailTemplate, mailMessage);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        public async Task TryToSendSubscriptionAssignedToAnotherEmail(int tenantId, DateTime utcNow,
            int expiringEditionId)
        {
            try
            {
                using (_unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                    {
                        var tenantAdmin = await _userManager.GetAdminAsync();
                        if (tenantAdmin == null || string.IsNullOrEmpty(tenantAdmin.EmailAddress))
                        {
                            return;
                        }

                        var hostAdminLanguage = await _settingManager.GetSettingValueForUserAsync(
                            LocalizationSettingNames.DefaultLanguage, tenantAdmin.TenantId, tenantAdmin.Id);
                        var culture = CultureHelper.GetCultureInfoByChecking(hostAdminLanguage);
                        var expringEdition = await _editionManager.GetByIdAsync(expiringEditionId);
                        var emailTemplate = GetTitleAndSubTitle(tenantId, L("SubscriptionExpire_Title"),
                            L("SubscriptionExpire_SubTitle"));
                        var mailMessage = new StringBuilder();

                        mailMessage.AppendLine("<b>" + L("Message") + "</b>: " +
                                               L("SubscriptionAssignedToAnother_Email_Body", culture,
                                                   expringEdition.DisplayName, utcNow.ToString("yyyy-MM-dd") + " UTC") +
                                               "<br />");
                        mailMessage.AppendLine("<br />");

                        await ReplaceBodyAndSendAsync(tenantAdmin.EmailAddress, L("SubscriptionExpire_Email_Subject"),
                            emailTemplate, mailMessage);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        public async Task TryToSendFailedSubscriptionTerminationsEmail(List<string> failedTenancyNames, DateTime utcNow)
        {
            try
            {
                var hostAdmin = await _userManager.GetAdminAsync();
                if (hostAdmin == null || string.IsNullOrEmpty(hostAdmin.EmailAddress))
                {
                    return;
                }

                var hostAdminLanguage =
                    await _settingManager.GetSettingValueForUserAsync(LocalizationSettingNames.DefaultLanguage,
                        hostAdmin.TenantId, hostAdmin.Id);
                var culture = CultureHelper.GetCultureInfoByChecking(hostAdminLanguage);
                var emailTemplate = GetTitleAndSubTitle(null, L("FailedSubscriptionTerminations_Title"),
                    L("FailedSubscriptionTerminations_SubTitle"));
                var mailMessage = new StringBuilder();

                mailMessage.AppendLine("<b>" + L("Message") + "</b>: " + L("FailedSubscriptionTerminations_Email_Body",
                    culture, string.Join(",", failedTenancyNames), utcNow.ToString("yyyy-MM-dd") + " UTC") + "<br />");
                mailMessage.AppendLine("<br />");

                await ReplaceBodyAndSendAsync(hostAdmin.EmailAddress, L("FailedSubscriptionTerminations_Email_Subject"),
                    emailTemplate, mailMessage);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        public async Task TryToSendSubscriptionExpiringSoonEmail(int tenantId, DateTime dateToCheckRemainingDayCount)
        {
            try
            {
                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (UnitOfWorkManager.Current.SetTenantId(tenantId))
                    {
                        var tenantAdmin = await _userManager.GetAdminAsync();
                        if (tenantAdmin == null || string.IsNullOrEmpty(tenantAdmin.EmailAddress))
                        {
                            return;
                        }

                        var tenantAdminLanguage = await _settingManager.GetSettingValueForUserAsync(
                            LocalizationSettingNames.DefaultLanguage,
                            tenantAdmin.TenantId,
                            tenantAdmin.Id
                        );

                        var culture = CultureHelper.GetCultureInfoByChecking(tenantAdminLanguage);

                        var emailTemplate = GetTitleAndSubTitle(
                            null,
                            L("SubscriptionExpiringSoon_Title"),
                            L("SubscriptionExpiringSoon_SubTitle")
                        );

                        var mailMessage = new StringBuilder();

                        mailMessage.AppendLine("<b>" + L("Message") + "</b>: " +
                                               L("SubscriptionExpiringSoon_Email_Body", culture,
                                                   dateToCheckRemainingDayCount.ToString("yyyy-MM-dd") + " UTC") +
                                               "<br />");
                        mailMessage.AppendLine("<br />");

                        await ReplaceBodyAndSendAsync(
                            tenantAdmin.EmailAddress,
                            L("SubscriptionExpiringSoon_Email_Subject"),
                            emailTemplate,
                            mailMessage
                        );
                    }
                });
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        public void TryToSendPaymentNotCompletedEmail(int tenantId, string urlToPayment)
        {
            try
            {
                _unitOfWorkManager.WithUnitOfWork(() =>
                {
                    using (UnitOfWorkManager.Current.SetTenantId(tenantId))
                    {
                        var tenantAdmin = _userManager.GetAdmin();
                        if (tenantAdmin == null || string.IsNullOrEmpty(tenantAdmin.EmailAddress))
                        {
                            return;
                        }

                        var emailTemplate = GetTitleAndSubTitle(null, L("SubscriptionPaymentNotCompleted_Title"),
                            L("SubscriptionPaymentNotCompleted_SubTitle"));
                        var mailMessage = new StringBuilder();

                        mailMessage.AppendLine(L("SubscriptionPaymentNotCompleted_Email_Body", urlToPayment) +
                                               "<br />");
                        mailMessage.AppendLine("<br />");

                        ReplaceBodyAndSend(tenantAdmin.EmailAddress, L("SubscriptionPaymentNotCompleted_Email_Subject"),
                            emailTemplate, mailMessage);
                    }
                });
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private string GetTenancyNameOrNull(int? tenantId)
        {
            if (tenantId == null)
            {
                return null;
            }

            using (_unitOfWorkProvider.Current.SetTenantId(null))
            {
                return _tenantRepository.Get(tenantId.Value).TenancyName;
            }
        }

        private StringBuilder GetTitleAndSubTitle(int? tenantId, string title, string subTitle)
        {
            var emailTemplate = new StringBuilder(_emailTemplateProvider.GetDefaultTemplate(tenantId));
            emailTemplate.Replace("{EMAIL_TITLE}", title);
            emailTemplate.Replace("{EMAIL_SUB_TITLE}", subTitle);

            return emailTemplate;
        }

        private async Task ReplaceBodyAndSendAsync(string emailAddress, string subject, StringBuilder emailTemplate,
            StringBuilder mailMessage)
        {
            emailTemplate.Replace("{EMAIL_BODY}", mailMessage.ToString());
            await _emailSender.SendAsync(new MailMessage
            {
                To = { emailAddress },
                Subject = subject,
                Body = emailTemplate.ToString(),
                IsBodyHtml = true
            });
        }

        private void ReplaceBodyAndSend(string emailAddress, string subject, StringBuilder emailTemplate,
            StringBuilder mailMessage)
        {
            emailTemplate.Replace("{EMAIL_BODY}", mailMessage.ToString());
            _emailSender.Send(new MailMessage
            {
                To = { emailAddress },
                Subject = subject,
                Body = emailTemplate.ToString(),
                IsBodyHtml = true
            });
        }

        /// <summary>
        /// Returns link with encrypted parameters
        /// </summary>
        /// <param name="link"></param>
        /// <param name="encrptedParameterName"></param>
        /// <returns></returns>
        private string EncryptQueryParameters(string link, string encrptedParameterName = "c")
        {
            if (!link.Contains("?"))
            {
                return link;
            }

            var basePath = link.Substring(0, link.IndexOf('?'));
            var query = link.Substring(link.IndexOf('?')).TrimStart('?');

            return basePath + "?" + encrptedParameterName + "=" +
                   HttpUtility.UrlEncode(SimpleStringCipher.Instance.Encrypt(query));
        }
    }
}
