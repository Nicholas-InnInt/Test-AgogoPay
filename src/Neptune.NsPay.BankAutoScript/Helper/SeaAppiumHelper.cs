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
    public class SeaAppiumHelper
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
                driverOptions.AddAdditionalAppiumOption("appPackage", "vn.com.seabank.mb1");
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
                var ele = CheckElement("new UiSelector().resourceId(\"vn.com.seabank.mb1:id/login_ivSotp\")", "android", 100);
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
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"vn.com.seabank.mb1:id/login_ivSotp\")"));
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
                var ele = CheckElement("new UiSelector().resourceId(\"vn.com.seabank.mb1:id/otpInput\")", "android", 100);
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
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"vn.com.seabank.mb1:id/otpInput\")"));
                if (otpResult != null)
                {
                    otpResult.SendKeys(pwd);
                }
                //var temp = pwd.ToCharArray();
                //for (var i = 0; i <= 3; i++)
                //{
                //    //小键盘点击
                //    var code = Convert.ToInt32(temp[i].ToString());
                //    if (code >= 1 && code <= 3)
                //    {
                //        var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"vn.com.seabank.mb1:id/nextBottomView\")"));
                //        if (otpResult != null)
                //        {
                //            var bounds = otpResult.GetAttribute("bounds");
                //            var firstCoordinate = GetFirstCoordinate(bounds);
                //            var secondCoordinate = GetSecondCoordinate(bounds);

                //            int x = 0;
                //            int y = 0;
                //            if (code == 1)
                //            {
                //                x = firstCoordinate.Item1 + 170;
                //            }
                //            if (code == 2)
                //            {
                //                x = secondCoordinate.Item1 / 2;
                //            }
                //            if (code == 3)
                //            {
                //                x = secondCoordinate.Item1 - 170;
                //            }
                //            y = firstCoordinate.Item2 + 20;
                //            ClickPoints(x, y);
                //        }
                //    }
                //    if (code >= 4 && code <= 6)
                //    {
                //        var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"vn.com.seabank.mb1:id/nextBottomView\")"));
                //        if (otpResult != null)
                //        {
                //            var bounds = otpResult.GetAttribute("bounds");
                //            var firstCoordinate = GetFirstCoordinate(bounds);
                //            var secondCoordinate = GetSecondCoordinate(bounds);

                //            int x = 0;
                //            int y = 0;
                //            if (code == 4)
                //            {
                //                x = firstCoordinate.Item1 + 20;
                //            }
                //            if (code == 5)
                //            {
                //                x = secondCoordinate.Item1 / 2;
                //            }
                //            if (code == 6)
                //            {
                //                x = secondCoordinate.Item1 - 20;
                //            }
                //            y = ((firstCoordinate.Item2 + secondCoordinate.Item2) / 2) - 70;
                //            ClickPoints(x, y);
                //        }
                //    }
                //    if (code >= 7 && code <= 9)
                //    {
                //        var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"vn.com.seabank.mb1:id/nextBottomView\")"));
                //        if (otpResult != null)
                //        {
                //            var bounds = otpResult.GetAttribute("bounds");
                //            var firstCoordinate = GetFirstCoordinate(bounds);
                //            var secondCoordinate = GetSecondCoordinate(bounds);

                //            int x = 0;
                //            int y = 0;
                //            if (code == 7)
                //            {
                //                x = firstCoordinate.Item1 + 20;
                //            }
                //            if (code == 8)
                //            {
                //                x = secondCoordinate.Item1 / 2;
                //            }
                //            if (code == 9)
                //            {
                //                x = secondCoordinate.Item1 - 20;
                //            }
                //            y = secondCoordinate.Item2 - 130;
                //            ClickPoints(x, y);
                //        }
                //    }
                //    if (code == 0)
                //    {
                //        var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"vn.com.seabank.mb1:id/nextBottomView\")"));
                //        if (otpResult != null)
                //        {
                //            var bounds = otpResult.GetAttribute("bounds");
                //            var firstCoordinate = GetFirstCoordinate(bounds);
                //            var secondCoordinate = GetSecondCoordinate(bounds);

                //            int x = 0;
                //            int y = 0;

                //            x = secondCoordinate.Item1 / 2;
                //            y = secondCoordinate.Item2 - 50;
                //            ClickPoints(x, y);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
            }
        }

        public bool IsTransferOtp()
        {
            try
            {
                var ele = CheckElement("new UiSelector().resourceId(\"vn.com.seabank.mb1:id/sotp_challenge_ivScanQr\")", "android", 100);
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
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"vn.com.seabank.mb1:id/otpInput\").instance(0)"));
                if (otpResult != null)
                {
                    otpResult.SendKeys(pwd);
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
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"vn.com.seabank.mb1:id/home_bottomNavigation\")"));
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
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"vn.com.seabank.mb1:id/sotp_generate_etOtp\")"));
                if (otpResult != null)
                {
                    otp = otpResult.Text;
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
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"vn.com.seabank.mb1:id/buttonView\").instance(0)"));
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
