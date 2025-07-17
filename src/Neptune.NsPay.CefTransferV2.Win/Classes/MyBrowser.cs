using CefSharp;
using CefSharp.DevTools.Page;
using CefSharp.WinForms;
using Neptune.NsPay.CefTransfer.Common.Classes;
using Neptune.NsPay.CefTransfer.Common.Models;
using Neptune.NsPay.CefTransfer.Common.MyEnums;
using Neptune.NsPay.CefTransferV2.Win.Helpers;
using Neptune.NsPay.CefTransferV2.Win.Models;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.WithdrawalOrders;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Neptune.NsPay.CefTransferV2.Win.Classes
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
        public ChromiumWebBrowser CefBrowser { get; set; }
        public CefSettings CefSettings { get; set; }
        public MyRequestHandler CefRequestHandler { get; set; }
        public MyDownloadHandler CefDownloadHandler { get; set; }
        public int NumOfFailure { get; set; }
        public int NumOfSuccess { get; set; }
        public int NrLoaded { get; set; }
        public int NrLoading { get; set; }
        public DateTime LastLoadingAt { get; set; } = DateTime.MinValue;
        public DateTime BankLastLoadingAt { get; set; } = DateTime.MinValue;
        public int PreviousRequestNrWhereLoadingFinished { get; set; } = 0;
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
        public bool IsStarted { get; set; } = false;
        public bool IsRunning { get; set; } = false;
        public TransferInfoModel TransferInfo { get; set; }
        public TcbReportModel TcbReport { get; set; }
        public AcbReportModel AcbReport { get; set; }
        public bool CheckHistory { get; set; }
        public bool CanStart { get; set; } = false;
        public string ReportStatistic { get; set; }
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

        private async void OnCefBrowser_LoadingStateChanged(object? sender, LoadingStateChangedEventArgs e)
        {
            try
            {
                DoIsLoading?.Invoke(this, e.IsLoading);
                if (e.IsLoading == false)
                {
                    NrLoaded = CefRequestHandler.NrOfCalls;
                    LogInfo($"NrLoaded, {NrLoaded}");
                }
                else
                {
                    LastLoadingAt = DateTime.Now;
                    NrLoading = CefRequestHandler.NrOfCalls;
                    LogInfo($"NrLoading, {NrLoading}");
                }

                if (PreviousRequestNrWhereLoadingFinished < CefRequestHandler.NrOfCalls)
                {
                    PreviousRequestNrWhereLoadingFinished = CefRequestHandler.NrOfCalls;
                    if (await WaitAsync(2))
                    {
                        if (!CanStart)
                        {
                            CanStart = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error OnCefBrowser_LoadingStateChanged, {ex}", $"Error OnCefBrowser_LoadingStateChanged, {ex.Message}");
            }
        }

        private void OnCefBrowser_AddressChanged(object? sender, AddressChangedEventArgs e)
        {
            DoSetUrl?.Invoke(this, e.Address);
        }

        private void OnRefreshTimer_Elapsed(object? source, System.Timers.ElapsedEventArgs e)
        {
            RefreshInterval++;
        }

        public async void OnResourceLoadComplete(IRequest request)
        {

        }
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
                Sign = AppSettings.Configuration["NsPayApi:Sign"];
                MerchantCode = AppSettings.Configuration["NsPayApi:MerchantCode"];
                Type = Convert.ToInt32(AppSettings.Configuration["NsPayApi:Type"]);

                Bank = BankType.GetConfigBank();
                if (Bank == Bank.Unknown)
                    throw new Exception($"Unknown bank {AppSettings.Configuration["BankType"]}");

                HttpClient = new MyHttpClient(BaseUrl, MerchantCode, Type);

                await InitCef();
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
                    LogInfo($"Worker running at: {DateTime.Now}");

                    await StartTransferAsync();
                    //await AcbTestAsync();
                    //await TcbTestAsync();

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
                DoWriteUiMessage?.Invoke(this, msg);
            }
            NlogLogger.Error(msg);
        }

        #endregion

        #region Private
        private async Task InitCef()
        {
            try
            {
                CefSettings = new CefSettings()
                {
                    CachePath = CachePath,
                };

                var proxyEnable = Convert.ToBoolean(AppSettings.Configuration["Proxy:Enable"]);
                var proxyAddress = string.Empty;
                if (proxyEnable)
                {
                    proxyAddress = Convert.ToString(AppSettings.Configuration["Proxy:Address"]);
                    CefSettings.CefCommandLineArgs.Add("proxy-server", proxyAddress);
                }

                var success = await Cef.InitializeAsync(CefSettings);
                if (!success)
                {
                    throw new Exception("Error initialise CEF");
                }

                CefRequestHandler = new MyRequestHandler(this);
                CefDownloadHandler = new MyDownloadHandler();
                //CefDownloadHandler.OnDownloading += this.OnDownloading;
                //CefDownloadHandler.CanDownload += this.CanDownload;

                CefBrowser = new ChromiumWebBrowser(Bank.GetBank()?.url)
                {
                    RequestHandler = CefRequestHandler,
                    //DownloadHandler = CefDownloadHandler,
                };
                CefBrowser.LoadingStateChanged += OnCefBrowser_LoadingStateChanged;
                CefBrowser.AddressChanged += OnCefBrowser_AddressChanged;
                //CefBrowser.JavascriptMessageReceived += OnBrowser_JavascriptMessageReceived;
                //CefBrowser.FrameLoadEnd += OnBrowser_FrameLoadEnd;

                var onUi = Cef.CurrentlyOnThread(CefThreadIds.TID_UI);

                //await Task.Delay(50);
                //if (!await WaitAsync(2)) return;
            }
            catch (Exception ex)
            {
                LogError($"Error InitCef, {ex}", $"Error InitCef, {ex.Message}");
                throw new Exception($"Error InitCef, {ex.Message}");
            }
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
                case Bank.TcbBank:
                    RefreshTime = 170;
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
                if (IsDebug) // TCB
                {
                    deviceInfo = new DeviceModel()
                    {
                        DeviceId = 1,
                        Phone = "0703805200",
                        Name = "NGUYEN VAN HUY",
                        Otp = "",
                        Status = false,
                        Process = 1,
                        LoginPassWord = "Win168168@",
                        CardNumber = "37393987",
                    };
                }
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

                                    LogInfo($"获取消息：{DateTime.Now}--OrderId:{transInfo.OrderNo}");

                                    try
                                    {
                                        ////更新订单未转账中
                                        blnUpdateOrder = await UpdateWithdrawOrder(transInfo.OrderId, "", WithdrawalOrderStatusEnum.Pending, "");
                                        LogInfo($"进入转账：{DateTime.Now}--OrderId:{transInfo.OrderNo}");

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

                                        LogInfo($"转账完成：{DateTime.Now}--OrderId:{orderNo}");
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
                LogError($"账户：{Phone}, 错误 {ex}", $"账户：{Phone}, 错误 {ex.Message}");
            }
        }

        private bool IsLoading()
        {
            return NrLoaded != CefRequestHandler.NrOfCalls;
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

                    userExists = await JsElementExists(userId, 2);
                    if (userExists)
                    {
                        inputUser = await CefBrowser.JsInputElementValue(userId, usernameValue, 2);
                        if (inputUser)
                        {
                            userVal = await CefBrowser.JsGetInputValue(userId, 2);
                            userResult = userVal == usernameValue;
                        }
                    }

                    passExists = await JsElementExists(passId, 2);
                    if (passExists)
                    {
                        inputPass = await CefBrowser.JsInputElementValue(passId, passwordValue, 2);
                        if (inputPass)
                        {
                            passVal = await CefBrowser.JsGetInputValue(passId, 2);
                            passResult = passVal == passwordValue;
                        }
                    }

                    result = userResult && passResult;
                    break;

                default: break;
            }

            return result;
        }

        private async Task<bool> IsLoggedInAsync(Bank bank)
        {
            JsResultModel result = new JsResultModel();
            //var result = false;
            var checkId = string.Empty;

            switch (bank)
            {
                case Bank.AcbBank:
                    checkId = "#menu li ul li a[href*=\"dse_operationName=ibkacctSumProc\"]";
                    result = await CefBrowser.JsElementExists(checkId, 2);
                    if (!result.IsSuccess)
                    {
                        LogError($"Error executing Js, special reload ...");
                        await AcbReloadPageAsync();
                    }
                    break;

                case Bank.TcbBank:
                    checkId = "a[href=\"/dashboard\"]";
                    result = await CefBrowser.JsElementExists(checkId, 2);
                    break;

                default: break;
            }

            return result.BoolResult;
        }

        //private async Task<string?> GetBalanceAsync(Bank bank)
        //{
        //    string? result = null;
        //    switch (bank)
        //    {
        //        case Bank.AcbBank:
        //            result = await CefBrowser.JsGetAcbBalance(CardNo, 2);
        //            break;

        //        default: break;
        //    }

        //    return result;
        //}

        private async Task<bool> LoadBankAsync(Bank bank)
        {
            var url = string.Empty;

            switch (bank)
            {
                case Bank.AcbBank:
                    url = bank.GetBank().url;
                    CefBrowser.LoadUrl(url);
                    if (!await WaitAsync(2)) return false;
                    break;

                case Bank.TcbBank:
                    url = bank.GetBank().url;
                    CefBrowser.LoadUrl(url);
                    if (!await WaitAsync(2, "app-loader .loading")) return false;
                    break;

                default: break;
            }

            return true;
        }

        private async Task<bool> ReloadPageAsync(Bank bank)
        {
            LogInfo($"Reload page ...");

            var result = false;
            switch (bank)
            {
                case Bank.AcbBank:
                    result = await AcbReloadPageAsync();
                    break;

                default: break;
            }

            return result;


            //var reload = await CefBrowser.JsReloadAsync(1);
            //await Task.Delay(1000 * 2);
            //if (!await WaitAsync(2)) return false;

            //CefBrowser.Reload(true);
            //await Task.Delay(1000 * 2);
            //if (!await WaitAsync(2)) return false;

            //await LoadBankAsync(Bank.AcbBank);
            //await Task.Delay(1000 * 2);
            //if (!await WaitAsync(2)) return false;
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

                    result = await AcbRefreshAsync(TransferInfo.UserName, TransferInfo.Password, TransferInfo.CardNo);

                    AcbReport.Result = result;
                    ReportStatistic = AcbReport.Statistic;
                    break;

                case Bank.TcbBank:
                    TcbReport = new TcbReportModel();

                    result = await TcbRefreshAsync(TransferInfo.UserName, TransferInfo.Password);

                    TcbReport.Result = result;
                    ReportStatistic = TcbReport.Statistic;
                    break;

                default: break;
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
                            LogError($"Unknown bank <{TransferInfo.ToBank}>");
                            return false;
                        }
                        AcbReport = new AcbReportModel();
                        transType = acbToBank == AcbExtBank.ACB ? TransferType.Internal : TransferType.External;
                        LogInfo(transType == TransferType.Internal ? "ACB" : "其他银行" + $"转账：{DateTime.Now}--OrderId:{TransferInfo.OrderNo}");
                        result = await AcbTransferAsync(transType, TransferInfo.UserName, TransferInfo.Password, TransferInfo.CardNo, acbToBank,
                            TransferInfo.ToAccountNumber, TransferInfo.ToAccountName, TransferInfo.ToAmount, TransferInfo.ToRemarks,
                            TransferInfo.DeviceId, TransferInfo.OrderId);

                        AcbReport.Result = result;
                        AcbReport.TransferType = transType == TransferType.Internal ? "I" : "E";
                        ReportStatistic = AcbReport.Statistic;
                        break;

                    case Bank.TcbBank:
                        var tcbToBank = TransferInfo.ToBank.ToTcbExtBank();

                        if (tcbToBank == TcbExtBank.Unknown)
                        {
                            await UpdateWithdrawOrder(TransferInfo.OrderId, "", WithdrawalOrderStatusEnum.ErrorBank, "");
                            LogError($"Unknown bank <{TransferInfo.ToBank}>");
                            return false;
                        }

                        TcbReport = new TcbReportModel();
                        transType = tcbToBank == TcbExtBank.Techcombank_TCB ? TransferType.Internal : TransferType.External;
                        LogInfo(transType == TransferType.Internal ? "TCB" : "其他银行" + $"转账：{DateTime.Now}--OrderId:{TransferInfo.OrderNo}");
                        result = await TcbTransferAsync(transType, TransferInfo.UserName, TransferInfo.Password, tcbToBank,
                            TransferInfo.ToAccountNumber, TransferInfo.ToAccountName, TransferInfo.ToAmount, TransferInfo.ToRemarks);

                        TcbReport.Result = result;
                        TcbReport.TransferType = transType == TransferType.Internal ? "I" : "E";
                        ReportStatistic = TcbReport.Statistic;

                        break;

                    default:
                        return result;
                }

                if (!result)
                {
                    var htmlFileName = $"{TransferInfo.OrderNo}_ErrHtml" + "_{0}";
                    await CefBrowser.WriteHtmlToFile(HtmlPath, htmlFileName);
                }

                sFileName = TransferInfo.OrderNo + (result ? "" : "_Err") + "_{0}";
                await ScreenshotAsync(sFileName, true);
            }
            catch (Exception ex)
            {
                LogError($"Error TransferAsync, {ex}", $"Error TransferAsync, {ex.Message}");
            }

            return result;
        }

        #endregion

        #region ACB

        public async Task AcbTestAsync()
        {
            try
            {
                var doTransfer = false;
                var blnTransfer = false;

                ////External Transfer
                Bank = Bank.AcbBank;
                var transInfo = new TransferInfoModel()
                {
                    UserName = "0966493205",
                    Password = "Win168168@",
                    CardNo = "37393987",
                    ToBank = "TCB",
                    ToAccountNumber = "7703099999",
                    ToAccountName = "NGUYEN VAN HUY",
                    ToAmount = 5000,
                    ToRemarks = "5000",
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

            Lang selectLang = IsDebug ? Lang.English : Lang.Vietnamese;
            var langId = "#acb-one-header-container form[name*=\"chooseLan\"] input[class=\"language\"]";

            var iMaxWait = 10;
            var stopWatch = Stopwatch.StartNew();
            while (true)
            {
                var langValue = await CefBrowser.JsGetInputValue(langId, 2);
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
                    var click = await CefBrowser.JsClickElement(langId, 2);
                    LogInfo($"Click {langId}, <{click}>");
                    await Task.Delay(1000);
                    await WaitAsync(2);

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

                var headerText = await CefBrowser.JsGetElementInnerText(popupHeaderId, 2);
                var closeText = await CefBrowser.JsGetInputValue(popupCloseId, 2);

                var closeExists = await JsElementExists(popupCloseId, 2);
                if (closeExists)
                {
                    var closeClick = await CefBrowser.JsClickElement(popupCloseId, 2);
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
            var isLoggedIn = await IsLoggedInAsync(Bank.AcbBank);
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

            var securityCodeId = "#acb-one-login-container form .aol-form input[id=\"security-code\"]";
            var securityCodeExists = await JsElementExists(securityCodeId, 1);
            if (securityCodeExists)
            {
                var securityCodeScroll = await CefBrowser.JsScrollToElement(securityCodeId, 65 + 58, 1);
            }

            DoLockBrowser?.Invoke(this, false);
            var iMaxWait = 25;
            var stopWatch = Stopwatch.StartNew();
            while (true)
            {
                var isLoginExists = await IsLoginExistsAsync(Bank.AcbBank);

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

                LogInfo($"Please input Security code, {stopWatch.Elapsed.TotalSeconds.DoubleToInt()}/{iMaxWait} ...");
                await Task.Delay(1000);
            }
            stopWatch.Stop();
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

            while (true)
            {
                var authExists = await JsElementExists(authId, 1);
                if (authExists)
                {
                    var authScroll = await CefBrowser.JsScrollToElement(authId, 65 + 17, 1);
                }

                var safeKeyExists = await JsElementExists(safeKeyId, 1);
                if (safeKeyExists)
                {
                    var safeKeyScroll = await CefBrowser.JsScrollToElement(safeKeyId, 65 + 45, 1);
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

                LogInfo($"Please input OTP, {stopWatch.Elapsed.TotalSeconds.DoubleToInt()}/{iMaxWait} ...");
                await Task.Delay(1000);
            }
            stopWatch.Stop();

            return result;
        }

        private async Task<bool> AcbGoDashboardScreenAsync()
        {
            var id = "#menu li ul li a[href*=\"dse_operationName=ibkacctSumProc\"]";
            var exists = await JsElementExists(id, 2);
            if (!exists) return false;
            var click = await CefBrowser.JsClickElement(id, 2);
            if (!click) return false;

            return true;
        }

        private async Task AcbGetBalanceAsync(string accNum)
        {
            var balance = await CefBrowser.JsGetAcbBalance(accNum, 2);
            if (string.IsNullOrWhiteSpace(balance))
            {
                DoSetBalance?.Invoke(this, 0.00M);
            }
            else
            {
                DoSetBalance?.Invoke(this, balance.ToNumber(Bank.AcbBank).StrToDec());
            }
        }

        private async Task<bool> AcbReloadPageAsync()
        {
            var reload = await CefBrowser.JsReloadAsync(1);
            await Task.Delay(1000 * 2);
            if (!await WaitAsync(2)) return false;
            if (!reload.BoolResult)
            {
                // In case Js execution failed, LoadUrl
                // Request BrowserId : 1 not found it's likely the browser is already closed 
                if (!reload.IsSuccess)
                {
                    var canExecuteJs = false;
                    var iTry = 0;
                    var iTryMax = 8;

                    while (iTry < iTryMax)
                    {
                        iTry++;

                        LogInfo($"Try reload {iTry}/{iTryMax} ...");

                        var exists = await CefBrowser.JsElementExists("html", 1);
                        canExecuteJs = exists.IsSuccess;
                        if (!canExecuteJs)
                        {
                            await LoadBankAsync(Bank.AcbBank);
                            await Task.Delay(1000 * 5);
                            if (!await WaitAsync(2)) return false;
                        }
                        else
                        {
                            LogInfo($"Reload success {iTry}/{iTryMax}");
                            return true;
                        }
                    }

                    if (!canExecuteJs)
                    {
                        LogError($"Reload failed {iTry}/{iTryMax}");
                        return false;
                    }
                }
            }
            return true;
        }

        private async Task<bool> AcbRefreshAsync(string username, string password, string cardNo)
        {
            MyStopWatch sWatch;

            sWatch = new MyStopWatch(true);
            var isLoggedIn = await IsLoggedInAsync(Bank.AcbBank);
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

                await AcbGetBalanceAsync(cardNo);
            }
            else
            {
                sWatch = new MyStopWatch(true);
                var reload = await ReloadPageAsync(Bank.AcbBank);
                AcbReport.Reload = (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!reload) return false;

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
                    await AcbGetBalanceAsync(cardNo);
                }
                else { return false; }
            }

            await AcbLoginPopup();

            return true;
        }

        private async Task<bool> AcbGoTransferScreenAsync()
        {
            var transferLinkId = ".main #menu li a[href*=\"dse_operationName=ibkcardToCardFundTransProc\"]";
            var transferLinkExists = await JsElementExists(transferLinkId, 2);
            if (!transferLinkExists) return false;

            var clickTransferLink = await CefBrowser.JsClickElement(transferLinkId, 2);
            await WaitAsync(2);
            if (!clickTransferLink) return false;

            return true;
        }

        private async Task<bool> AcbInputTransferAsync(TransferType transType, string fromAccNum,
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
                var fromAccValue = await CefBrowser.JsGetInputValue(fromAccId, 2);
                if (fromAccValue != fromAccNum)
                {
                    var selectFromAcc = await CefBrowser.JsSetSelectValue(fromAccId, fromAccNum, 2);
                    await Task.Delay(1000 * 2);
                    await WaitAsync(2);
                    if (selectFromAcc)
                    {
                        fromAccValue = await CefBrowser.JsGetInputValue(fromAccId, 2);
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
                    fromAccName = await CefBrowser.JsGetElementInnerText(fromAccNameId, 2);
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
                if (accountBalance.ToNumber(Bank.AcbBank).StrToDec() > amount)
                {
                    LogInfo($"Balance is <{accountBalance}>, {amount}, {fromAccNameId}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"Error insufficient balance, <{accountBalance}>, {amount}";
                    LogError($"Error insufficient balance, <{accountBalance}>, {amount}, {fromAccNameId}");
                    return false;
                }
            }

            // Tick Enter Ben Acc
            if (1 == 1)
            {
                var optEnterBenAccId = ".main .content-holder form[name=\"smlibt\"] .table-form #IsDonViThuHuong2";
                var optEnterBenAccExists = await JsElementExists(optEnterBenAccId, 2);
                if (!optEnterBenAccExists)
                {
                    LogError($"Error Enter the beneficiary account not exists, {optEnterBenAccId}");
                    return false;
                }
                var optEnterBenAccText = await CefBrowser.JsGetElementInnerText(optEnterBenAccId, 1);
                var optEnterBenAccChecked = await CefBrowser.JsIsInputChecked(optEnterBenAccId, 1);
                if (!optEnterBenAccChecked)
                {
                    var checkOptEnterBenAcc = await CefBrowser.JsSetCheckbox(optEnterBenAccId, true, 1);
                    await WaitAsync(2);
                    if (checkOptEnterBenAcc)
                    {
                        LogInfo($"Selected <{optEnterBenAccText}>, {optEnterBenAccId}");
                    }
                    else
                    {
                        LogError($"Error select <{optEnterBenAccText}>, {optEnterBenAccId}");
                        return false;
                    }
                }
                else
                {
                    LogInfo($"Selected <{optEnterBenAccText}>, {optEnterBenAccId}");
                }
            }

            // Enter Ben Acc
            if (1 == 1)
            {
                var benAccId = ".main .content-holder form[name=\"smlibt\"] .table-form #AccountNbrCoMask";
                var benAccExists = await JsElementExists(benAccId, 2);
                if (!benAccExists)
                {
                    LogError($"Error beneficiary account input not exists, {benAccId}");
                    return false;
                }
                var inputBenAcc = await CefBrowser.JsInputElementValue(benAccId, accNum, 2);
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

            // Select Ben Bank
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
                var selectBenBank = await CefBrowser.JsSetSelectValue(benBankId, beneBank, 2);
                if (!selectBenBank)
                {
                    LogError($"Error select beneficiary's bank, {beneBank}, {benBankId}");
                    return false;
                }
                var hasBenBankSelected = await CefBrowser.JsHasSelected(benBankId, 2);
                if (!hasBenBankSelected)
                {
                    TransferInfo.Status = TransferStatus.ErrorBank;
                    TransferInfo.ErrorMessage = $"Error select beneficiary's bank <{beneBank}>";
                    LogError($"Error select beneficiary's bank <{beneBank}>, {benBankId}");
                    return false;
                }
                var selectedBenBank = await CefBrowser.JsGetSelectText(benBankId, 2);
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
              
                var inputAmount = await CefBrowser.InputToElement(amountId, amount.ToString(), 65 - 5, 2);
                await Task.Delay(1000 * 2);
                await WaitAsync(2);
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

            // Input Description
            if (1 == 1)
            {
                var descId = ".main .content-holder form[name=\"smlibt\"] .table-form textarea[name=\"Description\"]";
                var descExists = await JsElementExists(descId, 2);
                if (!descExists)
                {
                    LogError($"Error Detail of transaction input not exists, {descId}");
                    return false;
                }
               
                var mouseClickDesc = await CefBrowser.MouseClickEvent(descId, 65 - 5, 2);
                await Task.Delay(1000 * 2);
                await WaitAsync(2);
                if (!mouseClickDesc)
                {
                    LogError($"Error click Detail of transaction input, {descId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Detail of transaction input, {descId}");
                }

                var inputDesc = await CefBrowser.JsInputElementValue(descId, remarks, 2);
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

            // Pick auth method
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
                var selectAuthMethod = await CefBrowser.JsSetSelectValue(authMethodId, AUTH_OTP, 2);
                if (!selectAuthMethod)
                {
                    LogError($"Error select authentication method, {AUTH_OTP}, {authMethodId}");
                    return false;
                }
                var hasAuthMethodSelected = await CefBrowser.JsHasSelected(authMethodId, 2);
                if (!hasAuthMethodSelected)
                {
                    LogError($"Error select authentication method, {AUTH_OTP}, {authMethodId}");
                    return false;
                }
                var selectedAuthMethod = await CefBrowser.JsGetSelectText(authMethodId, 2);
                LogInfo($"Selected <{selectedAuthMethod}>, {authMethodId}");

                var submitId = ".main .content-holder form[name=\"smlibt\"] .table-form #dongy";
                var submitExists = await JsElementExists(submitId, 2);
                if (!submitExists)
                {
                    LogError($"Error Agree button not exists, {submitId}");
                    return false;
                }
                var clickSubmit = await CefBrowser.JsClickElement(submitId, 2);
                await Task.Delay(1000 * 2);
                await WaitAsync(2);
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
                    var sendOtpReq = await UpdateOtptatus(deviceId, orderId, 0);
                    if (!sendOtpReq)
                    {
                        LogError($"Error Update Otptatus <0>");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Update Otptatus <0>");
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
                error1Text = await CefBrowser.JsGetElementInnerText(errorId1, 1);
                LogError($"Input error, <{error1Text}>");
            }

            string? error2Text = null;
            var errorId2 = $".main .content-holder form[name=\"{formName}\"] .table-form .detailerror";
            var error2Exists = await JsElementExists(errorId2, 1);
            if (error2Exists)
            {
                error2Text = await CefBrowser.JsGetElementInnerText(errorId2, 1);
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

        private async Task<bool> AcbIsTransferSuccess()
        {
            var result = false;

            // Html too common, check error first before check for success

            var errorId = ".main .content-holder .table-form table td[class*=\"center\"][class*=\"error\"]";
            var errorExists = await JsElementExists(errorId, 2);
            if (errorExists)
            {
                var errorText = await CefBrowser.JsGetElementInnerText(errorId, 2);
                LogError($"Transfer failed, <{errorText}>, {errorId}");
            }
            else
            {
                var successId = ".main .content-holder .table-form table td[class=\"center\"]";
                var successExists = await JsElementExists(successId, 2);

                if (successExists)
                {
                    var successText = await CefBrowser.JsGetElementInnerText(successId, 2);
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

        private async Task<bool> AcbConfirmTransferAsync(string accName, string password, int deviceId, string orderId)
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
                var benName = await CefBrowser.JsGetAcbConfirmTransferBenName(SelectedLang, 2);
                if (benName != null && benName.VietnameseToEnglish().ToLower().Trim() == accName.VietnameseToEnglish().ToLower().Trim())
                {
                    LogInfo($"Beneficiary name is <{benName}>, {accName}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"Error Beneficiary name not match <{benName}>, {accName}";
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

                var inputPassword = await CefBrowser.JsInputElementValue(passwordId, password, 2);
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

                        var inputOtp = await CefBrowser.InputToElement(otpId, otp, 65, 2);
                        if (!inputOtp)
                        {
                            LogError($"Error input OTP, <{otp}>, {otpId}");
                            return false;
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
                            return false;
                        }

                        if (IsDebug) { await Task.Delay(1000 * 30); } // Debug mode, allow manual input correct OTP

                        var clickSubmit = await CefBrowser.JsClickElement(submitId, 2);
                        await Task.Delay(2000);
                        await WaitAsync(2);
                        if (!clickSubmit)
                        {
                            LogError($"Error click Confirm button, {submitId}");
                            return false;
                        }
                        else
                        {
                            LogInfo($"Click Confirm button, {submitId}");
                        }
                        break;
                    }

                    if (otpWatch.Elapsed.TotalSeconds.DoubleToInt() > waitOtp)
                        break;

                    LogInfo($"Retrieving OTP, {otpWatch.Elapsed.TotalSeconds.DoubleToInt()}/{waitOtp} ...");
                    await Task.Delay(1000);
                }
                otpWatch.Stop();
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

            var refresh = await AcbRefreshAsync(username, password, cardNo);
            if (!refresh) return false;

            // Retry on failed transfer (possible wrong OTP)
            var iTry = 0;
            var iTryMax = 3;
            var isSuccess = false;
            while (iTry < iTryMax)
            {
                iTry++;
                LogInfo($"Try transfer {iTry}/{iTryMax} ...");

                sWatch = new MyStopWatch(true);
                var goTransferScreen = await AcbGoTransferScreenAsync();
                AcbReport.GoTransfer += (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!goTransferScreen)
                {
                    LogError($"Error go transfer screen");
                    return false;
                }

                sWatch = new MyStopWatch(true);
                var inputTransfer = await AcbInputTransferAsync(transType, cardNo, toBank, toAccNum, toAmount, remarks, deviceId, orderId);
                AcbReport.InputTransfer += (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!inputTransfer) return false;

                sWatch = new MyStopWatch(true);
                var confirmTransfer = await AcbConfirmTransferAsync(toAccName, password, deviceId, orderId);
                AcbReport.ConfirmTransfer += (int)sWatch.Elapsed(true)?.TotalSeconds;
                if (!confirmTransfer)
                {
                    LogError($"Error confirm transfer");
                    return false;
                }

                sWatch = new MyStopWatch(true);
                isSuccess = await AcbIsTransferSuccess();
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
            var selectLang = await CefBrowser.JsClickElement(selectLangId, 2);
            if (!await WaitAsync(2, ".app-loading")) return false;
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
            var langValue = await CefBrowser.JsGetElementInnerText(langId, 2);
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
            // Input User
            if (1 == 1)
            {
                var userId = "#username";

                //var scrollToUser = await CefBrowser.JsScrollToElement(userId, 1, 2);
                //if (!scrollToUser)
                //{
                //    LogError($"Error scroll to user, {userId}");
                //    return false;
                //}

                var inputUser = await CefBrowser.JsInputElementValue(userId, user, 2);
                if (!inputUser)
                {
                    LogError($"Error input user <{user}>, {userId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input user <{user}>, {userId}");
                }
            }

            // Input Password
            if (1 == 1)
            {
                var passId = "#password";
                var inputPassword = await CefBrowser.JsInputElementValue(passId, password, 2);
                if (!inputPassword)
                {
                    LogError($"Error input password <{password}>, {passId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input password <{password}>, {passId}");
                }
            }

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
                var loginValue = await CefBrowser.JsGetInputValue(loginId, 2);
                var clickLogin = await CefBrowser.JsClickElement(loginId, 2);
                if (!await WaitAsync(2, ".app-loading")) return false;
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
            var inputErrors = await CefBrowser.JsTcbGetInputLoginErrorMessage(1);
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
                var inputErrors2 = await CefBrowser.JsGetElementInnerText(inputErrors2Id, 1);
                LogError($"Error, <{inputErrors2}>, {inputErrors2Id}");
                return false;
            }

            // Credential not registered
            var notRegisteredId = "#kc-page-title";
            var notRegisteredExists = await JsElementExists(notRegisteredId, 1);
            if (notRegisteredExists)
            {
                var notRegisteredText = await CefBrowser.JsGetElementInnerText(notRegisteredId, 1);
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
                    return false;
                }

                isExpired = await JsElementExists(expiredId, 1);
                if (isExpired)
                {
                    LogError($"Login session expired, {expiredId}, {iTry + 1}/{iTryMax}");
                    return false;
                }

                countdownExists = await JsElementExists(countdownId, 1);
                if (!countdownExists)
                {
                    break;
                }
                else
                {
                    var timeout = await CefBrowser.JsGetElementInnerText(countdownId, 1);
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
                            return false;
                        }
                        var resendText = await CefBrowser.JsGetElementInnerText(resendId, 1);
                        var clickResend = await CefBrowser.JsClickElement(resendId, 2);
                        await Task.Delay(2000);
                        if (!await WaitAsync(2, ".app-loading")) return false;
                        if (clickResend)
                        {
                            LogInfo($"Click <{resendText}>, {resendId}, {iTry + 1}/{iTryMax}");
                            iTry++;
                        }
                        else
                        {
                            LogError($"Error click <{resendText}>, {resendId}, {iTry + 1}/{iTryMax}");
                            return false;
                        }
                    }
                }
            }

            if (!await WaitAsync(2, ".app-loading")) return false;
            var isLoggedIn = await IsLoggedInAsync(Bank.TcbBank);
            if (!isLoggedIn)
                return false;

            return true;
        }

        private async Task TcbSetBalanceAsync()
        {
            var id = ".account-balance .account-balance__amount";
            var balance = await CefBrowser.JsGetElementInnerText(id, 2);
            if (balance != null)
            {
                DoSetBalance?.Invoke(this, balance.ToNumber(Bank.TcbBank).StrToDec());
            }
        }

        private async Task<bool> TcbRefreshAsync(string username, string password)
        {
            MyStopWatch sWatch;

            sWatch = new MyStopWatch(true);
            var loadBank = await LoadBankAsync(Bank.TcbBank);
            TcbReport.LoadBank = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!loadBank)
            {
                LogError($"Error load bank url");
                return false;
            }

            sWatch = new MyStopWatch(true);
            var isLoggedIn = await IsLoggedInAsync(Bank.TcbBank);
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

            await TcbSetBalanceAsync();

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
            var clickEle = await CefBrowser.JsClickElement(eleId, 2);
            await Task.Delay(2000);
            if (!await WaitAsync(2, ".ng-spinner-loader")) return false;
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
            var headerOffset = await CefBrowser.JsGetElementOffsetHeight(".tcb-page-layout__topbar", 1);

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

                    var clickSearch = await CefBrowser.MouseClickEvent(searchId, headerOffset.StrToInt(), 2);
                    if (!await WaitAsync(2, ".list-bank-dropdown .bb-loading-indicator")) return false;
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
                    var inputBankName = await CefBrowser.JsInputElementValue(searchInputId, bank.GetTcbShortName(), 2);
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
                    var benBankPosition = await CefBrowser.JsTcbGetBeneficiaryBankPosition(bank.GetTcbShortName(), 2);
                    if (benBankPosition == null)
                    {
                        TransferInfo.Status = TransferStatus.ErrorBank;
                        LogError($"Beneficiary bank not exists <{bank.GetTcbShortName()}>");
                        return false;
                    }
                    var iTry = 0;
                    var iTryMax = 3;
                    while (iTry < iTryMax)
                    {
                        iTry++;
                        await Task.Delay(1000);

                        var checkBenBankPosition = await CefBrowser.JsTcbGetBeneficiaryBankPosition(bank.GetTcbShortName(), 1);
                        if (checkBenBankPosition == null || checkBenBankPosition == benBankPosition) break;
                        else if (checkBenBankPosition != benBankPosition)
                        {
                            benBankPosition = checkBenBankPosition;
                        }
                    }
                    var clickBenBankName = CefBrowser.MouseClickJsPosition(benBankPosition, 2);
                    if (!await WaitAsync(2)) return false;
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
                        var clickClear = await CefBrowser.JsClickElement(clearAccNumId, 1);
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
                                var checkAccNum = await CefBrowser.JsGetInputValue(clearAccNumId, 1);
                                if (string.IsNullOrEmpty(checkAccNum)) break;
                                else await Task.Delay(1000);

                                if (iTry >= iTryMax) break;
                            }

                            // After clear, click on Amount
                            var clickAmountAfterClear = await CefBrowser.MouseClickEvent(amountId, headerOffset.StrToInt(), 2);
                            if (!await WaitAsync(2, ".bb-loading-indicator")) return false;
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
                    var emptyAccNum = await CefBrowser.JsInputElementValue(accNumId, "", 1);
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
                    var inputAccNum = await CefBrowser.InputToElement(accNumId, accNum, headerOffset.StrToInt(), 2);
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
                    var clickAmount = await CefBrowser.MouseClickEvent(amountId, headerOffset.StrToInt(), 2);
                    if (!await WaitAsync(2, ".bb-loading-indicator")) return false;
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

                        var invalidBenMsg = await CefBrowser.JsGetElementInnerText(invalidBenId, 2);
                        if (invalidBenMsg?.Trim().ToLower().VietnameseToEnglish() == TcbText.ServiceUnavailable.TcbTranslate(SelectedLang).ToLower().VietnameseToEnglish())
                        {
                            blnAccNum = false;
                            LogError($"Service unavailable <{accNum}>, <{invalidBenMsg}>, {invalidBenId}, {iRetryAccNum}/{iMaxAccNum}");

                            var closeId = "tcb-transfer-bill-error-modal button[tcbtracker=\"btn_close\"]";
                            var closeExists = await JsElementExists(closeId, 1);
                            if (closeExists)
                            {
                                var clickClose = await CefBrowser.JsClickElement(closeId, 2);
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
                            TransferInfo.ErrorMessage = $"卡号无效： {accNum}";
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
                var benName = await CefBrowser.JsGetInputValue(benNameId, 2);
                if (benName != null && benName.VietnameseToEnglish().ToLower().Trim() == accName.VietnameseToEnglish().ToLower().Trim())
                {
                    LogInfo($"Beneficiary name is <{benName}>, {benNameId}");
                }
                else
                {
                    TransferInfo.Status = TransferStatus.ErrorCard;
                    TransferInfo.ErrorMessage = $"收款人姓名不符： {accName}， 订单姓名： {benName}";
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
                var accBal = await CefBrowser.JsGetElementInnerText(accBalId, 2);
                if (accBal != null && accBal.StrToDec() >= amount)
                {
                    BalanceAfterTransfer = accBal.StrToDec() - amount;
                    LogInfo($"Balance is <{accBal}>, {amount}, {accBalId}");
                }
                else
                {
                    LogError($"Insufficient balance <{accBal}>, {amount}, {accBalId}");
                    return false;
                }
            }

            // Input Amount
            if (1 == 1)
            {
                var inputAmount = await CefBrowser.JsInputElementValue(amountId, amount.ToString(), 2);
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
                var msgId = transType == TransferType.Internal ?
                    "techcom-internal-transfer textarea[formcontrolname=\"description\"]" :
                    "techcom-otherbank-transfer textarea[formcontrolname=\"description\"]";
                var msgExists = await JsElementExists(msgId, 2);
                if (!msgExists)
                {
                    LogError($"Message input not exists, {msgId}");
                    return false;
                }
                var inputMsg = await CefBrowser.JsInputElementValue(msgId, remarks, 2);
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

            // Click on amount again to triger validation
            var clickAmount2 = await CefBrowser.MouseClickEvent(amountId, headerOffset.StrToInt(), 2);
            if (!clickAmount2)
            {
                LogError($"Error click Amount, {amountId}");
                return false;
            }
            else
            {
                LogInfo($"Click Amount, {amountId}");
            }

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
                var nextEnabled = !await CefBrowser.JsElementDisabled(nextId, 1);
                if (!nextEnabled)
                {
                    LogError($"Next button not enabled, {nextId}");
                    return false;
                }
                else
                {
                    LogInfo($"Next button enabled, {nextId}");
                }
                var clickNext = await CefBrowser.JsClickElement(nextId, 2);
                if (!await WaitAsync(2, "tcb-screen-loading")) return false; // Loading screen after click next
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
            var clickConfirm = await CefBrowser.JsClickElement(confirmId, 2);
            if (!await WaitAsync(2, "tcb-screen-loading")) return false;
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
                    return false;
                }

                isExpired = await JsElementExists(expiredId, 1);
                if (isExpired)
                {
                    LogInfo($"Transfer session expired, {expiredId}");
                    CheckHistory = true;
                    return true; // Proceed with check history
                }

                resendExists = await JsElementExists(resendId, 1);
                if (resendExists)
                {
                    var resendDisabled = await CefBrowser.JsElementDisabled(resendId, 1);
                    if (resendDisabled)
                    {
                        var timeout = await CefBrowser.JsGetElementInnerText(timeoutId, 2);
                        LogInfo($"Transfer authentication timeout <{timeout}>");
                        await Task.Delay(500);
                    }
                    else
                    {
                        LogInfo($"Transfer authentication timeout, {timeoutId}, {resendId}");
                        CheckHistory = true;
                        return true; // Proceed with check history
                    }
                }
                else
                {
                    LogInfo($"Transfer authenticated, {timeoutId}");
                    //await WaitAsync(2);
                    CheckHistory = true;
                    return true; // Proceed with check history
                }
            }
        }

        private async Task<bool> TcbCheckTransactionHistoryAsync(int iTry, string remarks)
        {
            var loadBank = await LoadBankAsync(Bank.TcbBank);
            if (!loadBank) return false;

            // Click to go Account Details
            var accountId = "techcom-account-summary-quick-view-widget .current-account__item";
            var accountExists = await JsElementExists(accountId, 5);
            if (!accountExists)
            {
                LogError($"{iTry}|Current account not exists, {accountId}");
                return false;
            }

            var headerOffset = await CefBrowser.JsGetElementOffsetHeight(".tcb-page-layout__topbar", 1);
            
            var clickAccount = await CefBrowser.MouseClickEvent(accountId, headerOffset.StrToInt(), 1);
            if (!clickAccount)
            {
                LogError($"{iTry}|Error click current account, {accountId}");
                return false;
            }
            else
            {
                LogInfo($"{iTry}|Click current account, {accountId}");
            }
            if (!await WaitAsync(2, "techcom-account-detail-widget-extended .bb-loading-indicator")) return false; // Wait Currenct Account Loading
            if (!await WaitAsync(2, "techcom-account-transaction-history .bb-loading-indicator")) return false; // Wait Transaction History Loading

            var transListId = "techcom-account-transaction-history techcom-account-transaction-history-item"; // Transaction History Items (Column)
            var transListExists = await JsElementExists(transListId, 2);
            if (!transListExists)
            {
                LogError($"{iTry}|Transaction history not exists, {transListId}");
                return false;
            }
            var transCount = await CefBrowser.JsGetElementCount(transListId, 1);
            if (transCount == null || transCount.StrToInt() <= 0)
            {
                LogError($"{iTry}|Transaction history has no record, {transListId}");
                return false;
            }
            else
            {
                LogInfo($"{iTry}|Transaction history has record <{transCount}>, {transListId}");
            }
            var clickFirst = await CefBrowser.JsClickElementAtIndex(transListId, 0, 1);
            if (!await WaitAsync(1)) return false;
            if (!clickFirst)
            {
                LogError($"{iTry}|Error click transaction history at index 0, {transListId}");
                return false;
            }
            else
            {
                LogInfo($"{iTry}|Click transaction history at index 0, {transListId}");
            }

            var transMessage = await CefBrowser.JsGetTcbMessageFromTransHistory(SelectedLang, 2); // remarks
            if (transMessage == null || transMessage.Trim().ToLower() != remarks.Trim().ToLower())
            {
                LogError($"{iTry}|Transaction message not match <{transMessage}>, {remarks}");
                return false;
            }
            else
            {
                LogInfo($"{iTry}|Transaction message matched <{transMessage}>, {remarks}");
            }

            var transIdText = await CefBrowser.JsGetTcbTransIdFromTransHistory(SelectedLang, 2);
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

        private async Task<bool> TcbTransferStatusAsync(string remarks)
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
                // Transfer success not showing,
                // Check transaction history
                // Possible transfer success

                if (!CheckHistory)
                {
                    LogError($"Transfer failed, {successId}");
                    return false;
                }

                var foundHistory = false;
                var iTry = 0;
                var iTryMax = 2;
                while (iTry < iTryMax)
                {
                    iTry++;
                    if (iTry != 1)
                        await Task.Delay(1000 * 10);

                    foundHistory = await TcbCheckTransactionHistoryAsync(iTry, remarks);
                    if (foundHistory) break;
                }
                if (!foundHistory)
                {
                    LogError($"Transfer failed, transaction history not found.");
                    return false;
                }

                TransferInfo.Status = TransferStatus.Success;
                LogInfo($"Transfer success with history");
                return true;
            }

            TransferInfo.Status = TransferStatus.Success;
            LogInfo($"Transfer success, {successId}");

            // Retrieve transaction id
            var transId = await CefBrowser.JsGetTcbTransactionId(SelectedLang, 2);
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
            TcbExtBank toBank, string toAccNum, string toAccName, decimal toAmount, string remarks)
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
            var transferSuccess = await TcbTransferStatusAsync(remarks);
            TcbReport.TransferStatus = (int)sWatch.Elapsed(true)?.TotalSeconds;
            if (!transferSuccess)
            {
                LogError($"Error transfer");
                return false;
            }

            return true;
        }

        #endregion

        #region Helper 
        private async Task<bool> WaitAsync(int waitSeconds = 3, params string[] loadingElements)
        {
            DateTime currTime = DateTime.Now;
            TimeSpan diff = currTime - LastLoadingAt;

            while (diff.TotalSeconds <= waitSeconds || IsLoading())
            {
                LogInfo($"Browser loading buffer, {diff.TotalSeconds.To3Decimal()}, {NrLoading}/{NrLoaded}/{CefRequestHandler.NrOfCalls}, {CefBrowser.Address}");
                await Task.Delay(1000);
                currTime = DateTime.Now;
                diff = currTime - LastLoadingAt;

                if (diff.TotalSeconds >= 90)
                {
                    LogError($"Browser loading timeout, {diff.TotalSeconds.To3Decimal()}, {NrLoading}/{NrLoaded}/{CefRequestHandler.NrOfCalls}, {CefBrowser.Address}");
                    return false;
                }
            }
            LogInfo($"Browser loaded, {diff.TotalSeconds.To3Decimal()}, {NrLoading}/{NrLoaded}/{CefRequestHandler.NrOfCalls}, {CefBrowser.Address}");

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
                case Bank.VcbBank:
                    result = await WaitBankAsync(2, "vcb-loading .loading");
                    break;
                default: break;
            }
            return result;
        }

        private async Task<bool> WaitBankSpecialAsync()
        {
            var result = true;
            switch (Bank)
            {
                case Bank.AcbBank:
                    result = await CefBrowser.JsGetElementDisplay("#acbone-loader-container", 2) != "block";
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
                    elementExists = await CefBrowser.JsEitherElementExists(2, loadingElements);
                    isLoading = !string.IsNullOrEmpty(elementExists) && await WaitBankSpecialAsync();

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
                        await ScreenshotAsync("Error_{0}_BankLoadingTimeout", true);
                        return false;
                    }

                    i++;
                }
            }
            LogInfo($"Bank loaded.");

            return true;
        }

        private async Task ScreenshotAsync(string fileName = "", bool captureOnRelease = false)
        {
            // If debug mode, capture
            // If not debug mode, and require capture on release, capture
            var blnGo = IsDebug || (!IsDebug && captureOnRelease);

            if (blnGo)
            {
                if (IsLoading())
                {
                    LogError($"Unable to capture screenshot while browser is still loading, {NrLoading}/{NrLoaded}/{CefRequestHandler.NrOfCalls}, {fileName}");
                    return;
                }

                try
                {
                    var contentSize = await CefBrowser.GetContentSizeAsync();
                    var viewport = new Viewport
                    {
                        Height = contentSize.Height,
                        Width = contentSize.Width,
                        Scale = 1.0,
                    };
                    var bitmap = await CefBrowser.CaptureScreenshotAsync(CaptureScreenshotFormat.Jpeg, 25, viewPort: viewport, captureBeyondViewport: true); // viewPort: viewport, captureBeyondViewport: true
                    var screenshotPath = Path.Combine(ImgPath, string.Format(fileName + ".jpeg", DateTime.Now.ToString("yyyyMMdd_HHmmss.fff")));
                    LogInfo($"Capturing screenshot, {screenshotPath}");
                    File.WriteAllBytes(screenshotPath, bitmap);
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
            var exists = await CefBrowser.JsElementExists(element, numOfTry);
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

        private async Task<bool> JsReloadAsync(int numOfTry)
        {
            var exists = await CefBrowser.JsReloadAsync(numOfTry);
            if (!exists.IsSuccess)
            {
                if (!string.IsNullOrWhiteSpace(exists?.ErrorMessage))
                {
                    LogError(exists.ErrorMessage);
                }
                else
                {
                    LogError($"Error executing JsElementExists, {numOfTry}");
                }
            }
            return exists.BoolResult;
        }

        #endregion

        #region API / HttpClient
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

        #endregion
    }
}
