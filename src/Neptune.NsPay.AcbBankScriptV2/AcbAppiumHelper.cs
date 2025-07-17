using Neptune.NsPay.Commons;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Text.RegularExpressions;

namespace Neptune.NsPay.AcbBankScriptV2
{
    public class AcbAppiumHelper
    {
        private AndroidDriver _driver;
        public bool SetUp(string deviceName)
        {
            try
            {
                var serverUri = new Uri(AppSettings.Configuration["APPIUM_HOST"] ?? "http://127.0.0.1:4723/");
                var driverOptions = new AppiumOptions()
                {
                    AutomationName = AutomationName.AndroidUIAutomator2,
                    PlatformName = "Android",
                    DeviceName = deviceName,
                };

                driverOptions.AddAdditionalAppiumOption("udid", deviceName);
                driverOptions.AddAdditionalAppiumOption("appPackage", "mobile.acb.com.vn");
                driverOptions.AddAdditionalAppiumOption("waitForIdleTimeout", 100);
                driverOptions.AddAdditionalAppiumOption("noReset", true);

                _driver = new AndroidDriver(serverUri, driverOptions);
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                return true;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("初始化异常：" + ex);
                return false;
            }
        }

        public string Start(string passwd)
        {
            var otp = "";
            Console.WriteLine("click otp");
            var getotp = _driver.FindElement(By.XPath("//android.widget.TextView[@text=\"Lấy OTP\"]"));
            if (getotp != null)
            {
                getotp.Click();
                var okayOtp = CheckElement("//android.widget.TextView[@text=\"Nhập mã PIN\"]", "xpath");
                if (okayOtp)
                {
                    var tempStr = passwd.ToCharArray();
                    for (var i = 1; i <= 4; i++)
                    {
                        var pwd = _driver.FindElement(By.XPath("//android.widget.ScrollView/android.view.ViewGroup/android.view.ViewGroup/android.view.ViewGroup[" + i + "]"));
                        if (pwd != null)
                        {
                            _driver.PressKeyCode(GetKeyCode(tempStr[i - 1].ToString()));
                        }
                    }

                    var otpResult = CheckElement("//android.widget.TextView[@text=\"OTP Safekey\"]", "xpath");
                    if (otpResult)
                    {
                        //读取otp
                        otp = GetOtp();
                    }
                }
            }
            return otp;
        }

        public string GetOtp()
        {
            var otp = "";
            try
            {
                var otpElem = _driver.FindElement(By.XPath("//hierarchy/android.widget.FrameLayout[1]/android.widget.LinearLayout[1]/android.widget.FrameLayout[1]/android.widget.LinearLayout[1]/android.widget.FrameLayout[1]/android.widget.FrameLayout[1]/android.view.ViewGroup[1]/android.view.ViewGroup[1]/android.view.ViewGroup[1]/android.view.ViewGroup[1]/android.view.ViewGroup[1]/android.view.ViewGroup[1]/android.view.ViewGroup[1]/android.view.ViewGroup[1]/android.view.ViewGroup[1]/android.view.ViewGroup[1]/android.view.ViewGroup[2]/android.widget.TextView[1]"));
                if (otpElem != null)
                {
                    otp = otpElem.Text;
                    if (!string.IsNullOrEmpty(otp))
                    {
                        var check = IsAllDigits(otp);
                        if (check)
                        {
                            return otp;
                        }
                        else
                        {
                            return "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return otp;
        }

        public void BackMain()
        {
            var done = _driver.FindElement(By.XPath("//android.widget.TextView[@text=\"ĐÓNG\"]"));
            if(done != null)
            {
                done.Click();
            }
        }

        bool CheckElement(string id, string type)
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            try
            {

                // 修改为你应用中的弹窗元素标识符
                By by = null;
                if (type == "xpath")
                {
                    by = By.XPath(id);
                }
                AppiumElement alert = (AppiumElement)wait.Until(ExpectedConditions.ElementIsVisible(by));
                if (alert != null)
                {
                    return true;
                }
                return false;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public void GetDisplayDensity()
        {
            _driver.GetDisplayDensity();
        }

        public void KeepSessionAlive(string deviceName)
        {
            if (_driver.SessionId != null)
            {
                SetUp(deviceName);
            }
        }


        bool IsAllDigits(string input)
        {
            Regex regex = new Regex(@"^\d+$");
            return regex.IsMatch(input);
        }

        public int GetKeyCode(string str)
        {
            var code = str.ParseToInt();
            switch (code)
            {
                case 0:
                    return AndroidKeyCode.Keycode_0;
                case 1:
                    return AndroidKeyCode.Keycode_1;
                case 2:
                    return AndroidKeyCode.Keycode_2;
                case 3:
                    return AndroidKeyCode.Keycode_3;
                case 4:
                    return AndroidKeyCode.Keycode_4;
                case 5:
                    return AndroidKeyCode.Keycode_5;
                case 6:
                    return AndroidKeyCode.Keycode_6;
                case 7:
                    return AndroidKeyCode.Keycode_7;
                case 8:
                    return AndroidKeyCode.Keycode_8;
                case 9:
                    return AndroidKeyCode.Keycode_9;
            }
            return AndroidKeyCode.Keycode_0;
        }
    }
}
