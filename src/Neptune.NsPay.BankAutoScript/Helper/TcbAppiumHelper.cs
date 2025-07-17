using Neptune.NsPay.Commons;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Neptune.NsPay.BankAutoScript
{
    public class TcbAppiumHelper
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
            //var process = Process.Start(startInfo);
            //return process;
        }

        public bool SetUp(int port,string deviceName)
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
                    DeviceName = deviceName                    
                };

                driverOptions.AddAdditionalAppiumOption("udid", deviceName);
                driverOptions.AddAdditionalAppiumOption("appPackage", "vn.com.techcombank.bb.app");
                driverOptions.AddAdditionalAppiumOption("waitForIdleTimeout", 100);
                driverOptions.AddAdditionalAppiumOption("noReset", true);

                _driver = new AndroidDriver(serverUri, driverOptions);
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

                return true;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("初始化异常：" + ex);
                return false;
            }
        }

        public bool ClickAlert()
        {
            try
            {
                int x = 345; // 替换为你的X坐标
                int y = 135; // 替换为你的Y坐标

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
            catch (Exception ex)
            {

            }
            return false;
        }

        //检查是否登录
        public bool IsLogin()
        {
            try
            {
                //检查是否登录
                //android.widget.TextView[@resource-id="vn.com.techcombank.bb.app:id/tvRequestTime"]
                //android.widget.TextView[@resource-id="vn.com.techcombank.bb.app:id/tvDes"]
                //var otpResult = CheckElement("vn.com.techcombank.bb.app:id/tvRequestTime", "id", 3);
                var otpResult = CheckElement("new UiSelector().resourceId(\"vn.com.techcombank.bb.app:id/tvRequestTime\")", "android", 2000);
                if (otpResult)
                {
                    return true;
                }
                return false;
            } catch (Exception ex)
            {
                return false;
            }
        }

        //检查密码
        public bool CheckPassword(int seconds = 1000)
        {
            try
            {
                var otpResult = CheckElement("new UiSelector().resourceId(\"vn.com.techcombank.bb.app:id/passCodeInput\")", "android", seconds);
                if (otpResult)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool InputPassword(string pwd)
        {
            try
            {
                var temp = pwd.ToCharArray();
                for (var i = 0; i <= 5; i++)
                {
                    var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().className(\"android.widget.CompoundButton\").instance(" + i + ")"));
                    _driver.PressKeyCode(GetKeyCode(temp[i].ToString()));
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool CheckBtnAccept()
        {
            try
            {
                var otpResult = CheckElement("new UiSelector().resourceId(\"vn.com.techcombank.bb.app:id/btnAccept\")", "android", 100);
                return otpResult;
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
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"vn.com.techcombank.bb.app:id/btnAccept\")"));
                if (otpResult != null)
                {
                    otpResult.Click();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void ClickErrorAlert()
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromMilliseconds(100));
                //var otpResult = wait.Until(ExpectedConditions.ElementExists(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"vn.com.techcombank.bb.app:id/btnPositive\")")));
                var otpResult = wait.Until(ExpectedConditions.ElementExists(By.Id("vn.com.techcombank.bb.app:id/btnPositive")));
                if (otpResult != null)
                {
                    otpResult.Click();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public bool CheckAlert()
        {
            try
            {
                var otpResult = CheckElement("new UiSelector().resourceId(\"vn.com.techcombank.bb.app:id/btnPositive\")", "android", 100);
                return otpResult;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool IsMain()
        {
            try
            {
                var ele = CheckElement("new UiSelector().resourceId(\"vn.com.techcombank.bb.app:id/lyTabArea\")", "android", 100);
                return ele;
            }
            catch (Exception ex)
            {
                return false;
            }
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

        public void KeepSessionAlive(int port,string deviceName)
        {
            if (_driver.SessionId != null)
            {
                SetUp(port,deviceName);
            }
        }
    }
}
