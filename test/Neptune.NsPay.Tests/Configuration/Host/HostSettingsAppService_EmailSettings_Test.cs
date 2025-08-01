﻿using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Net.Mail;
using Abp.Runtime.Security;
using Neptune.NsPay.Configuration.Host;
using Neptune.NsPay.Test.Base;
using Shouldly;

namespace Neptune.NsPay.Tests.Configuration.Host
{
    // ReSharper disable once InconsistentNaming
    public class HostSettingsAppService_EmailSettings_Test : AppTestBase
    {
        private readonly IHostSettingsAppService _hostSettingsAppService;
        private readonly ISettingManager _settingManager;

        public HostSettingsAppService_EmailSettings_Test()
        {
            _hostSettingsAppService = Resolve<IHostSettingsAppService>();
            _settingManager = Resolve<ISettingManager>();

            LoginAsHostAdmin();
            InitializeTestSettings();
        }

        private void InitializeTestSettings()
        {
            _settingManager.ChangeSettingForApplication(EmailSettingNames.DefaultFromAddress, "test@mydomain.com");
            _settingManager.ChangeSettingForApplication(EmailSettingNames.DefaultFromDisplayName, "");
            _settingManager.ChangeSettingForApplication(EmailSettingNames.Smtp.Host, "100.101.102.103");
            _settingManager.ChangeSettingForApplication(EmailSettingNames.Smtp.UserName, "myuser");
            _settingManager.ChangeSettingForApplication(EmailSettingNames.Smtp.Password, SimpleStringCipher.Instance.Encrypt("123456"));
            _settingManager.ChangeSettingForApplication(EmailSettingNames.Smtp.Domain, "mydomain");
            _settingManager.ChangeSettingForApplication(EmailSettingNames.Smtp.EnableSsl, "true");
            _settingManager.ChangeSettingForApplication(EmailSettingNames.Smtp.UseDefaultCredentials, "false");
        }

        [MultiTenantFact]
        public async Task Should_Change_Email_Settings()
        {
            //Get and check current settings

            //Act
            var settings = await _hostSettingsAppService.GetAllSettings();

            //Assert
            settings.Email.DefaultFromAddress.ShouldBe("test@mydomain.com");
            settings.Email.DefaultFromDisplayName.ShouldBe("");
            settings.Email.SmtpHost.ShouldBe("100.101.102.103");
            settings.Email.SmtpPort.ShouldBe(25); //this is the default value
            settings.Email.SmtpUserName.ShouldBe("myuser");
            settings.Email.SmtpPassword.ShouldBe("123456");
            settings.Email.SmtpDomain.ShouldBe("mydomain");
            settings.Email.SmtpEnableSsl.ShouldBe(true);
            settings.Email.SmtpUseDefaultCredentials.ShouldBe(false);

            //Change and save settings

            //Arrange
            settings.Email.DefaultFromDisplayName = "My daily mailing service";
            settings.Email.SmtpHost = "100.101.102.104";
            settings.Email.SmtpPort = 42;
            settings.Email.SmtpUserName = "changeduser";
            settings.Email.SmtpPassword = "654321";
            settings.Email.SmtpDomain = "changeddomain";
            settings.Email.SmtpEnableSsl = false;
            
            //Act
            await _hostSettingsAppService.UpdateAllSettings(settings);

            //Assert
            (await _settingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress)).ShouldBe("test@mydomain.com"); //not changed
            (await _settingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromDisplayName)).ShouldBe("My daily mailing service");
            (await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Host)).ShouldBe("100.101.102.104");
            (await _settingManager.GetSettingValueAsync<int>(EmailSettingNames.Smtp.Port)).ShouldBe(42);
            (await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.UserName)).ShouldBe("changeduser");

            var smtpPassword = await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Password);
            SimpleStringCipher.Instance.Decrypt(smtpPassword).ShouldBe("654321");

            (await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Domain)).ShouldBe("changeddomain");
            (await _settingManager.GetSettingValueAsync<bool>(EmailSettingNames.Smtp.EnableSsl)).ShouldBe(false);
            (await _settingManager.GetSettingValueAsync<bool>(EmailSettingNames.Smtp.UseDefaultCredentials)).ShouldBe(false); //not changed
        }
    }
}
