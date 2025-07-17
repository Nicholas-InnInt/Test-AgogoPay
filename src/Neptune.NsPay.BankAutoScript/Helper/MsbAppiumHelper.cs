using Neptune.NsPay.Commons;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Interactions;

namespace Neptune.NsPay.BankAutoScript.Helper
{
    public class MsbAppiumHelper
    {
        private AndroidDriver _driver;

        public static ProcessStartInfo StartAppiumService(int port)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = @"C:\Windows\System32\cmd.exe",
                Arguments = "/C appium -p " + port + " --session-override",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            return startInfo;
        }


        public bool SetUp(int port, string deviceName)
        {
            try
            {
                //var serverUri = new Uri(AppSettings.Configuration["APPIUM_HOST"] ?? "http://127.0.0.1:4723/");
                var APPIUM_HOST = "http://127.0.0.1:" + port + "/";
                var serverUri = new Uri(APPIUM_HOST);
                var driverOptions = new AppiumOptions()
                {
                    AutomationName = AutomationName.AndroidUIAutomator2,
                    PlatformName = "Android",
                    DeviceName = deviceName,
                };

                driverOptions.AddAdditionalAppiumOption("udid", deviceName);
                driverOptions.AddAdditionalAppiumOption("appPackage", "vn.com.msb.smartBanking");
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

        public void KeepSessionAlive(int port, string deviceName)
        {
            if (_driver.SessionId != null)
            {
                SetUp(port, deviceName);
            }
        }

        public bool IsMain()
        {
            try
            {
                var ele = CheckElement("new UiSelector().text(\"QR code\")", "android", 100);
                return ele;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void ClickBtnAccept()
        {
            try
            {
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().className(\"android.view.ViewGroup\").instance(28)"));
                if (otpResult != null)
                {
                    otpResult.Click();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public bool IsOtp()
        {
            try
            {
                var ele = CheckElement("new UiSelector().text(\"Nhập mã PIN Soft Token\")", "android", 100);
                return ele;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void InputOtp(string pwd)
        {
            try
            {
                var tempStr = pwd.ToCharArray();
                for (var i = 0; i <= tempStr.Length; i++)
                {
                    var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().description(\"" + tempStr[i] + "\")"));
                    if (otpResult != null)
                    {
                        otpResult.Click();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public bool IsCheckOtp()
        {
            try
            {
                var ele = CheckElement("new UiSelector().resourceId(\"vn.com.msb.smartBanking:id/texture_view\")", "android", 100);
                return ele;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void BtnCheckOtp()
        {
            try
            {
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().className(\"android.view.ViewGroup\").instance(16)"));
                if (otpResult != null)
                {
                    otpResult.Click();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public bool IsTransferOtp()
        {
            try
            {
                var ele = CheckElement("new UiSelector().text(\"Nhập mã giao dịch\")", "android", 100);
                return ele;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void InputTransferOtp(string pwd)
        {
            try
            { 
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().text(\"Nhập mã giao dịch\")"));
                if (otpResult != null)
                {
                    otpResult.SendKeys(pwd);
                    //输入enter
                    _driver.PressKeyCode(AndroidKeyCode.Enter);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public bool ClickBtnNext()
        {
            try
            {
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().className(\"android.view.ViewGroup\").instance(17)"));
                if (otpResult != null)
                {
                    otpResult.Click();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        public string GetOtp()
        {
            try
            {
                var otp = "";
                var result = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().className(\"android.view.ViewGroup\").instance(12)"));
                if (result != null)
                {
                    var textViews = result.FindElements(By.ClassName("android.widget.TextView"));
                    if (textViews.Count > 1)
                    {
                        otp = textViews[1].Text;
                    }
                }
                return otp;
            }
            catch (Exception ex)
            {
                return "";
            }
        }


        public bool ClickBtnGoBack()
        {
            try
            {
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().className(\"android.view.ViewGroup\").instance(14)"));
                if (otpResult != null)
                {
                    otpResult.Click();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }


        Tuple<int, int> GetFirstCoordinate(string bounds)
        {
            // 移除括号并拆分字符串
            var parts = bounds.Replace("[", "").Replace("]", ",").Split(',');
            int x = int.Parse(parts[0].Trim());
            int y = int.Parse(parts[1].Trim());

            return new Tuple<int, int>(x, y);
        }

        Tuple<int, int> GetSecondCoordinate(string bounds)
        {
            // 移除括号并拆分字符串
            var parts = bounds.Replace("[", "").Replace("]", ",").Split(',');
            int x = int.Parse(parts[2].Trim());
            int y = int.Parse(parts[3].Trim());

            return new Tuple<int, int>(x, y);
        }

        void ClickPoints(int x, int y)
        {
            PointerInputDevice touchDevice = new PointerInputDevice(PointerKind.Touch);
            ActionSequence tapAction = new ActionSequence(touchDevice, 0);

            // 创建点击动作
            tapAction.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, y, TimeSpan.Zero));
            tapAction.AddAction(touchDevice.CreatePointerDown(MouseButton.Left));
            tapAction.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(100)));
            tapAction.AddAction(touchDevice.CreatePointerUp(MouseButton.Left));

            // 执行点击动作
            _driver.PerformActions(new ActionSequence[] { tapAction });
        }


        bool CheckElement(string id, string type, int seconds = 1000)
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromMilliseconds(seconds));
            try
            {

                // 修改为你应用中的弹窗元素标识符
                By by = null;
                if (type == "xpath")
                {
                    by = By.XPath(id);
                }
                if (type == "id")
                {
                    by = By.Id(id);
                }
                if (type == "android")
                {
                    by = ByAndroidUIAutomator.AndroidUIAutomator(id);
                }
                //AppiumElement alert = (AppiumElement)wait.Until(ExpectedConditions.ElementIsVisible(by));
                AppiumElement alert = (AppiumElement)wait.Until(ExpectedConditions.ElementExists(by));
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
    }
}
