using DotNetBrowser.Browser;
using DotNetBrowser.Engine;
using DotNetBrowser.Handlers;
using DotNetBrowser.Input.Mouse.Events;
using DotNetBrowser.Input;
using DotNetBrowser.Navigation.Events;
using DotNetBrowser.Passwords.Handlers;
using DotNetBrowser.WinForms;
using Neptune.NsPay.CefTransfer.Common.Classes;
using Neptune.NsPay.CefTransfer.Common.Models;
using Neptune.NsPay.CefTransfer.Common.MyEnums;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.Transfer.Win.Models;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.WithdrawalOrders;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing.Imaging;
using DotNetBrowser.Net.Proxy;

namespace Neptune.NsPay.Transfer.Win.Classes
{
    public class MyBrowser
    {
        #region Variable
        #endregion

        #region Property
        public string BaseDirectory { get; set; }
        public string CachePath { get; set; }
        public string HtmlPath { get; set; }
        public string ImgPath { get; set; }
        public MyHttpClient HttpClient { get; set; }
        public string BaseUrl { get; set; }
        public string Sign { get; set; }
        public string MerchantCode { get; set; }
        public int @Type { get; set; }
        public IBrowser Browser { get; set; }
        public BrowserView BrowserView { get; set; }
        public IEngine Engine { get; set; }
        public DateTime BankLastLoadingAt { get; set; } = DateTime.MinValue;
        public string Phone { get; set; }
        public WithdrawalDevicesBankTypeEnum BankType { get; set; }
        public Bank Bank { get; set; }
        public Lang SelectedLang { get; set; }
        public bool IsDebug { get; private set; } = false;
        public bool FirstTime { get; set; } = true;
        public int RefreshInterval { get; set; }
        public int RefreshTime { get; set; }
        public System.Timers.Timer RefreshTimer { get; set; }
        public decimal BalanceAfterTransfer { get; set; }
        public bool IsRunning { get; set; } = false;
        public TransferInfoModel TransferInfo { get; set; }
        public TcbReportModel TcbReport { get; set; }
        public AcbReportModel AcbReport { get; set; }
        public MsbReportModel MsbReport { get; set; }
        public SeaReportModel SeaReport { get; set; }
        public VtbReportModel VtbReport { get; set; }
        public bool CheckHistory { get; set; }
        public string ReportStatistic { get; set; }
        public int NrLoaded { get; set; }
        public int NrLoading { get; set; }
        public DateTime LastLoadingAt { get; set; } = DateTime.MinValue;
        public int Dpi { get; set; }
        public AppLogUpdateModel LogsCache { get; set; } = new AppLogUpdateModel();
        #endregion

        public MyBrowser()
        {
#if DEBUG
            IsDebug = true;
#endif
            BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            CachePath = Path.Combine(BaseDirectory, "cache");
            HtmlPath = Path.Combine(BaseDirectory, "html");
            ImgPath = Path.Combine(BaseDirectory, "img");
        }

        #region Event
        public event EventHandler<string> DoWriteUiMessage;
        public event EventHandler<decimal> DoSetBalance;
        public event EventHandler<bool> DoIsLoading;
        public event EventHandler<bool> DoIsLoaded;
        public event EventHandler<string> DoSetUrl;
        public event EventHandler<bool> DoLockBrowser;
        public event EventHandler<int> DoSetCountDown;
        public event EventHandler<bool> DoResetCountDown;
        public event EventHandler<string> DoSetPointer;

        //private void OnCefBrowser_AddressChanged(object? sender, AddressChangedEventArgs e)
        //{
        //    DoSetUrl?.Invoke(this, e.Address);
        //}

        private void OnBrowser_LoadFinished(object? sender, LoadFinishedEventArgs e)
        {
            var url = Browser.Url;
            DoSetUrl?.Invoke(this, url);
        }

        private void OnRefreshTimer_Elapsed(object? source, System.Timers.ElapsedEventArgs e)
        {
            RefreshInterval++;
        }

        //public async void OnResourceLoadComplete(IRequest request)
        //{

        //}
        #endregion

        #region Public
        public async Task Init()
        {
            try
            {
                if (!Directory.Exists(ImgPath))
                    Directory.CreateDirectory(ImgPath);

                if (!Directory.Exists(HtmlPath))
                    Directory.CreateDirectory(HtmlPath);

                if (!Directory.Exists(CachePath))
                    Directory.CreateDirectory(CachePath);

                Phone = AppSettings.Configuration["Phone"];
                BankType = (WithdrawalDevicesBankTypeEnum)Enum.Parse(typeof(WithdrawalDevicesBankTypeEnum), AppSettings.Configuration["BankType"]);
                BaseUrl = AppSettings.Configuration["NsPayApi:BaseApiUrl"];
                //Sign = AppSettings.Configuration["NsPayApi:Sign"];
                MerchantCode = AppSettings.Configuration["NsPayApi:MerchantCode"];
                Type = Convert.ToInt32(AppSettings.Configuration["NsPayApi:Type"]);

                Bank = BankType.GetConfigBank();
                if (Bank == Bank.Unknown)
                    throw new Exception($"Unknown bank {AppSettings.Configuration["BankType"]}");

                HttpClient = new MyHttpClient(BaseUrl, MerchantCode, Type);

                await InitBrowser();
                //await InitCef();
            }
            catch (Exception ex)
            {
                LogError($"Error Init, {ex}", $"Error Init, {ex.Message}");
                throw new Exception($"Error Init, {ex.Message}");
            }
        }

        public async Task StartAsync()
        {
            try
            {
                LogInfo($"Start ...");

                InitWorker();

                while (true)
                {
                    IsRunning = true;
                    ResetLog();
                    LogInfo($"Worker running at: {DateTime.Now}");

                    await StartTransferAsync();
                    //await AcbTestAsync();
                    //await TcbTestAsync();
                    //await VtbTestAsync();
                    //await SeaBankTestAsync();
                    //await MsbTestAsync();

                    await UploadLog();
                    IsRunning = false;
                    await Task.Delay(1000 * 10);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error StartAsync, {ex}", $"Error StartAsync, {ex.Message}");
            }
        }

        public void LogInfo(string msg)
        {
            if (IsDebug) Debug.WriteLine(msg);
            NlogLogger.Info(msg);
            LogsCache.Messages.Add(msg);
            DoWriteUiMessage?.Invoke(this, msg);
        }

        public void LogError(string msg, string infoMsg = "")
        {
            if (IsDebug) Debug.WriteLine(msg);
            if (!string.IsNullOrWhiteSpace(infoMsg))
            {
                NlogLogger.Info(infoMsg);
                DoWriteUiMessage?.Invoke(this, infoMsg);
            }
            else
            {
                NlogLogger.Info(msg);
                LogsCache.Messages.Add(msg);
                DoWriteUiMessage?.Invoke(this, msg);
            }
            NlogLogger.Error(msg);
        }

        public void Dispose()
        {
            Browser?.Dispose();
            Engine?.Dispose();
        }

        #endregion

        #region Private

        private async Task InitBrowser()
        {
            BrowserView = new BrowserView { Dock = DockStyle.Fill };
            Engine = EngineFactory.Create(new EngineOptions.Builder
            {
                RenderingMode = RenderingMode.HardwareAccelerated,
                UserDataDirectory = CachePath,
            }.Build());
            var proxyEnable = Convert.ToBoolean(AppSettings.Configuration["Proxy:Enable"]);
            var proxyAddress = string.Empty;
            if (proxyEnable)
            {
                proxyAddress = Convert.ToString(AppSettings.Configuration["Proxy:Address"]);
                Engine.Profiles.Default.Proxy.Settings = new CustomProxySettings(proxyAddress);
            }
            else
            {
                Engine.Profiles.Default.Proxy.Settings = new SystemProxySettings();
            }
            Browser = Engine.CreateBrowser();
            Browser.Navigation.LoadFinished += OnBrowser_LoadFinished;
            Browser.Mouse.Moved.Handler = new Handler<IMouseMovedEventArgs, InputEventResponse>(e =>
            {
                if (IsDebug) DoSetPointer?.Invoke(this, $"{e.Location} | {e.LocationOnScreen}");
                return InputEventResponse.Proceed;
            });
            Browser.Passwords.SavePasswordHandler = new Handler<SavePasswordParameters, SavePasswordResponse>(p =>
            {
                return SavePasswordResponse.NeverSave;
            });
            Browser.Navigation.LoadFinished += (o, e) =>
            {
                NrLoaded++;
            };
            Browser.Navigation.LoadStarted += (o, e) =>
            {
                NrLoading++;
                LastLoadingAt = DateTime.Now;
            };

            BrowserView.InitializeFrom(Browser);

            Browser.Navigation.LoadUrl(Bank.GetBank()?.url).Wait();
            await WaitAsync(2, 1);
        }

        private void InitWorker()
        {
            LogInfo($"InitWorker ...");

            RefreshTimer = new System.Timers.Timer();
            RefreshTimer.Interval = 1000;
            RefreshTimer.Elapsed += OnRefreshTimer_Elapsed;
            RefreshTimer.Enabled = true;

            switch (Bank)
            {
                case Bank.AcbBank:
                    RefreshTime = 20;
                    break;
                case Bank.SeaBank:
                case Bank.TcbBank:
                    RefreshTime = 170;
                    break;
                case Bank.MsbBank:
                case Bank.VtbBank:
                    RefreshTime = 45;
                    break;
                default: break;
            }
        }

        private async Task StartTransferAsync()
        {
            try
            {
                var blnUpdateOrder = false;
                var blnTransfer = false;

                var deviceInfo = await GetDevice(Phone, BankType);
                //if (IsDebug) // ACB
                //{
                //    deviceInfo = new DeviceModel()
                //    {
                //        DeviceId = 1,
                //        Phone = "0966493205",
                //        Name = "NGUYEN VAN HUY",
                //        Otp = "",
                //        Status = false,
                //        Process = 1,
                //        LoginPassWord = "Win168168@",
                //        CardNumber = "37393987",
                //    };
                //}
                //if (IsDebug) // TCB
                //{
                //    deviceInfo = new DeviceModel()
                //    {
                //        DeviceId = 1,
                //        Phone = "0703805200",
                //        Name = "NGUYEN VAN HUY",
                //        Otp = "",
                //        Status = false,
                //        Process = 1,
                //        LoginPassWord = "Win168168@",
                //        CardNumber = "37393987",
                //    };
                //}
                if (deviceInfo != null)
                {
                    var isLoggedIn = await IsLoggedInAsync(Bank);
                    if (isLoggedIn)
                    {
                        if (deviceInfo.Process == (int)WithdrawalDevicesProcessTypeEnum.Process)
                        {
                            LogInfo($"查询订单: {DateTime.Now}");

                            var transInfo = new TransferInfoModel();

                            var checkOrder = await CheckWithdrawOrder(Phone, BankType);
                            if (checkOrder == null)
                            {
                                var withdrawalOrder = await GetWithdrawOrder(Phone, BankType);
                                if (withdrawalOrder != null)
                                {
                                    transInfo.OrderNo = withdrawalOrder.OrderNo;
                                    transInfo.OrderId = withdrawalOrder.Id;

                                    LogInfo($"获取消息:{DateTime.Now}--OrderId:{transInfo.OrderNo}");

                                    try
                                    {
                                        ////更新订单未转账中
                                        blnUpdateOrder = await UpdateWithdrawOrder(transInfo.OrderId, "", WithdrawalOrderStatusEnum.Pending, "");
                                        LogInfo($"进入转账:{DateTime.Now}--OrderId:{transInfo.OrderNo}");

                                        //创建出款订单
                                        var orderNo = transInfo.OrderNo;
                                        if (orderNo.Contains("_"))
                                        {
                                            orderNo = orderNo.Split("_")[0];
                                        }
                                        else
                                        {
                                            orderNo = orderNo.Substring(2);
                                        }
                                        LogsCache.PaymentId = orderNo;
                                        transInfo.OrderNo = orderNo;

                                        transInfo.UserName = deviceInfo.Phone;
                                        transInfo.Password = deviceInfo.LoginPassWord;
                                        transInfo.CardNo = deviceInfo.CardNumber;
                                        transInfo.ToBank = withdrawalOrder.BenBankName;
                                        transInfo.ToAccountNumber = withdrawalOrder.BenAccountNo;
                                        transInfo.ToAccountName = withdrawalOrder.BenAccountName;
                                        transInfo.ToAmount = withdrawalOrder.OrderMoney.ToString("F0").StrToInt();
                                        transInfo.ToRemarks = $"{withdrawalOrder.BenAccountName} {orderNo}".VietnameseToEnglish();
                                        transInfo.DeviceId = deviceInfo.DeviceId;

                                        blnTransfer = true;
                                        RefreshInterval = 0;
                                        LogInfo($"Start transfer, {JsonConvert.SerializeObject(transInfo)}");
                                        //LogInfo($"Start transfer, {Bank}, {OrderNo}, {UserName}, {CardNo}, {ToBank}, {ToAccountNumber}, {ToAccountName}, {ToAmount}, {ToRemarks}");
                                        var transferWatch = Stopwatch.StartNew();
                                        var transfer = await TransferAsync(transInfo);
                                        FirstTime = false;
                                        transferWatch.Stop();
                                        LogInfo($"End transfer. Total time: {transferWatch.ElapsedMilliseconds.MilisecondsToTimeTaken()}");
                                        LogInfo($"Report statistic|{transferWatch.Elapsed.TotalSeconds.To3Decimal()}|{ReportStatistic}");

                                        if (TransferInfo.Status != TransferStatus.Success)
                                        {
                                            WithdrawalOrderStatusEnum orderStatus = WithdrawalOrderStatusEnum.Fail;
                                            if (TransferInfo.Status == TransferStatus.Failed)
                                            {
                                                orderStatus = WithdrawalOrderStatusEnum.Fail;
                                            }
                                            if (TransferInfo.Status == TransferStatus.ErrorBank)
                                            {
                                                orderStatus = WithdrawalOrderStatusEnum.ErrorBank;
                                            }
                                            if (TransferInfo.Status == TransferStatus.ErrorCard)
                                            {
                                                orderStatus = WithdrawalOrderStatusEnum.ErrorCard;
                                            }
                                            blnUpdateOrder = await UpdateWithdrawOrder(transInfo.OrderId, "", orderStatus, TransferInfo.ErrorMessage);
                                            LogInfo($"Withdrawal failed.");
                                            return;
                                        }

                                        blnUpdateOrder = await UpdateWithdrawOrder(transInfo.OrderId, TransferInfo.TransactionId, WithdrawalOrderStatusEnum.Success, "");
                                        LogInfo($"Withdrawal success.");

                                        if (!IsDebug)
                                        {
                                            LogInfo($"Set balance, {Phone}, {BalanceAfterTransfer}");
                                            await UpdateBalance(transInfo.DeviceId, BalanceAfterTransfer);
                                        }

                                        LogInfo($"转账完成:{DateTime.Now}--OrderId:{orderNo}");
                                    }
                                    catch (Exception ex)
                                    {
                                        blnUpdateOrder = await UpdateWithdrawOrder(transInfo.OrderId, "", WithdrawalOrderStatusEnum.Fail, TransferInfo.ErrorMessage);
                                        LogError($"Withdrawal error, {ex}", $"Withdrawal error, {ex.Message}");
                                    }
                                }
                            }
                        }
                    }

                    // Refresh if not performing transfer and first launch
                    // Refresh if not performing transfer each 3 minutes (180 seconds)
                    if (!blnTransfer)
                    {
                        if (!isLoggedIn || FirstTime || RefreshInterval >= RefreshTime)
                        {

                            var transInfo = new TransferInfoModel()
                            {
                                UserName = deviceInfo.Phone,
                                Password = deviceInfo.LoginPassWord,
                                CardNo = deviceInfo.CardNumber,
                            };

                            LogInfo($"Start refresh... {RefreshInterval}");
                            var refresh = await RefreshAsync(transInfo);
                            LogInfo($"End refresh... {RefreshInterval}");
                            LogInfo($"Report statistic||{ReportStatistic}");
                            if (FirstTime && !refresh)
                            { RefreshInterval = RefreshTime; }// Quickly trigger another refresh if first time login failed
                            else
                            { RefreshInterval = 0; }
                            FirstTime = false;
                        }
                        else
                        { LogInfo($"Next refresh at {RefreshInterval}/{RefreshTime}"); }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"账户:{Phone}, 错误 {ex}", $"账户:{Phone}, 错误 {ex.Message}");
            }
        }

        private async Task<bool> IsLoginExistsAsync(Bank bank)
        {
            var result = false;
            var userId = string.Empty;
            var passId = string.Empty;
            var userExists = false;
            var passExists = false;

            switch (bank)
            {
                case Bank.AcbBank:
                    userId = "#acb-one-login-container form .aol-form input[id=\"user-name\"]";
                    passId = "#acb-one-login-container form .aol-form input[id=\"password\"]";

                    userExists = await JsElementExists(userId, 2);
                    passExists = await JsElementExists(passId, 2);

                    result = userExists && passExists;
                    break;

                case Bank.MsbBank:
                    userId = "#_userName";
                    passId = "#msbPassword";

                    userExists = await JsElementExists(userId, 2);
                    passExists = await JsElementExists(passId, 2);

                    result = userExists && passExists;
                    break;

                case Bank.VtbBank:
                    userId = "#app .app-container .main-container .auth-layout .login input[name=\"userName\"]";
                    passId = "#app .app-container .main-container .auth-layout .login input[name=\"accessCode\"]";

                    userExists = await JsElementExists(userId, 2);
                    passExists = await JsElementExists(passId, 2);

                    result = userExists && passExists;
                    break;

                default: break;
            }
            return result;
        }

        private async Task<bool> SetLoginAsync(Bank bank, string usernameValue, string passwordValue)
        {
            var result = false;
            var userId = string.Empty;
            var passId = string.Empty;
            var userVal = string.Empty;
            var passVal = string.Empty;
            var userExists = false;
            var passExists = false;
            var inputUser = false;
            var inputPass = false;
            var userResult = false;
            var passResult = false;

            switch (bank)
            {
                case Bank.AcbBank:
                    userId = "#acb-one-login-container form .aol-form input[id=\"user-name\"]";
                    passId = "#acb-one-login-container form .aol-form input[id=\"password\"]";
                    break;

                case Bank.MsbBank:
                    userId = "#_userName";
                    passId = "#msbPassword";
                    break;

                case Bank.SeaBank:
                    userId = "input[name=\"username\"]";
                    passId = "input[name=\"new-password\"]";
                    break;

                case Bank.TcbBank:
                    userId = "#username";
                    passId = "#password";
                    break;

                case Bank.VtbBank:
                    userId = "#app .auth-layout .login input[name=\"userName\"]";
                    passId = "#app .auth-layout .login input[name=\"accessCode\"]";
                    break;

                default: break;
            }

            switch (bank)
            {
                case Bank.AcbBank:
                case Bank.MsbBank:
                case Bank.SeaBank:
                    userExists = await JsElementExists(userId, 2);
                    if (userExists)
                    {
                        inputUser = await Browser.JsInputElementValue(userId, usernameValue, 2);
                        if (inputUser)
                        {
                            LogInfo($"Input Username <{usernameValue}>, {userId}");
                            userVal = await Browser.JsGetInputValue(userId, 2);
                            userResult = userVal == usernameValue;
                        }
                        else
                        {
                            LogError($"Error input Username <{usernameValue}>, {userId}");
                        }
                    }

                    passExists = await JsElementExists(passId, 2);
                    if (passExists)
                    {
                        inputPass = await Browser.JsInputElementValue(passId, passwordValue, 2);
                        if (inputPass)
                        {
                            LogInfo($"Input Password <{passwordValue}>, {passId}");
                            passVal = await Browser.JsGetInputValue(passId, 2);
                            passResult = passVal == passwordValue;
                        }
                        else
                        {
                            LogError($"Error input Password <{passwordValue}>, {passId}");
                        }
                    }

                    result = userResult && passResult;
                    break;

                case Bank.TcbBank:
                case Bank.VtbBank:
                    userExists = await JsElementExists(userId, 2);
                    if (userExists)
                    {
                        userVal = await Browser.JsGetInputValue(userId, 2);
                        if (string.IsNullOrWhiteSpace(userVal))
                        {
                            inputUser = await Browser.InputToElementJsScroll(Bank, userId, usernameValue, 1, 2, Dpi);
                            if (inputUser)
                            {
                                LogInfo($"Input Username <{usernameValue}>, {userId}");
                                userVal = await Browser.JsGetInputValue(userId, 2);
                                userResult = userVal == usernameValue;
                            }
                            else
                            {
                                LogError($"Error input Username <{usernameValue}>, {userId}");
                            }
                        }
                        else userResult = true;
                    }

                    passExists = await JsElementExists(passId, 2);
                    if (passExists)
                    {
                        passVal = await Browser.JsGetInputValue(passId, 2);
                        if (string.IsNullOrWhiteSpace(passVal))
                        {
                            inputPass = await Browser.InputToElementJsScroll(Bank, passId, passwordValue, 1, 2, Dpi);
                            if (inputPass)
                            {
                                LogInfo($"Input Password <{passwordValue}>, {passId}");
                                passVal = await Browser.JsGetInputValue(passId, 2);
                                passResult = passVal == passwordValue;
                            }
                            else
                            {
                                LogError($"Error input Password <{passwordValue}>, {passId}");
                            }
                        }
                        else passResult = true;
                    }

                    result = userResult && passResult;
                    break;
            }
            return result;
        }

        private async Task<bool> IsLoggedInAsync(Bank bank, int numOfTry = 2)
        {
            JsResultModel result = new JsResultModel();
            //var result = false;
            var checkId = string.Empty;

            switch (bank)
            {
                case Bank.AcbBank:
                    checkId = "#menu li ul li a[href*=\"dse_operationName=ibkacctSumProc\"]";
                    result = await Browser.JsElementExists(checkId, numOfTry);
                    //if (!result.IsSuccess)
                    //{
                    //    LogError($"Error executing Js, special reload ...");
                    //    await AcbReloadPageAsync();
                    //}
                    break;

                case Bank.MsbBank:
                    // Has Logout link
                    checkId = ".msb-container-header .msb-navigation-user .user-menu-view a[onclick*=\"lout()\"]";
                    result = await Browser.JsElementExists(checkId, numOfTry);
                    break;

                case Bank.SeaBank:
                    // Has Change Password link
                    checkId = "kt-user-profile a[href*=\"/change-password\"]";
                    result = await Browser.JsElementExists(checkId, numOfTry);
                    break;

                case Bank.TcbBank:
                    checkId = "a[href=\"/dashboard\"]";
                    result = await Browser.JsElementExists(checkId, numOfTry);
                    break;

                case Bank.VtbBank:
                    // Button logout is not hidden
                    checkId = "div[class=\"main-header  \"] .main-header__actions .btn-logout";
                    result = await Browser.JsElementExists(checkId, numOfTry);
                    break;

                default: break;
            }

            return result.BoolResult;
        }

        private async Task ShowBalanceAsync(Bank bank)
        {
            var balId = string.Empty;
            var balance = string.Empty;

            switch (bank)
            {
                case Bank.AcbBank:
                    balance = await Browser.JsGetAcbBalance(TransferInfo.CardNo, 2);
                    break;

                case Bank.SeaBank:
                    balance = await SeaGetBalanceAsync();
                    break;

                case Bank.TcbBank:
                    balId = ".account-balance .account-balance__amount";
                    balance = await Browser.JsGetElementInnerText(balId, 2);
                    break;

                case Bank.VtbBank:
                    balance = await Browser.JsVtbGetBalanceAtTransferPage(2);
                    break;

                default: return;
            }

            LogInfo($"Balance is <{balance}>");
            if (balance != null)
            {
                DoSetBalance?.Invoke(this, balance.ToNumber(bank).StrToDec());
            }
        }

        private async Task ShowBalance2Async(string? balance)
        {
            // to replace ShowBalanceAsync

            LogInfo($"Balance is <{balance}>");
            if (balance != null)
            {
                DoSetBalance?.Invoke(this, balance.ToNumber(Bank).StrToDec());
            }
        }

        private async Task<bool> LoadBankAsync(Bank bank)
        {
            var url = string.Empty;
            switch (bank)
            {
                case Bank.AcbBank:
                case Bank.SeaBank:
                case Bank.VtbBank:
                    url = bank.GetBank().url;
                    Browser.Navigation.LoadUrl(url).Wait();
                    if (!await WaitAsync(2, 1)) return false;
                    break;

                case Bank.TcbBank:
                    url = bank.GetBank().url;
                    Browser.Navigation.LoadUrl(url).Wait();
                    if (!await WaitAsync(2, 1, "app-loader .loading")) return false;
                    break;

                default: break;
            }

            return true;
        }

        //private async Task<bool> ReloadPageAsync(Bank bank)
        //{
        //    LogInfo($"Reload page ...");

        //    var result = false;
        //    switch (bank)
        //    {
        //        case Bank.AcbBank:
        //            result = await AcbReloadPageAsync();
        //            break;

        //        default: break;
        //    }

        //    return result;


        //    //var reload = await CefBrowser.JsReloadAsync(1);
        //    //await Task.Delay(1000 * 2);
        //    //if (!await WaitAsync(2)) return false;

        //    //CefBrowser.Reload(true);
        //    //await Task.Delay(1000 * 2);
        //    //if (!await WaitAsync(2)) return false;

        //    //await LoadBankAsync(Bank.AcbBank);
        //    //await Task.Delay(1000 * 2);
        //    //if (!await WaitAsync(2)) return false;
        //}

        private async Task<bool> GoHomePage(Bank bank)
        {
            var result = false;
            var id = string.Empty;
            var exists = false;

            switch (bank)
            {
                case Bank.MsbBank:
                    // Click Logo link 
                    id = "#logo a";
                    exists = await JsElementExists(id, 2);
                    if (exists)
                    {
                        result = await Browser.JsClickElement(id, 2);
                        if (result)
                        {
                            await WaitAsync(2, 1);
                        }
                    }
                    break;

                default: break;
            }

            return result;
        }

        private async Task<bool> IsPage(Bank bank, CommonPage page)
        {
            string? id = null;
            var result = false;

            switch (bank)
            {
                case Bank.MsbBank:
                    switch (page)
                    {
                        case CommonPage.InternalTransfer:
                            // Selected Link is Internal Transfer
                            id = ".msb-submenu .menu-item.active a[onclick*=\"retailInternalTransferProc\"]";
                            result = await JsElementExists(id, 2);
                            break;
                        case CommonPage.ExternalTransfer:
                            // Selected Link is Interbank Transfer
                            id = ".msb-submenu .menu-item.active a[onclick*=\"retailInterbankTransferProc\"]";
                            result = await JsElementExists(id, 2);
                            break;
                    }
                    break;

                default: break;
            }

            return result;
        }

        private async Task<string?> GetBalanceAsync(Bank bank, CommonPage page)
        {
            string? id = null;
            string? balance = null;
            string? result = null;

            switch (bank)
            {
                case Bank.MsbBank:
                    switch (page)
                    {
                        case CommonPage.Account:
                            // Balance at Swiper
                            id = ".msb-acclist-detail .msb-account-slider .swiper-slide-active .msb-balance .msb-money";
                            balance = await Browser.JsGetElementInnerText(id, 2);
                            if (!string.IsNullOrWhiteSpace(balance)) result = balance;
                            break;
                        case CommonPage.InternalTransfer:
                        case CommonPage.ExternalTransfer:
                            // Default Source Account Balance
                            id = "a.sbSelector span";
                            balance = await Browser.JsGetElementInnerText(id, 2);
                            if (!string.IsNullOrWhiteSpace(balance)) result = balance;
                            break;
                    }
                    break;

                default: break;
            }

            return result;
        }

        private async Task<bool> RefreshAsync(TransferInfoModel transferInfo)
        {
            TransferInfo = transferInfo;
            DoLockBrowser?.Invoke(this, true);
            ReportStatistic = string.Empty;

            var result = false;

            switch (Bank)
            {
                case Bank.AcbBank:
                    AcbReport = new AcbReportModel();

                    result = await AcbRefreshAsync(TransferInfo.UserName, TransferInfo.Password);

                    AcbReport.Result = result;
                    ReportStatistic = AcbReport.Statistic;
                    break;

                case Bank.MsbBank:
                    MsbReport = new MsbReportModel();

                    result = await MsbRefreshAsync(TransferInfo.UserName, TransferInfo.Password, 1);

                    MsbReport.Result = result;
                    ReportStatistic = MsbReport.Statistic;
                    break;

                case Bank.SeaBank:
                    SeaReport = new SeaReportModel();

                    result = await SeaRefreshAsync(TransferInfo.UserName, TransferInfo.Password, 1);

                    SeaReport.Result = result;
                    ReportStatistic = SeaReport.Statistic;
                    break;

                case Bank.TcbBank:
                    TcbReport = new TcbReportModel();

                    result = await TcbRefreshAsync(TransferInfo.UserName, TransferInfo.Password);

                    TcbReport.Result = result;
                    ReportStatistic = TcbReport.Statistic;
                    break;

                case Bank.VtbBank:
                    VtbReport = new VtbReportModel();

                    result = await VtbRefreshAsync(TransferInfo.UserName, TransferInfo.Password, 1);

                    VtbReport.Result = result;
                    ReportStatistic = VtbReport.Statistic;
                    break;

                default:
                    return false;
            }

            if (!result)
            {
                LogsCache.Html = Browser.MainFrame.Html;

                var htmlFileName = $"Refresh_Html" + "_{0}";
                await Browser.WriteHtmlToFile(HtmlPath, htmlFileName);

                await Screenshot("Error_{0}_Refresh", true);
            }

            return result;
        }

        private async Task<bool> TransferAsync(TransferInfoModel transferInfo)
        {
            TransferInfo = transferInfo;
            DoLockBrowser?.Invoke(this, true);
            ReportStatistic = string.Empty;

            var result = false;
            try
            {
                TransferType transType;
                string? sFileName;

                switch (Bank)
                {
                    case Bank.AcbBank:
                        var acbToBank = TransferInfo.ToBank.ToAcbExtBank();

                        if (acbToBank == AcbExtBank.Unknown)
                        {
                            await UpdateWithdrawOrder(TransferInfo.OrderId, "", WithdrawalOrderStatusEnum.ErrorBank, "");
                            TransferInfo.ErrorMessage = $"银行无效: <{TransferInfo.ToBank}>, 无法匹配";
                            LogError($"Unknown bank <{TransferInfo.ToBank}>");
                            return false;
                        }
                        AcbReport = new AcbReportModel();
                        transType = acbToBank == AcbExtBank.ACB ? TransferType.Internal : TransferType.External;
                        LogInfo((transType == TransferType.Internal ? "ACB" : "其他银行") + $"转账:{DateTime.Now}--OrderId:{TransferInfo.OrderNo}");
                        result = await AcbTransferAsync(transType, TransferInfo.UserName, TransferInfo.Password, TransferInfo.CardNo, acbToBank,
                            TransferInfo.ToAccountNumber, TransferInfo.ToAccountName, TransferInfo.ToAmount, TransferInfo.ToRemarks,
                            TransferInfo.DeviceId, TransferInfo.OrderId);

                        AcbReport.Result = result;
                        AcbReport.TransferType = transType == TransferType.Internal ? "I" : "E";
                        ReportStatistic = AcbReport.Statistic;
                        break;

                    case Bank.MsbBank:
                        var msbToBank = TransferInfo.ToBank.ToMsbExtBank();

                        if (msbToBank == MsbExtBank.Unknown)
                        {
                            await UpdateWithdrawOrder(TransferInfo.OrderId, "", WithdrawalOrderStatusEnum.ErrorBank, "");
                            TransferInfo.ErrorMessage = $"银行无效: <{TransferInfo.ToBank}>, 无法匹配";
                            LogError($"Unknown bank <{TransferInfo.ToBank}>");
                            return false;
                        }
                        MsbReport = new MsbReportModel();
                        transType = msbToBank == MsbExtBank.MSB ? TransferType.Internal : TransferType.External;
                        LogInfo((transType == TransferType.Internal ? "MSB" : "其他银行") + $"转账:{DateTime.Now}--OrderId:{TransferInfo.OrderNo}");
                        result = await MsbTransferAsync(transType, TransferInfo.UserName, TransferInfo.Password, msbToBank,
                            TransferInfo.ToAccountNumber, TransferInfo.ToAccountName, TransferInfo.ToAmount, TransferInfo.ToRemarks,
                            TransferInfo.DeviceId, TransferInfo.OrderId);

                        MsbReport.Result = result;
                        MsbReport.TransferType = transType == TransferType.Internal ? "I" : "E";
                        ReportStatistic = MsbReport.Statistic;
                        break;

                    case Bank.SeaBank:
                        var seaToBank = TransferInfo.ToBank.ToSeaExtBank();

                        if (seaToBank == SeaExtBank.Unknown)
                        {
                            await UpdateWithdrawOrder(TransferInfo.OrderId, "", WithdrawalOrderStatusEnum.ErrorBank, "");
                            TransferInfo.ErrorMessage = $"银行无效: <{TransferInfo.ToBank}>, 无法匹配";
                            LogError($"Unknown bank <{TransferInfo.ToBank}>");
                            return false;
                        }
                        SeaReport = new SeaReportModel();
                        transType = seaToBank == SeaExtBank.SEABANK_317 ? TransferType.Internal : TransferType.External;
                        LogInfo((transType == TransferType.Internal ? "SEAB" : "其他银行") + $"转账:{DateTime.Now}--OrderId:{TransferInfo.OrderNo}");
                        result = await SeaTransferAsync(transType, TransferInfo.UserName, TransferInfo.Password, seaToBank,
                            TransferInfo.ToAccountNumber, TransferInfo.ToAccountName, TransferInfo.ToAmount, TransferInfo.ToRemarks,
                            TransferInfo.DeviceId, TransferInfo.OrderId);

                        SeaReport.Result = result;
                        SeaReport.TransferType = transType == TransferType.Internal ? "I" : "E";
                        ReportStatistic = SeaReport.Statistic;
                        break;

                    case Bank.TcbBank:
                        var tcbToBank = TransferInfo.ToBank.ToTcbExtBank();

                        if (tcbToBank == TcbExtBank.Unknown)
                        {
                            await UpdateWithdrawOrder(TransferInfo.OrderId, "", WithdrawalOrderStatusEnum.ErrorBank, "");
                            TransferInfo.ErrorMessage = $"银行无效: <{TransferInfo.ToBank}>, 无法匹配";
                            LogError($"Unknown bank <{TransferInfo.ToBank}>");
                            return false;
                        }

                        TcbReport = new TcbReportModel();
                        transType = tcbToBank == TcbExtBank.Techcombank_TCB ? TransferType.Internal : TransferType.External;
                        LogInfo((transType == TransferType.Internal ? "TCB" : "其他银行") + $"转账:{DateTime.Now}--OrderId:{TransferInfo.OrderNo}");
                        result = await TcbTransferAsync(transType, TransferInfo.UserName, TransferInfo.Password, tcbToBank,
                            TransferInfo.ToAccountNumber, TransferInfo.ToAccountName, TransferInfo.ToAmount, TransferInfo.ToRemarks,
                            TransferInfo.DeviceId, TransferInfo.OrderId);

                        TcbReport.Result = result;
                        TcbReport.TransferType = transType == TransferType.Internal ? "I" : "E";
                        ReportStatistic = TcbReport.Statistic;

                        break;

                    case Bank.VtbBank:
                        var vtbToBank = TransferInfo.ToBank.ToVtbExtBank();

                        if (vtbToBank == VtbExtBank.Unknown)
                        {
                            await UpdateWithdrawOrder(TransferInfo.OrderId, "", WithdrawalOrderStatusEnum.ErrorBank, "");
                            TransferInfo.ErrorMessage = $"银行无效: <{TransferInfo.ToBank}>, 无法匹配";
                            LogError($"Unknown bank <{TransferInfo.ToBank}>");
                            return false;
                        }

                        VtbReport = new VtbReportModel();
                        transType = vtbToBank == VtbExtBank.Vtb ? TransferType.Internal : TransferType.External;
                        LogInfo((transType == TransferType.Internal ? "VTB" : "其他银行") + $"转账:{DateTime.Now}--OrderId:{TransferInfo.OrderNo}");
                        result = await VtbTransferAsync(transType, TransferInfo.UserName, TransferInfo.Password, vtbToBank,
                            TransferInfo.ToAccountNumber, TransferInfo.ToAccountName, TransferInfo.ToAmount, TransferInfo.ToRemarks);

                        VtbReport.Result = result;
                        VtbReport.TransferType = transType == TransferType.Internal ? "I" : "E";
                        ReportStatistic = VtbReport.Statistic;

                        break;

                    default:
                        return result;
                }

                if (!result)
                {
                    var htmlFileName = $"{TransferInfo.OrderNo}_ErrHtml" + "_{0}";
                    await Browser.WriteHtmlToFile(HtmlPath, htmlFileName);
                }

                sFileName = TransferInfo.OrderNo + (result ? "" : "_Err") + "_{0}";
                await Screenshot(sFileName, true);
            }
            catch (Exception ex)
            {
                LogError($"Error TransferAsync, {ex}", $"Error TransferAsync, {ex.Message}");
            }

            return result;
        }

        private bool IsLoading()
        {
            return NrLoaded < NrLoading;
        }

        private void ResetLog()
        {
            LogsCache = new AppLogUpdateModel();
        }

        private async Task UploadLog()
        {
            LogsCache.Phone = Phone;
            var messages = new AppLogMsgModel()
            {
                Messages = string.Join("^", LogsCache.Messages),
                Html = LogsCache.Html,
            };
            await LogUpdate(JsonConvert.SerializeObject(messages), DateTime.Now, LogsCache.Photo, LogsCache.PaymentId, LogsCache.Phone);
        }
        #endregion

        #region ACB

        public async Task AcbTestAsync()
        {
            try
            {
                var doTransfer = false;
                var blnTransfer = false;

                Bank = Bank.AcbBank;
                ////External Transfer
                //var transInfo = new TransferInfoModel()
                //{
                //    UserName = "0703805200", // "0966493205"
                //    Password = "Win168168@",
                //    CardNo = "40483827", // 37393987 
                //    ToBank = "VPBANK",
                //    ToAccountNumber = "040619921992",
                //    ToAccountName = "NGUYENTANGTHOAI",
                //    ToAmount = 50000000,
                //    ToRemarks = "50000000",
                //    OrderNo = "[ORDERNO]",

                //    DeviceId = 8,
                //    OrderId = "0",
                //};

                // Internal Transfer
                var transInfo = new TransferInfoModel()
                {
                    UserName = "0703805200",
                    Password = "Win168168@",
                    CardNo = "40483827",
                    ToBank = "ACB",
                    ToAccountNumber = "39681667",
                    ToAccountName = "HUYNH THI THANH THUY",
                    ToAmount = 10000,
                    ToRemarks = "10000",
                    OrderNo = "[ORDERNO]",

                    DeviceId = 8,
                    OrderId = "0",
                };

                if (doTransfer)
                {
                    blnTransfer = true;
                    LogInfo($"Start test transfer, {JsonConvert.SerializeObject(transInfo)}");
                    //LogInfo($"Start test transfer, {Bank}, {OrderNo}, {UserName}, {CardNo}, {ToBank}, {ToAccountNumber}, {ToAccountName}, {ToAmount}, {ToRemarks}");
                    var transferWatch = Stopwatch.StartNew();
                    var result = await TransferAsync(transInfo);
                    FirstTime = false;
                    transferWatch.Stop();
                    LogInfo($"End test transfer. Total time: {transferWatch.ElapsedMilliseconds.MilisecondsToTimeTaken()}");
                    LogInfo($"Report statistic|{transferWatch.Elapsed.TotalSeconds.To3Decimal()}|{ReportStatistic}");
                    RefreshInterval = 0;
                }

                // Refresh if not performing transfer and first launch
                // Refresh if not performing transfer each 3 minutes (180 seconds)
                if (!blnTransfer)
                {
                    if (FirstTime || RefreshInterval >= RefreshTime)
                    {
                        LogInfo($"Start refresh... {RefreshInterval}");
                        // Refresh Browser
                        var refresh = await RefreshAsync(transInfo);
                        LogInfo($"End refresh... {RefreshInterval}");
                        LogInfo($"Report statistic||{ReportStatistic}");
                        if (FirstTime && !refresh)
                        {
                            RefreshInterval = RefreshTime; // Quickly trigger another refresh if first time login failed
                        }
                        else
                        {
                            RefreshInterval = 0;
                        }
                        FirstTime = false;
                    }
                    else
                    {
                        LogInfo($"Next refresh at {RefreshInterval}/{RefreshTime}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error, {ex}", $"Error, {ex.Message}");
            }
        }

        private async Task<bool> AcbGetLanguageAsync()
        {
            var result = false;
            SelectedLang = Lang.Unknown;

            //Lang selectLang = Lang.Vietnamese; //test
            Lang selectLang = IsDebug ? Lang.English : Lang.Vietnamese;
            var langId = "#acb-one-header-container form[name*=\"chooseLan\"] input[class=\"language\"]";

            var iMaxWait = 10;
            var stopWatch = Stopwatch.StartNew();
            while (true)
            {
                var langValue = await Browser.JsGetInputValue(langId, 2);
                if (langValue == AcbText.Language.AcbTranslate(Lang.English))
                {
                    SelectedLang = Lang.Vietnamese;
                }
                else if (langValue == AcbText.Language.AcbTranslate(Lang.Vietnamese))
                {
                    SelectedLang = Lang.English;
                }

                if (selectLang != SelectedLang)
                {
                    var click = await Browser.JsClickElement(langId, 2);
                    LogInfo($"Click {langId}, <{click}>");
                    if (!await WaitAsync(2, 1)) { result = false; break; }

                    if (!click)
                        break;
                }
                else
                {
                    result = true;
                    break;
                }

                if (stopWatch.Elapsed.TotalSeconds > iMaxWait)
                {
                    break;
                }

                LogInfo($"Retrieving language, {stopWatch.Elapsed.TotalSeconds.DoubleToInt()}/{iMaxWait} ...");
                await Task.Delay(1000);
            }
            stopWatch.Stop();

            LogInfo($"Language is <{AcbText.Language.AcbTranslate(SelectedLang)}>");
            return result;
        }

        private async Task AcbLoginPopup()
        {
            var popupId = "#pop-up-warning";

            var popupExists = await JsElementExists(popupId, 2);
            if (popupExists)
            {
                var popupHeaderId = "#pop-up-warning #pop-up-body #header";
                var popupCloseId = "#pop-up-warning #pop-up-body #pop-up-footer input[id=\"agree-condition\"]";

                var headerText = await Browser.JsGetElementInnerText(popupHeaderId, 2);
                var closeText = await Browser.JsGetInputValue(popupCloseId, 2);

                var closeExists = await JsElementExists(popupCloseId, 2);
                if (closeExists)
                {
                    var closeClick = await Browser.JsClickElement(popupCloseId, 2);
                    if (closeClick)
                    {
                        LogInfo($"Click <{closeText}>, {headerText}");
                    }
                    else
                    {
                        LogError($"Error click <{closeText}>, {headerText}");
                    }
                }
            }
        }

        private async Task<bool> AcbLogin(string username, string password)
        {
            MyStopWatch sWatch;

            sWatch = new MyStopWatch(true);
            var setLogin = await SetLoginAsync(Bank, username, password);
            AcbReport.InputLogin = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!setLogin)
            {
                LogError($"Error set login");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var manualLogin = await AcbManualLoginAsync();
            AcbReport.ManualLogin = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!manualLogin) return false;

            sWatch = new MyStopWatch(true);
            var loginAuth = await AcbLoginAuthenticateAsync();
            AcbReport.LoginAuth = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!loginAuth) return false;

            sWatch = new MyStopWatch(true);
            var isLoggedIn = await IsLoggedInAsync(Bank);
            AcbReport.IsLoggedIn2 = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!isLoggedIn)
            {
                LogError($"Error not logged in");
                return false;
            }

            return true;
        }

        private async Task<bool> AcbManualLoginAsync()
        {
            var result = false;

            DoLockBrowser?.Invoke(this, false);
            var securityCodeId = "#acb-one-login-container form .aol-form input[id=\"security-code\"]";
            var securityCodeExists = await JsElementExists(securityCodeId, 1);
            if (securityCodeExists)
            {
                var clickSecurityCode = await Browser.MouseClickEventJsScroll(Bank, securityCodeId, 65 + 58 + 20, 1, Dpi);
            }

            var iMaxWait = 25;
            var stopWatch = Stopwatch.StartNew();
            while (true)
            {
                var isLoginExists = await IsLoginExistsAsync(Bank);

                if (!isLoginExists)
                {
                    result = true;
                    break;
                }
                else if (stopWatch.Elapsed.TotalSeconds > iMaxWait)
                {
                    result = false;
                    LogError($"Manual login timeout, {stopWatch.Elapsed.TotalSeconds.DoubleToInt()}/{iMaxWait}");
                    break;
                }

                DoSetCountDown?.Invoke(this, iMaxWait - stopWatch.Elapsed.TotalSeconds.DoubleToInt());
                LogInfo($"Please input Security code, {stopWatch.Elapsed.TotalSeconds.DoubleToInt()}/{iMaxWait} ...");
                await Task.Delay(1000);
            }
            stopWatch.Stop();
            DoResetCountDown?.Invoke(this, true);
            DoLockBrowser?.Invoke(this, true);

            return result;
        }

        private async Task<bool> AcbLoginAuthenticateAsync()
        {
            var result = false;
            var authId = "#main-acb-one form .wl-select input[id=\"safekey\"]";
            var safeKeyId = "#main-acb-one #help-container form .warning-login .userInput";
            var iMaxWait = 45;
            var stopWatch = Stopwatch.StartNew();

            DoLockBrowser?.Invoke(this, false);
            while (true)
            {
                var authExists = await JsElementExists(authId, 1);
                if (authExists)
                {
                    var authScroll = await Browser.JsScrollToElement(Bank, authId, 65 + 17, 1);
                }

                var safeKeyExists = await JsElementExists(safeKeyId, 1);
                if (safeKeyExists)
                {
                    var safeKeyScroll = await Browser.JsScrollToElement(Bank, safeKeyId, 65 + 45, 1);
                }

                if (!authExists && !safeKeyExists)
                {
                    result = true;
                    break;
                }
                else if (stopWatch.Elapsed.TotalSeconds > iMaxWait)
                {
                    result = false;
                    LogError($"Login auth timeout, {stopWatch.Elapsed.TotalSeconds.DoubleToInt()}/{iMaxWait}");
                    break;
                }

                DoSetCountDown?.Invoke(this, iMaxWait - stopWatch.Elapsed.TotalSeconds.DoubleToInt());
                LogInfo($"Please input OTP, {stopWatch.Elapsed.TotalSeconds.DoubleToInt()}/{iMaxWait} ...");
                await Task.Delay(1000);
            }
            stopWatch.Stop();
            DoResetCountDown?.Invoke(this, true);
            DoLockBrowser?.Invoke(this, true);

            return result;
        }

        private async Task<bool> AcbGoDashboardScreenAsync()
        {
            var id = "#menu li ul li a[href*=\"dse_operationName=ibkacctSumProc\"]";
            var exists = await JsElementExists(id, 2);
            if (!exists) return false;
            var click = await Browser.JsClickElement(id, 2);
            if (!await WaitAsync(2, 1)) return false;
            if (!click) return false;

            return true;
        }

        //private async Task<bool> AcbReloadPageAsync()
        //{
        //    var reload = await Browser.JsReloadAsync(1);
        //    await Task.Delay(1000 * 2);
        //    if (!await WaitAsync(2)) return false;
        //    if (!reload.BoolResult)
        //    {
        //        // In case Js execution failed, LoadUrl
        //        // Request BrowserId : 1 not found it's likely the browser is already closed 
        //        if (!reload.IsSuccess)
        //        {
        //            var canExecuteJs = false;
        //            var iTry = 0;
        //            var iTryMax = 8;

        //            while (iTry < iTryMax)
        //            {
        //                iTry++;

        //                LogInfo($"Try reload {iTry}/{iTryMax} ...");

        //                var exists = await Browser.JsElementExists("html", 1);
        //                canExecuteJs = exists.IsSuccess;
        //                if (!canExecuteJs)
        //                {
        //                    await LoadBankAsync(Bank);
        //                    await Task.Delay(1000 * 5);
        //                    if (!await WaitAsync(2)) return false;
        //                }
        //                else
        //                {
        //                    LogInfo($"Reload success {iTry}/{iTryMax}");
        //                    return true;
        //                }
        //            }

        //            if (!canExecuteJs)
        //            {
        //                LogError($"Reload failed {iTry}/{iTryMax}");
        //                return false;
        //            }
        //        }
        //    }
        //    return true;
        //}

        private async Task<bool> AcbRefreshAsync(string username, string password)
        {
            MyStopWatch sWatch;

            sWatch = new MyStopWatch(true);
            var isLoggedIn = await IsLoggedInAsync(Bank);
            AcbReport.IsLoggedIn = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (isLoggedIn)
            {
                sWatch = new MyStopWatch(true);
                var goDashboard = await AcbGoDashboardScreenAsync();
                AcbReport.GoDashboard = (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!goDashboard)
                {
                    LogError($"Error go dashboard screen");
                    return false;
                }

                await ShowBalanceAsync(Bank);
            }
            else
            {
                sWatch = new MyStopWatch(true);
                var loadBank = await LoadBankAsync(Bank);
                //var reload = await ReloadPageAsync(Bank.AcbBank);
                AcbReport.Reload = (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!loadBank) return false;

                sWatch = new MyStopWatch(true);
                var lang = await AcbGetLanguageAsync();
                AcbReport.GetLang = (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!lang)
                {
                    LogError($"Error get language");
                    return false;
                }

                var login = await AcbLogin(username, password);
                if (login)
                {
                    await ShowBalanceAsync(Bank);
                }
                else { return false; }
            }

            await AcbLoginPopup();

            return true;
        }

        private async Task<bool> AcbGoTransferPageAsync(TransferType transType)
        {
            var transferLinkId = transType == TransferType.Internal ?
                ".main #menu li a[href*=\"/online-transfer-acb\"]" :
                ".main #menu li a[href*=\"dse_operationName=ibkcardToCardFundTransProc\"]";
            var transferLinkExists = await JsElementExists(transferLinkId, 2);
            if (!transferLinkExists) return false;

            var clickTransferLink = await Browser.JsClickElement(transferLinkId, 2);
            if (!await WaitAsync(2, 1)) return false;
            if (!clickTransferLink) return false;

            return true;
        }

        private async Task<bool> AcbInputInternalTransferAsync(string accNum, string accName, decimal amount, string remarks, int deviceId, string orderId)
        {
            // Check Balance | Wait 10 seconds for page loading buffer
            if (1 == 1)
            {
                var accBalId = "form .p-dropdown-label strong";
                var accBalExists = await JsElementExists(accBalId, 10);
                if (!accBalExists)
                {
                    LogError($"Error deduction account missing, {accBalId}");
                    return false;
                }
                var accBal = await Browser.JsGetElementInnerText(accBalId, 2);
                if (accBal != null && accBal.ToNumber(Bank).StrToDec() >= amount)
                {
                    BalanceAfterTransfer = accBal.ToNumber(Bank).StrToDec() - amount;
                    LogInfo($"Balance is <{accBal}>, {amount}, {accBalId}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"余额不足: <{accBal}>, 转账金额: {amount}";
                    LogError($"Error insufficient balance <{accBal}>, {amount}, {accBalId}");
                    return false;
                }
            }

            // Input Beneficiary account 
            if (1 == 1)
            {
                var benAccId = "#beneficiaryAccount";
                var benAccExists = await JsElementExists(benAccId, 2);
                if (!benAccExists)
                {
                    LogError($"Error Beneficiary account input not exists, {benAccId}");
                    return false;
                }
                var inputBenAcc = await Browser.InputToElementJsScroll(Bank, benAccId, accNum, 1, 2, Dpi);
                if (!inputBenAcc)
                {
                    LogError($"Error input Beneficiary account <{accNum}>, {benAccId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Beneficiary account <{accNum}>, {benAccId}");
                }
            }

            // Click Remark then check Beneficiary account has error?
            var descId = "#description";
            if (1 == 1)
            {
                var descExists = await JsElementExists(descId, 2);
                if (!descExists)
                {
                    LogError($"Error Transfer description input not exists, {descId}");
                    return false;
                }
                var clickDesc = await Browser.MouseClickEventJsScroll(Bank, descId, 1, 2, Dpi);
                if (!await WaitAsync(2, 1)) return false;
                if (!clickDesc)
                {
                    LogError($"Error click Transfer description, {descId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Transfer description, {descId}");
                }
            }

            // Verify Beneficiary account | Wait 5 seconds
            if (1 == 1)
            {
                var benErrId = ".p-error";
                var benErrExists = await JsElementExists(benErrId, 5);
                if (benErrExists)
                {
                    var benErrText = await Browser.JsGetElementInnerText(benErrId, 1);
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"卡号无效: {accNum}";
                    LogError($"Error Beneficiary account <{benErrText}>, {benErrId}");
                    return false;
                }
            }

            // Verify Beneficiary unit
            if (1 == 1)
            {
                var benNameId = "#beneficiaryName";
                var benNameExists = await JsElementExists(benNameId, 2);
                if (!benNameExists)
                {
                    LogError($"Error Beneficiary unit not exists, {benNameId}");
                    return false;
                }
                var benName = await Browser.JsGetInputValue(benNameId, 2);
                if (benName != null && benName.ToBeneficiaryNameCompare() == accName.ToBeneficiaryNameCompare())
                {
                    LogInfo($"Beneficiary name is <{benName}>, {benNameId}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"收款人姓名不符: {accName}, 订单姓名: {benName}";
                    LogError($"Beneficiary name not match <{benName}>, <{accName}>, {benNameId}");
                    return false;
                }
            }

            // Input Amount
            if (1 == 1)
            {
                var amountId = "#amount";
                var amountExists = await JsElementExists(amountId, 2);
                if (!amountExists)
                {
                    LogError($"Error Amount input not exists, {amountId}");
                    return false;
                }
                var clickAmount = await Browser.MouseClickEventJsScroll(Bank, amountId, 1, 2, Dpi);
                if (!await WaitAsync(2, 1)) return false;
                if (!clickAmount)
                {
                    LogError($"Error click Amount, {amountId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Amount, {amountId}");
                }

                var inputAmount = await Browser.InputToElementJsScroll(Bank, amountId, amount.ToString(), 1, 2, Dpi, false);
                if (!inputAmount)
                {
                    LogError($"Error input Amount <{amount}>, {amountId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Amount <{amount}>, {amountId}");
                }
            }

            // Input Transfer description
            if (1 == 1)
            {
                var inputDesc = await Browser.InputToElementJsScroll(Bank, descId, remarks, 1, 2, Dpi);
                if (!inputDesc)
                {
                    LogError($"Error input Transfer description <{remarks}>, {descId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Transfer description <{remarks}>, {descId}");
                }
            }

            // Choose authentication method
            if (1 == 1)
            {
                var authPos = await Browser.JsAcbGetAuthMethodPostnIntTransPage(2);
                if (authPos == null)
                {
                    LogError($"Error Choose authentication method drop down not exists");
                    return false;
                }
                var scrollToAuth = await Browser.JsScrollToJsPosition(Bank, authPos, 2);
                await Task.Delay(1000);

                var authPos2 = await Browser.JsAcbGetAuthMethodPostnIntTransPage(2);
                var clickAuth = Browser.MouseClickJsPosition(authPos2, 2, 25, 25);
                if (!clickAuth)
                {
                    LogError($"Error click Choose authentication method");
                    return false;
                }
                else
                {
                    LogInfo($"Click Choose authentication method");
                }

                // Pick "Static password + Advance OTP SafeKey"
                string authOpt1 = AcbText.AuthMethodSafeKey.AcbTranslate(SelectedLang);
                var authOpt1Pos = await Browser.JsAcbGetAuthSafekeyOtpPostnIntTransPage(authOpt1, 2);
                if (authOpt1Pos == null)
                {
                    LogError($"Error authentication method not found <{authOpt1}>");
                    return false;
                }
                var clickAuthOpt1 = Browser.MouseClickJsPosition(authOpt1Pos, 2, Dpi);
                if (!clickAuthOpt1)
                {
                    LogError($"Error click authentication method <{authOpt1}>");
                    return false;
                }
                else
                {
                    LogInfo($"Click authentication method <{authOpt1}>");
                }
            }

            // Click Continue
            if (1 == 1)
            {
                var submitId = "form button.p-button";
                var submitExists = await JsElementExists(submitId, 2);
                if (!submitExists)
                {
                    LogError($"Error Continue button not exists, {submitId}");
                    return false;
                }
                var clickSubmit = await Browser.JsClickElement(submitId, 2);
                if (!await WaitAsync(2, 1)) return false;
                if (!clickSubmit)
                {
                    LogError($"Error click Continue, {submitId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Continue, {submitId}");
                }
            }

            // Check has input error?
            if (1 == 1)
            {
                var errMsg = await Browser.JsAcbGetErrMsgIntTransPage(2);
                if (errMsg != null)
                {
                    LogError($"Error input <{errMsg}>");
                    return false;
                }
            }

            // API - Send OTP Request
            if (1 == 1)
            {
                if (!IsDebug)
                {
                    var sendOtpReq = await CreateOrderOtp(deviceId, orderId);
                    if (!sendOtpReq)
                    {
                        LogError($"Error CreateOrderOtp");
                        return false;
                    }
                    else
                    {
                        LogInfo($"CreateOrderOtp");
                    }
                }
                else
                {
                    LogInfo($"This is debug mode, no API call will be issued.");
                }
            }

            return true;
        }

        private async Task<bool> AcbInputExternalTransferAsync(string fromAccNum,
            AcbExtBank bank, string accNum, decimal amount, string remarks, int deviceId, string orderId)
        {
            // Choose debit account
            if (1 == 1)
            {
                var fromAccId = ".main .content-holder form[name=\"smlibt\"] .table-form #TkNo";
                var fromAccExists = await JsElementExists(fromAccId, 2);
                if (!fromAccExists)
                {
                    LogError($"Error Choose debit account not exists, {fromAccId}");
                    return false;
                }
                var fromAccValue = await Browser.JsGetInputValue(fromAccId, 2);
                if (fromAccValue != fromAccNum)
                {
                    var selectFromAcc = await Browser.JsSetSelectValue(fromAccId, fromAccNum, 2);
                    if (!await WaitAsync(2, 2)) return false;
                    if (selectFromAcc)
                    {
                        fromAccValue = await Browser.JsGetInputValue(fromAccId, 2);
                        if (fromAccValue == fromAccNum)
                        {
                            LogInfo($"Choose debit account, <{fromAccValue}>, {fromAccId}");
                        }
                        else
                        {
                            LogError($"Error Choose debit account, <{fromAccValue}>, {fromAccNum}, {fromAccId}");
                            return false;
                        }
                    }
                    else
                    {
                        LogError($"Error Choose debit account, {fromAccNum}, {fromAccId}");
                        return false; // select error
                    }
                }
                else
                {
                    LogInfo($"Choose debit account, <{fromAccValue}>, {fromAccId}");
                }
            }

            // Wait Balance
            if (1 == 1)
            {
                var fromAccNameId = ".main .content-holder form[name=\"smlibt\"] .table-form tr td table tr td .alert";
                string? fromAccName = null;
                var waitAccName = 10;
                var accNameWatch = Stopwatch.StartNew();
                while (true)
                {
                    fromAccName = await Browser.JsGetElementInnerText(fromAccNameId, 2);
                    if (!string.IsNullOrWhiteSpace(fromAccName))
                        break;

                    if (accNameWatch.Elapsed.TotalSeconds.DoubleToInt() > waitAccName)
                        break;

                    LogInfo($"Retrieving balance {accNameWatch.Elapsed.TotalSeconds.DoubleToInt()}/{waitAccName} ...");
                    await Task.Delay(1000);
                }
                accNameWatch.Stop();

                if (string.IsNullOrWhiteSpace(fromAccName))
                {
                    LogError($"Error balance not exists, {fromAccNameId}");
                    return false;
                }

                var productAndBalance = fromAccName.Replace(AcbText.Balance.AcbTranslate(SelectedLang), "").Split(":");
                var accountBalance = productAndBalance.Length > 1 ? productAndBalance[1].Trim() : "";
                if (accountBalance == null)
                {
                    LogError($"Error retrieving balance, <{fromAccName}>, {fromAccNameId}");
                    return false;
                }
                if (accountBalance.ToNumber(Bank).StrToDec() > amount)
                {
                    BalanceAfterTransfer = accountBalance.ToNumber(Bank).StrToDec() - amount;
                    LogInfo($"Balance is <{accountBalance}>, {amount}, {fromAccNameId}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"余额不足: <{accountBalance}>, 转账金额: {amount}";
                    LogError($"Error insufficient balance, <{accountBalance}>, {amount}, {fromAccNameId}");
                    return false;
                }
            }

            // Tick Enter the beneficiary account
            if (1 == 1)
            {
                var optEnterBenAccId = ".main .content-holder form[name=\"smlibt\"] .table-form #IsDonViThuHuong2";
                var optEnterBenAccExists = await JsElementExists(optEnterBenAccId, 2);
                if (!optEnterBenAccExists)
                {
                    LogError($"Error Enter the beneficiary account not exists, {optEnterBenAccId}");
                    return false;
                }
                var optEnterBenAccChecked = await Browser.JsIsInputChecked(optEnterBenAccId, 1);
                if (!optEnterBenAccChecked)
                {
                    var checkOptEnterBenAcc = await Browser.JsSetCheckbox(optEnterBenAccId, true, 1);
                    if (!await WaitAsync(2)) return false;
                    if (checkOptEnterBenAcc)
                    {
                        LogInfo($"Selected Enter the beneficiary account option, {optEnterBenAccId}");
                    }
                    else
                    {
                        LogError($"Error select Enter the beneficiary account option, {optEnterBenAccId}");
                        return false;
                    }
                }
                else
                {
                    LogInfo($"Selected Enter the beneficiary account option, {optEnterBenAccId}");
                }
            }

            // Enter the beneficiary account
            if (1 == 1)
            {
                var benAccId = ".main .content-holder form[name=\"smlibt\"] .table-form #AccountNbrCoMask";
                var benAccExists = await JsElementExists(benAccId, 2);
                if (!benAccExists)
                {
                    LogError($"Error beneficiary account input not exists, {benAccId}");
                    return false;
                }
                var inputBenAcc = await Browser.JsInputElementValue(benAccId, accNum, 2);
                if (!inputBenAcc)
                {
                    LogError($"Error input beneficiary account <{accNum}>, {benAccId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input beneficiary account <{accNum}> {benAccId}");
                }
            }

            // Choose the beneficiary's bank
            if (1 == 1)
            {
                var beneBank = bank.GetAcbExtBank()?.ShortName;
                var benBankId = ".main .content-holder form[name=\"smlibt\"] .table-form #NganHang";
                var benBankExist = await JsElementExists(benBankId, 2);
                if (!benBankExist)
                {
                    LogError($"Error Choose the beneficiary's bank input not exists, {benBankId}");
                    return false;
                }
                var selectBenBank = await Browser.JsSetSelectValue(benBankId, beneBank, 2);
                if (!selectBenBank)
                {
                    LogError($"Error select beneficiary's bank, {beneBank}, {benBankId}");
                    return false;
                }
                var hasBenBankSelected = await Browser.JsHasSelected(benBankId, 2);
                if (!hasBenBankSelected)
                {
                    TransferInfo.Status = TransferStatus.ErrorBank;
                    TransferInfo.ErrorMessage = $"银行无效 <{beneBank}>";
                    LogError($"Error select beneficiary's bank <{beneBank}>, {benBankId}");
                    return false;
                }
                var selectedBenBank = await Browser.JsGetSelectText(benBankId, 2);
                LogInfo($"Selected beneficiary's bank <{selectedBenBank}>, {beneBank}, {benBankId}");
            }

            // Input amount
            if (1 == 1)
            {
                var amountId = ".main .content-holder form[name=\"smlibt\"] .table-form #Amount";
                var amountExists = await JsElementExists(amountId, 2);
                if (!amountExists)
                {
                    LogError($"Error Amount input not exists, {amountId}");
                    return false;
                }

                var inputAmount = await Browser.InputToElementJsScroll(Bank, amountId, amount.ToString(), 65 - 5, 2, Dpi, false);
                await Task.Delay(1000 * 2);
                if (!await WaitAsync(2)) return false;
                if (!inputAmount)
                {
                    LogError($"Error input Amount <{amount}> {amountId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Amount <{amount}> {amountId}");
                }
            }

            // Input Detail of transaction | Description
            if (1 == 1)
            {
                var descId = ".main .content-holder form[name=\"smlibt\"] .table-form textarea[name=\"Description\"]";
                var descExists = await JsElementExists(descId, 2);
                if (!descExists)
                {
                    LogError($"Error Detail of transaction input not exists, {descId}");
                    return false;
                }

                var mouseClickDesc = await Browser.MouseClickEventJsScroll(Bank, descId, 65 - 5, 2, Dpi);
                if (!await WaitAsync(2, 1)) return false;
                if (!mouseClickDesc)
                {
                    LogError($"Error click Detail of transaction input, {descId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Detail of transaction input, {descId}");
                }

                var inputDesc = await Browser.JsInputElementValue(descId, remarks, 2);
                if (!inputDesc)
                {
                    LogError($"Error input Detail of transaction, {remarks}, {descId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Detail of transaction, {remarks}, {descId}");
                }
            }

            // Choose authentication method
            if (1 == 1)
            {
                const string AUTH_OTP = "OTPA";
                //const string AUTH_SMS = "SMS";

                var authMethodId = ".main .content-holder form[name=\"smlibt\"] .table-form #authTypList";
                var authMethodExists = await JsElementExists(authMethodId, 2);
                if (!authMethodExists)
                {
                    LogError($"Error Choose authentication method not exists, {authMethodId}");
                    return false;
                }
                var selectAuthMethod = await Browser.JsSetSelectValue(authMethodId, AUTH_OTP, 2);
                if (!selectAuthMethod)
                {
                    LogError($"Error select authentication method, {AUTH_OTP}, {authMethodId}");
                    return false;
                }
                var hasAuthMethodSelected = await Browser.JsHasSelected(authMethodId, 2);
                if (!hasAuthMethodSelected)
                {
                    LogError($"Error select authentication method, {AUTH_OTP}, {authMethodId}");
                    return false;
                }
                var selectedAuthMethod = await Browser.JsGetSelectText(authMethodId, 2);
                LogInfo($"Selected <{selectedAuthMethod}>, {authMethodId}");
            }

            // Click Agree
            if (1 == 1)
            {
                var submitId = ".main .content-holder form[name=\"smlibt\"] .table-form #dongy";
                var submitExists = await JsElementExists(submitId, 2);
                if (!submitExists)
                {
                    LogError($"Error Agree button not exists, {submitId}");
                    return false;
                }
                var clickSubmit = await Browser.JsClickElement(submitId, 2);
                if (!await WaitAsync(2, 1)) return false;
                if (!clickSubmit)
                {
                    LogError($"Error click Agree, {submitId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Agree, {submitId}");
                }
            }

            // Check has error
            if (await AcbCommonErrorPage("smlibt")) return false;

            // API - Send OTP Request
            if (1 == 1)
            {
                if (!IsDebug)
                {
                    var sendOtpReq = await CreateOrderOtp(deviceId, orderId);
                    if (!sendOtpReq)
                    {
                        LogError($"Error CreateOrderOtp");
                        return false;
                    }
                    else
                    {
                        LogInfo($"CreateOrderOtp");
                    }
                }
                else
                {
                    LogInfo($"This is debug mode, no API call will be issued.");
                }
            }

            return true;
        }

        private async Task<bool> AcbCommonErrorPage(string formName)
        {
            var result = false;

            string? error1Text = null;
            var errorId1 = $".main .content-holder form[name=\"{formName}\"] .table-form .error";
            var error1Exists = await JsElementExists(errorId1, 1);
            if (error1Exists)
            {
                error1Text = await Browser.JsGetElementInnerText(errorId1, 1);
                LogError($"Input error, <{error1Text}>");
            }

            string? error2Text = null;
            var errorId2 = $".main .content-holder form[name=\"{formName}\"] .table-form .detailerror";
            var error2Exists = await JsElementExists(errorId2, 1);
            if (error2Exists)
            {
                error2Text = await Browser.JsGetElementInnerText(errorId2, 1);
                LogError($"Input error, <{error2Text}>");
            }

            if (error1Exists || error2Exists)
            {
                TransferInfo.ErrorMessage = $"{error1Exists},{error2Text}";
                result = true;
            };

            return result;
        }

        private async Task<bool> AcbIsTransferResultPage()
        {
            var btnId = ".main .content-holder .table-form form[name=\"next\"] #button";
            var btnExists = await JsElementExists(btnId, 2);

            return btnExists;
        }

        private async Task<bool> AcbIsExternalTransferSuccess()
        {
            var result = false;

            // Html too common, check error first before check for success

            var errorId = ".main .content-holder .table-form table td[class*=\"center\"][class*=\"error\"]";
            var errorExists = await JsElementExists(errorId, 2);
            if (errorExists)
            {
                var errorText = await Browser.JsGetElementInnerText(errorId, 2);
                LogError($"Transfer failed, <{errorText}>, {errorId}");
            }
            else
            {
                var successId = ".main .content-holder .table-form table td[class=\"center\"]";
                var successExists = await JsElementExists(successId, 2);

                if (successExists)
                {
                    var successText = await Browser.JsGetElementInnerText(successId, 2);
                    LogInfo($"Transfer success, <{successText}>, {successId}");
                    result = true;
                }
                else
                {
                    LogError($"Transfer failed, unexpected error.");
                }
            }

            return result;
        }

        private async Task<bool> AcbConfirmInternalTransferAsync(string password, int deviceId, string orderId)
        {
            // Is confirm transfer screen 
            // Check password input exists with 10 seconds buffer
            var passwordId = ".p-password input";
            if (1 == 1)
            {
                var passExists = await JsElementExists(passwordId, 10);
                if (!passExists)
                {
                    LogError($"Error confirm transfer screen not loaded, {passwordId}");
                    return false;
                }
            }

            // Input Enter password
            if (1 == 1)
            {
                var inputPassword = await Browser.InputToElementJsScroll(Bank, passwordId, password, 1, 2, Dpi);
                if (!inputPassword)
                {
                    LogError($"Error input password <{password}>, {passwordId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input password <{password}>, {passwordId}");
                }
            }

            // Input Enter Safekey OTP
            if (1 == 1)
            {
                var otpId = "input[aria-describedby=\"otp-help\"]";
                var otpExists = await JsElementExists(otpId, 2);
                if (!otpExists)
                {
                    LogError($"Error OTP SafeKey input not exists, {otpId}");
                    return false;
                }

                // Retrieve OTP
                var authResult = true;
                string? otp = null;
                DateTime? otpAt = DateTime.MinValue;
                var waitOtp = 90;
                var otpWatch = Stopwatch.StartNew();
                while (true)
                {
                    otp = "123123"; // Default OTP for debug mode, during wait time, manual enter correct OTP
                    OrderOtpModel? otpResult = null;
                    if (!IsDebug)
                    {
                        otpResult = await GetOrderOtp(deviceId, orderId);
                        otp = otpResult?.Otp;
                    }

                    if (!string.IsNullOrWhiteSpace(otp) &&
                        IsDebug ? true : otpResult?.UpdateTime != otpAt) // Skip condition for debug mode
                    {
                        if (!IsDebug) otpAt = otpResult?.UpdateTime; // Skip for debug mode

                        var inputOtp = await Browser.InputToElementJsScroll(Bank, otpId, otp, 65, 2, Dpi);
                        if (!inputOtp)
                        {
                            LogError($"Error input OTP, <{otp}>, {otpId}");
                            authResult = false; break;
                        }
                        else
                        {
                            LogInfo($"Input OTP, <{otp}>, {otpId}");
                        }

                        var submitId = "button.p-button:not(.button-back)";
                        var submitExists = await JsElementExists(submitId, 2);
                        if (!submitExists)
                        {
                            LogError($"Error Confirm button not exists, {submitId}");
                            authResult = false; break;
                        }

                        if (IsDebug) { await Task.Delay(1000 * 30); } // Debug mode, allow manual input correct OTP

                        var clickSubmit = await Browser.JsClickElement(submitId, 2);
                        if (!await WaitAsync(2, 1)) { authResult = false; break; }
                        if (!clickSubmit)
                        {
                            LogError($"Error click Confirm button, {submitId}");
                            authResult = false; break;
                        }
                        else
                        {
                            LogInfo($"Click Confirm button, {submitId}");
                        }
                        break;
                    }

                    if (otpWatch.Elapsed.TotalSeconds.DoubleToInt() > waitOtp)
                        break;

                    DoSetCountDown?.Invoke(this, waitOtp - otpWatch.Elapsed.TotalSeconds.DoubleToInt());
                    LogInfo($"Retrieving OTP, {otpWatch.Elapsed.TotalSeconds.DoubleToInt()}/{waitOtp} ...");
                    await Task.Delay(1000);
                }
                otpWatch.Stop();

                DoResetCountDown?.Invoke(this, true);
                if (!authResult) return false;
            }

            // Check has error
            var errMsg = await Browser.JsAcbGetErrMsgIntTransPage(2);
            if (errMsg != null)
            {
                LogError($"Error input <{errMsg}>");
                return false;
            }

            // Wait Transfer result page | Check New transaction button | Wait 10 seconds
            if (1 == 1)
            {
                var newTransId = "button.p-button.XfUdQ";
                var newTransExists = await JsElementExists(newTransId, 10);
                if (!newTransExists)
                {
                    LogError($"Error transfer status page not loaded, {newTransId}");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> AcbConfirmExternalTransferAsync(string accName, string password, int deviceId, string orderId)
        {
            // Is confirm transfer screen
            if (1 == 1)
            {
                var confirmScreenId = ".main .content-holder form[name=\"confirm_smlibt_bn\"]";
                var confirmScreenExists = await JsElementExists(confirmScreenId, 2);
                if (!confirmScreenExists)
                {
                    LogError($"Error confirm transfer screen not loaded");
                    return false;
                }
            }

            // Compare Ben name
            if (1 == 1)
            {
                var benName = await Browser.JsGetAcbConfirmTransferBenName(SelectedLang, 2);
                if (benName != null && benName.ToBeneficiaryNameCompare() == accName.ToBeneficiaryNameCompare())
                {
                    LogInfo($"Beneficiary name is <{benName}>, {accName}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"收款人姓名不符: {accName}, 订单姓名: {benName}";
                    LogError($"Error Beneficiary name not match <{benName}>, {accName}");
                    return false;
                }
            }

            // Input password
            if (1 == 1)
            {
                var passwordId = ".main .content-holder form[name=\"confirm_smlibt_bn\"] input[name=\"Password\"]";
                var passwordExists = await JsElementExists(passwordId, 2);
                if (!passwordExists)
                {
                    LogError($"Error Enter login password input not exists, {passwordId}");
                    return false;
                }

                var inputPassword = await Browser.JsInputElementValue(passwordId, password, 2);
                if (!inputPassword)
                {
                    LogError($"Error input password, <{password}>, {passwordId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input password, <{password}>, {passwordId}");
                }
            }

            // Input OTP
            if (1 == 1)
            {
                var otpId = ".main .content-holder form[name=\"confirm_smlibt_bn\"] input[name=\"EdtOtp\"]";
                var otpExists = await JsElementExists(otpId, 2);
                if (!otpExists)
                {
                    LogError($"Error OTP SafeKey input not exists, {otpId}");
                    return false;
                }

                // Retrieve OTP
                var authResult = true;
                string? otp = null;
                DateTime? otpAt = DateTime.MinValue;
                var waitOtp = 90;
                var otpWatch = Stopwatch.StartNew();
                while (true)
                {
                    otp = "123123"; // Default OTP for debug mode, during wait time, manual enter correct OTP
                    OrderOtpModel? otpResult = null;
                    if (!IsDebug)
                    {
                        otpResult = await GetOrderOtp(deviceId, orderId);
                        otp = otpResult?.Otp;
                    }

                    if (!string.IsNullOrWhiteSpace(otp) &&
                        IsDebug ? true : otpResult?.UpdateTime != otpAt) // Skip condition for debug mode
                    {
                        if (!IsDebug) otpAt = otpResult?.UpdateTime; // Skip for debug mode

                        var inputOtp = await Browser.InputToElementJsScroll(Bank, otpId, otp, 65, 2, Dpi);
                        if (!inputOtp)
                        {
                            LogError($"Error input OTP, <{otp}>, {otpId}");
                            authResult = false; break;
                        }
                        else
                        {
                            LogInfo($"Input OTP, <{otp}>, {otpId}");
                        }

                        var submitId = ".main .content-holder form[name=\"confirm_smlibt_bn\"] #button";
                        var submitExists = await JsElementExists(submitId, 2);
                        if (!submitExists)
                        {
                            LogError($"Error Confirm button not exists, {submitId}");
                            authResult = false; break;
                        }

                        if (IsDebug) { await Task.Delay(1000 * 30); } // Debug mode, allow manual input correct OTP

                        var clickSubmit = await Browser.JsClickElement(submitId, 2);
                        if (!await WaitAsync(2, 1)) { authResult = false; break; }
                        if (!clickSubmit)
                        {
                            LogError($"Error click Confirm button, {submitId}");
                            authResult = false; break;
                        }
                        else
                        {
                            LogInfo($"Click Confirm button, {submitId}");
                        }
                        break;
                    }

                    if (otpWatch.Elapsed.TotalSeconds.DoubleToInt() > waitOtp)
                        break;

                    DoSetCountDown?.Invoke(this, waitOtp - otpWatch.Elapsed.TotalSeconds.DoubleToInt());
                    LogInfo($"Retrieving OTP, {otpWatch.Elapsed.TotalSeconds.DoubleToInt()}/{waitOtp} ...");
                    await Task.Delay(1000);
                }
                otpWatch.Stop();

                DoResetCountDown?.Invoke(this, true);
                if (!authResult) return false;
            }

            // Check has error
            if (await AcbCommonErrorPage("confirm_smlibt_bn")) return false;

            // Wait Transfer result page
            if (1 == 1)
            {
                var isTransferResult = false;
                var resultWatch = Stopwatch.StartNew();
                var waitResult = 10;
                while (true)
                {
                    isTransferResult = await AcbIsTransferResultPage();
                    if (isTransferResult)
                        break;

                    if (resultWatch.Elapsed.TotalSeconds.DoubleToInt() > waitResult)
                        break;

                    await Task.Delay(1000);
                }
                resultWatch.Stop();

                if (!isTransferResult)
                    return false;
            }

            return true;
        }

        private async Task<bool> AcbTransferAsync(TransferType transType, string username, string password, string cardNo,
            AcbExtBank toBank, string toAccNum, string toAccName, decimal toAmount, string remarks, int deviceId, string orderId)
        {
            MyStopWatch sWatch;

            var refresh = await AcbRefreshAsync(username, password);
            if (!refresh) return false;

            // Retry on failed transfer for External (possible wrong OTP) 
            // Retry input OTP for Internal transfer, not required to key in again
            var iTry = 0;
            var iTryMax = transType == TransferType.Internal ? 1 : 3;
            var isSuccess = false;
            while (iTry < iTryMax)
            {
                iTry++;
                LogInfo($"Try transfer {iTry}/{iTryMax} ...");

                sWatch = new MyStopWatch(true);
                var goTransferScreen = await AcbGoTransferPageAsync(transType);
                AcbReport.GoTransfer += (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!goTransferScreen)
                {
                    LogError($"Error go transfer screen");
                    return false;
                }

                sWatch = new MyStopWatch(true);
                var inputTransfer = transType == TransferType.Internal ?
                    await AcbInputInternalTransferAsync(toAccNum, toAccName, toAmount, remarks, deviceId, orderId) :
                    await AcbInputExternalTransferAsync(cardNo, toBank, toAccNum, toAmount, remarks, deviceId, orderId);
                AcbReport.InputTransfer += (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!inputTransfer) return false;

                sWatch = new MyStopWatch(true);
                var confirmTransfer = transType == TransferType.Internal ?
                    await AcbConfirmInternalTransferAsync(password, deviceId, orderId) :
                    await AcbConfirmExternalTransferAsync(toAccName, password, deviceId, orderId);
                AcbReport.ConfirmTransfer += (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!confirmTransfer)
                {
                    LogError($"Error confirm transfer");
                    return false;
                }

                sWatch = new MyStopWatch(true);
                isSuccess = transType == TransferType.Internal ? true : await AcbIsExternalTransferSuccess();
                if (isSuccess)
                {
                    if (!IsDebug)
                    {
                        // Update Order Otp = 1
                        var updateOtp = await UpdateOtptatus(deviceId, orderId, 1);
                        if (!updateOtp)
                        {
                            LogError($"Error Update Otptatus <1>");
                        }
                        else
                        {
                            LogInfo("Update Otptatus <1>");
                        }
                    }
                    LogInfo($"Transfer success");
                    TransferInfo.Status = TransferStatus.Success;
                    break;
                }
                AcbReport.TransferStatus += (int)sWatch.Elapsed(true)?.TotalSeconds;
            }

            if (!isSuccess)
            {
                LogError($"Transfer failed");
                return false;
            }

            return true;
        }

        #endregion

        #region MSB

        public async Task MsbTestAsync()
        {
            try
            {
                var doTransfer = false;
                var blnTransfer = false;

                Bank = Bank.MsbBank;
                ////External Transfer
                //var transInfo = new TransferInfoModel()
                //{
                //    UserName = "0338314730",
                //    Password = "Win168168@",
                //    CardNo = "80000173162",
                //    ToBank = "vietcombank",
                //    ToAccountNumber = "1042306840", 
                //    ToAccountName = "NGUYEN VAN HUY",
                //    ToAmount = 5000,
                //    ToRemarks = "5000",
                //    OrderNo = "[ORDERNO]",

                //    DeviceId = 8,
                //    OrderId = "0",
                //};

                // Internal Transfer
                var transInfo = new TransferInfoModel()
                {
                    UserName = "0338314730",
                    Password = "Win168168@",
                    CardNo = "80000173162",
                    ToBank = "msb",
                    ToAccountNumber = "80000174771",
                    ToAccountName = "TRAN SON TRUNG",
                    ToAmount = 5000,
                    ToRemarks = "5000",
                    OrderNo = "[ORDERNO]",

                    DeviceId = 8,
                    OrderId = "0",
                };

                if (doTransfer)
                {
                    blnTransfer = true;
                    LogInfo($"Start test transfer, {JsonConvert.SerializeObject(transInfo)}");
                    var transferWatch = Stopwatch.StartNew();
                    var result = await TransferAsync(transInfo);
                    FirstTime = false;
                    transferWatch.Stop();
                    LogInfo($"End test transfer. Total time: {transferWatch.ElapsedMilliseconds.MilisecondsToTimeTaken()}");
                    LogInfo($"Report statistic|{transferWatch.Elapsed.TotalSeconds.To3Decimal()}|{ReportStatistic}");
                    RefreshInterval = 0;
                }

                // Refresh if not performing transfer and first launch
                // Refresh if not performing transfer each 3 minutes (180 seconds)
                if (!blnTransfer)
                {
                    if (FirstTime || RefreshInterval >= RefreshTime)
                    {
                        LogInfo($"Start refresh... {RefreshInterval}");
                        // Refresh Browser
                        var refresh = await RefreshAsync(transInfo);
                        LogInfo($"End refresh... {RefreshInterval}");
                        LogInfo($"Report statistic||{ReportStatistic}");
                        if (FirstTime && !refresh)
                        {
                            RefreshInterval = RefreshTime; // Quickly trigger another refresh if first time login failed
                        }
                        else
                        {
                            RefreshInterval = 0;
                        }
                        FirstTime = false;
                    }
                    else
                    {
                        LogInfo($"Next refresh at {RefreshInterval}/{RefreshTime}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error, {ex}", $"Error, {ex.Message}");
            }
        }

        private async Task MsbHandleTimeoutPageAsync()
        {
            var id = "section.section-timeout";
            var exists = await JsElementExists(id, 1);
            if (exists)
            {
                var msg = await Browser.JsGetElementInnerText(id, 2);
                LogInfo($"Session timeout <{msg}>");

                var btnId = "section.section-timeout input";
                var btnExists = await JsElementExists(btnId, 2);
                if (btnExists)
                {
                    var btnValue = await Browser.JsGetInputValue(btnId, 2);
                    var clickBtn = await Browser.JsClickElement(btnId, 2);
                    if (!clickBtn)
                    {
                        LogError($"Error click <{btnValue}>, {btnId}");
                    }
                    else
                    {
                        LogInfo($"Click <{btnValue}>, {btnId}");
                    }
                }
            }
        }

        private async Task<bool> MsbGetLanguageAsync()
        {
            SelectedLang = Lang.Unknown;

            //var selectLang = Lang.Vietnamese; // test
            // Set to EN on debug mode
            var selectLang = IsDebug ? Lang.English : Lang.Vietnamese;

            var langId = ".msb-language li.active a";
            var langExists = await JsElementExists(langId, 2);
            if (!langExists)
            {
                LogError($"Error language not exists, {langId}");
                return false;
            }

            var iTry = 0;
            var iTryMax = 5;

            while (true)
            {
                iTry++;
                // a data-lang-id="vi_VN" Tiếng Việt
                // a data-lang-id="en_US" English, US

                var langText = await Browser.JsGetElementInnerText(langId, 2);
                if (langText.VietnameseToEnglish().ToLower() == "English, US".VietnameseToEnglish().ToLower())
                {
                    SelectedLang = Lang.English;
                }
                else if (langText.VietnameseToEnglish().ToLower() == "Tiếng Việt".VietnameseToEnglish().ToLower())
                {
                    SelectedLang = Lang.Vietnamese;
                }
                else
                {
                    LogError($"Error unexpected language <{langText}>, {langId}");
                    return false;
                }

                if (iTry > iTryMax) break;

                if (SelectedLang != selectLang)
                {
                    // Click Lang / Switch Lang
                    var enId = ".msb-language li a[data-lang-id=\"en_US\"]";
                    var vnId = ".msb-language li a[data-lang-id=\"vi_VN\"]";

                    var clickId = SelectedLang == Lang.English ? vnId : enId;

                    var clickLang = await Browser.JsClickElement(clickId, 2);
                    if (!await WaitAsync(2, 1)) return false;
                    if (!clickLang)
                    {
                        LogError($"Error click language, {clickId}, {iTry}/{iTryMax}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Click language, {clickId}, {iTry}/{iTryMax}");
                    }
                }
                else break;
            }

            if (SelectedLang != selectLang)
            {
                LogError($"Error select language <{selectLang}>");
                return false;
            }
            LogInfo($"Selected language <{SelectedLang}>");

            return true;
        }

        private async Task<bool> MsbManualLoginAsync()
        {
            var headerOffset = await Browser.JsGetElementOffsetHeight(".msb-container-header", 1);

            var result = false;

            DoLockBrowser?.Invoke(this, false);
            var captchaId = "input[name=\"_verifyCode\"]";
            var captchaExists = await JsElementExists(captchaId, 1);
            if (captchaExists)
            {
                var clickCaptcha = await Browser.MouseClickEventJsScroll(Bank, captchaId, headerOffset.StrToInt(), 1, Dpi);
            }

            var iMaxWait = 25;
            var stopWatch = Stopwatch.StartNew();
            while (true)
            {
                var isLoginExists = await IsLoginExistsAsync(Bank);

                if (!isLoginExists)
                {
                    result = true;
                    break;
                }
                else if (stopWatch.Elapsed.TotalSeconds > iMaxWait)
                {
                    result = false;
                    LogError($"Manual login timeout, {stopWatch.Elapsed.TotalSeconds.DoubleToInt()}/{iMaxWait}");
                    break;
                }

                DoSetCountDown?.Invoke(this, iMaxWait - stopWatch.Elapsed.TotalSeconds.DoubleToInt());
                LogInfo($"Please input Captcha, {stopWatch.Elapsed.TotalSeconds.DoubleToInt()}/{iMaxWait} ...");
                await Task.Delay(1000);
            }
            stopWatch.Stop();
            DoResetCountDown?.Invoke(this, true);
            DoLockBrowser?.Invoke(this, true);

            await WaitAsync(2, 1);

            return result;
        }

        private async Task<bool> MsbLoginAsync(string username, string password)
        {
            MyStopWatch sWatch;

            sWatch = new MyStopWatch(true);
            var setLogin = await SetLoginAsync(Bank, username, password);
            MsbReport.InputLogin = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!setLogin)
            {
                LogError($"Error set login");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var manualLogin = await MsbManualLoginAsync();
            MsbReport.ManualLogin = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!manualLogin) return false;

            sWatch = new MyStopWatch(true);
            var isLoggedIn = await IsLoggedInAsync(Bank, 5); // Wait longer time | White screen before content loaded
            MsbReport.IsLoggedIn2 = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!isLoggedIn)
            {
                LogError($"Error not logged in");
                return false;
            }

            return true;
        }

        private async Task<bool> MsbGoTransferScreenAsync(TransferType transType)
        {
            var eleId = transType == TransferType.Internal ?
                ".msb-container-header ul.msb-submenu a[onclick*=\"retailInternalTransferProc\"]" :
                ".msb-container-header ul.msb-submenu a[onclick*=\"retailInterbankTransferProc\"]";
            var eleExists = await JsElementExists(eleId, 2);
            if (!eleExists)
            {
                LogError($"Error transfer link not exists, {eleId}");
                return false;
            }

            var iTry = 0;
            var iTryMax = 3;
            while (iTry < iTryMax)
            {
                var clickEle = await Browser.JsClickElement(eleId, 2);
                if (!await WaitAsync(2, 1)) return false;
                if (!clickEle)
                {
                    LogError($"Error click transfer link, {eleId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click transfer link, {eleId}");
                }

                if (await IsPage(Bank, transType.ToCommonPage()))
                    break;

                iTry++;
                await Task.Delay(500);
            }

            var balance = await GetBalanceAsync(Bank, transType.ToCommonPage());
            await ShowBalance2Async(balance);

            return true;
        }

        private async Task<bool> MsbRefreshAsync(string username, string password, int mode = 0)
        {
            //
            // mode 
            // 0 : do not go transfer screen
            // 1 : refresh then go transfer screen
            //

            MyStopWatch sWatch;

            sWatch = new MyStopWatch(true);
            var goHome = await GoHomePage(Bank);
            MsbReport.GoHome = (int)sWatch.Elapsed(true)?.TotalSeconds;

            await MsbHandleTimeoutPageAsync();

            sWatch = new MyStopWatch(true);
            var isLoggedIn = await IsLoggedInAsync(Bank);
            MsbReport.IsLoggedIn = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!isLoggedIn)
            {
                sWatch = new MyStopWatch(true);
                var lang = await MsbGetLanguageAsync();
                MsbReport.GetLang = (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!lang)
                {
                    LogError($"Error get language");
                    return false;
                }

                var login = await MsbLoginAsync(username, password);
                if (!login)
                {
                    LogError($"Error login");
                    return false;
                }
            }

            if (mode == 1)
            {
                sWatch = new MyStopWatch(true);
                var goInternalTransfer = await MsbGoTransferScreenAsync(TransferType.Internal);
                MsbReport.GoTransfer = (int)sWatch.Elapsed(true)?.TotalSeconds;
            }

            return true;
        }

        private async Task<bool> MsbInputTransferAsync(TransferType transType, MsbExtBank bank, string accNum, string accName, decimal amount, string remarks)
        {
            var headerOffset = await Browser.JsGetElementOffsetHeight(".msb-container-header", 1);

            if (!await IsPage(Bank, transType.ToCommonPage()))
            {
                LogError($"Error, unexpected page");
                return false;
            }

            // Input To | Enter beneficiary account number | TAB
            if (1 == 1)
            {
                var accNumId = "input#toBenefitAcc";
                var accNumExists = await JsElementExists(accNumId, 2);
                if (!accNumExists)
                {
                    LogError($"Error beneficiary account number input not exists, {accNumId}");
                    return false;
                }
                var inputAccNum = await Browser.InputToElementJsScroll(Bank, accNumId, accNum, headerOffset.StrToInt(), 2, Dpi, true, true);
                if (!inputAccNum)
                {
                    LogError($"Error input beneficiary account number <{accNum}>, {accNumId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input beneficiary account number <{accNum}>, {accNumId}");
                }
            }

            if (transType == TransferType.External)
            {
                // Input To | Enter beneficiary bank
                if (1 == 1)
                {
                    var bankNameId = "input#autocomplete-bank";
                    var bankNameExists = await JsElementExists(bankNameId, 2);
                    if (!bankNameExists)
                    {
                        LogError($"Error beneficiary bank input not exists, {bankNameId}");
                        return false;
                    }
                    var inputBankName = await Browser.JsInputElementValue(bankNameId, bank.GetMsbBankName(), 2);
                    if (!inputBankName)
                    {
                        LogError($"Errror input beneficiary bank <{bank.GetMsbBankName()}>, {bankNameId}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Input beneficiary bank <{bank.GetMsbBankName()}>, {bankNameId}");
                    }
                }

                // Click Inquiry
                if (1 == 1)
                {
                    var inquiryId = "button#check-account";
                    var inquiryExists = await JsElementExists(inquiryId, 2);
                    if (!inquiryExists)
                    {
                        LogError($"Error Inquiry button not exists, {inquiryId}");
                        return false;
                    }
                    var clickInquiry = await Browser.JsClickElement(inquiryId, 2);
                    if (!await WaitAsync(2, 1)) return false;
                    if (!clickInquiry)
                    {
                        LogError($"Error click Inquiry button, {inquiryId}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Click Inquiry button, {inquiryId}");
                    }
                }
            }

            // Check beneficiary bank error | Input error | Wait 3 seconds
            if (1 == 1)
            {
                var errText = await Browser.JsMsbGetErrMsgAtTransPage(3);
                if (!string.IsNullOrWhiteSpace(errText))
                {
                    LogError($"Error beneficiary bank <{errText}>");
                    return false;
                }
            }

            // Match beneficiary name | Wait 10 seconds
            if (1 == 1)
            {
                var benNameId = transType == TransferType.Internal ?
                    "#lblBeneficaryName" :
                    "input#beneficaryName:not(.msb-hidden)";
                var benNameExists = await JsElementExists(benNameId, 10);
                if (!benNameExists)
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"受益人无效";
                    LogError($"Error beneficiary name not exists, {benNameId}");
                    return false;
                }
                var benName = transType == TransferType.Internal ?
                    await Browser.JsGetElementInnerText(benNameId, 10) :
                    await Browser.JsGetInputValue(benNameId, 10);
                if (benName != null && benName.ToBeneficiaryNameCompare() == accName.ToBeneficiaryNameCompare())
                {
                    LogInfo($"Beneficiary name is <{benName}>, {benNameId}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"收款人姓名不符: {accName}, 订单姓名: {benName}";
                    LogError($"Error beneficiary name not match <{benName}>, <{accName}>, {benNameId}");
                    return false;
                }
            }

            // Check available balance
            if (1 == 1)
            {
                var accBal = await GetBalanceAsync(Bank, transType.ToCommonPage());
                if (accBal != null && accBal.ToNumber(Bank).StrToDec() >= amount)
                {
                    BalanceAfterTransfer = accBal.ToNumber(Bank).StrToDec() - amount;
                    LogInfo($"Balance is <{accBal}>, {amount}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"余额不足: <{accBal}>, 转账金额: {amount}";
                    LogError($"Error insufficient balance <{accBal}>, {amount}");
                    return false;
                }
            }

            // Input Amount
            if (1 == 1)
            {
                var amountId = "#amount";
                var amountExists = await JsElementExists(amountId, 2);
                if (!amountExists)
                {
                    LogError($"Error Amount input not exists, {amountId}");
                    return false;
                }
                var inputAmount = await Browser.JsInputElementValue(amountId, amount.ToString(), 2);
                if (!inputAmount)
                {
                    LogError($"Error input Amount <{amount}>, {amountId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Amount <{amount}>, {amountId}");
                }
            }

            // Input Remark
            if (1 == 1)
            {
                var descId = "#purpose";
                var descExists = await JsElementExists(descId, 2);
                if (!descExists)
                {
                    LogError($"Error Remark input not exists, {descId}");
                    return false;
                }
                var inputDesc = await Browser.JsInputElementValue(descId, remarks, 2);
                if (!inputDesc)
                {
                    LogError($"Error input Remark <{remarks}>, {descId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Remark <{remarks}>, {descId}");
                }
            }

            // Click Continue
            if (1 == 1)
            {
                var submitId = "form .step-two button.msb-button-confirm";
                var submitExists = await JsElementExists(submitId, 2);
                if (!submitExists)
                {
                    LogError($"Error Continue button not exists, {submitId}");
                    return false;
                }
                var clickSubmit = await Browser.JsClickElement(submitId, 2);
                if (!await WaitAsync(2, 1)) return false;
                if (!clickSubmit)
                {
                    LogError($"Error click Continue button, {submitId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Continue button, {submitId}");
                }
            }

            // Check input error
            if (1 == 1)
            {
                var errText = await Browser.JsMsbGetErrMsgAtTransPage(2);
                if (!string.IsNullOrWhiteSpace(errText))
                {
                    LogError($"Error input <{errText}>");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> MsbTransferAuthenticationAsync(int deviceId, string orderId)
        {
            // Wait Transaction code loaded | Wait 15 seconds
            var transCode = string.Empty;
            if (1 == 1)
            {
                var transCodeId = "#lblTransactionCode";
                var transCodeExists = await JsElementExists(transCodeId, 15);
                if (!transCodeExists)
                {
                    LogError($"Error Transaction code not exists, {transCodeId}");
                    return false;
                }

                var tCode = await Browser.JsGetElementInnerText(transCodeId, 2);
                if (tCode.GetDigitOnly().Length == 6)
                {
                    transCode = tCode;
                }
                else
                {
                    LogError($"Invalid Transaction code <{tCode}>, {transCodeId}");
                    return false;
                }
            }

            // API - Send OTP Request
            if (1 == 1)
            {
                if (!IsDebug)
                {
                    var sendOtpReq = await CreateOrderOtp(deviceId, orderId, transCode);
                    if (!sendOtpReq)
                    {
                        LogError($"Error CreateOrderOtp <{transCode}>");
                        return false;
                    }
                    else
                    {
                        LogInfo($"CreateOrderOtp <{transCode}>");
                    }
                }
                else
                {
                    LogInfo($"This is debug mode, no API call will be issued.");
                }
            }

            // Input OTP | Retrieve OTP via API | Wait 90 seconds
            if (1 == 1)
            {
                var otpId = $"input#softTokenInput";
                var otpExists = await JsElementExists(otpId, 2);
                if (!otpExists)
                {
                    LogError($"Error OTP input not exists, {otpId}");
                    return false;
                }

                var scrollToOtp = await Browser.JsScrollToElement(Bank, otpId, 1, 1);

                var iTryOtp = 1;
                var iMaxTryOtp = 3;
                var authResult = true;
                string? otp = null;
                DateTime? otpAt = DateTime.MinValue;
                var waitOtp = 90;
                var otpWatch = new MyStopWatch(true);
                while (true)
                {
                    otp = "12312312"; // Default OTP for debug mode, during wait time, manual enter correct OTP
                    OrderOtpModel? otpResult = null;
                    if (!IsDebug)
                    {
                        otpResult = await GetOrderOtp(deviceId, orderId); // to pass transCode
                        otp = otpResult?.Otp;
                    }

                    if (!string.IsNullOrWhiteSpace(otp) &&
                        IsDebug ? true : otpResult?.UpdateTime != otpAt) // Skip condition for debug mode
                    {
                        if (otp.Length != 8)
                        {
                            LogError($"Error OTP is invalid <{otp}>");
                            authResult = false; break;
                        }

                        if (!IsDebug) otpAt = otpResult?.UpdateTime; // Skip for debug mode

                        var inputOtp = await Browser.JsInputElementValue(otpId, otp, 2);
                        if (!inputOtp)
                        {
                            LogError($"Error input OTP <{otp}>, {otpId}");
                            authResult = false; break;
                        }
                        else
                        {
                            LogInfo($"Input OTP <{otp}>");
                        }

                        var submitId = "button#transferInternalConfirmButton";
                        var submitExists = await JsElementExists(submitId, 2);
                        if (!submitExists)
                        {
                            LogError($"Error Continue button not exists, {submitId}");
                            authResult = false; break;
                        }

                        if (IsDebug) { await Task.Delay(1000 * 10); } // Debug mode, allow manual input correct OTP

                        iTryOtp++;
                        var clickSubmit = await Browser.JsClickElement(submitId, 2);
                        if (!await WaitAsync(2, 1)) { authResult = false; break; }
                        if (!clickSubmit)
                        {
                            LogError($"Error click Continue button, {submitId}");
                            authResult = false; break;
                        }
                        else
                        {
                            LogInfo($"Click Continue button, {submitId}");
                        }

                        // Check has error for wrong OTP | Retry for 3 time | Wait 3 seconds make sure no error
                        var errText = await Browser.JsMsbGetErrMsgAtTransPage(3);
                        if (!string.IsNullOrWhiteSpace(errText))
                        {
                            LogError($"Error enter OTP <{errText}>");
                            authResult = false;
                        }

                        // Check is Finish page? | Finish page then quit, else continue try
                        var pageId = ".result-view-final.active";
                        var pageExists = await JsElementExists(pageId, 2);
                        if (pageExists)
                        {
                            authResult = true; break;
                        }
                    }

                    if (iTryOtp > iMaxTryOtp) break;

                    if (otpWatch.Elapsed()?.TotalSeconds.DoubleToInt() > waitOtp)
                        break;

                    DoSetCountDown?.Invoke(this, waitOtp - (int)otpWatch.Elapsed()?.TotalSeconds.DoubleToInt());
                    LogInfo($"Retrieving OTP, {iTryOtp}/{iMaxTryOtp}, {otpWatch.Elapsed()?.TotalSeconds.DoubleToInt()}/{waitOtp} ...");
                    await Task.Delay(1000);
                }

                DoResetCountDown?.Invoke(this, true);
                if (!authResult) return false;
            }

            return true;
        }

        private async Task<bool> MsbTransferStatusAsync(int deviceId, string orderId)
        {
            // Wait Finish page loaded | Wait 15 seconds
            var pageId = ".result-view-final.active";
            var pageExists = await JsElementExists(pageId, 15);
            if (!pageExists)
            {
                LogError($"Error Finish page not loaded, {pageId}");
                return false;
            }

            // Finish with error
            var failedId = ".result-view-final.active #errorDetail";
            var failedExists = await JsElementExists(failedId, 2);
            if (failedExists)
            {
                var failedText = await Browser.JsGetElementInnerText(failedId, 2);
                LogError($"Error <{failedText}>, {failedId}");
                return false;
            }

            // Finish with success
            var successId = ".result-view-final.active #successDetail";
            var successExists = await JsElementExists(successId, 2);
            if (!successExists)
            {
                LogError($"Error not success, {successId}");
                return false;
            }
            else
            {
                var successText = await Browser.JsGetElementInnerText(successId, 2);
                LogInfo($"Transfer success <{successText}>, {successId}");
            }

            if (!IsDebug)
            {
                // Update Order Otp = 1
                var updateOtp = await UpdateOtptatus(deviceId, orderId, 1);
                if (!updateOtp)
                {
                    LogError($"Error Update Otptatus <1>");
                }
                else
                {
                    LogInfo("Update Otptatus <1>");
                }
            }

            LogInfo($"Transfer success, {successId}");

            TransferInfo.Status = TransferStatus.Success;
            TransferInfo.TransactionId = TransferInfo.OrderNo;

            return true;
        }

        private async Task<bool> MsbTransferAsync(TransferType transType, string username, string password,
         MsbExtBank toBank, string toAccNum, string toAccName, decimal toAmount, string remarks, int deviceId, string orderId)
        {
            MyStopWatch sWatch;

            var refresh = await MsbRefreshAsync(username, password);
            if (!refresh) return false;

            sWatch = new MyStopWatch(true);
            var goTransferScreen = await MsbGoTransferScreenAsync(transType);
            MsbReport.GoTransfer = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!goTransferScreen)
            {
                LogError($"Error go transfer screen");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var inputTransfer = await MsbInputTransferAsync(transType, toBank, toAccNum, toAccName, toAmount, remarks);
            MsbReport.InputTransfer = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!inputTransfer)
            {
                LogError($"Error input transfer");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var authTransfer = await MsbTransferAuthenticationAsync(deviceId, orderId);
            MsbReport.TransferAuth = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!authTransfer)
            {
                LogError($"Error authenticate transfer");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var transferSuccess = await MsbTransferStatusAsync(deviceId, orderId);
            MsbReport.TransferStatus = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!transferSuccess)
            {
                LogError($"Error transfer");
                return false;
            }

            return true;
        }

        #endregion

        #region SEAB

        public async Task SeaBankTestAsync()
        {
            try
            {
                var doTransfer = false;
                var blnTransfer = false;

                Bank = Bank.SeaBank;
                ////External Transfer
                //var transInfo = new TransferInfoModel()
                //{
                //    UserName = "1758067500",
                //    Password = "Win168168@",
                //    CardNo = "0338314760",
                //    ToBank = "vietcombank",
                //    ToAccountNumber = "1042306840",
                //    ToAccountName = "NGUYEN VAN HUY",
                //    ToAmount = 10000,
                //    ToRemarks = "10000",
                //    OrderNo = "[ORDERNO]",

                //    DeviceId = 8,
                //    OrderId = "0",
                //};

                // Internal Transfer
                var transInfo = new TransferInfoModel()
                {
                    UserName = "1758067500",
                    Password = "Win168168@",
                    CardNo = "0338314760",
                    ToBank = "seabank",
                    ToAccountNumber = "1042306840", // invalid to acc
                    ToAccountName = "NGUYEN VAN HUY",
                    ToAmount = 10000,
                    ToRemarks = "10000",
                    OrderNo = "[ORDERNO]",

                    DeviceId = 8,
                    OrderId = "0",
                };

                if (doTransfer)
                {
                    blnTransfer = true;
                    LogInfo($"Start test transfer, {JsonConvert.SerializeObject(transInfo)}");
                    //LogInfo($"Start test transfer, {Bank}, {OrderNo}, {UserName}, {CardNo}, {ToBank}, {ToAccountNumber}, {ToAccountName}, {ToAmount}, {ToRemarks}");
                    var transferWatch = Stopwatch.StartNew();
                    var result = await TransferAsync(transInfo);
                    FirstTime = false;
                    transferWatch.Stop();
                    LogInfo($"End test transfer. Total time: {transferWatch.ElapsedMilliseconds.MilisecondsToTimeTaken()}");
                    LogInfo($"Report statistic|{transferWatch.Elapsed.TotalSeconds.To3Decimal()}|{ReportStatistic}");
                    RefreshInterval = 0;
                }

                // Refresh if not performing transfer and first launch
                // Refresh if not performing transfer each 3 minutes (180 seconds)
                if (!blnTransfer)
                {
                    if (FirstTime || RefreshInterval >= RefreshTime)
                    {
                        LogInfo($"Start refresh... {RefreshInterval}");
                        // Refresh Browser
                        var refresh = await RefreshAsync(transInfo);
                        LogInfo($"End refresh... {RefreshInterval}");
                        LogInfo($"Report statistic||{ReportStatistic}");
                        if (FirstTime && !refresh)
                        {
                            RefreshInterval = RefreshTime; // Quickly trigger another refresh if first time login failed
                        }
                        else
                        {
                            RefreshInterval = 0;
                        }
                        FirstTime = false;
                    }
                    else
                    {
                        LogInfo($"Next refresh at {RefreshInterval}/{RefreshTime}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error, {ex}", $"Error, {ex.Message}");
            }
        }

        private async Task<bool> SeaGetLanguageAsync()
        {
            SelectedLang = Lang.Unknown;

            //var selectLang = Lang.Vietnamese; // test
            // Set to EN on debug mode
            var selectLang = IsDebug ? Lang.English : Lang.Vietnamese;

            var langId = "kt-language-selector #imgLang img";
            var langExists = await JsElementExists(langId, 2);
            if (!langExists)
            {
                LogError($"Error language not exists, {langId}");
                return false;
            }

            var iTry = 0;
            var iTryMax = 5;

            while (true)
            {
                iTry++;
                // src="./assets/media/flags/260-united-kingdom.svg"
                // src="./assets/media/flags/220-vietnam.svg"

                var enId = "kt-language-selector #imgLang img[src*=\"united-kingdom\"]";
                var vnId = "kt-language-selector #imgLang img[src*=\"vietnam\"]";

                if (await JsElementExists(enId, 2))
                {
                    SelectedLang = Lang.Vietnamese;
                }
                else if (await JsElementExists(vnId, 2))
                {
                    SelectedLang = Lang.English;
                }
                else
                {
                    LogError($"Error unexpected language, {langId}");
                    return false;
                }

                if (iTry > iTryMax) break;

                if (SelectedLang != selectLang)
                {
                    // Click Lang / Switch Lang

                    var clickLang = await Browser.JsClickElement(langId, 2);
                    if (!await WaitAsync(2, 1)) return false;
                    if (!clickLang)
                    {
                        LogError($"Error click language, {langId}, {iTry}/{iTryMax}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Click language, {langId}, {iTry}/{iTryMax}");
                    }
                }
                else break;
            }

            if (SelectedLang != selectLang)
            {
                LogError($"Error select language <{selectLang}>");
                return false;
            }
            LogInfo($"Selected language <{SelectedLang}>");

            return true;
        }

        private async Task<bool> SeaLoginAsync(string username, string password)
        {
            MyStopWatch sWatch;

            sWatch = new MyStopWatch(true);
            var setLogin = await SetLoginAsync(Bank, username, password);
            SeaReport.InputLogin = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!setLogin)
            {
                LogError($"Error set login");
                return false;
            }

            // Click Login
            if (1 == 1)
            {
                var loginId = "kt-login button.button-login";
                var loginExists = await JsElementExists(loginId, 2);
                if (!loginExists)
                {
                    LogError($"Error Login button not exists, {loginId}");
                    return false;
                }
                var loginText = await Browser.JsGetElementInnerText(loginId, 2);
                var clickLogin = await Browser.JsClickElement(loginId, 2);
                if (!await WaitAsync(2, 1)) return false;
                if (!clickLogin)
                {
                    LogError($"Error click <{loginText}>, {loginId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click <{loginText}>, {loginId}");
                }
            }

            sWatch = new MyStopWatch(true);
            var isLoggedIn = await IsLoggedInAsync(Bank, 5); // Wait longer time | White screen before content loaded
            SeaReport.IsLoggedIn2 = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!isLoggedIn)
            {
                LogError($"Error not logged in");
                return false;
            }

            return true;
        }

        private async Task<bool> SeaGoTransferScreenAsync(TransferType transType)
        {
            var eleId = "kt-aside-left ul li a[href*=\"ft-by-account-number\"]";
            var eleExists = await JsElementExists(eleId, 2);
            if (!eleExists)
            {
                LogError($"Error transfer link not exists, {eleId}");
                return false;
            }
            var clickEle = await Browser.JsClickElement(eleId, 2);
            // Available Balance table loading: sb-dynamic-table .kt-spinner
            if (!await WaitAsync(2, 1, "sb-dynamic-table .kt-spinner")) return false;
            if (!clickEle)
            {
                LogError($"Error click transfer link, {eleId}");
                return false;
            }
            else
            {
                LogInfo($"Click transfer link, {eleId}");
            }

            await ShowBalanceAsync(Bank);

            return true;
        }

        private async Task<bool> SeaRefreshAsync(string username, string password, int mode = 0)
        {
            //
            // mode 
            // 0 : do not go transfer screen
            // 1 : refresh then go transfer screen
            //

            MyStopWatch sWatch;

            sWatch = new MyStopWatch(true);
            var loadBank = await LoadBankAsync(Bank);
            SeaReport.LoadBank = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!loadBank)
            {
                LogError($"Error load bank url");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var isLoggedIn = await IsLoggedInAsync(Bank);
            SeaReport.IsLoggedIn = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!isLoggedIn)
            {
                sWatch = new MyStopWatch(true);
                var lang = await SeaGetLanguageAsync();
                SeaReport.GetLang = (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!lang)
                {
                    LogError($"Error get language");
                    return false;
                }

                var login = await SeaLoginAsync(username, password);
                if (!login)
                {
                    LogError($"Error login");
                    return false;
                }
            }

            if (mode == 1)
            {
                sWatch = new MyStopWatch(true);
                var goInternalTransfer = await SeaGoTransferScreenAsync(TransferType.Internal);
                SeaReport.GoTransfer = (int)sWatch.Elapsed(true)?.TotalSeconds;
            }

            return true;
        }

        private async Task<string?> SeaGetBalanceAsync()
        {
            // Balance on Internal and External transfer page
            var balId = "sb-dynamic-table span.text-red";
            return await Browser.JsGetElementInnerText(balId, 2);
        }

        private async Task<bool> SeaInputTransferAsync(TransferType transType, SeaExtBank bank, string accNum, string accName, decimal amount, string remarks)
        {
            var webHeader = await Browser.JsGetElementOffsetHeight("#kt_header_menu", 1);
            var mobileHeader = await Browser.JsGetElementOffsetHeight("#kt_header_mobile", 1);
            var headerOffset = Math.Max(webHeader.StrToInt(), mobileHeader.StrToInt());

            // Wait Bank list loaded
            if (!await WaitAsync(2, 1, "#bank .kt-spinner")) return false;

            // Click Bank drop down | Mouse click
            var searchId = "#bank ng-select input";
            if (1 == 1)
            {
                var searchExists = await JsElementExists(searchId, 2);
                if (!searchExists)
                {
                    LogError($"Error Bank dropdown not exists, {searchId}");
                    return false;
                }
                var clickSearch = await Browser.MouseClickEventJsScroll(Bank, searchId, headerOffset, 2, Dpi);
                if (!await WaitAsync(2, 1)) return false;
                if (!clickSearch)
                {
                    LogError($"Error click Bank dropdown, {searchId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Bank dropdown, {searchId}");
                }
            }

            // Skip search, the searching not based on the fullname, direct use Js click the bank img
            // Input bank name to Bank to search
            if (1 == 1)
            {
                //var inputBankName = await Browser.InputToElementJsScroll(Bank, searchId, bank.GetSeaBankName(SelectedLang), headerOffset, 2);
                //if (!await WaitAsync(2)) return false;
                //if (!inputBankName)
                //{
                //    LogError($"Error input bank name <{bank.GetSeaBankName(SelectedLang)}>, {searchId}");
                //    return false;
                //}
                //else
                //{
                //    LogInfo($"Input bank name <{bank.GetSeaBankName(SelectedLang)}>, {searchId}");
                //}
            }

            // Select Bank | Click
            if (1 == 1)
            {
                var bankCode = bank.GetSeaShortName();
                var bankNameId = $"#bank ng-select ng-dropdown-panel .ng-option img.logo-bank[src*=\"/logo-bank/b{bankCode}\"]";
                var bankNameExists = await JsElementExists(bankNameId, 5);
                if (!bankNameExists)
                {
                    TransferInfo.Status = TransferStatus.ErrorBank;
                    TransferInfo.ErrorMessage = $"银行无效: {bank.GetSeaBankName(SelectedLang)}";
                    LogError($"Error Bank name not exists <{bank.GetSeaBankName(SelectedLang)}>, {bankNameId}");
                    return false;
                }
                var clickBankName = await Browser.JsClickElement(bankNameId, 2);
                if (!await WaitAsync(2, 1)) return false;
                if (!clickBankName)
                {
                    LogError($"Error click Bank name <{bank.GetSeaBankName(SelectedLang)}>, {bankNameId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Bank name <{bank.GetSeaBankName(SelectedLang)}>, {bankNameId}");
                }
            }

            // Input Account received | TAB | Wait Recipient's name loaded
            if (1 == 1)
            {
                var accNumId = "input#accountCredit";
                var accNumExists = await JsElementExists(accNumId, 2);
                if (!accNumExists)
                {
                    LogError($"Error Account received input not exists, {accNumId}");
                    return false;
                }
                var inputAccNum = await Browser.InputToElementJsScroll(Bank, accNumId, accNum, headerOffset, 2, Dpi, true, true);
                if (!await WaitAsync(2, 1)) return false; // TAB will trigger Beneficiary name loading
                if (!inputAccNum)
                {
                    LogError($"Error input Account received <{accNum}>, {accNumId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Account received <{accNum}>, {accNumId}");
                }
            }

            // Check has error
            // Eg: Receiving account is invalid
            if (1 == 1)
            {
                var errId = "#toast-container .toast-error .toast-message";
                var errExists = await JsElementExists(errId, 3);
                if (errExists)
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"受益人无效";

                    var errText = await Browser.JsGetElementInnerText(errId, 1);
                    LogError($"Error Account received <{errText}>, {errId}");
                    return false;
                }
            }

            // Verify Recipient's name
            if (1 == 1)
            {
                var benNameId = "input#receiverName";
                var benExists = await JsElementExists(benNameId, 5);
                if (!benExists)
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"受益人无效";
                    LogError($"Error Recipient's name not exists, {benNameId}");
                    return false;
                }
                var benName = await Browser.JsGetInputValue(benNameId, 2);
                if (benName != null && benName.ToBeneficiaryNameCompare() == accName.ToBeneficiaryNameCompare())
                {
                    LogInfo($"Recipient's name is <{benName}>, {benNameId}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"收款人姓名不符: {accName}, 订单姓名: {benName}";
                    LogError($"Error Recipient's name not match <{benName}>, <{accName}>, {benNameId}");
                    return false;
                }
            }

            // Check Available balance
            if (1 == 1)
            {
                var accBal = await SeaGetBalanceAsync();
                if (accBal != null && accBal.ToNumber(Bank).StrToDec() >= amount)
                {
                    BalanceAfterTransfer = accBal.ToNumber(Bank).StrToDec() - amount;
                    LogInfo($"Balance is <{accBal}>, {amount}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"余额不足: <{accBal}>, 转账金额: {amount}";
                    LogError($"Error insufficient balance <{accBal}>, {amount}");
                    return false;
                }
            }

            // Input Amount
            if (1 == 1)
            {
                var amountId = "#amount input";
                var amountExists = await JsElementExists(amountId, 2);
                if (!amountExists)
                {
                    LogError($"Error Amount input not exists, {amountId}");
                    return false;
                }
                var inputAmount = await Browser.JsInputElementValue(amountId, amount.ToString(), 2);
                if (!inputAmount)
                {
                    LogError($"Error input Amount <{amount}>, {amountId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Amount <{amount}>, {amountId}");
                }
            }

            // Input Content | Description
            if (1 == 1)
            {
                var descId = "textarea#transDesc";
                var descExists = await JsElementExists(descId, 2);
                if (!descExists)
                {
                    LogError($"Error Content input not exists, {descId}");
                    return false;
                }
                var inputDesc = await Browser.JsInputElementValue(descId, remarks, 2);
                if (!inputDesc)
                {
                    LogError($"Error input Content <{remarks}>, {descId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Content <{remarks}>, {descId}");
                }
            }

            // Check has input error?
            if (1 == 1)
            {
                var errMsg = await Browser.JsSeaGetErrMsgAtTransPage(2);
                if (!string.IsNullOrWhiteSpace(errMsg))
                {
                    LogError($"Error input <{errMsg}>");
                    return false;
                }
            }

            // Check Button Next enabled? | Click Next
            if (1 == 1)
            {
                var submitId = "form#ftForm button[type=\"submit\"]";
                var submitEnabled = !await Browser.JsElementDisabled(submitId, 2);
                if (!submitEnabled)
                {
                    LogError($"Error Next button not enabled, {submitId}");
                    return false;
                }
                var clickSubmit = await Browser.JsClickElement(submitId, 2);
                if (!await WaitAsync(2, 1)) return false;
                if (!clickSubmit)
                {
                    LogError($"Error click Next button, {submitId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Next button, {submitId}");
                }
            }

            return true;
        }

        private async Task<bool> SeaConfirmTransferAsync()
        {
            // Wait confirm page loaded | Wait 15 seconds
            if (1 == 1)
            {
                var id = "kt-ft-account-confirm";
                var exists = await JsElementExists(id, 15);
                if (!exists)
                {
                    LogError($"Error Confirm page not loaded, {id}");
                    return false;
                }
            }

            // Wait Next enabled | Wait 15 seconds
            var submitId = "kt-ft-account-confirm button:not(.sb-button-exit)";
            if (1 == 1)
            {
                var submitExists = await JsElementExists(submitId, 2);
                if (!submitExists)
                {
                    LogError($"Error Next button not exists, {submitId}");
                    return false;
                }
                var submitEnabled = false;
                var sWatch = new MyStopWatch(true);
                var iMaxWait = 15;
                while (true)
                {
                    submitEnabled = !await Browser.JsElementDisabled(submitId, 2);
                    if (submitEnabled)
                    {
                        break;
                    }
                    else if (sWatch.Elapsed()?.TotalSeconds >= iMaxWait)
                    {
                        LogError($"Wait Next button enable timeout, {sWatch.Elapsed()?.TotalSeconds.DoubleToInt()}/{iMaxWait}");
                        break;
                    }

                    LogInfo($"Waiting Next button enable, {sWatch.Elapsed()?.TotalSeconds.DoubleToInt()}/{iMaxWait} ...");
                    await Task.Delay(1000);
                }
                sWatch.Stop();
                if (!submitEnabled)
                {
                    LogError($"Error Next button not enabled, {submitId}");
                    return false;
                }
            }

            // Click Next
            if (1 == 1)
            {
                var clickSubmit = await Browser.JsClickElement(submitId, 2);
                if (!await WaitAsync(2, 1)) return false;
                if (!clickSubmit)
                {
                    LogError($"Error click Next button, {submitId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Next button, {submitId}");
                }
            }

            return true;
        }

        private async Task<bool> SeaTransferAuthenticationAsync(int deviceId, string orderId)
        {
            // Wait OTP Transaction code loaded | Wait 15 seconds
            var transCodeId = "modal-otp div.sb-color";
            if (1 == 1)
            {
                var transCodeExists = await JsElementExists(transCodeId, 15);
                if (!transCodeExists)
                {
                    LogError($"Error OTP Transaction not exists, {transCodeId}");
                    return false;
                }
            }

            var transCode = string.Empty;
            // Wait OTP Transaction code with 6 digit loaded | Wait 15 seconds
            if (1 == 1)
            {
                MyStopWatch sWatch = new MyStopWatch(true);
                var iMaxWait = 15;
                while (true)
                {
                    var tCode = await Browser.JsGetElementInnerText(transCodeId, 2);
                    if (tCode.GetDigitOnly().Length == 6)
                    {
                        transCode = tCode;
                        break;
                    }
                    else if (sWatch.Elapsed()?.TotalSeconds >= iMaxWait)
                    {
                        LogError($"Loading OTP Transaction code timeout, {sWatch.Elapsed()?.TotalSeconds.DoubleToInt()}/{iMaxWait}");
                        break;
                    }

                    LogInfo($"Loading OTP Transaction code, {sWatch.Elapsed()?.TotalSeconds.DoubleToInt()}/{iMaxWait} ...");
                    await Task.Delay(1000);
                }
                sWatch.Stop();

                if (string.IsNullOrWhiteSpace(transCode))
                {
                    LogError($"Error retrieving OTP Transaction code");
                    return false;
                }
            }

            // API - Send OTP Request
            if (1 == 1)
            {
                if (!IsDebug)
                {
                    var sendOtpReq = await CreateOrderOtp(deviceId, orderId);
                    if (!sendOtpReq)
                    {
                        LogError($"Error CreateOrderOtp <{transCode}>");
                        return false;
                    }
                    else
                    {
                        LogInfo($"CreateOrderOtp <{transCode}>");
                    }
                }
                else
                {
                    LogInfo($"This is debug mode, no API call will be issued.");
                }
            }

            // Input OTP | Retrieve OTP via API | Wait 90 seconds
            if (1 == 1)
            {
                for (int i = 1; i <= 6; i++)
                {
                    var otpId = $".sb-otp-form input#input-otp-{i}"; // 1-6
                    var otpExists = await JsElementExists(otpId, 2);
                    if (!otpExists)
                    {
                        LogError($"Error OTP input not exists, {otpId}");
                        return false;
                    }
                }

                var iTryOtp = 1;
                var iMaxTryOtp = 3;
                var authResult = true;
                string? otp = null;
                DateTime? otpAt = DateTime.MinValue;
                var waitOtp = 90;
                var otpWatch = new MyStopWatch(true);
                while (true)
                {
                    otp = "123123"; // Default OTP for debug mode, during wait time, manual enter correct OTP
                    OrderOtpModel? otpResult = null;
                    if (!IsDebug)
                    {
                        otpResult = await GetOrderOtp(deviceId, orderId); // to pass transCode
                        otp = otpResult?.Otp;
                    }

                    if (!string.IsNullOrWhiteSpace(otp) &&
                        IsDebug ? true : otpResult?.UpdateTime != otpAt) // Skip condition for debug mode
                    {
                        if (otp.Length != 6)
                        {
                            LogError($"Error OTP is invalid <{otp}>");
                            authResult = false; break;
                        }

                        if (!IsDebug) otpAt = otpResult?.UpdateTime; // Skip for debug mode

                        var hasErrorInput = false;
                        for (int i = 1; i <= 6; i++)
                        {
                            var otpId = $".sb-otp-form input#input-otp-{i}"; // 1-6
                            var inputOtp = await Browser.JsInputElementValue(otpId, otp[i - 1].ToString(), 2);
                            if (!inputOtp)
                            {
                                LogError($"Error input OTP at <{otp[i - 1]}>, {otp}, {otpId}");
                                hasErrorInput = true; break;
                            }
                        }
                        if (hasErrorInput) { authResult = false; break; }
                        LogInfo($"Input OTP <{otp}>");

                        var submitId = "modal-otp button.button1";
                        var submitExists = await JsElementExists(submitId, 2);
                        if (!submitExists)
                        {
                            LogError($"Error Next button not exists, {submitId}");
                            authResult = false; break;
                        }
                        var submitEnabled = !await Browser.JsElementDisabled(submitId, 2);
                        if (!submitEnabled)
                        {
                            LogError($"Error Next button not enabled, {submitId}");
                            authResult = false; break;
                        }

                        if (IsDebug) { await Task.Delay(1000 * 30); } // Debug mode, allow manual input correct OTP

                        iTryOtp++;
                        var clickSubmit = await Browser.JsClickElement(submitId, 2);
                        if (!await WaitAsync(2, 1, "modal-otp button.button1 .kt-spinner")) { authResult = false; break; }
                        if (!clickSubmit)
                        {
                            LogError($"Error click Next button, {submitId}");
                            authResult = false; break;
                        }
                        else
                        {
                            LogInfo($"Click Next button, {submitId}");
                        }

                        // Check has error for wrong OTP | Retry for 3 time | Wait 3 seconds make sure no error
                        var errId = "#toast-container .toast-error .toast-message";
                        var errExists = await JsElementExists(errId, 3);
                        if (errExists)
                        {
                            var errText = await Browser.JsGetElementInnerText(errId, 1);
                            LogError($"Error confirm transfer <{errText}>, {errId}");
                            authResult = false;
                        }
                        else
                        {
                            authResult = true; break;
                        }
                    }

                    if (iTryOtp > iMaxTryOtp) break;

                    if (otpWatch.Elapsed()?.TotalSeconds.DoubleToInt() > waitOtp)
                        break;

                    DoSetCountDown?.Invoke(this, waitOtp - (int)otpWatch.Elapsed()?.TotalSeconds.DoubleToInt());
                    LogInfo($"Retrieving OTP, {iTryOtp}/{iMaxTryOtp}, {otpWatch.Elapsed()?.TotalSeconds.DoubleToInt()}/{waitOtp} ...");
                    await Task.Delay(1000);
                }

                DoResetCountDown?.Invoke(this, true);
                if (!authResult) return false;
            }

            return true;
        }

        private async Task<bool> SeaTransferStatusAsync(int deviceId, string orderId)
        {
            // Wait Transaction result page loaded | Wait 15 seconds
            var pageId = "kt-ft-account-confirm sb-transaction-result";
            var pageExists = await JsElementExists(pageId, 15);
            if (!pageExists)
            {
                LogError($"Error Transaction result page not loaded, {pageId}");
                return false;
            }

            // Check has error | Amount limit error (toast)
            // The transaction is only applicable for the amount from 10,000 VND. Contact details Hotline 190055587
            var errId = "#toast-container .toast-error .toast-message";
            var errExists = await JsElementExists(errId, 3);
            if (errExists)
            {
                var errText = await Browser.JsGetElementInnerText(errId, 1);
                LogError($"Error confirm transfer <{errText}>, {errId}");
                return false;
            }

            // Check has error | OTP error | others error
            // Transaction fail - Error in processing, please repeat the transaction. Sorry for the inconvenience
            var failId = "kt-ft-account-confirm sb-transaction-result .sb-msg-fail";
            var failExists = await JsElementExists(failId, 2);
            if (failExists)
            {
                var failMsgId = "kt-ft-account-confirm sb-transaction-result";
                var failMsg = await Browser.JsGetElementInnerText(failMsgId, 1);
                LogError($"Error confirm transfer <{failMsg}>, {failId}");
                return false;
            }

            // Check is success?
            // Transaction's successful
            var successId = "kt-ft-account-confirm sb-transaction-result .sb-msg-success";
            var successExists = await JsElementExists(successId, 2);
            if (!successExists)
            {
                LogError($"Error confirm transfer, not success, {successId}");
                return false;
            }

            if (!IsDebug)
            {
                // Update Order Otp = 1
                var updateOtp = await UpdateOtptatus(deviceId, orderId, 1);
                if (!updateOtp)
                {
                    LogError($"Error Update Otptatus <1>");
                }
                else
                {
                    LogInfo("Update Otptatus <1>");
                }
            }
            TransferInfo.Status = TransferStatus.Success;
            LogInfo($"Transfer success, {successId}");

            // Retrieve Reference code | First row in table, text on right
            var transId = "kt-ft-account-confirm sb-transaction-result table tr td.text-right";
            var transCode = await Browser.JsGetElementInnerText(transId, 1);
            if (transCode == null)
            {
                TransferInfo.TransactionId = TransferInfo.OrderNo;
                LogError($"Transaction id not exists, {transId}");
            }
            else
            {
                TransferInfo.TransactionId = transCode;
                LogInfo($"Transaction id is <{transCode}>");
            }

            return true;
        }

        private async Task<bool> SeaTransferAsync(TransferType transType, string username, string password,
           SeaExtBank toBank, string toAccNum, string toAccName, decimal toAmount, string remarks, int deviceId, string orderId)
        {
            MyStopWatch sWatch;

            var refresh = await SeaRefreshAsync(username, password);
            if (!refresh) return false;

            sWatch = new MyStopWatch(true);
            var goTransferScreen = await SeaGoTransferScreenAsync(transType);
            SeaReport.GoTransfer = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!goTransferScreen)
            {
                LogError($"Error go transfer screen");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var inputTransfer = await SeaInputTransferAsync(transType, toBank, toAccNum, toAccName, toAmount, remarks);
            SeaReport.InputTransfer = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!inputTransfer)
            {
                LogError($"Error input transfer");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var confirmTransfer = await SeaConfirmTransferAsync();
            SeaReport.ConfirmTransfer = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!confirmTransfer)
            {
                LogError($"Error confirm transfer");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var authTransfer = await SeaTransferAuthenticationAsync(deviceId, orderId);
            SeaReport.TransferAuth = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!authTransfer)
            {
                LogError($"Error authenticate transfer");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var transferSuccess = await SeaTransferStatusAsync(deviceId, orderId);
            SeaReport.TransferStatus = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!transferSuccess)
            {
                LogError($"Error transfer");
                return false;
            }

            return true;
        }

        #endregion

        #region TCB

        public async Task TcbTestAsync()
        {
            try
            {
                var doTransfer = false;
                var blnTransfer = false;

                ////External Transfer
                Bank = Bank.TcbBank;
                var transInfo = new TransferInfoModel()
                {
                    UserName = "0703805200",
                    Password = "Win168168@",
                    CardNo = "9703805200",
                    ToBank = "vietcombank",
                    ToAccountNumber = "1042306840",
                    ToAccountName = "NGUYEN VAN HUY",
                    ToAmount = 2000,
                    ToRemarks = "2000",
                    OrderNo = "[ORDERNO]",

                    DeviceId = 8,
                    OrderId = "0",
                };

                ////// Internal Transfer
                //var fromBank = Bank.VcbBank;
                //var user = "0966493205"; // "0966493205";
                //var password = "Win168168@";
                //var toBank = "VCB";
                //var toAccNum = "336639128"; // 3366391283 // 0984875872
                //var toAccName = "HA THI PHUONG HAO";
                //int toAmount = 2000; // int
                //var remarks = "2000";

                if (doTransfer)
                {
                    blnTransfer = true;
                    LogInfo($"Start test transfer, {JsonConvert.SerializeObject(transInfo)}");
                    //LogInfo($"Start test transfer, {Bank}, {OrderNo}, {UserName}, {CardNo}, {ToBank}, {ToAccountNumber}, {ToAccountName}, {ToAmount}, {ToRemarks}");
                    var transferWatch = Stopwatch.StartNew();
                    var result = await TransferAsync(transInfo);
                    FirstTime = false;
                    transferWatch.Stop();
                    LogInfo($"End test transfer. Total time: {transferWatch.ElapsedMilliseconds.MilisecondsToTimeTaken()}");
                    LogInfo($"Report statistic|{transferWatch.Elapsed.TotalSeconds.To3Decimal()}|{ReportStatistic}");
                    RefreshInterval = 0;
                }

                // Refresh if not performing transfer and first launch
                // Refresh if not performing transfer each 3 minutes (180 seconds)
                if (!blnTransfer)
                {
                    if (FirstTime || RefreshInterval >= RefreshTime)
                    {
                        LogInfo($"Start refresh... {RefreshInterval}");
                        // Refresh Browser
                        var refresh = await RefreshAsync(transInfo);
                        LogInfo($"End refresh... {RefreshInterval}");
                        //LogInfo($"Report statistic||{VcbReportText}");
                        if (FirstTime && !refresh)
                        {
                            RefreshInterval = RefreshTime; // Quickly trigger another refresh if first time login failed
                        }
                        else
                        {
                            RefreshInterval = 0;
                        }
                        FirstTime = false;
                    }
                    else
                    {
                        LogInfo($"Next refresh at {RefreshInterval}/{RefreshTime}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error, {ex}", $"Error, {ex.Message}");
            }
        }

        private async Task<bool> TcbGetLanguageAsync()
        {
            SelectedLang = Lang.Unknown;

            // Set to EN on debug mode
            // Text: Set Language to VN:
            var selectLangId = IsDebug ? ".locale-icon--EN" : ".locale-icon--VN"; // .locale-icon--EN .locale-icon--VN
            var selectLang = await Browser.JsClickElement(selectLangId, 2);
            if (!await WaitAsync(2, 1, ".app-loading")) return false;
            if (!selectLang)
            {
                LogError($"Error select language, {selectLang}");
                return false;
            }

            // Language exists?
            var langId = "#kc-locale-wrapper .selected-locale";
            var langExists = await JsElementExists(langId, 2);
            if (!langExists)
            {
                LogError($"Error language not exists, {langId}");
                return false;
            }
            var langValue = await Browser.JsGetElementInnerText(langId, 2);
            if (langValue == null)
            {
                LogError($"Error language empty, {langId}");
                return false;
            }
            if (langValue.ToUpper().Equals("EN"))
            {
                SelectedLang = Lang.English;
            }
            else if (langValue.ToUpper().Equals("VN"))
            {
                SelectedLang = Lang.Vietnamese;
            }
            else
            {
                LogError($"Invalid language, {langValue}, {langId}");
                return false;
            }
            LogInfo($"Language is <{langValue}>");

            return true;
        }

        private async Task<bool> TcbLoginAsync(string user, string password)
        {
            // Input User and Password
            var inputUserPass = await SetLoginAsync(Bank, user, password);
            if (!inputUserPass) return false;

            // Click Login
            if (1 == 1)
            {
                var loginId = "#kc-login";
                var loginExists = await JsElementExists(loginId, 2);
                if (!loginExists)
                {
                    LogError($"Login button not exists, {loginId}");
                    return false;
                }
                var loginValue = await Browser.JsGetInputValue(loginId, 2);
                var clickLogin = await Browser.JsClickElement(loginId, 2);
                if (!await WaitAsync(2, 1, ".app-loading")) return false;
                if (!clickLogin)
                {
                    LogError($"Error click <{loginValue}>, {loginId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click <{loginValue}>, {loginId}");
                }
            }

            // Skip error checking is success nagivated to next screen
            var otpId = "#bb-check-device-countdown-info";
            var otpExists = await JsElementExists(otpId, 2);
            if (otpExists)
            {
                LogInfo($"OTP screen loaded, skip errors checking, {otpId}");
                return true;
            }

            // Errors checking
            // User or password not filled up
            var inputErrors = await Browser.JsTcbGetInputLoginErrorMessage(1);
            if (!string.IsNullOrWhiteSpace(inputErrors))
            {
                LogError($"Error, <{inputErrors}>");
                return false;
            }

            //  Incorrect username or password
            var inputErrors2Id = "#ss-error-msg";
            var inputErrors2Exists = await JsElementExists(inputErrors2Id, 1);
            if (inputErrors2Exists)
            {
                //_loginSuccess = false;
                var inputErrors2 = await Browser.JsGetElementInnerText(inputErrors2Id, 1);
                LogError($"Error, <{inputErrors2}>, {inputErrors2Id}");
                return false;
            }

            // Credential not registered
            var notRegisteredId = "#kc-page-title";
            var notRegisteredExists = await JsElementExists(notRegisteredId, 1);
            if (notRegisteredExists)
            {
                var notRegisteredText = await Browser.JsGetElementInnerText(notRegisteredId, 1);
                if (notRegisteredText.ToLower().VietnameseToEnglish().Contains(TcbText.NotRegistered.TcbTranslate(SelectedLang).ToLower().VietnameseToEnglish()))
                {
                    //_loginSuccess = false;
                    LogError($"User is not registered <{user}>, {notRegisteredId}");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> TcbLoginAuthenticationAsync()
        {
            var countdownId = "#canvas-progress-bar-text";
            var countdownExists = await JsElementExists(countdownId, 15);
            if (!countdownExists)
            {
                LogError($"Login authentication not exists, timeout, {countdownId}");
                return false;
            }

            var isExpired = false;
            var loginRejected = false;
            var rejectedId = "#tc-login-denied"; // Rejected
            var notRejectedId = "#tc-login-denied.d-none"; // not Rejected
            var expiredId = "#bb-modal-header .tcb-timeout-icon";
            var resendId = "#bb-check-device-resend-form button";

            var authResult = true;
            var iTry = 0;
            var iTryMax = 2; // Max resend 2
            while (iTry < iTryMax)
            {
                var rejected = await JsElementExists(rejectedId, 1);
                var notRejected = await JsElementExists(notRejectedId, 1);
                loginRejected = rejected && !notRejected; // Found rejected and do not found .d-none => Rejected
                if (loginRejected)
                {
                    LogError($"Login rejected, {rejectedId}, {iTry + 1}/{iTryMax}");
                    authResult = false; break;
                }

                isExpired = await JsElementExists(expiredId, 1);
                if (isExpired)
                {
                    LogError($"Login session expired, {expiredId}, {iTry + 1}/{iTryMax}");
                    authResult = false; break;
                }

                countdownExists = await JsElementExists(countdownId, 1);
                if (!countdownExists)
                {
                    break;
                }
                else
                {
                    var timeout = await Browser.JsGetElementInnerText(countdownId, 1);
                    DoSetCountDown?.Invoke(this, timeout.StrToInt());
                    LogInfo($"Login authentication timeout <{timeout}>, {iTry + 1}/{iTryMax}");
                    if (timeout != null && timeout != "0")
                    {
                        await Task.Delay(1000);
                    }
                    else if (timeout != null && timeout == "0")
                    {
                        await Task.Delay(2000); // Wait for 2 second before resend again

                        var resendExists = await JsElementExists(resendId, 2);
                        if (!resendExists)
                        {
                            LogError($"Resend request button not exists, {resendId}, {iTry + 1}/{iTryMax}");
                            authResult = false; break;
                        }
                        var resendText = await Browser.JsGetElementInnerText(resendId, 1);
                        var clickResend = await Browser.JsClickElement(resendId, 2);
                        if (!await WaitAsync(2, 2, ".app-loading")) { authResult = false; break; }
                        if (clickResend)
                        {
                            LogInfo($"Click <{resendText}>, {resendId}, {iTry + 1}/{iTryMax}");
                            iTry++;
                        }
                        else
                        {
                            LogError($"Error click <{resendText}>, {resendId}, {iTry + 1}/{iTryMax}");
                            authResult = false; break;
                        }
                    }
                }
            }

            DoResetCountDown?.Invoke(this, true);
            if (!authResult) return false;

            if (!await WaitAsync(2, 1, ".app-loading")) return false;
            var isLoggedIn = await IsLoggedInAsync(Bank);
            if (!isLoggedIn)
                return false;

            return true;
        }

        private async Task<bool> TcbRefreshAsync(string username, string password)
        {
            MyStopWatch sWatch;

            sWatch = new MyStopWatch(true);
            var loadBank = await LoadBankAsync(Bank);
            TcbReport.LoadBank = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!loadBank)
            {
                LogError($"Error load bank url");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var isLoggedIn = await IsLoggedInAsync(Bank);
            TcbReport.IsLoggedIn = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!isLoggedIn)
            {
                sWatch = new MyStopWatch(true);
                var lang = await TcbGetLanguageAsync();
                TcbReport.GetLang = (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!lang)
                {
                    LogError($"Error get language");
                    return false;
                }

                sWatch = new MyStopWatch(true);
                var login = await TcbLoginAsync(username, password);
                TcbReport.Login = (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!login)
                {
                    LogError($"Error login");
                    return false;
                }

                sWatch = new MyStopWatch(true);
                var authenticate = await TcbLoginAuthenticationAsync();
                TcbReport.LoginAuth = (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!authenticate)
                {
                    LogError($"Error login authenticate");
                    return false;
                }
            }

            await ShowBalanceAsync(Bank);

            return true;
        }

        private async Task<bool> TcbGoTransferScreenAsync(TransferType transType)
        {
            var eleId = transType == TransferType.Internal ?
                "li.nav__item a[href=\"/transfers-payments/pay-someone?transferType=internal\"]" :
                "li.nav__item a[href=\"/transfers-payments/pay-someone?transferType=other\"]";
            var eleExists = await JsElementExists(eleId, 2);
            if (!eleExists)
            {
                LogError($"Error transfer link not exists, {eleId}");
                return false;
            }
            var clickEle = await Browser.JsClickElement(eleId, 2);
            if (!await WaitAsync(2, 1, ".ng-spinner-loader")) return false;
            if (!clickEle)
            {
                LogError($"Error click transfer link, {eleId}");
                return false;
            }
            else
            {
                LogInfo($"Click transfer link, {eleId}");
            }
            return true;
        }

        private async Task<bool> TcbInputTransferAsync(TransferType transType, TcbExtBank bank, string accNum, string accName, decimal amount, string remarks)
        {
            var headerOffset = await Browser.JsGetElementOffsetHeight(".tcb-page-layout__topbar", 1);

            if (transType == TransferType.External)
            {
                // Click Beneficiary bank
                if (1 == 1)
                {
                    var searchId = ".otherbank-transfer-wrapper .selector-input";
                    var searchExists = await JsElementExists(searchId, 2);
                    if (!searchExists)
                    {
                        LogError($"Beneficiary bank dropdown not exists, {searchId}");
                        return false;
                    }

                    var clickSearch = await Browser.MouseClickEventJsScroll(Bank, searchId, headerOffset.StrToInt(), 2, Dpi);
                    if (!await WaitAsync(2, 1, ".list-bank-dropdown .bb-loading-indicator")) return false;
                    if (!clickSearch)
                    {
                        LogError($"Error click Beneficiary bank dropdown, {searchId}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Click Beneficiary bank dropdown, {searchId}");
                    }
                }

                // Input Bank name to Select a bank to search
                if (1 == 1)
                {
                    var searchInputId = ".search-box input.search-box__input";
                    var searchInputExists = await JsElementExists(searchInputId, 10);
                    if (!searchInputExists)
                    {
                        LogError($"Search input not exists, timeout, {searchInputId}");
                        return false;
                    }

                    // Input Bank Name
                    var inputBankName = await Browser.JsInputElementValue(searchInputId, bank.GetTcbShortName(), 2);
                    if (!inputBankName)
                    {
                        LogError($"Error input bank name <{bank.GetTcbShortName()}>, {searchInputId}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Input bank name <{bank.GetTcbShortName()}>, {searchInputId}");
                    }
                    await Task.Delay(1000); // Wait input result reflected
                }

                // Select Beneficiary bank / Click
                if (1 == 1)
                {
                    // Bank ShortName exists?
                    var benBankPosition = await Browser.JsTcbGetBeneficiaryBankPosition(bank.GetTcbShortName(), 2);
                    if (benBankPosition == null)
                    {
                        TransferInfo.Status = TransferStatus.ErrorBank;
                        TransferInfo.ErrorMessage = $"银行无效: {bank.GetTcbShortName()}";
                        LogError($"Beneficiary bank not exists <{bank.GetTcbShortName()}>");
                        return false;
                    }
                    var iTry = 0;
                    var iTryMax = 3;
                    while (iTry < iTryMax)
                    {
                        iTry++;
                        await Task.Delay(1000);

                        var checkBenBankPosition = await Browser.JsTcbGetBeneficiaryBankPosition(bank.GetTcbShortName(), 1);
                        if (checkBenBankPosition == null || checkBenBankPosition == benBankPosition) break;
                        else if (checkBenBankPosition != benBankPosition)
                        {
                            benBankPosition = checkBenBankPosition;
                        }
                    }
                    var clickBenBankName = Browser.MouseClickJsPosition(benBankPosition, 2, Dpi);
                    if (!await WaitAsync(2, 1)) return false;
                    if (!clickBenBankName)
                    {
                        LogError($"Error click Beneficiary bank name <{bank.GetTcbShortName()}>");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Click Beneficiary bank name <{bank.GetTcbShortName()}>");
                    }
                }
            }

            // Input Account number, wait Beneficiary info (with retry)
            var amountId = transType == TransferType.Internal ?
                   "techcom-internal-transfer techcom-currency-input-with-suggestion input" :
                   "techcom-otherbank-transfer techcom-currency-input-with-suggestion input";
            var msgId = transType == TransferType.Internal ?
                  "techcom-internal-transfer textarea[formcontrolname=\"description\"]" :
                  "techcom-otherbank-transfer textarea[formcontrolname=\"description\"]";
            if (1 == 1)
            {
                var iMaxAccNum = 3;
                var iRetryAccNum = 0;
                var blnAccNum = true;
                while (true)
                {
                    // In case the error message throw - "Service temporarily interrupted. Please try again later.",
                    // retry input Account number again
                    iRetryAccNum++;
                    blnAccNum = true;
                    LogInfo($"Try input account number, {iRetryAccNum}/{iMaxAccNum}");

                    // Clear Acc Num if not empty
                    var clearAccNumId = transType == TransferType.Internal ?
                        "techcom-internal-transfer techcom-beneficiary-input button[tcbtracker=\"link_clear-input\"]" :
                        "techcom-otherbank-transfer techcom-otherbank-beneficiary-input button[tcbtracker=\"btn_clear-input\"]";
                    var clearAccNumExists = await JsElementExists(clearAccNumId, 1);
                    if (clearAccNumExists)
                    {
                        var clickClear = await Browser.JsClickElement(clearAccNumId, 1);
                        if (!clickClear)
                        {
                            LogError($"Error click Clear, {clearAccNumId}");
                            return false;
                        }
                        else
                        {
                            LogInfo($"Click Clear, {clearAccNumId}");

                            // Wait untill Account number cleared, max 2 seconds
                            var iTry = 0;
                            var iTryMax = 2;
                            while (true)
                            {
                                iTry++;
                                var checkAccNum = await Browser.JsGetInputValue(clearAccNumId, 1);
                                if (string.IsNullOrEmpty(checkAccNum)) break;
                                else await Task.Delay(1000);

                                if (iTry >= iTryMax) break;
                            }

                            // After clear, click on Amount
                            var clickAmountAfterClear = await Browser.MouseClickEventJsScroll(Bank, amountId, headerOffset.StrToInt(), 2, Dpi);
                            if (!await WaitAsync(2, 1, ".bb-loading-indicator")) return false;
                            if (!clickAmountAfterClear)
                            {
                                LogError($"Error click Amount after Clear, {amountId}");
                                return false;
                            }
                            else
                            {
                                LogInfo($"Click Amount after Clear, {amountId}");
                            }
                        }
                    }

                    // Enter Acc Num
                    var accNumId = transType == TransferType.Internal ?
                        "techcom-internal-transfer techcom-beneficiary-input input" :
                        "techcom-otherbank-transfer techcom-otherbank-beneficiary-input input";
                    var accNumExists = await JsElementExists(accNumId, 2);
                    if (!accNumExists)
                    {
                        LogError($"Account number input not exists, {accNumId}");
                        return false;
                    }

                    // Empty Acc Num
                    var emptyAccNum = await Browser.JsInputElementValue(accNumId, "", 1);
                    if (!emptyAccNum)
                    {
                        LogError($"Error empty Account number, {accNumId}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Empty Account number, {accNumId}");
                    }

                    // Input Acc Num 
                    var inputAccNum = await Browser.InputToElementJsScroll(Bank, accNumId, accNum, headerOffset.StrToInt(), 2, Dpi, true, true);
                    if (!inputAccNum)
                    {
                        LogError($"Error input Account number <{accNum}>, {accNumId}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Input Account number <{accNum}>, {accNumId}");
                    }

                    // Click on amount and wait Beneficiary show, else invalid Beneficiary acc num
                    var amountExists = await JsElementExists(amountId, 2);
                    if (!amountExists)
                    {
                        LogError($"Amount input not exists, {amountId}");
                        return false;
                    }
                    var clickAmount = await Browser.MouseClickEventJsScroll(Bank, amountId, headerOffset.StrToInt(), 2, Dpi);
                    if (!await WaitAsync(2, 1, ".bb-loading-indicator")) return false;
                    if (!clickAmount)
                    {
                        LogError($"Error click Amount, {amountId}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Click Amount, {amountId}");
                    }

                    // Check is invalid Beneficiary?
                    var invalidBenId = ".transfer-modal__message.typo-default-regular";
                    var invalidBenExists = await JsElementExists(invalidBenId, 2);
                    if (invalidBenExists)
                    {
                        // If Service unavailable, retry...

                        // Dịch vụ tạm thời gián đoạn. Quý khách vui lòng thử lại sau.
                        // The service is temporarily unavailable. Please try again later.

                        var invalidBenMsg = await Browser.JsGetElementInnerText(invalidBenId, 2);
                        if (invalidBenMsg?.Trim().ToLower().VietnameseToEnglish() == TcbText.ServiceUnavailable.TcbTranslate(SelectedLang).ToLower().VietnameseToEnglish())
                        {
                            blnAccNum = false;
                            LogError($"Service unavailable <{accNum}>, <{invalidBenMsg}>, {invalidBenId}, {iRetryAccNum}/{iMaxAccNum}");

                            var closeId = "tcb-transfer-bill-error-modal button[tcbtracker=\"btn_close\"]";
                            var closeExists = await JsElementExists(closeId, 1);
                            if (closeExists)
                            {
                                var clickClose = await Browser.JsClickElement(closeId, 2);
                                if (!clickClose)
                                {
                                    LogError($"Error click Close, {closeId}");
                                    return false;
                                }
                                else
                                {
                                    LogInfo($"Click Close, {closeId}");
                                }
                            }
                        }
                        else
                        {
                            TransferInfo.Status = TransferStatus.ErrorCard;
                            TransferInfo.ErrorMessage = $"卡号无效: {accNum}";
                            LogError($"Invalid Account number <{accNum}>, <{invalidBenMsg}>, {invalidBenId}");
                            return false;
                        }
                    }

                    if (blnAccNum) break; // Success, continue
                    else if (iRetryAccNum >= iMaxAccNum) // Failed,
                    {
                        LogError($"Try input account number reach maximum tries, {iRetryAccNum}/{iMaxAccNum}");
                        return false;
                    }
                }
            }

            // Check Beneficiary (Acc Name)
            if (1 == 1)
            {
                var benNameId = transType == TransferType.Internal ?
                    "techcom-internal-transfer .beneficiary-info input" :
                    "techcom-otherbank-transfer .beneficiary-info input";
                var benNameExists = await JsElementExists(benNameId, 10);
                if (!benNameExists)
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"受益人无效";
                    LogError($"Beneficiary name not exists, {benNameId}");
                    return false;
                }
                var benName = await Browser.JsGetInputValue(benNameId, 2);
                if (benName != null && benName.ToBeneficiaryNameCompare() == accName.ToBeneficiaryNameCompare())
                {
                    LogInfo($"Beneficiary name is <{benName}>, {benNameId}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"收款人姓名不符: {accName}, 订单姓名: {benName}";
                    LogError($"Beneficiary name not match <{benName}>, <{accName}>, {benNameId}");
                    return false;
                }
            }

            // Check Account Balance
            if (1 == 1)
            {
                var accBalId = ".account-balance__amount";
                var accBalExists = await JsElementExists(accBalId, 2);
                if (!accBalExists)
                {
                    LogError($"Balance not exists, {accBalId}");
                    return false;
                }
                var accBal = await Browser.JsGetElementInnerText(accBalId, 2);
                if (accBal != null && accBal.StrToDec() >= amount)
                {
                    BalanceAfterTransfer = accBal.StrToDec() - amount;
                    LogInfo($"Balance is <{accBal}>, {amount}, {accBalId}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"余额不足: <{accBal}>, 转账金额: {amount}";
                    LogError($"Error insufficient balance <{accBal}>, {amount}, {accBalId}");
                    return false;
                }
            }

            // Input Amount
            if (1 == 1)
            {
                var inputAmount = await Browser.JsInputElementValue(amountId, amount.ToString(), 2);
                if (!inputAmount)
                {
                    LogError($"Error input amount <{amount}>, {amountId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input amount <{amount}>, {amountId}");
                }
            }

            // Input Description
            if (1 == 1)
            {
                var msgExists = await JsElementExists(msgId, 2);
                if (!msgExists)
                {
                    LogError($"Message input not exists, {msgId}");
                    return false;
                }
                var inputMsg = await Browser.JsInputElementValue(msgId, remarks, 2);
                if (!inputMsg)
                {
                    LogError($"Error input Message <{remarks}>, {msgId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Message <{remarks}>, {msgId}");
                }
            }

            //// Click on amount again to triger validation
            //var clickAmount2 = await Browser.MouseClickEventJsScroll(amountId, headerOffset.StrToInt(), 2);
            //await Task.Delay(1000);
            //if (!clickAmount2)
            //{
            //    LogError($"Error click Amount, {amountId}");
            //    return false;
            //}
            //else
            //{
            //    LogInfo($"Click Amount, {amountId}");
            //}

            // Click Next // Need check next button is enabled //
            if (1 == 1)
            {
                var nextId = transType == TransferType.Internal ?
                    "techcom-internal-transfer bb-load-button-ui[tcbtracker=\"btn_next\"] button" :
                    "techcom-otherbank-transfer bb-load-button-ui[tcbtracker=\"btn_next\"] button";
                var nextExists = await JsElementExists(nextId, 2);
                if (!nextExists)
                {
                    LogError($"Next button not exists, {nextId}");
                    return false;
                }
                var nextEnabled = !await Browser.JsElementDisabled(nextId, 1);
                if (!nextEnabled)
                {
                    LogError($"Next button not enabled, {nextId}");
                    return false;
                }
                else
                {
                    LogInfo($"Next button enabled, {nextId}");
                }
                var clickNext = await Browser.JsClickElement(nextId, 2);
                if (!await WaitAsync(2, 1, "tcb-screen-loading")) return false; // Loading screen after click next
                if (!clickNext)
                {
                    LogError($"Error click Next, {nextId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Next, {nextId}");
                }
            }

            return true;
        }

        private async Task<bool> TcbConfirmTransferAsync()
        {
            // Wait confirm page loaded
            var confirmPageId = "techcom-transfer-confirmation";
            var confirmPageExists = await JsElementExists(confirmPageId, 15);
            if (!confirmPageExists)
            {
                LogError($"Confirm page not exists, timeout, {confirmPageId}");
                return false;
            }

            // Click Confirm
            var confirmId = "techcom-transfer-confirmation bb-load-button-ui[tcbtracker=\"btn_confirm\"]";
            var confirmExists = await JsElementExists(confirmId, 2);
            if (!confirmExists)
            {
                LogError($"Confirm button not exists, {confirmId}");
                return false;
            }
            var clickConfirm = await Browser.JsClickElement(confirmId, 2);
            if (!await WaitAsync(2, 1, "tcb-screen-loading")) return false;
            if (!clickConfirm)
            {
                LogError($"Error click confirm, {confirmId}");
                return false;
            }
            else
            {
                LogInfo($"Click confirm, {confirmId}");
            }

            return true;
        }

        private async Task<bool> TcbTransferAuthenticationAsync()
        {
            var rejectId = "techcom-transaction-signing-error";
            var isRejected = false;

            // Wait until authentication available
            var authId = "techcom-transaction-signing-oob-device";
            var authExists = await JsElementExists(authId, 10);
            if (!authExists)
            {
                // Posible user rejected before OTP detected by CEF on browser?
                isRejected = await JsElementExists(rejectId, 1);
                if (isRejected)
                {
                    LogInfo($"Transfer rejected, {rejectId}");
                    return false;
                }

                // Posible user approved before OTP detected by CEF on browser
                var successId = "techcom-transfer-successful";
                var isSuccess = await JsElementExists(successId, 1);
                if (isSuccess)
                {
                    LogInfo($"Transfer success, {successId}");
                    return true;
                }

                LogError($"Transfer authentication not exists, timeout, {authId}");
                CheckHistory = true;
                return true; // Proceed with check history
            }

            var authResult = true;
            var expiredId = "techcom-transfer-failure";
            var isExpired = false;
            var resendId = "techcom-transaction-signing-oob-device button[tcbtracker=\"btn_resend\"]";
            var timeoutId = "#base-timer-label";
            var resendExists = false;
            while (true)
            {
                isRejected = await JsElementExists(rejectId, 1);
                if (isRejected)
                {
                    LogInfo($"Transfer rejected, {rejectId}");
                    authResult = false; break;
                }

                isExpired = await JsElementExists(expiredId, 1);
                if (isExpired)
                {
                    LogInfo($"Transfer session expired, {expiredId}");
                    CheckHistory = true;
                    authResult = true; break; // Proceed with check history
                }

                resendExists = await JsElementExists(resendId, 1);
                if (resendExists)
                {
                    var resendDisabled = await Browser.JsElementDisabled(resendId, 1);
                    if (resendDisabled)
                    {
                        var timeout = await Browser.JsGetElementInnerText(timeoutId, 2);
                        DoSetCountDown?.Invoke(this, timeout.StrToInt());
                        LogInfo($"Transfer authentication timeout <{timeout}>");
                        await Task.Delay(1000);
                    }
                    else
                    {
                        LogInfo($"Transfer authentication timeout, {timeoutId}, {resendId}");
                        CheckHistory = true;
                        authResult = true; break; // Proceed with check history
                    }
                }
                else
                {
                    LogInfo($"Transfer authenticated, {timeoutId}");
                    //await WaitAsync(2);
                    CheckHistory = true;
                    authResult = true; break; // Proceed with check history
                }
            }

            DoResetCountDown?.Invoke(this, true);
            return authResult;
        }

        private async Task<bool> TcbCheckTransactionHistoryAsync(int iTry, string remarks)
        {
            var loadBank = await LoadBankAsync(Bank);
            if (!loadBank) return false;

            // Click to go Account Details
            var accountId = "techcom-account-summary-quick-view-widget .current-account__item";
            var accountExists = await JsElementExists(accountId, 5);
            if (!accountExists)
            {
                LogError($"{iTry}|Current account not exists, {accountId}");
                return false;
            }

            var headerOffset = await Browser.JsGetElementOffsetHeight(".tcb-page-layout__topbar", 1);

            var clickAccount = await Browser.MouseClickEventJsScroll(Bank, accountId, headerOffset.StrToInt(), 1, Dpi);
            if (!clickAccount)
            {
                LogError($"{iTry}|Error click current account, {accountId}");
                return false;
            }
            else
            {
                LogInfo($"{iTry}|Click current account, {accountId}");
            }
            if (!await WaitAsync(2, 1, "techcom-account-detail-widget-extended .bb-loading-indicator")) return false; // Wait Currenct Account Loading
            if (!await WaitAsync(2, 1, "techcom-account-transaction-history .bb-loading-indicator")) return false; // Wait Transaction History Loading

            var transListId = "techcom-account-transaction-history techcom-account-transaction-history-item"; // Transaction History Items (Column)
            var transListExists = await JsElementExists(transListId, 2);
            if (!transListExists)
            {
                LogError($"{iTry}|Transaction history not exists, {transListId}");
                return false;
            }
            var transCount = await Browser.JsGetElementCount(transListId, 1);
            if (transCount == null || transCount.StrToInt() <= 0)
            {
                LogError($"{iTry}|Transaction history has no record, {transListId}");
                return false;
            }
            else
            {
                LogInfo($"{iTry}|Transaction history has record <{transCount}>, {transListId}");
            }
            var clickFirst = await Browser.JsClickElementAtIndex(transListId, 0, 1);
            if (!await WaitAsync(1, 1)) return false;
            if (!clickFirst)
            {
                LogError($"{iTry}|Error click transaction history at index 0, {transListId}");
                return false;
            }
            else
            {
                LogInfo($"{iTry}|Click transaction history at index 0, {transListId}");
            }

            var transMessage = await Browser.JsGetTcbMessageFromTransHistory(SelectedLang, 2); // remarks
            if (transMessage == null || transMessage.Trim().ToLower() != remarks.Trim().ToLower())
            {
                LogError($"{iTry}|Transaction message not match <{transMessage}>, {remarks}");
                return false;
            }
            else
            {
                LogInfo($"{iTry}|Transaction message matched <{transMessage}>, {remarks}");
            }

            var transIdText = await Browser.JsGetTcbTransIdFromTransHistory(SelectedLang, 2);
            TransferInfo.TransactionId = transIdText;
            if (transIdText == null)
            {
                LogError($"{iTry}|Transaction id not exists");
            }
            else
            {
                LogInfo($"{iTry}|Transaction id is <{transIdText}>");
            }

            return true;
        }

        private async Task<bool> TcbTransferStatusAsync(int deviceId, string orderId)
        {
            var successId = "techcom-transfer-successful";
            var success = await JsElementExists(successId, 15);
            if (!success)
            {
                //// Check redis
                //var redisStatus = TcbGetTransferOrderStatus(_user);
                //LogInfo($"Redis status is <{redisStatus}>");
                //success = redisStatus == 1;
                //if (success)
                //{
                //    TcbRemoveTransferOrder(_user);

                //    Status = TransferStatus.Success;
                //    TransactionId = $"PH{_orderNo}";
                //    LogInfo($"Transfer success with redis");
                //    return true;
                //}
            }

            if (!success)
            {
                // Removed the check history logic
                // Replaced with call API for validation

                //// Transfer success not showing,
                //// Check transaction history
                //// Possible transfer success

                if (!CheckHistory)
                {
                    LogError($"Transfer failed, {successId}");
                    return false;
                }

                // CheckHistory changed to check API GetSuccessOrder
                if (!await GetSuccessOrder(deviceId, orderId))
                {
                    LogError($"Transfer failed, mobile order failed");
                    return false;
                }
                TransferInfo.TransactionId = TransferInfo.OrderNo;

                //var foundHistory = false;
                //var iTry = 0;
                //var iTryMax = 2;
                //while (iTry < iTryMax)
                //{
                //    iTry++;
                //    if (iTry != 1)
                //        await Task.Delay(1000 * 10);

                //    foundHistory = await TcbCheckTransactionHistoryAsync(iTry, remarks);
                //    if (foundHistory) break;
                //}
                //if (!foundHistory)
                //{
                //    LogError($"Transfer failed, transaction history not found.");
                //    return false;
                //}

                TransferInfo.Status = TransferStatus.Success;
                LogInfo($"Transfer success, with mobile order success");
                return true;
            }

            TransferInfo.Status = TransferStatus.Success;
            LogInfo($"Transfer success, {successId}");

            // Retrieve transaction id
            var transId = await Browser.JsGetTcbTransactionId(SelectedLang, 2);
            if (transId == null)
            {
                TransferInfo.TransactionId = TransferInfo.OrderNo;
                LogError($"Transaction id not exists");
            }
            else
            {
                TransferInfo.TransactionId = transId;
                LogInfo($"Transaction id is <{transId}>");
            }
            return true;
        }

        private async Task<bool> TcbTransferAsync(TransferType transType, string username, string password,
            TcbExtBank toBank, string toAccNum, string toAccName, decimal toAmount, string remarks, int deviceId, string orderId)
        {
            MyStopWatch sWatch;

            var refresh = await TcbRefreshAsync(username, password);
            if (!refresh) return false;

            sWatch = new MyStopWatch(true);
            var goTransferScreen = await TcbGoTransferScreenAsync(transType);
            TcbReport.GoTransfer = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!goTransferScreen)
            {
                LogError($"Error go transfer screen");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var inputTransfer = await TcbInputTransferAsync(transType, toBank, toAccNum, toAccName, toAmount, remarks);
            TcbReport.InputTransfer = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!inputTransfer)
            {
                LogError($"Error input transfer");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var confirmTransfer = await TcbConfirmTransferAsync();
            TcbReport.ConfirmTransfer = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!confirmTransfer)
            {
                LogError($"Error confirm transfer");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var authTransfer = await TcbTransferAuthenticationAsync();
            TcbReport.TransferAuth = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!authTransfer && !CheckHistory)
            {
                LogError($"Error authenticate transfer");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var transferSuccess = await TcbTransferStatusAsync(deviceId, orderId);
            TcbReport.TransferStatus = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!transferSuccess)
            {
                LogError($"Error transfer");
                return false;
            }

            return true;
        }

        #endregion

        #region VTB

        public async Task VtbTestAsync()
        {
            try
            {
                var doTransfer = false;
                var blnTransfer = false;

                //External Transfer
                Bank = Bank.VtbBank;
                var transInfo = new TransferInfoModel()
                {
                    UserName = "0368527514",
                    Password = "Winbet888999@!!",
                    CardNo = "103881849311",
                    ToBank = "vietcombank",
                    ToAccountNumber = "1042306840",
                    ToAccountName = "NGUYEN VAN HUY",
                    ToAmount = 2000,
                    ToRemarks = "2000",
                    OrderNo = "[ORDERNO]",

                    DeviceId = 8,
                    OrderId = "0",
                };

                //// Internal Transfer
                //Bank = Bank.VtbBank;
                //var transInfo = new TransferInfoModel()
                //{
                //    UserName = "0368527514",
                //    Password = "Winbet888999@!!",
                //    CardNo = "103881849311",
                //    ToBank = "vietinbank",
                //    ToAccountNumber = "100874806215",
                //    ToAccountName = "LE THI LAI",
                //    ToAmount = 2000,
                //    ToRemarks = "2000",
                //    OrderNo = "[ORDERNO]",

                //    DeviceId = 8,
                //    OrderId = "0",
                //};

                if (doTransfer)
                {
                    blnTransfer = true;
                    LogInfo($"Start test transfer, {JsonConvert.SerializeObject(transInfo)}");
                    var transferWatch = Stopwatch.StartNew();
                    var result = await TransferAsync(transInfo);
                    FirstTime = false;
                    transferWatch.Stop();
                    LogInfo($"End test transfer. Total time: {transferWatch.ElapsedMilliseconds.MilisecondsToTimeTaken()}");
                    LogInfo($"Report statistic|{transferWatch.Elapsed.TotalSeconds.To3Decimal()}|{ReportStatistic}");
                    RefreshInterval = 0;
                }

                // Refresh if not performing transfer and first launch
                // Refresh if not performing transfer each 3 minutes (180 seconds)
                if (!blnTransfer)
                {
                    if (FirstTime || RefreshInterval >= RefreshTime)
                    {
                        LogInfo($"Start refresh... {RefreshInterval}");
                        // Refresh Browser
                        var refresh = await RefreshAsync(transInfo);
                        LogInfo($"End refresh... {RefreshInterval}");
                        //LogInfo($"Report statistic||{VcbReportText}");
                        if (FirstTime && !refresh)
                        {
                            RefreshInterval = RefreshTime; // Quickly trigger another refresh if first time login failed
                        }
                        else
                        {
                            RefreshInterval = 0;
                        }
                        FirstTime = false;
                    }
                    else
                    {
                        LogInfo($"Next refresh at {RefreshInterval}/{RefreshTime}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error, {ex}", $"Error, {ex.Message}");
            }
        }

        private async Task<bool> VtbGetLanguageAsync()
        {
            SelectedLang = Lang.Unknown;

            //var selectLang = Lang.Vietnamese; // test
            // Set to EN on debug mode
            var selectLang = IsDebug ? Lang.English : Lang.Vietnamese;

            var hotlineId = "#app .app-container .main-container .auth-layout .auth-layout-header__end";
            var hotlineExists = await JsElementExists(hotlineId, 2);
            if (!hotlineExists)
            {
                LogError($"Error hotline text not exists, {hotlineId}");
                return false;
            }

            var langId = "#app .app-container .main-container .auth-layout .auth-layout-header__end .language-switch";
            var langExists = await JsElementExists(langId, 2);
            if (!langExists)
            {
                LogError($"Error language not exists, {langId}");
                return false;
            }

            var iTry = 0;
            var iTryMax = 5;

            while (true)
            {
                iTry++;
                // Hotline: 1900 558 868
                // Số điện thoại hỗ trợ: 1900 558 868

                var hotlineText = await Browser.JsGetElementInnerText(hotlineId, 2);
                if (hotlineText.Trim().VietnameseToEnglish().ToLower()
                    .StartsWith(VtbText.Hotline.VtbTranslate(Lang.English).VietnameseToEnglish().ToLower()))
                {
                    SelectedLang = Lang.English;
                }
                else if (hotlineText.Trim().VietnameseToEnglish().ToLower()
                    .StartsWith(VtbText.Hotline.VtbTranslate(Lang.Vietnamese).VietnameseToEnglish().ToLower()))
                {
                    SelectedLang = Lang.Vietnamese;
                }
                else
                {
                    LogError($"Error unexpected language <{hotlineText}>, {langId}");
                    return false;
                }

                if (iTry > iTryMax) break;

                if (SelectedLang != selectLang)
                {
                    // Click Lang / Switch Lang

                    var clickLang = await Browser.MouseClickEventJsScroll(Bank, langId, 1, 2, Dpi);
                    if (!await WaitAsync(2, 1)) return false;
                    if (!clickLang)
                    {
                        LogError($"Error click language, {langId}, {iTry}/{iTryMax}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Click language, {langId}, {iTry}/{iTryMax}");
                    }
                }
                else break;
            }

            if (SelectedLang != selectLang)
            {
                LogError($"Error select language <{selectLang}>");
                return false;
            }
            LogInfo($"Selected language <{SelectedLang}>");

            return true;
        }

        private async Task<bool> VtbManualLoginAsync()
        {
            var result = false;

            DoLockBrowser?.Invoke(this, false);

            var captchaId = "#app .app-container .main-container .auth-layout .login input[name=\"captchaCode\"]";
            var captchaExists = await JsElementExists(captchaId, 1);
            if (captchaExists)
            {
                var captchaScroll = await Browser.MouseClickEventJsScroll(Bank, captchaId, 1, 2, Dpi);
            }

            var iMaxWait = 25;
            var stopWatch = Stopwatch.StartNew();
            while (true)
            {
                var isLoginExists = await IsLoginExistsAsync(Bank);

                if (!isLoginExists)
                {
                    result = true;
                    break;
                }
                else if (stopWatch.Elapsed.TotalSeconds > iMaxWait)
                {
                    result = false;
                    LogError($"Manual login timeout, {stopWatch.Elapsed.TotalSeconds.DoubleToInt()}/{iMaxWait}");
                    break;
                }

                LogInfo($"Please input Captcha, {stopWatch.Elapsed.TotalSeconds.DoubleToInt()}/{iMaxWait} ...");
                DoSetCountDown?.Invoke(this, iMaxWait - stopWatch.Elapsed.TotalSeconds.DoubleToInt());
                await Task.Delay(1000);
            }
            stopWatch.Stop();
            DoResetCountDown?.Invoke(this, true);
            DoLockBrowser?.Invoke(this, true);

            return result;
        }

        private async Task<bool> VtbLoginAsync(string username, string password)
        {
            MyStopWatch sWatch;

            sWatch = new MyStopWatch(true);
            var setLogin = await SetLoginAsync(Bank, username, password);
            VtbReport.InputLogin = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!setLogin)
            {
                LogError($"Error set login");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var manualLogin = await VtbManualLoginAsync();
            VtbReport.ManualLogin = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!manualLogin) return false;

            sWatch = new MyStopWatch(true);
            var isLoggedIn = await IsLoggedInAsync(Bank);
            VtbReport.IsLoggedIn2 = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!isLoggedIn)
            {
                LogError($"Error not logged in");
                return false;
            }

            return true;
        }

        private async Task<bool> VtbGoTransferScreenAsync(TransferType transType)
        {
            var url = transType == TransferType.Internal ?
                "https://ipay.vietinbank.vn/transfer/internal" :
                "https://ipay.vietinbank.vn/transfer/external";

            Browser.Navigation.LoadUrl(url).Wait();
            if (!await WaitAsync(2)) return false;

            await ShowBalanceAsync(Bank);

            return true;
        }

        private async Task VtbHandleNewVersionPopupAsync()
        {
            var msgId = "#root-modal .modal-content .paragraph-2";
            var msgExists = await JsElementExists(msgId, 2);
            if (msgExists)
            {
                var msgText = await Browser.JsGetElementInnerText(msgId, 1);
                LogInfo($"New Version popup: <{msgText}>, {msgId}");

                var btnId = "#root-modal button.app-btn";
                var btnExists = await JsElementExists(btnId, 2);
                if (!btnExists)
                {
                    LogError($"Error popup button not exists, {btnId}");
                    return;
                }
                var btnText = await Browser.JsGetElementInnerText(btnId, 2);
                var clickBtn = await Browser.JsClickElement(btnId, 2);
                if (!clickBtn)
                {
                    LogError($"Error click button <{btnText}>, {btnId}");
                    return;
                }
                else
                {
                    LogInfo($"Click button <{btnText}>, {btnId}");
                }
            }
        }

        private async Task<bool> VtbRefreshAsync(string username, string password, int mode = 0)
        {
            //
            // mode 
            // 0 : do not go transfer screen
            // 1 : refresh then go transfer screen
            //

            MyStopWatch sWatch;

            sWatch = new MyStopWatch(true);
            var loadBank = await LoadBankAsync(Bank);
            VtbReport.LoadBank = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!loadBank)
            {
                LogError($"Error load bank url");
                return false;
            }

            await VtbHandleNewVersionPopupAsync();

            sWatch = new MyStopWatch(true);
            var isLoggedIn = await IsLoggedInAsync(Bank);
            VtbReport.IsLoggedIn = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!isLoggedIn)
            {
                sWatch = new MyStopWatch(true);
                var lang = await VtbGetLanguageAsync();
                VtbReport.GetLang = (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!lang)
                {
                    LogError($"Error get language");
                    return false;
                }

                var login = await VtbLoginAsync(username, password);
                if (!login)
                {
                    LogError($"Error login");
                    return false;
                }
            }

            if (mode == 1)
            {
                sWatch = new MyStopWatch(true);
                var goInternalTransfer = await VtbGoTransferScreenAsync(TransferType.Internal);
                VtbReport.GoTransfer = (int)sWatch.Elapsed(true)?.TotalSeconds;
            }

            return true;
        }

        private async Task<bool> VtbInputTransferAsync(TransferType transType, VtbExtBank bank, string accNum, string accName, decimal amount, string remarks)
        {
            var headerOffset = await Browser.JsGetElementOffsetHeight(".main-header", 1);

            // Check account Available Balance
            if (1 == 1)
            {
                var accBal = await Browser.JsVtbGetBalanceAtTransferPage(2);
                if (accBal != null && accBal.ToNumber(Bank).StrToDec() >= amount)
                {
                    BalanceAfterTransfer = accBal.ToNumber(Bank).StrToDec() - amount;
                    LogInfo($"Balance is <{accBal}>, {amount}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"余额不足: <{accBal}>, 转账金额: {amount}";
                    LogError($"Error insufficient balance <{accBal}>, {amount}");
                    return false;
                }
            }

            // Input Account number | Keyboard input
            if (1 == 1)
            {
                var accNumId = transType == TransferType.Internal ?
                    "#app .transfer-internal .payment-layout__body .beneficiary-account input" :
                    "#app .transfer-external .payment-layout__body .beneficiary-account input";
                var accNumExists = await JsElementExists(accNumId, 2);
                if (!accNumExists)
                {
                    LogError($"Error Account number input not exists, {accNumId}");
                    return false;
                }
                var inputAccNum = await Browser.InputToElementJsScroll(Bank, accNumId, accNum, headerOffset.StrToInt(), 2, Dpi);
                if (!inputAccNum)
                {
                    LogError($"Error input Account number <{accNum}>, {accNumId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Account number <{accNum}>, {accNumId}");
                }
            }

            if (transType == TransferType.External)
            {
                // Input Select beneficiary's bank
                if (1 == 1)
                {
                    var benBankId = ".wrap-input-select .vt-select__input input";
                    var benBankExists = await JsElementExists(benBankId, 2);
                    if (!benBankExists)
                    {
                        LogError($"Error Select beneficiary's bank input not exists, {benBankId}");
                        return false;
                    }

                    var inputBenBank = await Browser.InputToElementJsScroll(Bank, benBankId, bank.GetVtbName(SelectedLang), headerOffset.StrToInt(), 1, Dpi);
                    await Task.Delay(1000);
                    if (!await WaitAsync(2)) return false;
                    if (!inputBenBank)
                    {
                        LogError($"Error input Select beneficiary's bank <{bank.GetVtbName(SelectedLang)}>, {benBankId}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Input Select beneficiary's bank <{bank.GetVtbName(SelectedLang)}>, {benBankId}");
                    }

                    var benBankPos = await Browser.JsVtbGetBeneficiaryBankPosition(bank.GetVtbName(SelectedLang), 2);
                    if (benBankPos == null)
                    {
                        LogError($"Beneficiary bank not exists <{bank.GetVtbName(SelectedLang)}>");
                        return false;
                    }
                    var clickBenBankPos = Browser.MouseClickJsPosition(benBankPos, 2, Dpi);
                    if (!await WaitAsync(2, 1)) return false;

                }
            }

            var amountId = transType == TransferType.Internal ?
                "#app .transfer-internal .payment-layout__body .input-money input" :
                "#app .transfer-external .payment-layout__body .input-money input";
            // Check Amount exists
            if (1 == 1)
            {
                var amountExists = await JsElementExists(amountId, 2);
                if (!amountExists)
                {
                    LogError($"Error Amount input not exists, {amountId}");
                    return false;
                }
            }

            if (transType == TransferType.Internal)
            {
                // Click Amount input to trigger Beneficiary name validation
                if (1 == 1)
                {
                    var clickAmount = await Browser.MouseClickEventJsScroll(Bank, amountId, headerOffset.StrToInt(), 2, Dpi);
                    if (!await WaitAsync(2, 1)) return false;
                    if (!clickAmount)
                    {
                        LogError($"Error click Amount, {amountId}");
                        return false;
                    }
                    else
                    {
                        LogError($"Click Amount, {amountId}");
                    }
                }
            }

            // Wait for toast validation for beneficiary details
            if (1 == 1)
            {
                var errBenId = ".Toastify .Toastify__toast--error .Toastify__toast-body";
                var errBenExists = await JsElementExists(errBenId, 5);
                if (errBenExists)
                {
                    var errBenText = await Browser.JsGetElementInnerText(errBenId, 1);
                    LogError($"Error input Beneficiary details, <{errBenText}>, {errBenId}");
                    return false;
                }
            }

            // If no error, check Beneficiary name
            if (1 == 1)
            {
                var benName = await Browser.JsVtbGetBeneficiaryNameAtTransferPage(transType, SelectedLang, 2);
                if (benName != null && benName.ToBeneficiaryNameCompare() == accName.ToBeneficiaryNameCompare())
                {
                    LogInfo($"Beneficiary name is <{benName}>, {accName}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"收款人姓名不符: {accName}, 订单姓名: {benName}";
                    LogError($"Error Beneficiary name not match <{benName}>, {accName}");
                    return false;
                }
            }

            // Input Amount
            if (1 == 1)
            {
                var inputAmount = await Browser.InputToElementJsScroll(Bank, amountId, amount.ToString(), headerOffset.StrToInt(), 2, Dpi, false);
                if (!inputAmount)
                {
                    LogError($"Error input Amount <{amount}>, {amountId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Amount <{amount}>, {amountId}");
                }
            }

            // Clear Remark (x) then Input Remark
            if (1 == 1)
            {
                var remarkId = transType == TransferType.Internal ?
                    "#app .transfer-internal .payment-layout__body input[name=\"reference\"]" :
                    "#app .transfer-external .payment-layout__body input[name=\"reference\"]";
                var remarkExists = await JsElementExists(remarkId, 2);
                if (!remarkExists)
                {
                    LogError($"Error Remark input not exists, {remarkId}");
                    return false;
                }

                var clickRemark = await Browser.MouseClickAndSetCursorActiveJsScroll(Bank, remarkId, headerOffset.StrToInt(), 2, Dpi);
                if (!clickRemark)
                {
                    LogError($"Error click Remark, {remarkId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Remark, {remarkId}");
                }

                var clearRemarkId = transType == TransferType.Internal ?
                    "#app .transfer-internal .payment-layout__body .wrap-input .icon-postfix img.icon-clear" :
                    "#app .transfer-external .payment-layout__body .wrap-input .icon-postfix img.icon-clear";
                var clearRemarkExists = await JsElementExists(clearRemarkId, 2);
                if (clearRemarkExists)
                {
                    var clickClearRemark = await Browser.MouseClickEvent(clearRemarkId, headerOffset.StrToInt(), 2, Dpi);
                    if (!clickClearRemark)
                    {
                        LogError($"Error click clear Remark, {clearRemarkId}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Click clear Remark, {clearRemarkId}");
                    }
                }

                var inputRemark = await Browser.InputToElementJsScroll(Bank, remarkId, remarks, headerOffset.StrToInt(), 2, Dpi);
                if (!inputRemark)
                {
                    LogError($"Error input Remark <{remarks}>, {remarkId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Remark <{remarks}>, {remarkId}");
                }
            }

            // Click Continue
            if (1 == 1)
            {
                var submitId = transType == TransferType.Internal ?
                    "#app .transfer-internal button.btn-continue" :
                    "#app .transfer-external button.btn-continue";
                var submitExists = await JsElementExists(submitId, 2);
                if (!submitExists)
                {
                    LogError($"Error Continue button not exists, {submitId}");
                    return false;
                }
                var clickSubmit = await Browser.MouseClickEvent(submitId, headerOffset.StrToInt(), 2, Dpi);
                if (!await WaitAsync(2, 1)) return false;
                if (!clickSubmit)
                {
                    LogError($"Error click Continue, {submitId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Continue, {submitId}");
                }
            }

            // Check has error input
            if (1 == 1)
            {
                // .beneficiary-account__error-message
                // .wrap-input-select .hint
                // .input-money .hint
                // .input-fill-group .hint

                var accNumErrid = transType == TransferType.Internal ?
                    "#app .transfer-internal .payment-layout__body .beneficiary-account__error-message" :
                    "#app .transfer-external .payment-layout__body .beneficiary-account__error-message";
                var accNumErrText = await Browser.JsGetElementInnerText(accNumErrid, 2);
                if (!string.IsNullOrWhiteSpace(accNumErrText))
                {
                    LogError($"Error Account Number <{accNumErrText}>, {accNumErrid}");
                    return false;
                }

                var benBankErrId = transType == TransferType.Internal ?
                    "#app .transfer-internal .payment-layout__body .wrap-input-select .hint" :
                    "#app .transfer-external .payment-layout__body .wrap-input-select .hint";
                var benBankErrExists = await JsElementExists(benBankErrId, 2);
                if (benBankErrExists)
                {
                    var benBankErrText = await Browser.JsGetElementInnerText(benBankErrId, 2);
                    LogError($"Error Select beneficiary's bank <{benBankErrText}>, {benBankErrId}");
                    return false;
                }

                var amountErrId = transType == TransferType.Internal ?
                    "#app .transfer-internal .payment-layout__body .input-money .hint" :
                    "#app .transfer-external .payment-layout__body .input-money .hint";
                var amountErrExists = await JsElementExists(amountErrId, 2);
                if (amountErrExists)
                {
                    var amounErrText = await Browser.JsGetElementInnerText(amountErrId, 2);
                    LogError($"Error Amount <{amounErrText}>, {amountErrId}");
                    return false;
                }

                var remarkErrId = transType == TransferType.Internal ?
                    "#app .transfer-internal .payment-layout__body .input-fill-group .hint" :
                    "#app .transfer-external .payment-layout__body .input-fill-group .hint";
                var remarkErrExists = await JsElementExists(remarkErrId, 2);
                if (remarkErrExists)
                {
                    var remarkErrText = await Browser.JsGetElementInnerText(remarkErrId, 2);
                    LogError($"Error Remark <{remarkErrText}>, {remarkErrId}");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> VtbTransferAuthAsync(TransferType transType)
        {
            // Wait count down exists | 10 second
            var countdownId = transType == TransferType.Internal ?
                "#app .transfer-internal .auth-waiting .text-danger" :
                "#app .transfer-external .auth-waiting .text-danger";
            var timeoutExists = await JsElementExists(countdownId, 10);
            if (!timeoutExists)
            {
                LogError($"Error count down not exists, {countdownId}");
                return false;
            }

            // Check count down text and expired popup
            var expiredId = "#root-modal .modal-content.modal-soft-otp";
            var expiredMsgId = "#root-modal .modal-content.modal-soft-otp .paragraph-2";

            var isExpired = false;
            var iTry = 0;
            var iTryMax = 3;
            while (iTry < iTryMax)
            {
                var timeoutText = await Browser.JsGetElementInnerText(countdownId, 2);
                if (timeoutText != null && timeoutText.GetIntegerOnly() > 0)
                {
                    LogInfo($"Please check your mobile <{timeoutText.GetIntegerOnly()}> ...");
                    DoSetCountDown?.Invoke(this, timeoutText.GetIntegerOnly());
                    await Task.Delay(1000);
                    continue;
                }

                var expiredExists = await JsElementExists(expiredId, 2);
                if (expiredExists)
                {
                    var expiredMsg = await Browser.JsGetElementInnerText(expiredMsgId, 1);
                    LogError($"Transfer expired, {expiredMsg}, {expiredMsg}");
                    isExpired = true;
                    break;
                }

                await Task.Delay(1000);
                iTry++;
            }

            DoResetCountDown?.Invoke(this, true);
            if (isExpired) return false;

            return true;
        }

        private async Task<bool> VtbTransferStatusAsync(TransferType transType)
        {
            var transactionId = string.Empty;

            // No count down and not expired
            // Check whether transfer success | wait 10 seconds
            var statusId = transType == TransferType.Internal ?
                "#app .transfer-internal .payment-header__transaction--success" : // success or failed
                "#app .transfer-external .payment-header__transaction--success";
            var statusExists = await JsElementExists(statusId, 10);
            if (!statusExists)
            {
                LogError($"Transfer status not exists, {statusId}");
                return false;
            }
            var statusText = await Browser.JsGetElementInnerText(statusId, 1);

            var transId = transType == TransferType.Internal ?
                "#app .transfer-internal .payment-header__transaction--id span" :
                "#app .transfer-external .payment-header__transaction--id span";
            var transExists = await JsElementExists(transId, 2);
            if (!transExists)
            {
                LogError($"Reference Number not exists, {transId}");
                LogError($"Transfer failed <{statusText}>, {statusId}");
                return false;
            }

            var transText = await Browser.JsGetElementInnerText(transId, 1);
            if (transText != null)
            {
                LogInfo($"Reference Number is <{transText}>, {transId}");
                transactionId = transText;
            }
            else
            {
                LogInfo($"Reference Number empty, {transId}");
                transactionId = TransferInfo.OrderNo;
            }

            TransferInfo.TransactionId = transactionId;
            TransferInfo.Status = TransferStatus.Success;

            LogInfo($"Tranasfer success");
            return true;
        }

        private async Task<bool> VtbTransferAsync(TransferType transType, string username, string password,
            VtbExtBank toBank, string toAccNum, string toAccName, decimal toAmount, string remarks)
        {
            MyStopWatch sWatch;

            var refresh = await VtbRefreshAsync(username, password, 0);
            if (!refresh) return false;

            sWatch = new MyStopWatch(true);
            var goTransferScreen = await VtbGoTransferScreenAsync(transType);
            VtbReport.GoTransfer = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!goTransferScreen)
            {
                LogError($"Error go transfer screen");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var inputTransfer = await VtbInputTransferAsync(transType, toBank, toAccNum, toAccName, toAmount, remarks);
            VtbReport.InputTransfer = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!inputTransfer)
            {
                LogError($"Error input transfer");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var authTransfer = await VtbTransferAuthAsync(transType);
            VtbReport.TransferAuth = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!authTransfer)
            {
                LogError($"Error authenticate transfer");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var transferStatus = await VtbTransferStatusAsync(transType);
            VtbReport.TransferStatus = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!transferStatus) return false;

            return true;
        }
        #endregion

        #region Helper 

        private async Task<bool> WaitAsync(int waitSeconds = 3, double mustWait = 0.5, params string[] loadingElements)
        {
            if (mustWait > 0)
                await Task.Delay((int)(1000 * mustWait));

            int maxCanWait = 90;
            DateTime currTime = DateTime.Now;
            TimeSpan diff = currTime - LastLoadingAt;

            while ((diff.TotalSeconds + mustWait) <= waitSeconds || IsLoading())
            {
                LogInfo($"Browser loading buffer, {diff.TotalSeconds.To3Decimal()}/{waitSeconds}, {NrLoading}/{NrLoaded}, {Browser.Url}");
                await Task.Delay(1000);
                currTime = DateTime.Now;
                diff = currTime - LastLoadingAt;

                if (diff.TotalSeconds >= maxCanWait)
                {
                    LogError($"Browser loading timeout, {diff.TotalSeconds.To3Decimal()}/{maxCanWait}, {NrLoading}/{NrLoaded}, {Browser.Url}");
                    return false;
                }
            }
            LogInfo($"Browser loaded, {diff.TotalSeconds.To3Decimal()}/{waitSeconds}, {NrLoading}/{NrLoaded}, {Browser.Url}");

            foreach (var loading in loadingElements)
            {
                await WaitBankAsync(2, loading);
            }

            return await WaitBankAsync();
        }

        private async Task<bool> WaitBankAsync()
        {
            var result = true;
            switch (Bank)
            {
                case Bank.TcbBiz:
                case Bank.TcbBank:
                    result = await WaitBankAsync(2, ".app-loading");
                    break;
                case Bank.MbBiz:
                    result = await WaitBankAsync(2, ".loadingActivityIndicator");
                    break;
                case Bank.BidvBank:
                    result = await WaitBankAsync(2, "app-loading .ld-visible");
                    break;
                case Bank.SeaBank:
                    result = await WaitBankAsync(2, "app-progress-bar ngb-progressbar");
                    break;
                case Bank.VcbBank:
                    result = await WaitBankAsync(2, "vcb-loading .loading");
                    break;
                case Bank.MsbBank:
                    result = await WaitBankAsync(2, ".page-loading-overlay");
                    break;
                case Bank.VtbBank:
                    result = await WaitBankAsync(2, "nspay-dummy");
                    break;
                default: break;
            }
            return result;
        }

        private async Task<bool> WaitBankSpecialAsync()
        {
            var result = false;
            var d = string.Empty;
            switch (Bank)
            {
                // To review
                //case Bank.AcbBank:
                //    result = await Browser.JsGetElementDisplay("#acbone-loader-container", 2) != "block";
                //    break;

                case Bank.MsbBank:
                    d = await Browser.JsGetElementOpacity(".main-content-container", 2);
                    result = d != null && d != "";
                    break;

                case Bank.VtbBank:
                    d = await Browser.JsGetElementDisplay("#root .app-loading", 2);
                    result = d != "none";
                    break;

                default: break;
            }
            return result;
        }

        private async Task<bool> WaitBankAsync(int waitSeconds, params string[] loadingElements)
        {
            if (loadingElements != null)
            {
                var isLoading = true;
                var elementExists = string.Empty;
                var i = 0;
                while (isLoading)
                {
                    elementExists = await Browser.JsEitherElementExists(2, loadingElements);
                    isLoading = !string.IsNullOrEmpty(elementExists) || await WaitBankSpecialAsync();

                    if (isLoading)
                    {
                        BankLastLoadingAt = DateTime.Now;
                        LogInfo($"Bank is loading, {elementExists}, {i}");
                        await Task.Delay(1000);
                    }
                    else
                    {
                        var currTime = DateTime.Now;
                        TimeSpan diff = currTime - BankLastLoadingAt;

                        while (diff.TotalSeconds <= waitSeconds)
                        {
                            LogInfo($"Bank loading buffer, {diff.TotalSeconds.To3Decimal()}");
                            await Task.Delay(1000);
                            currTime = DateTime.Now;
                            diff = currTime - BankLastLoadingAt;
                        }
                    }

                    if (i >= 90)
                    {
                        LogError($"Bank loading timeout, {i}");
                        await Screenshot("Error_{0}_BankLoadingTimeout", true);
                        return false;
                    }

                    i++;
                }
            }
            LogInfo($"Bank loaded.");

            return true;
        }

        private async Task Screenshot(string fileName = "", bool captureOnRelease = false)
        {
            // If debug mode, capture
            // If not debug mode, and require capture on release, capture
            var blnGo = IsDebug || (!IsDebug && captureOnRelease);

            if (blnGo)
            {
                try
                {
                    var screenshotPath = Path.Combine(ImgPath, string.Format(fileName + ".jpeg", DateTime.Now.ToString("yyyyMMdd_HHmmss.fff")));
                    LogInfo($"Capturing screenshot, {screenshotPath}");

                    Bitmap bitmap = await Browser.Screenshot(Bank);
                    bitmap.Save(screenshotPath, ImageFormat.Jpeg);

                    LogsCache.Photo = bitmap.ToBase64();
                }
                catch (Exception ex)
                {
                    LogError($"Error ScreenshotAsync, {fileName}, {ex}", $"Error ScreenshotAsync, {fileName}, {ex.Message}");
                }
            }
        }
        #endregion

        #region Js Helper

        private async Task<bool> JsElementExists(string element, int numOfTry)
        {
            var exists = await Browser.JsElementExists(element, numOfTry);
            if (!exists.IsSuccess)
            {
                if (!string.IsNullOrWhiteSpace(exists?.ErrorMessage))
                {
                    LogError(exists.ErrorMessage);
                }
                else
                {
                    LogError($"Error executing JsElementExists, {element}, {numOfTry}");
                }
            }
            return exists.BoolResult;
        }

        //private async Task<bool> JsReloadAsync(int numOfTry)
        //{
        //    var exists = await Browser.JsReloadAsync(numOfTry);
        //    if (!exists.IsSuccess)
        //    {
        //        if (!string.IsNullOrWhiteSpace(exists?.ErrorMessage))
        //        {
        //            LogError(exists.ErrorMessage);
        //        }
        //        else
        //        {
        //            LogError($"Error executing JsElementExists, {numOfTry}");
        //        }
        //    }
        //    return exists.BoolResult;
        //}

        #endregion

        #region API / HttpClient

        public async Task<bool> LogUpdate(string messages, DateTime logDate, string photoBase64, string paymentId, string phone)
        {
            var result = await HttpClient.LogUpdateAsync(messages, logDate, photoBase64, paymentId, phone);
            if (result.MyIsSuccess)
            {
                if (result.code == 200)
                {
                    LogInfo($"LogUpdate, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}, {JsonConvert.SerializeObject(result.data)}");
                    return true;
                }
                else
                {
                    LogError($"Error LogUpdate, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}");
                }
            }
            else
            {
                LogError($"Error LogUpdate, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyErrorMessage}",
                    $"Error LogUpdate, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyExceptionMessage}");
            }
            return false;
        }

        public async Task<DeviceModel?> GetDevice(string phone, WithdrawalDevicesBankTypeEnum bankType)
        {
            var result = await HttpClient.GetDeviceAsync(phone, bankType);
            if (result.MyIsSuccess)
            {
                if (result.code == 200)
                {
                    LogInfo($"GetDevice, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}, {JsonConvert.SerializeObject(result.data)}");
                    return result.data == null ? null : JsonConvert.DeserializeObject<DeviceModel>(result.data.ToString());
                }
                else
                {
                    LogError($"Error GetDevice, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}");
                }
            }
            else
            {
                LogError($"Error GetDevice, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyErrorMessage}",
                    $"Error GetDevice, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyExceptionMessage}");
            }
            return null;
        }

        public async Task<WithdrawOrderModel?> GetWithdrawOrder(string phone, WithdrawalDevicesBankTypeEnum bankType)
        {
            var result = await HttpClient.GetWithdrawOrderAsync(phone, bankType);
            if (result.MyIsSuccess)
            {
                if (result.code == 200)
                {
                    LogInfo($"GetWithdrawOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}, {JsonConvert.SerializeObject(result.data)}");
                    return result.data == null ? null : JsonConvert.DeserializeObject<WithdrawOrderModel>(result.data.ToString());
                }
                else
                {
                    LogError($"Error GetWithdrawOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}");
                }
            }
            else
            {
                LogError($"Error GetWithdrawOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyErrorMessage}",
                    $"Error GetWithdrawOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyExceptionMessage}");
            }
            return null;
        }

        public async Task<WithdrawalOrdersMongoEntity?> CheckWithdrawOrder(string phone, WithdrawalDevicesBankTypeEnum bankType)
        {
            var result = await HttpClient.CheckWithdrawOrderAsync(phone, bankType);
            if (result.MyIsSuccess)
            {
                if (result.code == 200)
                {
                    LogInfo($"CheckWithdrawOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}, {JsonConvert.SerializeObject(result.data)}");
                    return result.data == null ? null : JsonConvert.DeserializeObject<WithdrawalOrdersMongoEntity?>(result.data.ToString());
                }
                else
                {
                    LogError($"Error CheckWithdrawOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}");
                }
            }
            else
            {
                LogError($"Error CheckWithdrawOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyErrorMessage}",
                    $"Error CheckWithdrawOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyExceptionMessage}");
            }
            return null;
        }

        public async Task<bool> UpdateWithdrawOrder(string orderId, string transactionNo, WithdrawalOrderStatusEnum orderStatus, string remark)
        {
            var result = await HttpClient.UpdateWithdrawOrderAsync(orderId, transactionNo, orderStatus, remark);
            if (result.MyIsSuccess)
            {
                if (result.code == 200)
                {
                    LogInfo($"UpdateWithdrawOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}, {JsonConvert.SerializeObject(result.data)}");
                    return true;
                }
                else
                {
                    LogError($"Error UpdateWithdrawOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}");
                }
            }
            else
            {
                LogError($"Error UpdateWithdrawOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyErrorMessage}",
                    $"Error UpdateWithdrawOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyExceptionMessage}");
            }
            return false;
        }

        public async Task<bool> UpdateBalance(int deviceId, decimal balance)
        {
            var result = await HttpClient.UpdateBalanceAsync(deviceId, balance);
            if (result.MyIsSuccess)
            {
                if (result.code == 200)
                {
                    LogInfo($"UpdateBalance, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}, {JsonConvert.SerializeObject(result.data)}");
                    return true;
                }
                else
                {
                    LogError($"Error UpdateBalance, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}");
                }
            }
            else
            {
                LogError($"Error UpdateBalance, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyErrorMessage}",
                    $"Error UpdateBalance, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyExceptionMessage}");
            }
            return false;
        }

        public async Task<OrderOtpModel?> GetOrderOtp(int deviceId, string orderId)
        {
            var result = await HttpClient.GetOrderOtpAsync(deviceId, orderId);
            if (result.MyIsSuccess)
            {
                if (result.code == 200)
                {
                    LogInfo($"GetOrderOtp, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}, {JsonConvert.SerializeObject(result.data)}");
                    return result.data == null ? null : JsonConvert.DeserializeObject<OrderOtpModel?>(result.data.ToString());
                }
                else
                {
                    LogError($"Error GetOrderOtp, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}");
                }
            }
            else
            {
                LogError($"Error GetOrderOtp, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyErrorMessage}",
                    $"Error GetOrderOtp, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyExceptionMessage}");
            }
            return null;
        }

        public async Task<bool> UpdateOtptatus(int deviceId, string orderId, int orderStatus)
        {
            var result = await HttpClient.UpdateOtptatusAsync(deviceId, orderId, orderStatus);
            if (result.MyIsSuccess)
            {
                if (result.code == 200)
                {
                    LogInfo($"UpdateOtptatus, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}, {JsonConvert.SerializeObject(result.data)}");
                    return true;
                }
                else
                {
                    LogError($"Error UpdateOtptatus, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}");
                }
            }
            else
            {
                LogError($"Error UpdateOtptatus, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyErrorMessage}",
                    $"Error UpdateOtptatus, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyExceptionMessage}");
            }
            return false;
        }

        public async Task<bool> CreateOrderOtp(int deviceId, string orderId, string transferOtp = "")
        {
            var result = await HttpClient.CreateOrderOtpAsync(deviceId, orderId, transferOtp);
            if (result.MyIsSuccess)
            {
                if (result.code == 200)
                {
                    LogInfo($"CreateOrderOtp, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}, {JsonConvert.SerializeObject(result.data)}");
                    return true;
                }
                else
                {
                    LogError($"Error CreateOrderOtp, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}");
                }
            }
            else
            {
                LogError($"Error CreateOrderOtp, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyErrorMessage}",
                    $"Error CreateOrderOtp, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyExceptionMessage}");
            }
            return false;
        }

        public async Task<bool> GetSuccessOrder(int deviceId, string orderId)
        {
            var result = await HttpClient.GetSuccessOrderAsync(deviceId, orderId);
            if (result.MyIsSuccess)
            {
                if (result.code == 200)
                {
                    LogInfo($"GetSuccessOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}, {JsonConvert.SerializeObject(result.data)}");
                    return true;
                }
                else
                {
                    LogError($"Error GetSuccessOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.message}");
                }
            }
            else
            {
                LogError($"Error GetSuccessOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyErrorMessage}",
                    $"Error GetSuccessOrder, {result.MyRequestUri}, {result.MyRequestData}, {result.code}, {result.MyExceptionMessage}");
            }
            return false;
        }

        #endregion
    }
}
