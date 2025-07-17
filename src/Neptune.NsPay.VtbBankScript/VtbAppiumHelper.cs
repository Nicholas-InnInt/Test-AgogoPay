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

namespace Neptune.NsPay.VtbBankScript
{
    public class VtbAppiumHelper
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
                driverOptions.AddAdditionalAppiumOption("appPackage", "com.vietinbank.ipay");
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

        public void ClickAlert()
        {
            try
            {
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().className(\"android.widget.LinearLayout\").instance(1)"));
                if (otpResult != null)
                {
                    //[70,957][650,1215]
                    var bounds = otpResult.GetAttribute("bounds");

                    var firstCoordinate = GetFirstCoordinate(bounds);

                    int x = firstCoordinate.Item1+20; // 替换为你的X坐标
                    int y = firstCoordinate.Item2+20; // 替换为你的Y坐标

                    ClickPoints(x,y);
                }
            }
            catch (Exception ex)
            {
            }
            //return false;
        }

        //检查是否登录
        public bool IsTransAlert()
        {
            try
            {
                //检查是否登录
                var otpResult = CheckElement("new UiSelector().className(\"android.widget.LinearLayout\").instance(0)", "android", 2000);
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
                var otpResult = CheckElement("new UiSelector().resourceId(\"com.vietinbank.ipay:id/wdSendOtp\")", "android", seconds);
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
                var index = 5;
                for (var i = 0; i <= 3; i++)
                {
                    //var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().className(\"android.widget.FrameLayout\").instance(" + index + ")"));
                    //_driver.PressKeyCode(GetKeyCode(temp[i].ToString()));
                    //index = index + 1;
                    //小键盘点击
                    var code = Convert.ToInt32(temp[i].ToString());
                    if (code >= 1 && code <= 3)
                    {
                        var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().className(\"android.widget.LinearLayout\").instance(4)"));
                        if (otpResult != null)
                        {
                            var bounds = otpResult.GetAttribute("bounds");
                            var firstCoordinate = GetFirstCoordinate(bounds);
                            var secondCoordinate = GetSecondCoordinate(bounds);

                            int x = 0;
                            int y = 0;
                            if (code == 1)
                            {
                                x = firstCoordinate.Item1 + 20;
                            }
                            if (code == 2)
                            {
                                x = secondCoordinate.Item1 / 2;
                            }
                            if (code == 3)
                            {
                                x = secondCoordinate.Item1 - 20;
                            }
                            y = (firstCoordinate.Item2 + secondCoordinate.Item2) / 2;
                            ClickPoints(x, y);
                        }
                    }
                    if (code >= 4 && code <= 6)
                    {
                        var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().className(\"android.widget.LinearLayout\").instance(5)"));
                        if (otpResult != null)
                        {
                            var bounds = otpResult.GetAttribute("bounds");
                            var firstCoordinate = GetFirstCoordinate(bounds);
                            var secondCoordinate = GetSecondCoordinate(bounds);

                            int x = 0;
                            int y = 0;
                            if (code == 4)
                            {
                                x = firstCoordinate.Item1 + 20;
                            }
                            if (code == 5)
                            {
                                x = secondCoordinate.Item1 / 2;
                            }
                            if (code == 6)
                            {
                                x = secondCoordinate.Item1 - 20;
                            }
                            y = (firstCoordinate.Item2 + secondCoordinate.Item2) / 2;
                            ClickPoints(x, y);
                        }
                    }
                    if (code >= 7 && code <= 9)
                    {
                        var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().className(\"android.widget.LinearLayout\").instance(6)"));
                        if (otpResult != null)
                        {
                            var bounds = otpResult.GetAttribute("bounds");
                            var firstCoordinate = GetFirstCoordinate(bounds);
                            var secondCoordinate = GetSecondCoordinate(bounds);

                            int x = 0;
                            int y = 0;
                            if (code == 7)
                            {
                                x = firstCoordinate.Item1 + 20;
                            }
                            if (code == 8)
                            {
                                x = secondCoordinate.Item1 / 2;
                            }
                            if (code == 9)
                            {
                                x = secondCoordinate.Item1 - 20;
                            }
                            y = (firstCoordinate.Item2 + secondCoordinate.Item2) / 2;
                            ClickPoints(x, y);
                        }
                    }
                    if (code == 0)
                    {
                        var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().className(\"android.widget.LinearLayout\").instance(7)"));
                        if (otpResult != null)
                        {
                            var bounds = otpResult.GetAttribute("bounds");
                            var firstCoordinate = GetFirstCoordinate(bounds);
                            var secondCoordinate = GetSecondCoordinate(bounds);

                            int x = 0;
                            int y = 0;

                            x = secondCoordinate.Item1 / 2;
                            y = (firstCoordinate.Item2 + secondCoordinate.Item2) / 2;
                            ClickPoints(x, y);
                        }
                    }
                }
                return true;
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
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"com.vietinbank.ipay:id/li1\")"));
                if (otpResult != null)
                {
                    //[70,957][650,1215]
                    var bounds = otpResult.GetAttribute("bounds");

                    var firstCoordinate = GetFirstCoordinate(bounds);
                    var secondCoordinate = GetSecondCoordinate(bounds);

                    int x = (firstCoordinate.Item1 + secondCoordinate.Item1) / 2; // 替换为你的X坐标
                    int y = secondCoordinate.Item2 - 50; // 替换为你的Y坐标

                    ClickPoints(x, y);
                }
            }
            catch (Exception ex)
            {
            }
        }


        public bool IsComfirm()
        {
            try
            {
                var otpResult = CheckElement("new UiSelector().resourceId(\"com.vietinbank.ipay:id/lvConfirm\")", "android", 100);
                return otpResult;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void ComfirmTransfer()
        {
            try
            {
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"com.vietinbank.ipay:id/otp_container\")"));
                if (otpResult != null)
                {
                    //[70,957][650,1215]
                    var bounds = otpResult.GetAttribute("bounds");

                    var firstCoordinate = GetFirstCoordinate(bounds);
                    var secondCoordinate = GetSecondCoordinate(bounds);

                    int x = (firstCoordinate.Item1 + secondCoordinate.Item1) / 2; // 替换为你的X坐标
                    int y = secondCoordinate.Item2 + 80; // 替换为你的Y坐标

                    ClickPoints(x, y);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void ClickNotice()
        {
            try
            {
                var otpResult = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().resourceId(\"android:id/content\")"));
                if (otpResult != null)
                {
                    var agree = _driver.FindElement(ByAndroidUIAutomator.AndroidUIAutomator("new UiSelector().className(\"android.widget.LinearLayout\").instance(1)"));
                    if (agree != null)
                    {
                        agree.Click();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            //return false;
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

        void ClickPoints(int x, int y)
        {
            //int x = firstCoordinate.Item1 + 20; // 替换为你的X坐标
            //int y = firstCoordinate.Item2 + 20; // 替换为你的Y坐标

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
