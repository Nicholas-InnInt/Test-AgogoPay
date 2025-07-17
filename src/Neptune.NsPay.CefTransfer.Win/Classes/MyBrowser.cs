using CefSharp.DevTools.Page;
using CefSharp.OffScreen;
using CefSharp;
using Neptune.NsPay.CefTransfer.Common.Classes;
using Neptune.NsPay.CefTransfer.Common.Models;
using Neptune.NsPay.CefTransfer.Common.MyEnums;
using Neptune.NsPay.CefTransfer.Win.Helpers;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.WithdrawalOrders;

namespace Neptune.NsPay.CefTransfer.Win.Classes
{
    public class MyBrowser
    {
        #region Variables
        private CefSettings _settings;
        private ChromiumWebBrowser _browser;
        private IBrowserHost _browserHost;
        private MyRequestHandler _request;
        private System.Timers.Timer _busyTimer;
        private MyHttpClient _httpClient;

        private string _baseDir;
        private string _cachePath;
        private string _imgPath;
        private string _htmlPath;
        private int _numOfSuccess;
        private int _numOfFailure;
        private bool _isBusy;
        private Bank _bank;
        private string _orderNo;
        private int _iLoading;
        private int _iLoaded;
        private DateTime _lastLoadingTime = DateTime.MinValue;
        private DateTime _lastBankLoadingTime = DateTime.MinValue;
        private bool _isDebug;
        private bool _waitTimeout;
        private bool _waitManualLogin;
        private string _bankUrl;
        private Lang _lang;
        private bool _proxyEnable;
        private string _proxyAddress;
        private VcbVerificationCodeModel _vcbVerificationCode;
        private string _transactionCode;
        private string _transferTransCode;
        private string _user;
        private Stopwatch _reportStopWatch;
        private bool _reportStarted;
        private string _apiBaseUrl;
        private string _apiSign;
        private string _apiMerchantCode;
        private int _apiType;
        #endregion

        #region Property
        public ChromiumWebBrowser Browser { get => _browser; }
        public int NumOfSuccess { get => _numOfSuccess; set => _numOfSuccess = value; }
        public int NumOfFailure { get => _numOfFailure; set => _numOfFailure = value; }
        public bool WaitManualLogin { get => _waitManualLogin; }
        public bool EnteringCaptcha { get; set; }
        public string Captcha { get; set; }
        public string OTP { get; set; }
        public string TransferOTP { get; set; }
        public bool Capturing { get; set; }
        public bool EnteringOtp { get; set; }
        public bool EnteringTransOtp { get; set; }
        public VcbVerificationCodeModel VcbVerificationCode { get => _vcbVerificationCode; }
        public string TransactionCode { get => _transactionCode; }
        public string TransferTransCode { get => _transferTransCode; }
        public TransferStatus Status { get; private set; }
        public string TransactionId { get; private set; }
        public decimal BalanceAfterTransfer { get; private set; }
        public string OrderId { get; set; }
        public int DeviceId { get; set; }
        public ScreenshotModel Screenshot { get; private set; }
        public bool FirstTime { get; set; }
        public bool CheckHistory { get; private set; }
        public TcbReportModel TcbReport { get; private set; }
        public string TcbReportText
        {
            get
            {
                return TcbReport == null ? "" :
                    Regex.Replace(
                    @$"
                        {TcbReport.TransferType}|
                        {TcbReport.LoadBank}|
                        {TcbReport.IsLoggedIn}|
                        {TcbReport.GetLang}|
                        {TcbReport.Login}|
                        {TcbReport.LoginAuth}|
                        {TcbReport.GoTransfer}|
                        {TcbReport.InputTransfer}|
                        {TcbReport.ConfirmTransfer}|
                        {TcbReport.TransferAuth}|
                        {TcbReport.TransferStatus}
                    ", @"\s+", string.Empty);
            }
        }
        public VcbReportModel VcbReport { get; private set; }
        public string VcbReportText
        {
            get
            {
                return VcbReport == null ? "" :
                    Regex.Replace(
                    @$"
                        {VcbReport.TransferType}|
                        {VcbReport.LoadBank}|
                        {VcbReport.IsLoggedIn}|
                        {VcbReport.GetLang}|
                        {VcbReport.Login}|
                        {VcbReport.GoTransfer}|
                        {VcbReport.InputTransfer1}|
                        {VcbReport.InputTransfer2}|
                        {VcbReport.InputTransfer3}|
                        {VcbReport.TransferStatus}
                    ", @"\s+", string.Empty);
            }
        }
        public string TransferErrorMessage { get; set; }
        #endregion

        public MyBrowser()
        {
            _isDebug = false;
#if DEBUG
            _isDebug = true;
#endif
            _baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _cachePath = Path.Combine(_baseDir, "cache");
            _imgPath = Path.Combine(_baseDir, "img");
            _htmlPath = Path.Combine(_baseDir, "html");

            _busyTimer = new System.Timers.Timer();
            _busyTimer.Interval = 1000;
            _busyTimer.Elapsed += busyTimer_Elapsed;
            _busyTimer.Enabled = true;

            _waitManualLogin = false;
        }

        #region Event
        public event EventHandler<string> WriteUiMessage;
        public event EventHandler<bool> IsBusy;
        public event EventHandler<VcbVerificationCodeModel> VcbGetCaptcha;
        public event EventHandler<ScreenshotModel?> GetScreenshot;
        public event EventHandler<string> GetTransactionCode;
        public event EventHandler<string> GetTransferTransactionCode;
        public event EventHandler<bool> ShowLoginPage1;
        public event EventHandler<bool> EditingLoginPage1;
        public event EventHandler<bool> ShowLoginPage3;
        public event EventHandler<bool> EditingLoginPage3;
        public event EventHandler<bool> ShowTransferPage3;
        public event EventHandler<bool> EditingTransferPage3;
        public event EventHandler<int> SetCountdown;
        public event EventHandler<bool> TcbShowLoginAuth;
        public event EventHandler<decimal> DoSetBalance;

        private void browser_LoadingStateChanged(object? sender, LoadingStateChangedEventArgs e)
        {
            try
            {
                if (e.IsLoading == false)
                {
                    _iLoaded = _request.NrOfCalls;
                }
                else
                {
                    _lastLoadingTime = DateTime.Now;
                    _iLoading = _request.NrOfCalls;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error LoadingStateChanged, {ex}", $"Error LoadingStateChanged, {ex.Message}");
            }
        }

        public void browser_OnResourceLoadComplete(IRequest request)
        {
            var pattern = string.Empty;
            try
            {
                pattern = @"^https:\/\/digiapp.vietcombank.com.vn.*captcha.*$";
                if (new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(request.Url))
                {
                    var filter = MyFilterManager.GetFilter(request.Identifier.ToString()) as MyIResponseFilter;
                    var data = filter.DataAll.ToArray();
                    var uri = new Uri(request.Url);
                    var iSegment = Array.FindIndex(uri.Segments, s => s.ToLower() == "captcha/");
                    if (uri.Segments.Length > iSegment)
                    {
                        var captchaId = uri.Segments[iSegment + 1];
                        _vcbVerificationCode = new VcbVerificationCodeModel() { Url = request.Url, CaptchatId = captchaId, CaptchaImage = data };
                        VcbGetCaptcha?.Invoke(this, _vcbVerificationCode);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error browser_OnResourceLoadComplete, {ex}", $"Error browser_OnResourceLoadComplete, {ex.Message}");
            }
        }

        private void busyTimer_Elapsed(object? source, System.Timers.ElapsedEventArgs e)
        {
            IsBusy?.Invoke(this, _isBusy);
        }

        #endregion

        #region Public
        public async Task Init()
        {
            try
            {
                EnteringCaptcha = false;
                EnteringOtp = false;
                EnteringTransOtp = false;

                if (!Directory.Exists(_cachePath))
                    Directory.CreateDirectory(_cachePath);

                if (!Directory.Exists(_imgPath))
                    Directory.CreateDirectory(_imgPath);

                if (!Directory.Exists(_htmlPath))
                    Directory.CreateDirectory(_htmlPath);

                await InitCefAsync();
                FirstTime = true;

                _apiBaseUrl = AppSettings.Configuration["NsPayApi:BaseApiUrl"];
                _apiSign = AppSettings.Configuration["NsPayApi:Sign"];
                _apiMerchantCode = AppSettings.Configuration["NsPayApi:MerchantCode"];
                _apiType = Convert.ToInt32(AppSettings.Configuration["NsPayApi:Type"]);

                _httpClient = new MyHttpClient(_apiBaseUrl, _apiMerchantCode, _apiType);
            }
            catch (Exception ex)
            {
                LogError($"Error Init, {ex}", $"Error Init, {ex.Message}");
            }
        }

        public async Task<DeviceModel?> GetDevice(string phone, WithdrawalDevicesBankTypeEnum bankType)
        {
            var result = await _httpClient.GetDeviceAsync(phone, bankType);
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
            var result = await _httpClient.GetWithdrawOrderAsync(phone, bankType);
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
            var result = await _httpClient.CheckWithdrawOrderAsync(phone, bankType);
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
            var result = await _httpClient.UpdateWithdrawOrderAsync(orderId, transactionNo, orderStatus, remark);
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
            // front end
            switch (_bank)
            {
                case Bank.TcbBank:
                    DoSetBalance?.Invoke(this, balance);
                    break;

                default: break;
            }

            // back end
            var result = await _httpClient.UpdateBalanceAsync(deviceId, balance);
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

        public async Task<bool> TransferAsync(Bank fromBank, string orderNo, string user, string password, string toBank,
            string toAccNum, string toAccName, decimal toAmount, string remarks)
        {
            _bank = fromBank;
            _orderNo = orderNo;
            _user = user;

            CheckHistory = false;
            Status = TransferStatus.Failed;
            TransferErrorMessage = string.Empty;
            TransactionId = string.Empty;
            BalanceAfterTransfer = 0.00M;
            TcbReport = new TcbReportModel();
            VcbReport = new VcbReportModel();

            var result = false;
            try
            {
                TransferType transType;
                string? sFileName;
                switch (_bank)
                {
                    case Bank.TcbBank:
                        transType = TransferType.Internal;
                        var tcbToBank = toBank.ToTcbExtBank();
                        if (tcbToBank != TcbExtBank.Techcombank_TCB)
                        {
                            // External Transfer
                            transType = TransferType.External;
                        }
                        result = await TcbTransferAsync(transType, user, password, tcbToBank, toAccNum, toAccName, toAmount, remarks.VietnameseToEnglish());
                        TcbReport.Result = result;
                        TcbReport.TransferType = transType == TransferType.Internal ? "I" : "E";

                        if (!result)
                        {
                            var htmlFileName = $"{_orderNo}_ErrHtml" + "_{0}";
                            await _browser.WriteHtmlToFile(_htmlPath, htmlFileName);
                        }

                        sFileName = _orderNo + (result ? "" : "_Err") + "_{0}";
                        await ScreenshotAsync(sFileName, true);

                        break;

                    case Bank.VcbBank:
                        transType = TransferType.Internal;
                        var vcbToBank = toBank.ToVcbExtBank();
                        if (vcbToBank != VcbExtBank.VCB)
                        {
                            // External Transfer
                            transType = TransferType.External;
                        }
                        result = await VcbTransferAsync(transType, user, password, vcbToBank, toAccNum, toAccName, toAmount, remarks.VietnameseToEnglish());
                        VcbReport.Result = result;
                        VcbReport.TransferType = transType == TransferType.Internal ? "I" : "E";

                        if (!result)
                        {
                            var htmlFileName = $"{_orderNo}_ErrHtml" + "_{0}";
                            await _browser.WriteHtmlToFile(_htmlPath, htmlFileName);
                        }

                        sFileName = _orderNo + (result ? "" : "_Err") + "_{0}";
                        await ScreenshotAsync(sFileName, true);

                        break;

                    default: break;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error TransferAsync, {ex}", $"Error TransferAsync, {ex.Message}");
            }

            return result;
        }

        public async Task<bool> RefreshAsync(Bank fromBank, string user, string password)
        {
            _user = user;
            _bank = fromBank;
            TcbReport = new TcbReportModel();
            VcbReport = new VcbReportModel();

            var result = false;

            switch (_bank)
            {
                case Bank.TcbBank:
                    result = await TcbRefreshAsync(user, password);
                    if (!result)
                    {
                        var htmlFileName = $"Refresh_Html" + "_{0}";
                        await _browser.WriteHtmlToFile(_htmlPath, htmlFileName);

                        await ScreenshotAsync("Error_{0}_Refresh", true);
                    }
                    break;

                case Bank.VcbBank:
                    result = await VcbRefreshAsync(user, password);
                    if (!result)
                    {
                        var htmlFileName = $"Refresh_Html" + "_{0}";
                        await _browser.WriteHtmlToFile(_htmlPath, htmlFileName);

                        await ScreenshotAsync("Error_{0}_Refresh", true);
                    }
                    break;

                default: break;
            }

            return result;
        }

        public async Task ShowScreenshot(bool show = true)
        {
            if (!show)
                GetScreenshot?.Invoke(this, null);
            else
            {
                Capturing = true;

                var screenshot = await GetScreenshotAsync();
                if (screenshot != null)
                {
                    Screenshot = new ScreenshotModel() { Image = screenshot, CapturedAt = DateTime.Now };
                }
                GetScreenshot?.Invoke(this, Screenshot);
                Capturing = false;
            }
        }

        public bool SaveScreenshot()
        {
            try
            {
                if (Screenshot != null)
                {
                    var imgPath = Path.Combine(_imgPath, string.Format("Screenshot_{0}.jpeg", Screenshot.CapturedAt.ToString("yyyyMMdd_HHmmss.fff")));
                    using (var image = Screenshot.Image.ToBitmap())
                    {
                        image.Save(imgPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    LogInfo($"Screenshot saved to {imgPath}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error SaveScreenshot, {ex}", $"Error SaveScreenshot, {ex.Message}");
            }
            return false;
        }

        //public async Task WriteHtmlDebug() // For debug only
        //{
        //    var htmlFileName = $"Debug_Html" + "_{0}";
        //    await _browser.WriteHtmlToFile(_htmlPath, htmlFileName);
        //}

        public void LogInfo(string msg)
        {
            if (_isDebug) Debug.WriteLine(msg);
            NlogLogger.Info(msg);
            WriteUiMessage?.Invoke(this, msg);
        }

        public void LogError(string msg, string infoMsg = "")
        {
            if (_isDebug) Debug.WriteLine(msg);
            if (!string.IsNullOrWhiteSpace(infoMsg))
            {
                NlogLogger.Info(infoMsg);
                WriteUiMessage?.Invoke(this, infoMsg);
            }
            else
            {
                NlogLogger.Info(msg);
                WriteUiMessage?.Invoke(this, msg);
            }
            NlogLogger.Error(msg);
        }

        #endregion

        #region Private
        private async Task InitCefAsync()
        {
            Cef.EnableWaitForBrowsersToClose();

            ClearCache();

            _settings = new CefSettings() { CachePath = _cachePath };

            _proxyEnable = Convert.ToBoolean(AppSettings.Configuration["Proxy:Enable"]);
            _proxyAddress = string.Empty;
            if (_proxyEnable)
            {
                _proxyAddress = Convert.ToString(AppSettings.Configuration["Proxy:Address"]);
                _settings.CefCommandLineArgs.Add("proxy-server", _proxyAddress);
            }

            var success = await Cef.InitializeAsync(_settings);
            if (!success)
                throw new Exception("Error initialise CEF");

            var browserSettings = new BrowserSettings { WindowlessFrameRate = 1 };
            var requestContextSettings = new RequestContextSettings { CachePath = _cachePath };
            _request = new MyRequestHandler(this);
            _browser = new ChromiumWebBrowser("https://www.google.com/", browserSettings) { RequestHandler = _request };
            _browser.LoadingStateChanged += browser_LoadingStateChanged;

            var initialLoadResponse = await _browser.WaitForInitialLoadAsync();
            if (!initialLoadResponse.Success)
            {
                throw new Exception($"Page load failed with ErrorCode:{initialLoadResponse.ErrorCode}, HttpStatusCode:{initialLoadResponse.HttpStatusCode}");
            }

            var onUi = Cef.CurrentlyOnThread(CefThreadIds.TID_UI);

            _browserHost = _browser.GetBrowserHost();
            //You can call Invalidate to redraw/refresh the image
            _browserHost.Invalidate(PaintElementType.View);
            await Task.Delay(50);
            if (!await WaitAsync(2)) return;
        }

        private async Task ScreenshotAsync(string fileName = "", bool captureOnRelease = false)
        {
            // If debug mode, capture
            // If not debug mode, and require capture on release, capture
            var blnGo = _isDebug || (!_isDebug && captureOnRelease);

            if (blnGo)
            {
                if (IsLoading())
                {
                    _waitTimeout = true;
                    LogError($"Unable to capture screenshot while browser is still loading, {_iLoading}/{_iLoaded}/{_request.NrOfCalls}, {fileName}");
                    return;
                }

                try
                {
                    var contentSize = await _browser.GetContentSizeAsync();
                    var viewport = new Viewport
                    {
                        Height = contentSize.Height,
                        Width = contentSize.Width,
                        Scale = 1.0,
                    };
                    var bitmap = await _browser.CaptureScreenshotAsync(CaptureScreenshotFormat.Jpeg, 25, viewport: viewport); // viewPort: viewport, captureBeyondViewport: true
                    var screenshotPath = Path.Combine(_imgPath, string.Format(fileName + ".jpeg", DateTime.Now.ToString("yyyyMMdd_HHmmss.fff")));
                    LogInfo($"Capturing screenshot, {screenshotPath}");
                    File.WriteAllBytes(screenshotPath, bitmap);
                }
                catch (Exception ex)
                {
                    LogError($"Screenshot failed, {fileName}, {ex}", $"Screenshot failed, {fileName}, {ex.Message}");
                }
            }
        }

        private async Task<byte[]?> GetScreenshotAsync()
        {
            if (IsLoading())
            {
                _waitTimeout = true;
                LogError($"Unable to capture screenshot while browser is still loading, {_iLoading}/{_iLoaded}/{_request.NrOfCalls}");
                return null;
            }

            try
            {
                var contentSize = await _browser.GetContentSizeAsync();
                var viewport = new Viewport
                {
                    Height = contentSize.Height,
                    Width = contentSize.Width,
                    Scale = 1.0,
                };
                return await _browser.CaptureScreenshotAsync(CaptureScreenshotFormat.Jpeg, 25, viewport: viewport);
            }
            catch (Exception ex)
            {
                LogError($"Error screenshot,{ex}", $"Error screenshot, {ex.Message}");
            }

            return null;
        }

        private bool IsLoading()
        {
            var isLoading = _iLoaded != _request.NrOfCalls;
            return isLoading;
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
                    elementExists = await _browser.JsEitherElementExists(2, loadingElements);
                    isLoading = !string.IsNullOrEmpty(elementExists);

                    if (isLoading)
                    {
                        _lastBankLoadingTime = DateTime.Now;
                        //if (IsEachFiveInterval(i))
                        LogInfo($"Bank is loading, {elementExists}, {i}");
                        await Task.Delay(1000);
                    }
                    else
                    {
                        var currTime = DateTime.Now;
                        TimeSpan diff = currTime - _lastBankLoadingTime;

                        while (diff.TotalSeconds <= waitSeconds)
                        {
                            //if (IsEachFiveInterval(diff.TotalSeconds))
                            LogInfo($"Bank loading buffer, {diff.TotalSeconds.To3Decimal()}");
                            await Task.Delay(1000);
                            currTime = DateTime.Now;
                            diff = currTime - _lastBankLoadingTime;
                        }
                    }

                    if (i >= 90)
                    {
                        _waitTimeout = true;
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

        private async Task<bool> WaitAsync(int waitSeconds, params string[] loadingElements)
        {
            DateTime currTime = DateTime.Now;
            TimeSpan diff = currTime - _lastLoadingTime;

            while (diff.TotalSeconds <= waitSeconds || IsLoading())
            {
                //if (IsEachFiveInterval(diff.TotalSeconds))
                LogInfo($"Browser loading buffer, {diff.TotalSeconds.To3Decimal()}, {_iLoading}/{_iLoaded}/{_request.NrOfCalls}, {_browser.Address}");
                await Task.Delay(1000);
                currTime = DateTime.Now;
                diff = currTime - _lastLoadingTime;

                if (diff.TotalSeconds >= 90)
                {
                    _waitTimeout = true;
                    LogError($"Browser loading timeout, {diff.TotalSeconds.To3Decimal()}, {_iLoading}/{_iLoaded}/{_request.NrOfCalls}, {_browser.Address}");
                    return false;
                }
            }
            LogInfo($"Browser loaded, {diff.TotalSeconds.To3Decimal()}, {_iLoading}/{_iLoaded}/{_request.NrOfCalls}, {_browser.Address}");

            return await WaitBankAsync(waitSeconds, loadingElements);
        }

        private void ClearCache()
        {
            var dir = new DirectoryInfo(_cachePath);
            if (!dir.Exists)
                return;

            foreach (var d in dir.EnumerateDirectories())
            {
                Directory.Delete(d.FullName, true);
            }

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                file.Delete();
            }
        }

        #endregion

        #region Common

        private void StartReportStopWatch()
        {
            _reportStarted = true;
            _reportStopWatch = Stopwatch.StartNew();
        }

        private int StopReportStopWatch()
        {
            if (_reportStarted)
            {
                _reportStopWatch.Stop();
                _reportStarted = true;
                return (int)_reportStopWatch.Elapsed.TotalSeconds;
            }
            return 0;
        }

        #endregion

        #region VCB
        private async Task<bool> VcbLoadBank()
        {
            _bankUrl = _bank.GetBank().url;
            if (_bankUrl == null)
            {
                LogError($"Error, empty bank url, {_bankUrl}");
                return false;
            }

            _browser.LoadUrl(_bankUrl);
            if (!await WaitAsync(2, "app-loader .loading")) return false;
            _browser.LoadUrl(_bankUrl);
            if (!await WaitAsync(2, "app-loader .loading")) return false;

            LogInfo("Load bank successfully");
            return true;
        }

        private async Task<bool> VcbIsLoginPage1()
        {
            var url = "https://vcbdigibank.vietcombank.com.vn/login";
            var urlMatch = _browser.Address.ToLower().StartsWith(url);
            var loginId = "app-login-form";
            var loginExists = await _browser.JsElementExists(loginId, 1);

            return urlMatch && loginExists;
        }

        private async Task<bool> VcbIsLoginPage2()
        {
            var url = "https://vcbdigibank.vietcombank.com.vn/login";
            var urlMatch = _browser.Address.ToLower().StartsWith(url);
            var selectId = "app-trust-browser";
            var selectIdExists = await _browser.JsElementExists(selectId, 1);

            return urlMatch && selectIdExists;
        }

        private async Task<bool> VcbIsLoginPage3()
        {
            var url = "https://vcbdigibank.vietcombank.com.vn/login";
            var urlMatch = _browser.Address.ToLower().StartsWith(url);
            var otpId = "app-trust-browser-smart";
            var optIdExists = await _browser.JsElementExists(otpId, 1);

            return urlMatch && optIdExists;
        }

        private async Task<bool> VcbIsHomePage()
        {
            var url = "https://vcbdigibank.vietcombank.com.vn/";
            var urlMatch = _browser.Address.ToLower() == url;
            var homeId = ".sidebar a.sidebar-mini--home";
            var homeExists = await _browser.JsElementExists(homeId, 1);

            return urlMatch && homeExists;
        }

        public async Task<bool> VcbIsLoggedIn()
        {
            var isLoginPage1 = await VcbIsLoginPage1();
            var isLoginPage2 = await VcbIsLoginPage2();
            var isLoginPage3 = await VcbIsLoginPage3();
            var isHomePage = await VcbIsHomePage();

            var isLoggedIn = !isLoginPage1 && !isLoginPage2 && !isLoginPage3 && isHomePage;
            if (isLoggedIn)
            {
                LogInfo("Logged in");
                return true; // Logged in and Home button exists
            }

            LogInfo("Not logged in");
            return false; // Not logged in
        }

        private async Task<bool> VcbIsTransferPage1(TransferType transType)
        {
            var url = transType == TransferType.Internal ?
                "https://vcbdigibank.vietcombank.com.vn/chuyentien/chuyentientronghethong" :
                "https://vcbdigibank.vietcombank.com.vn/chuyentien/chuyentienquataikhoan";
            var urlMatch = _browser.Address.ToLower() == url;
            var pageId = "form[id=\"Step1\"]";
            var pageExists = await _browser.JsElementExists(pageId, 1);

            return urlMatch && pageExists;
        }

        private async Task<bool> VcbIsTransferPage2(TransferType transType)
        {
            var url = transType == TransferType.Internal ?
                "https://vcbdigibank.vietcombank.com.vn/chuyentien/chuyentientronghethong" :
                "https://vcbdigibank.vietcombank.com.vn/chuyentien/chuyentienquataikhoan";
            var urlMatch = _browser.Address.ToLower() == url;
            var pageId = "form[id=\"Step2\"]";
            var pageExists = await _browser.JsElementExists(pageId, 1);

            return urlMatch && pageExists;
        }

        private async Task<bool> VcbIsTransferPage3(TransferType transType)
        {
            var url = transType == TransferType.Internal ?
                "https://vcbdigibank.vietcombank.com.vn/chuyentien/chuyentientronghethong" :
                "https://vcbdigibank.vietcombank.com.vn/chuyentien/chuyentienquataikhoan";
            var urlMatch = _browser.Address.ToLower() == url;
            var pageId = "form[id=\"Step3\"]";
            var pageExists = await _browser.JsElementExists(pageId, 1);

            return urlMatch && pageExists;
        }

        private async Task<bool> VcbGetLanguage()
        {
            var langId = "footer div.footer-item-globe span";
            var langIdExists = await _browser.JsElementExists(langId, 2);
            if (!langIdExists)
            {
                LogError($"Language not exists, {langId}");
                return false;
            }

            string? selectedLang = null;
            string? langText = null;
            var iTry = 0;
            while (iTry < 3)
            {
                selectedLang = null;
                langText = await _browser.JsGetElementInnerHtml(langId, 2);
                if (langText == null)
                {
                    LogError($"Error language is empty, {langId}");
                    return false;
                }

                langText = langText.Trim();
                if (langText.ToLower() == "English".ToLower())
                {
                    _lang = Lang.Vietnamese;
                    selectedLang = "Vietnamese";
                }
                else if (langText.ToLower() == "Vietnamese".ToLower())
                {
                    _lang = Lang.English;
                    selectedLang = "English";
                }
                else
                {
                    LogError($"Invalid language <{langText}>, {langId}");
                    return false;
                }

                //if (_lang == Lang.English) break; // Switch to EN
                if (_lang == (_isDebug ? Lang.English : Lang.Vietnamese)) break; // Switch to VN if not debug
                else
                {
                    var clickLang = await _browser.JsClickElement(langId, 2);
                    if (!clickLang)
                    {
                        LogError($"Error click language, {langId}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Click language, {langId}");
                    }
                    await Task.Delay(1000);
                }
                iTry++;
            }
            LogInfo($"Language is <{selectedLang}>");

            return true;
        }

        private async Task<bool> VcbInputLoginPage1(string user, string password)
        {
            if (!await VcbIsLoginPage1())
                return true; // Skip

            var userId = _bank.GetBank().ele_user;
            var userExists = await _browser.JsElementExists(userId, 2);
            if (!userExists)
            {
                LogError($"User input not exists, {userId}");
                return false;
            }
            var passId = _bank.GetBank().ele_password;
            var passExists = await _browser.JsElementExists(passId, 2);
            if (!passExists)
            {
                LogError($"Password input not exists, {passId}");
                return false;
            }
            var captchaId = "input[formcontrolname=\"captcha\"]";
            var captchaExists = await _browser.JsElementExists(captchaId, 2);
            if (!captchaExists)
            {
                LogError($"Captha input not exists, {captchaId}");
                return false;
            }
            var loginId = "#btnLogin";
            var loginExists = await _browser.JsElementExists(loginId, 2);
            if (!loginExists)
            {
                LogError($"Login button not exists, {loginId}");
                return false;
            }

            var inputUser = await _browser.JsInputElementValue(userId, user, 2);
            if (!inputUser)
            {
                LogError($"Error input user <{user}>, {userId}");
                return false;
            }
            else
            {
                LogInfo($"Input user <{user}>, {userId}");
            }

            var inputPass = await _browser.JsInputElementValue(passId, password, 2);
            if (!inputPass)
            {
                LogError($"Error input password <{password}>, {passId}");
                return false;
            }
            else
            {
                LogInfo($"Input password <{password}>, {passId}");
            }

            ShowLoginPage1?.Invoke(this, true);

            EnteringCaptcha = true;
            EnteringOtp = false;
            EnteringTransOtp = false;
            var iTry = 0;
            var iMax = 1;
            while (iTry < iMax && await VcbIsLoginPage1())
            {
                iTry++;
                EditingLoginPage1?.Invoke(this, true);
                EnteringCaptcha = true;
                await ShowScreenshot();

                var iTryEnter = 0;
                var iMaxEnter = 60;
                SetCountdown?.Invoke(this, iMaxEnter);
                while (iTryEnter < iMaxEnter)
                {
                    iTryEnter++;
                    SetCountdown?.Invoke(this, iMaxEnter - iTryEnter);
                    LogInfo($"Please enter Captcha and click <Enter Captcha>, {iTry}/{iMax}, {iTryEnter}/{iMaxEnter}");

                    if (iTryEnter != 1)
                        await Task.Delay(1000);

                    if (!await VcbIsLoginPage1()) break;

                    if (!EnteringCaptcha) break;
                }
                EditingLoginPage1?.Invoke(this, false);

                if (string.IsNullOrWhiteSpace(Captcha))
                {
                    LogError($"Captcha is empty");
                    if (iTry < iMax) continue; else return false;
                }

                var inputCaptcha = await _browser.JsInputElementValue(captchaId, Captcha, 2);
                if (!inputCaptcha)
                {
                    LogError($"Error input Captcha <{Captcha}>, {captchaId}, {iTry}/{iMax},");
                    if (iTry < iMax) continue; else return false;
                }
                else
                {
                    LogInfo($"Input Captcha <{Captcha}>, {captchaId}, {iTry}/{iMax},");
                }

                var clickLogin = await _browser.JsClickElement(loginId, 2);
                if (!await WaitAsync(2, "app-loader .loading")) return false;
                if (!clickLogin)
                {
                    LogError($"Error click login, {loginId}, {iTry}/{iMax}");
                    return false;
                }
                else
                {
                    LogInfo($"Click login, {loginId}, {iTry}/{iMax}");
                }

                var error1Id = "app-login-form .login-alert.login-error";
                var error1Exists = await _browser.JsElementExists(error1Id, 1);
                if (error1Exists)
                {
                    var error1 = await _browser.JsGetElementInnerText(error1Id, 1);
                    LogError($"Login failed - 1, <{error1}>, {iTry}/{iMax}, {error1Id}");
                    if (iTry < iMax) continue; else return false;
                }
            }

            return true;
        }

        private async Task<bool> VcbInputLoginPage2()
        {
            if (!await VcbIsLoginPage2())
                return true; // Skip

            EnteringCaptcha = false;
            EnteringOtp = false;
            EnteringTransOtp = false;

            var otpId = "app-trust-browser app-select2 select";
            var otpExists = await _browser.JsElementExists(otpId, 2);
            if (!otpExists)
            {
                LogError($"Authentication method dropdown not exists, {otpId}");
                return false;
            }

            var clickOtp = await _browser.MouseClickEvent(otpId, 2);
            if (!clickOtp)
            {
                LogError($"Error click OTP dropdown, {otpId}");
                return false;
            }
            else
            {
                LogInfo($"Click OTP dropdown, {otpId}");
            }
            clickOtp = await _browser.MouseClickEvent(otpId, 2, 5, 5 + 105); // Click at position
            if (!clickOtp)
            {
                LogError($"Error click OTP dropdown at position, {otpId}");
                return false;
            }
            else
            {
                LogInfo($"Click OTP dropdown at position, {otpId}");
            }

            var otpValue = await _browser.JsGetInputValue(otpId, 2);
            if (otpValue != "5") // 5 is VCB-Smart OTP
            {
                LogError($"Error click VCB-Smart OTP, {otpId}");
                return false;
            }
            else
            {
                LogInfo($"Authentication method is <{otpValue}>, {otpId}");
            }

            var submitId = "app-trust-browser button[type=\"submit\"]";
            var submitExists = await _browser.JsElementExists(submitId, 2);
            if (!submitExists)
            {
                LogError($"Continue button not exists, {submitId}");
                return false;
            }
            var clickSubmit = await _browser.JsClickElement(submitId, 2);
            if (!await WaitAsync(2, "app-loader .loading")) return false;
            if (!clickSubmit)
            {
                LogError($"Error click Continue, {submitId}");
                return false;
            }
            else
            {
                LogInfo($"Click Continue, {submitId}");
            }

            // Possible error
            // 1: Transaction is expired. Please try again 
            var popup = await VcbPopup(true);
            if (!popup)
                return false;

            return true;
        }

        private async Task<bool> VcbInputLoginPage3()
        {
            if (!await VcbIsLoginPage3())
                return true; // Skip

            var transCodeId = "app-trust-browser-smart input[disabled]";
            var transCodeExists = await _browser.JsElementExists(transCodeId, 2);
            if (!transCodeExists)
            {
                LogError($"Transaction Code input not exists, {transCodeId}");
                return false;
            }
            var transCode = await _browser.JsGetInputValue(transCodeId, 2);
            if (string.IsNullOrEmpty(transCode))
            {
                LogError($"Transaction Code empty, {transCodeId}");
                return false;
            }
            else
            {
                LogInfo($"Transaction Code is <{transCode}>");
                VcbSetRedisTransactionCode(_user, transCode); // Retrieving OTP in 2 way, redis or manual key in
            }

            ShowLoginPage3?.Invoke(this, true);
            _transactionCode = transCode;
            GetTransactionCode?.Invoke(this, transCode);

            EnteringCaptcha = false;
            EnteringOtp = true;
            EnteringTransOtp = false;

            var iTry = 0;
            var iMax = 1;
            while (iTry < iMax && await VcbIsLoginPage3())
            {
                iTry++;
                OTP = string.Empty;
                EnteringOtp = true;
                EditingLoginPage3?.Invoke(this, true);
                await ShowScreenshot();

                var otpId = "app-trust-browser-smart input[formcontrolname=\"otp\"]";
                var otpExists = await _browser.JsElementExists(otpId, 2);
                if (!otpExists)
                {
                    LogError($"OTP input not exists, {otpId}");
                    return false;
                }

                var iTryOtp = 0;
                var iOtpMax = 90;
                SetCountdown?.Invoke(this, iOtpMax);
                while (iTryOtp < iOtpMax)
                {
                    iTryOtp++;
                    SetCountdown?.Invoke(this, iOtpMax - iTryOtp);
                    LogInfo($"Retrieving OTP from redis / Please enter OTP and click <Enter OTP> , {iTry}/{iMax}, {iTryOtp}/{iOtpMax}");

                    if (iTryOtp != 1)
                        await Task.Delay(1000);

                    if (!await VcbIsLoginPage3()) break;

                    if (!EnteringOtp) break;

                    var otp = VcbGetRedisOtp(_user);
                    if (otp != null)
                    {
                        OTP = otp;
                        EnteringOtp = false;
                        LogInfo($"Retrieved OTP from redis <{otp}>");
                        break;
                    }
                }
                EditingLoginPage3?.Invoke(this, false);

                var inputOTP = await _browser.JsInputElementValue(otpId, OTP, 2);
                if (!inputOTP)
                {
                    LogError($"Error input OTP <{OTP}>, {otpId}, {iTry}/{iMax}, {iTryOtp}/{iOtpMax}");
                    if (iTry < iMax) continue; else return false;
                }
                else
                {
                    LogInfo($"Input OTP <{OTP}>, {otpId}, {iTry}/{iMax}, {iTryOtp}/{iOtpMax}");
                }

                var submitId = "app-trust-browser-smart button.ubg-primary";
                var submitExists = await _browser.JsElementExists(submitId, 2);
                if (!submitExists)
                {
                    LogError($"Continue button not exists, {submitId}, {iTry}/{iMax}, {iTryOtp}/{iOtpMax}");
                    return false;
                }
                var clickSubmit = await _browser.JsClickElement(submitId, 2);
                if (!await WaitAsync(2, "app-loader .loading")) return false;
                if (!clickSubmit)
                {
                    LogError($"Error click Continue, {submitId}, {iTry}/{iMax}, {iTryOtp}/{iOtpMax}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Continue, {submitId}, {iTry}/{iMax}, {iTryOtp}/{iOtpMax}");
                }

                var error1Id = "app-trust-browser-smart label.error";
                var error1Exists = await _browser.JsElementExists(error1Id, 1);
                if (error1Exists)
                {
                    var error1 = await _browser.JsGetElementInnerText(error1Id, 1);
                    LogError($"Login failed - 3, <{error1}>, {error1Id}, {iTry}/{iMax}, {iTryOtp}/{iOtpMax}");
                    if (iTry < iMax) continue; else return false;
                }

                // Popup can be:
                // 1. OTP is not correct. Please try again.
                // 2. Successful login authentication. Do you want to save your Web browser to skip the authentication steps for the next logins?
                var popup = await VcbPopup();
                if (!popup)
                    return false;
            }

            return true;
        }

        private async Task<bool> VcbPopup(bool popupIsError = false)
        {
            // Any popup appear, click accept button

            var popupId = "app-my-dialog .modal-body";
            var popupExists = await _browser.JsElementExists(popupId, 1);
            if (popupExists)
            {
                var popupMsg = await _browser.JsGetElementInnerText(popupId, 1);

                if (popupIsError)
                {
                    LogError($"Popup message, <{popupMsg}>");
                    return false;
                }
                else LogInfo($"Popup message, <{popupMsg}>");

                var acceptId = "app-my-dialog .modal-footer button.ubg-primary";
                var acceptExists = await _browser.JsElementExists(acceptId, 1);
                if (acceptExists)
                {
                    var acceptText = await _browser.JsGetElementInnerText(acceptId, 1);
                    var clickAccept = await _browser.JsClickElement(acceptId, 1);
                    if (!await WaitAsync(2, "app-loader .loading")) return false;
                    if (!clickAccept)
                    {
                        LogError($"Error click <{acceptText}>, {acceptId}");
                        return false;
                    }
                    else
                    {
                        LogInfo($"Click <{acceptText}>, {acceptId}");
                    }
                }
            }
            return true;
        }

        private async Task<bool> VcbLogin(string user, string password)
        {
            var loginPage1 = await VcbInputLoginPage1(user, password);
            ShowLoginPage1?.Invoke(this, false);
            if (!loginPage1) return false;

            var loginPage2 = await VcbInputLoginPage2();
            if (!loginPage2) return false;

            var loginPage3 = await VcbInputLoginPage3();
            ShowLoginPage3?.Invoke(this, false);
            if (!loginPage3) return false;

            EnteringCaptcha = false;
            EnteringOtp = false;
            EnteringTransOtp = false;

            if (!await VcbIsLoggedIn()) return false;

            return true;
        }

        private async Task<bool> VcbRefreshAsync(string user, string password)
        {
            StartReportStopWatch();
            var loadBank = await VcbLoadBank();
            VcbReport.LoadBank = StopReportStopWatch();
            if (!loadBank)
                return false;

            StartReportStopWatch();
            var isLoggedIn = await VcbIsLoggedIn();
            VcbReport.IsLoggedIn = StopReportStopWatch();
            if (!isLoggedIn)
            {
                StartReportStopWatch();
                var lang = await VcbGetLanguage();
                VcbReport.GetLang = StopReportStopWatch();
                if (!lang)
                {
                    LogError($"Get language failed.");
                    return false;
                }

                StartReportStopWatch();
                var login = await VcbLogin(user, password);
                VcbReport.Login = StopReportStopWatch();
                if (!login)
                {
                    LogError($"Login failed.");
                    return false;
                }
            }

            LogInfo($"Refresh successfully");
            return true;
        }

        private async Task<bool> VcbGoTransferScreen(TransferType transType)
        {
            var url = transType == TransferType.Internal ? "https://vcbdigibank.vietcombank.com.vn/chuyentien/chuyentientronghethong" :
                "https://vcbdigibank.vietcombank.com.vn/chuyentien/chuyentienquataikhoan";
            var eleId = transType == TransferType.Internal ? ".main-section.home-section img[src*=\"ic_chuyen-tien.svg\"]" :
                ".main-section.home-section img[src*=\"ic_chuyen-tien-nhanh.svg\"]";

            var eleExists = await _browser.JsElementExists(eleId, 2);
            if (!eleExists)
            {
                LogError($"Transfer link not exists, {eleId}");
                return false;
            }

            var clickEle = await _browser.JsClickElementToParentNode(eleId, 2, 2);
            if (!await WaitAsync(2, "app-loader .loading")) return false;
            if (!clickEle)
            {
                LogError($"Error click transfer link, {eleId}");
                return false;
            }
            else
            {
                LogInfo($"Click transfer link, {eleId}");
            }

            // Possible popup
            // 1. Agreement
            var popup = await VcbPopup();
            if (!popup)
                return false;

            if (_browser.Address.ToLower() != url)
            {
                LogError($"Error go transfer screen, {_browser.Address}, {url}");
                return false;
            }

            return true;
        }

        private async Task<bool> VcbInputTransferInfo1(TransferType transType, VcbExtBank toBank, string accNum, string accName, decimal amount, string remarks)
        {
            if (!await VcbIsTransferPage1(transType))
                return true; // Skip

            // Check VcbExtBank config
            if (transType == TransferType.External)
            {
                if (toBank.GetVcbExtBank() == null)
                {
                    LogError($"Error Vcb Bank name not found, please check configuration.");
                    return false;
                }
            }

            // Check Available balance
            var balance = await _browser.JsVcbGetInputTransferBalance(_lang, 2);
            if (balance == null)
            {
                LogError($"Available balance not exists");
                return false;
            }
            var balanceAmount = balance.ToNumber().StrToDec();
            if (balanceAmount >= amount)
            {
                BalanceAfterTransfer = balanceAmount - amount;
                LogInfo($"Balance is <{balance}>");
            }
            else
            {
                Status = TransferStatus.ErrorCard;
                TransferErrorMessage = $"余额不足： {balanceAmount}，转账金额： {amount}";
                LogError($"Insufficient balance, <{balance}>, {amount}");
                return false;
            }

            if (transType == TransferType.External)
            {
                // Click Beneficiary bank, input and match & select (click)
                // Click
                var benBankId = "form[id=\"Step1\"] app-select2-common[data-parsley-group=\"beneAcc\"] ng-select2";
                var benBankExists = await _browser.JsElementExists(benBankId, 2);
                if (!benBankExists)
                {
                    Status = TransferStatus.ErrorBank;
                    LogError($"Beneficiary bank dropdown not exists, {benBankId}");
                    return false;
                }
                var clickBenBank = await _browser.MouseClickEvent(benBankId, 2);
                if (!clickBenBank)
                {
                    LogError($"Error click Beneficiary bank dropdown, {benBankId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Beneficiary bank dropdown, {benBankId}");
                }

                // Input
                var inputBankId = "input.select2-search__field";
                var inputBankExists = await _browser.JsElementExists(inputBankId, 2);
                if (!inputBankExists)
                {
                    Status = TransferStatus.ErrorBank;
                    LogError($"Beneficiary bank search input not exists, {inputBankId}");
                    return false;
                }
                var inputBank = await _browser.JsInputElementValue(inputBankId, toBank.GetVcbShortName(), 2);
                if (!inputBank)
                {
                    LogError($"Error input search Beneficiary bank <{toBank.GetVcbShortName()}>, {inputBankId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input search Beneficiary bank <{toBank.GetVcbShortName()}>, {inputBankId}");
                }
                await Task.Delay(2000); // Wait input result reflected

                // Match & select (click)
                var benBankPosition = await _browser.JsVcbGetBeneficiaryBankPosition(toBank.GetVcbName(_lang), 2);
                if (benBankPosition == null)
                {
                    Status = TransferStatus.ErrorBank;
                    LogError($"Beneficiary bank not exists <{toBank.GetVcbName(_lang)}>");
                    return false;
                }
                var clickBenBankName = _browser.MouseClickJsPosition(benBankPosition, 2); // Double check if clicked the Bank name by check the position again.
                if (!clickBenBankName)
                {
                    LogError($"Error click Beneficiary bank name <{toBank.GetVcbName(_lang)}>");
                    return false;
                }
                else
                {
                    LogInfo($"Click Beneficiary bank name <{toBank.GetVcbName(_lang)}>");
                }
            }

            if (transType == TransferType.Internal)
            {
                var accNumId = "form[id=\"Step1\"] input[formcontrolname=\"beneAcc\"]";
                var accNumExists = await _browser.JsElementExists(accNumId, 2);
                if (!accNumExists)
                {
                    LogError($"Beneficiary information input not exists, {accNumId}");
                    return false;
                }
                var inputAccNum = await _browser.JsInputElementValue(accNumId, accNum, 2);
                if (!inputAccNum)
                {
                    LogError($"Error input Beneficiary account <{accNum}>, {accNumId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Beneficiary account <{accNum}>, {accNumId}");
                }

                // Amount exists
                var amountId = "form[id=\"Step1\"] input[formcontrolname=\"amount\"]";
                var amountExists = await _browser.JsElementExists(amountId, 2);
                if (!amountExists)
                {
                    LogError($"Amount input not exists, {amountId}");
                    return false;
                }
                // Input Amount
                var inputAmount = await _browser.JsInputElementValue(amountId, amount.ToString("F0"), 2);
                if (!inputAmount)
                {
                    LogError($"Error input Amount <{amount}>, {amountId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Amount <{amount}>, {amountId}");
                }

                // Input Note
                var noteId = "form[id=\"Step1\"] textarea[formcontrolname=\"content\"]";
                var noteExists = await _browser.JsElementExists(noteId, 2);
                if (!noteExists)
                {
                    LogError($"Note textarea not exists, {noteId}");
                    return false;
                }
                var inputNote = await _browser.JsInputElementValue(noteId, remarks, 2);
                if (!inputNote)
                {
                    LogError($"Error input Note <{remarks}>, {noteId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Note <{remarks}>, {noteId}");
                }
            }
            else if (transType == TransferType.External)
            {
                // Click and Input Beneficiary account
                var accNumId = "form[id=\"Step1\"] input[id=\"SoTaiKhoanNguoiHuong\"]";
                var accNumExists = await _browser.JsElementExists(accNumId, 2);
                if (!accNumExists)
                {
                    LogError($"Beneficiary account input not exists, {accNumId}");
                    return false;
                }
                var inputAccNum = await _browser.InputToElement(accNum, accNumId, 2);
                if (!inputAccNum)
                {
                    LogError($"Error input Beneficiary account <{accNum}>, {accNumId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Beneficiary account <{accNum}>, {accNumId}");
                }

                // Click Amount and wait loading Ben Name
                // Amount exists
                var amountId = "form[id=\"Step1\"] input[id=\"SoTien\"]";
                var amountExists = await _browser.JsElementExists(amountId, 2);
                if (!amountExists)
                {
                    LogError($"Amount input not exists, {amountId}");
                    return false;
                }
                var clickAmount = await _browser.MouseClickEvent(amountId, 2);
                if (!await WaitAsync(2, "app-loader .loading")) return false;
                if (!clickAmount)
                {
                    LogError($"Error click Amount, {amountId}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Amount, {amountId}");
                }

                // Possible popup
                // 1. Beneficiary account not valid
                var popup1 = await VcbPopup(true);
                if (!popup1)
                {
                    Status = TransferStatus.ErrorCard;
                    TransferErrorMessage = "受益人无效";
                    LogError($"Error Beneficiary");
                    return false;
                }

                // Get Ben Name
                var benNameId = "form[id=\"Step1\"] .tennguoihuong .input-group";
                var benNameExists = await _browser.JsElementExists(benNameId, 2);
                if (!benNameExists)
                {
                    LogError($"Beneficiary name not exists, {benNameId}");
                    return false;
                }
                var benName = await _browser.JsGetElementInnerText(benNameId, 2);
                if (string.IsNullOrWhiteSpace(benName))
                {
                    LogError($"Beneficiary name empty, {benNameId}");
                    return false;
                }
                if (benName.VietnameseToEnglish().ToLower().Trim() != accName.VietnameseToEnglish().ToLower().Trim())
                {
                    Status = TransferStatus.ErrorCard;
                    TransferErrorMessage = $"收款人姓名不符： {accName}， 订单姓名： {benName}";
                    LogError($"Beneficiary name not match <{benName}>, <{accName}>, {benNameId}");
                    return false;
                }
                else
                {
                    LogInfo($"Beneficiary name is <{benName}>, {benNameId}");
                }

                // Input Amount
                var inputAmount = await _browser.JsInputElementValue(amountId, amount.ToString("F0"), 2);
                if (!inputAmount)
                {
                    LogError($"Error input Amount <{amount}>, {amountId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Amount <{amount}>, {amountId}");
                }

                // Input Note
                var noteId = "form[id=\"Step1\"] textarea[formcontrolname=\"NoiDungThanhToan\"]";
                var noteExists = await _browser.JsElementExists(noteId, 2);
                if (!noteExists)
                {
                    LogError($"Note textarea not exists, {noteId}");
                    return false;
                }
                var inputNote = await _browser.JsInputElementValue(noteId, remarks, 2);
                if (!inputNote)
                {
                    LogError($"Error input Note <{remarks}>, {noteId}");
                    return false;
                }
                else
                {
                    LogInfo($"Input Note <{remarks}>, {noteId}");
                }
            }

            // Click continue
            var submitId = "form[id=\"Step1\"] .form-main-footer button.ubg-primary";
            var submitExists = await _browser.JsElementExists(submitId, 2);
            if (!submitExists)
            {
                LogError($"Button Continue not exists, {submitId}");
                return false;
            }
            var clickSubmit = await _browser.JsClickElement(submitId, 2);
            if (!await WaitAsync(2, "app-loader .loading")) return false;
            if (!clickSubmit)
            {
                LogError($"Error click Continue, {submitId}");
                return false;
            }
            else
            {
                LogInfo($"Click Continue, {submitId}");
            }

            // Any input errors?
            var inputErrors = await _browser.JsVcbGetInputTransferErrorMessage(1);
            if (!string.IsNullOrWhiteSpace(inputErrors))
            {
                LogError($"Error entry, <{inputErrors}>");
                return false;
            }

            // Possible pop up
            // 1. transfer error
            var popup2 = await VcbPopup(true);
            if (!popup2)
            {
                LogError($"Error transfer - 1");
                return false;
            }

            return true;
        }

        private async Task<bool> VcbInputTransferInfo2(TransferType transType, string accName = "")
        {
            if (!await VcbIsTransferPage2(transType))
                return true; // Skip

            if (transType == TransferType.Internal)
            {
                // Check Beneficiary name
                var benName = await _browser.JsVcbGetInternalTransferBenName(_lang, 2);
                if (string.IsNullOrEmpty(benName))
                {
                    LogError($"Beneficiary name not exists");
                    return false;
                }
                if (benName.VietnameseToEnglish().ToLower().Trim() != accName.VietnameseToEnglish().ToLower().Trim())
                {
                    LogError($"Beneficiary name not match <{benName}>, <{accName}>");
                    return false;
                }
                else
                {
                    LogInfo($"Beneficiary name is <{benName}>");
                }
            }

            var authId = transType == TransferType.Internal ?
                "form[id=\"Step2\"] ng-select2 select" :
                "form[id=\"Step2\"] ng-select2[id=\"selectAuthMethod\"] select";
            var authExists = await _browser.JsElementExists(authId, 2);
            if (!authExists)
            {
                LogError($"Authentication method not exists, {authId}");
                return false;
            }
            var auth = await _browser.JsGetInputValue(authId, 2);
            if (auth != "5") // 5 is VCB-Smart OTP
            {
                LogError($"Error click VCB-Smart OTP, {authId}");
                return false;
            }
            else
            {
                LogInfo($"Authentication method is <{auth}>, {authId}");
            }

            var submitId = "form[id=\"Step2\"] .form-main-footer button.ubg-primary";
            var submitExists = await _browser.JsElementExists(submitId, 2);
            if (!submitExists)
            {
                LogError($"Confirm button not exists, {submitId}");
                return false;
            }
            var clickSubmit = await _browser.JsClickElement(submitId, 2);
            if (!await WaitAsync(2, "app-loader .loading")) return false;
            if (!clickSubmit)
            {
                LogError($"Error click Confirm, {submitId}");
                return false;
            }
            else
            {
                LogInfo($"Click Confirm, {submitId}");
            }

            var popup = await VcbPopup(true);
            if (!popup)
                return false;

            return true;
        }

        private async Task<bool> VcbInputTransferInfo3(TransferType transType)
        {
            if (!await VcbIsTransferPage3(transType))
                return true; // Skip

            var transCodeId = transType == TransferType.Internal ?
                "form[id=\"Step3\"] input[name=\"challenge\"]" :
                "form[id=\"Step3\"] input[formcontrolname=\"Challenge\"]";
            var transCodeExists = await _browser.JsElementExists(transCodeId, 2);
            if (!transCodeExists)
            {
                LogError($"Transaction Code not exists, {transCodeId}");
                return false;
            }
            var transCode = await _browser.JsGetInputValue(transCodeId, 2);
            if (transCode == null)
            {
                LogError($"Transaction Code empty");
                return false;
            }
            else
            {
                LogInfo($"Transfer transaction Code is <{transCode}>");
                VcbSetRedisTransactionCode(_user, transCode);
            }

            ShowTransferPage3?.Invoke(this, true);
            _transferTransCode = transCode;
            GetTransferTransactionCode?.Invoke(this, transCode);

            EnteringCaptcha = false;
            EnteringOtp = false;
            EnteringTransOtp = true;

            var iTry = 0;
            var iMax = 1;
            while (iTry < iMax && await VcbIsTransferPage3(transType))
            {
                iTry++;
                TransferOTP = string.Empty;
                EnteringTransOtp = true;
                EditingTransferPage3?.Invoke(this, true);
                await ShowScreenshot();

                var otpId = transType == TransferType.Internal ?
                    "form[id=\"Step3\"] input[name=\"authValue\"]" :
                    "form[id=\"Step3\"] input[formcontrolname=\"OtpText\"]";
                var otpExists = await _browser.JsElementExists(otpId, 2);
                if (!otpExists)
                {
                    LogError($"Enter OTP input not exists, {otpId}, {iTry}/{iMax}");
                    return false;
                }

                var iTryOtp = 0;
                var iOtpMax = 90;
                SetCountdown?.Invoke(this, iOtpMax);
                while (iTryOtp < iOtpMax)
                {
                    iTryOtp++;
                    SetCountdown?.Invoke(this, iOtpMax - iTryOtp);
                    LogInfo($"Retrieving OTP from redis / Please enter OTP and click <Enter OTP>, {iTry}/{iMax}, {iTryOtp}/{iOtpMax}");

                    if (iTryOtp != 1)
                        await Task.Delay(1000);

                    if (!await VcbIsTransferPage3(transType)) break;

                    if (!EnteringTransOtp) break;

                    var otp = VcbGetRedisOtp(_user);
                    if (otp != null)
                    {
                        TransferOTP = otp;
                        EnteringTransOtp = false;
                        LogInfo($"Retrieved OTP from redis <{otp}>");
                        break;
                    }
                }
                EditingTransferPage3?.Invoke(this, false);

                // Input OTP
                var inputOTP = await _browser.JsInputElementValue(otpId, TransferOTP, 2);
                if (!inputOTP)
                {
                    LogError($"Error input OTP <{TransferOTP}>, {otpId}, {iTry}/{iMax}, {iTryOtp}/{iOtpMax}");
                    if (iTry < iMax) continue; else return false;
                }
                else
                {
                    LogInfo($"Input OTP <{TransferOTP}>, {otpId}, {iTry}/{iMax}, {iTryOtp}/{iOtpMax}");
                }

                // Click Confirm
                var submitId = "form[id=\"Step3\"] .form-main-footer button.ubg-primary";
                var submitExists = await _browser.JsElementExists(submitId, 2);
                if (!submitExists)
                {
                    LogError($"Confirm button not exists, {submitId}");
                    return false;
                }
                var clickSubmit = await _browser.JsClickElement(submitId, 2);
                if (!await WaitAsync(2, "app-loader .loading")) return false;
                if (!clickSubmit)
                {
                    LogError($"Error click Confirm, {submitId}, {iTry}/{iMax}, {iTryOtp}/{iOtpMax}");
                    return false;
                }
                else
                {
                    LogInfo($"Click Confirm, {submitId}, {iTry}/{iMax}, {iTryOtp}/{iOtpMax}");
                }

                // Any input errors?
                var inputErrors = await _browser.JsVcbGetInputTransferErrorMessage(1);
                if (!string.IsNullOrWhiteSpace(inputErrors))
                {
                    LogError($"Error input, <{inputErrors}>");
                    if (iTry < iMax) continue; else return false;
                }

                var popup = await VcbPopup(true);
                if (!popup)
                    return false;
            }

            return true;
        }

        private async Task<bool> VcbTransferStatus(TransferType transType)
        {
            var successId = transType == TransferType.Internal ? "form[id=\"Step4\"] .success-box" : "div[id=\"Step4\"] .success-box";
            var successExists = await _browser.JsElementExists(successId, 2);
            if (!successExists)
            {
                LogError($"Transfer failed, {successId}");
                return false;
            }

            var transId = await _browser.JsVcbGetTransactionId(transType, _lang, 2);
            if (transId == null)
            {
                TransactionId = _orderNo;
                LogError($"Transaction id empty");
            }
            else
            {
                TransactionId = transId;
                LogInfo($"Transaction id is <{transId}>");
            }

            Status = TransferStatus.Success;
            return true;
        }

        //private async Task<bool> VcbCheckTransactionHistory()
        //{
        //    var loadBank = await VcbLoadBank();
        //    if (!loadBank)
        //        return false;

        //    var historyUrl = "https://vcbdigibank.vietcombank.com.vn/chuyentien/trangthailenhchuyentien/1";
        //    var historyId = ".main-section.home-section img[src*=\"ic_trang-thai-lenh-chuyen-tien\"]";
        //    var historyExists = await _browser.JsElementExists(historyId, 2);
        //    var clickHistory = await _browser.JsClickElement(historyId, 2);

        //    var transTypeId = "";
        //}

        private async Task<bool> VcbTransferAsync(TransferType transType, string user, string password,
            VcbExtBank toBank, string toAccNum, string toAccName, decimal toAmount, string remarks)
        {
            var canRefresh = await VcbRefreshAsync(user, password);
            if (!canRefresh)
                return false;

            StartReportStopWatch();
            var goTransferScreen = await VcbGoTransferScreen(transType);
            VcbReport.GoTransfer = StopReportStopWatch();
            if (!goTransferScreen)
            {
                LogError($"Error go transfer screen.");
                return false;
            }

            StartReportStopWatch();
            var inputTransfer1 = await VcbInputTransferInfo1(transType, toBank, toAccNum, toAccName, toAmount, remarks);
            VcbReport.InputTransfer1 = StopReportStopWatch();
            if (!inputTransfer1)
            {
                LogError($"Error input transfer details - Step 1.");
                return false;
            }

            StartReportStopWatch();
            var inputTransfer2 = await VcbInputTransferInfo2(transType, toAccName);
            VcbReport.InputTransfer2 = StopReportStopWatch();
            if (!inputTransfer2)
            {
                LogError($"Error input transfer details - Step 2.");
                return false;
            }

            StartReportStopWatch();
            var inputTransfer3 = await VcbInputTransferInfo3(transType);
            VcbReport.InputTransfer3 = StopReportStopWatch();
            ShowTransferPage3?.Invoke(this, false);
            if (!inputTransfer3)
            {
                LogError($"Error input transfer details - Step 3.");
                return false;
            }

            StartReportStopWatch();
            var transferStatus = await VcbTransferStatus(transType);
            VcbReport.TransferStatus = StopReportStopWatch();
            if (!transferStatus)
            {
                LogError($"Transfer failed.");
                return false;
            }

            return true;
        }

        private void VcbSetRedisTransactionCode(string user, string transactionCode)
        {
            try
            {
                //VcbTransferOrder vcbTransferOrder = new VcbTransferOrder()
                //{
                //    //OrderId = OrderId, // Not mandatory
                //    DeviceId = DeviceId,
                //    Phone = user,
                //    TranxId = "",
                //    Challenge = transactionCode,
                //};
                //_redisService.SetVcbTransferOrder(user, vcbTransferOrder);
                //LogInfo($"VcbSetRedisTransactionCode, {JsonConvert.SerializeObject(vcbTransferOrder)}");
            }
            catch (Exception ex)
            {
                LogError($"Error VcbSetRedisTransactionCode, {user}, {transactionCode}, {ex}", $"Error VcbSetRedisTransactionCode, {user}, {transactionCode}, {ex.Message}");
            }
        }

        private string? VcbGetRedisOtp(string user)
        {
            try
            {
                //var orderOtp = _redisService.GetVcbTransferOrder(user);
                //if (orderOtp != null)
                //{
                //    if (!string.IsNullOrEmpty(orderOtp.SmartOtp))
                //    {
                //        return orderOtp.SmartOtp;
                //    }
                //}
            }
            catch (Exception ex)
            {
                LogError($"Error VcbGetRedisOtp, {user}, {ex}", $"Error VcbGetRedisOtp, {user}, {ex.Message}");
            }
            return null;
        }

        private void VcbRemoveRedisTransactionCode(string user)
        {
            try
            {
                //_redisService.RemoveVcbTransferOrder(user);
            }
            catch (Exception ex)
            {
                LogError($"Error VcbRemoveRedisTransactionCode, {user}, {ex}", $"Error VcbRemoveRedisTransactionCode, {user}, {ex.Message}");
            }
        }

        #endregion

        #region TCB
        private async Task<bool> TcbLoadBank()
        {
            _bankUrl = _bank.GetBank().url;
            if (_bankUrl == null)
            {
                LogError($"Error, empty bank url, {_bankUrl}");
                return false;
            }

            _browser.LoadUrl(_bankUrl);
            if (!await WaitAsync(2, "app-loader .loading")) return false;
            //_browser.LoadUrl(_bankUrl);
            //if (!await WaitAsync(2, "app-loader .loading")) return false;

            LogInfo("Load bank successfully");
            return true;
        }

        private async Task<bool> TcbIsLoginPage()
        {
            var userId = _bank.GetBank()?.ele_user;
            var userExists = await _browser.JsElementExists(userId, 1);

            var passId = _bank.GetBank()?.ele_password;
            var passExists = await _browser.JsElementExists(passId, 1);

            return userExists && passExists;
        }

        private async Task<bool> TcbIsDashboardScreen()
        {
            var dashboardId = "a[href=\"/dashboard\"]";
            return await _browser.JsElementExists(dashboardId, 3);
        }

        public async Task<bool> TcbIsLoggedIn()
        {
            return await TcbIsDashboardScreen();
            //return !await TcbIsLoginPage() && await TcbIsDashboardScreen();
        }

        private async Task<bool> TcbGetLanguage()
        {
            // Set to EN on debug mode
            // Text: Set Language to VN:
            var selectLangId = _isDebug ? ".locale-icon--EN" : ".locale-icon--VN"; // .locale-icon--EN .locale-icon--VN
            var selectLang = await _browser.JsClickElement(selectLangId, 2);
            if (!await WaitAsync(2, ".app-loading")) return false;
            if (!selectLang)
            {
                LogError($"Error select language, {selectLang}");
                return false;
            }

            // Language exists?
            var langId = "#kc-locale-wrapper .selected-locale";
            var langExists = await _browser.JsElementExists(langId, 2);
            if (!langExists)
            {
                LogError($"Error language not exists, {langId}");
                return false;
            }
            var langValue = await _browser.JsGetElementInnerText(langId, 2);
            if (langValue == null)
            {
                LogError($"Error language empty, {langId}");
                return false;
            }
            if (langValue.ToUpper().Equals("EN"))
            {
                _lang = Lang.English;
            }
            else if (langValue.ToUpper().Equals("VN"))
            {
                _lang = Lang.Vietnamese;
            }
            else
            {
                LogError($"Invalid language, {langValue}, {langId}");
                return false;
            }
            LogInfo($"Language is <{langValue}>");

            return true;
        }

        private async Task<bool> TcbLogin(string user, string password)
        {
            // Enter User
            var userId = _bank.GetBank()?.ele_user;
            var inputUser = await _browser.JsInputElementValue(userId, user, 2); // _browser.InputToElement(_browserHost, user, userId, 2); // try change to js to speed up performance
            if (!inputUser)
            {
                LogError($"Error input user <{user}>, {userId}");
                return false;
            }
            else
            {
                LogInfo($"Input user <{user}>, {userId}");
            }

            var passId = _bank.GetBank()?.ele_password;
            var inputPassword = await _browser.JsInputElementValue(passId, password, 2); // _browser.InputToElement(_browserHost, password, passId, 2);
            if (!inputPassword)
            {
                LogError($"Error input password <{password}>, {passId}");
                return false;
            }
            else
            {
                LogInfo($"Input password <{password}>, {passId}");
            }

            var loginId = "#kc-login";
            var loginExists = await _browser.JsElementExists(loginId, 2);
            if (!loginExists)
            {
                LogError($"Login button not exists, {loginId}");
                return false;
            }
            var loginValue = await _browser.JsGetInputValue(loginId, 2);
            var clickLogin = await _browser.JsClickElement(loginId, 2);
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

            // Skip error checking is success nagivated to next screen
            var otpId = "#bb-check-device-countdown-info";
            var otpExists = await _browser.JsElementExists(otpId, 2);
            if (otpExists)
            {
                LogInfo($"OTP screen loaded, skip errors checking, {otpId}");
                return true;
            }

            // Errors checking
            // User or password not filled up
            var inputErrors = await _browser.JsTcbGetInputLoginErrorMessage(1);
            if (!string.IsNullOrWhiteSpace(inputErrors))
            {
                LogError($"Error, <{inputErrors}>");
                return false;
            }

            //  Incorrect username or password
            var inputErrors2Id = "#ss-error-msg";
            var inputErrors2Exists = await _browser.JsElementExists(inputErrors2Id, 1);
            if (inputErrors2Exists)
            {
                //_loginSuccess = false;
                var inputErrors2 = await _browser.JsGetElementInnerText(inputErrors2Id, 1);
                LogError($"Error, <{inputErrors2}>, {inputErrors2Id}");
                return false;
            }

            // Credential not registered
            var notRegisteredId = "#kc-page-title";
            var notRegisteredExists = await _browser.JsElementExists(notRegisteredId, 1);
            if (notRegisteredExists)
            {
                var notRegisteredText = await _browser.JsGetElementInnerText(notRegisteredId, 1);
                if (notRegisteredText.ToLower().VietnameseToEnglish().Contains(TcbText.NotRegistered.TcbTranslate(_lang).ToLower().VietnameseToEnglish()))
                {
                    //_loginSuccess = false;
                    LogError($"User is not registered <{user}>, {notRegisteredId}");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> TcbLoginAuthentication()
        {
            var countdownId = "#canvas-progress-bar-text";
            var countdownExists = await _browser.JsElementExists(countdownId, 15);
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

            TcbShowLoginAuth?.Invoke(this, true);
            await ShowScreenshot();
            var iTry = 0;
            var iTryMax = 2; // Max resend 2
            while (iTry < iTryMax)
            {
                var rejected = await _browser.JsElementExists(rejectedId, 1);
                var notRejected = await _browser.JsElementExists(notRejectedId, 1);
                loginRejected = rejected && !notRejected; // Found rejected and do not found .d-none => Rejected
                if (loginRejected)
                {
                    LogError($"Login rejected, {rejectedId}, {iTry + 1}/{iTryMax}");
                    return false;
                }

                isExpired = await _browser.JsElementExists(expiredId, 1);
                if (isExpired)
                {
                    LogError($"Login session expired, {expiredId}, {iTry + 1}/{iTryMax}");
                    return false;
                }

                countdownExists = await _browser.JsElementExists(countdownId, 1);
                if (!countdownExists)
                {
                    break;
                }
                else
                {
                    var timeout = await _browser.JsGetElementInnerText(countdownId, 1);
                    LogInfo($"Login authentication timeout <{timeout}>, {iTry + 1}/{iTryMax}");
                    SetCountdown?.Invoke(this, timeout.StrToInt());
                    if (timeout != null && timeout != "0")
                    {
                        await Task.Delay(1000);
                    }
                    else if (timeout != null && timeout == "0")
                    {
                        var resendExists = await _browser.JsElementExists(resendId, 2);
                        if (!resendExists)
                        {
                            LogError($"Resend request button not exists, {resendId}, {iTry + 1}/{iTryMax}");
                            return false;
                        }
                        var resendText = await _browser.JsGetElementInnerText(resendId, 2);
                        var clickResend = await _browser.JsClickElement(resendId, 2);
                        await Task.Delay(1000);
                        if (!await WaitAsync(2, ".app-loading")) return false;
                        if (!clickResend)
                        {
                            LogError($"Error click <{resendText}>, {resendId}, {iTry + 1}/{iTryMax}");
                            return false;
                        }
                        else
                        {
                            LogInfo($"Click <{resendText}>, {resendId}, {iTry + 1}/{iTryMax}");
                            iTry++;
                            await ShowScreenshot();
                        }
                    }
                }
            }
            TcbShowLoginAuth?.Invoke(this, false);

            if (!await WaitAsync(2, ".app-loading")) return false;
            var isLoggedIn = await TcbIsLoggedIn();
            if (!isLoggedIn)
                return false;

            return true;
        }

        private async Task TcbSetBalance()
        {
            var id = ".account-balance .account-balance__amount";
            var balance = await _browser.JsGetElementInnerText(id, 2);
            if (balance != null)
            {
                DoSetBalance?.Invoke(this, balance.ToNumber(Bank.TcbBank).StrToDec());
            }
        }

        private async Task<bool> TcbRefreshAsync(string user, string password)
        {
            StartReportStopWatch();
            var loadBank = await TcbLoadBank();
            TcbReport.LoadBank = StopReportStopWatch();
            if (!loadBank)
                return false;

            StartReportStopWatch();
            var isLoggedIn = await TcbIsLoggedIn();
            TcbReport.IsLoggedIn = StopReportStopWatch();
            if (!isLoggedIn)
            {
                StartReportStopWatch();
                var lang = await TcbGetLanguage();
                TcbReport.GetLang = StopReportStopWatch();
                if (!lang)
                {
                    LogError($"Get language failed.");
                    return false;
                }

                StartReportStopWatch();
                var login = await TcbLogin(user, password);
                TcbReport.Login = StopReportStopWatch();
                if (!login)
                {
                    LogError($"Login failed.");
                    return false;
                }

                StartReportStopWatch();
                var authenticate = await TcbLoginAuthentication();
                TcbReport.LoginAuth = StopReportStopWatch();
                if (!authenticate)
                {
                    LogError($"Authenticate failed.");
                    return false;
                }
            }

            await TcbSetBalance();

            return true;
        }

        private async Task<bool> TcbGoTransferScreen(TransferType transType)
        {
            var eleId = transType == TransferType.Internal ?
                "li.nav__item a[href=\"/transfers-payments/pay-someone?transferType=internal\"]" :
                "li.nav__item a[href=\"/transfers-payments/pay-someone?transferType=other\"]";
            var eleExists = await _browser.JsElementExists(eleId, 2);
            if (!eleExists)
            {
                LogError($"Error transfer link not exists, {eleId}");
                return false;
            }
            var clickEle = await _browser.JsClickElement(eleId, 2);
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

        private async Task<bool> TcbInputTransferInfo(TransferType transType, TcbExtBank bank, string accNum, string accName, decimal amount, string remarks)
        {
            if (transType == TransferType.External)
            {
                if (bank.GetTcbExtBank() == null)
                {
                    LogError($"Error Tcb Bank name not found, please check configuration.");
                    return false;
                }

                // Click Search
                var searchId = ".otherbank-transfer-wrapper .selector-input";
                var searchExists = await _browser.JsElementExists(searchId, 2);
                if (!searchExists)
                {
                    LogError($"Beneficiary bank dropdown not exists, {searchId}");
                    return false;
                }
                var clickSearch = await _browser.MouseClickEvent(searchId, 2);
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

                var searchInputId = ".search-box input.search-box__input";
                var searchInputExists = await _browser.JsElementExists(searchInputId, 10);
                if (!searchInputExists)
                {
                    LogError($"Search input not exists, timeout, {searchInputId}");
                    return false;
                }

                // Input Bank Name
                var inputBankName = await _browser.JsInputElementValue(searchInputId, bank.GetTcbShortName(), 2); // InputToElementAsync(bankName, bankNameId, true, true); 
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

                // Bank ShortName exists?
                var benBankPosition = await _browser.JsTcbGetBeneficiaryBankPosition(bank.GetTcbShortName(), 2);
                if (benBankPosition == null)
                {
                    Status = TransferStatus.ErrorBank;
                    LogError($"Beneficiary bank not exists <{bank.GetTcbShortName()}>");
                    return false;
                }
                var iTry = 0;
                var iTryMax = 3;
                while (iTry < iTryMax)
                {
                    iTry++;
                    await Task.Delay(1000);

                    var checkBenBankPosition = await _browser.JsTcbGetBeneficiaryBankPosition(bank.GetTcbShortName(), 1);
                    if (checkBenBankPosition == null || checkBenBankPosition == benBankPosition) break;
                    else if (checkBenBankPosition != benBankPosition)
                    {
                        benBankPosition = checkBenBankPosition;
                    }
                }
                var clickBenBankName = _browser.MouseClickJsPosition(benBankPosition, 2);
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

            var amountId = transType == TransferType.Internal ?
                 "techcom-internal-transfer techcom-currency-input-with-suggestion input" :
                 "techcom-otherbank-transfer techcom-currency-input-with-suggestion input";
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
                var clearAccNumExists = await _browser.JsElementExists(clearAccNumId, 1);
                if (clearAccNumExists)
                {
                    var clickClear = await _browser.JsClickElement(clearAccNumId, 1);
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
                            var checkAccNum = await _browser.JsGetInputValue(clearAccNumId, 1);
                            if (string.IsNullOrEmpty(checkAccNum)) break;
                            else await Task.Delay(1000);

                            if (iTry >= iTryMax) break;
                        }

                        // After clear, click on Amount
                        var clickAmountAfterClear = await _browser.MouseClickEvent(amountId, 2);
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
                var accNumExists = await _browser.JsElementExists(accNumId, 2);
                if (!accNumExists)
                {
                    LogError($"Account number input not exists, {accNumId}");
                    return false;
                }

                // Empty Acc Num
                var emptyAccNum = await _browser.JsInputElementValue(accNumId, "", 1);
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
                var inputAccNum = await _browser.InputToElement(accNum, accNumId, 2);
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
                var amountExists = await _browser.JsElementExists(amountId, 2);
                if (!amountExists)
                {
                    LogError($"Amount input not exists, {amountId}");
                    return false;
                }
                var clickAmount = await _browser.MouseClickEvent(amountId, 2);
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
                var invalidBenExists = await _browser.JsElementExists(invalidBenId, 2);
                if (invalidBenExists)
                {
                    // If Service unavailable, retry...

                    // Dịch vụ tạm thời gián đoạn. Quý khách vui lòng thử lại sau.
                    // The service is temporarily unavailable. Please try again later.

                    var invalidBenMsg = await _browser.JsGetElementInnerText(invalidBenId, 2);
                    if (invalidBenMsg?.Trim().ToLower().VietnameseToEnglish() == TcbText.ServiceUnavailable.TcbTranslate(_lang).ToLower().VietnameseToEnglish())
                    {
                        blnAccNum = false;
                        LogError($"Service unavailable <{accNum}>, <{invalidBenMsg}>, {invalidBenId}, {iRetryAccNum}/{iMaxAccNum}");

                        var closeId = "tcb-transfer-bill-error-modal button[tcbtracker=\"btn_close\"]";
                        var closeExists = await _browser.JsElementExists(closeId, 1);
                        if (closeExists)
                        {
                            var clickClose = await _browser.JsClickElement(closeId, 2);
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
                        Status = TransferStatus.ErrorCard;
                        TransferErrorMessage = $"卡号无效： {accNum}";
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

            // Check Beneficiary (Acc Name)
            var benNameId = transType == TransferType.Internal ?
                "techcom-internal-transfer .beneficiary-info input" :
                "techcom-otherbank-transfer .beneficiary-info input";
            var benNameExists = await _browser.JsElementExists(benNameId, 10);
            if (!benNameExists)
            {
                Status = TransferStatus.ErrorCard;
                TransferErrorMessage = $"受益人无效";
                LogError($"Beneficiary name not exists, {benNameId}");
                return false;
            }
            var benName = await _browser.JsGetInputValue(benNameId, 2);
            if (benName != null && benName.VietnameseToEnglish().ToLower().Trim() == accName.VietnameseToEnglish().ToLower().Trim())
            {
                LogInfo($"Beneficiary name is <{benName}>, {benNameId}");
            }
            else
            {
                Status = TransferStatus.ErrorCard;
                TransferErrorMessage = $"收款人姓名不符： {accName}， 订单姓名： {benName}";
                LogError($"Beneficiary name not match <{benName}>, <{accName}>, {benNameId}");
                return false;
            }

            // Check Account Balance
            var accBalId = ".account-balance__amount";
            var accBalExists = await _browser.JsElementExists(accBalId, 2);
            if (!accBalExists)
            {
                LogError($"Balance not exists, {accBalId}");
                return false;
            }
            var accBal = await _browser.JsGetElementInnerText(accBalId, 2);
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

            // Input Amount
            var inputAmount = await _browser.JsInputElementValue(amountId, amount.ToString(), 2);
            if (!inputAmount)
            {
                LogError($"Error input amount <{amount}>, {amountId}");
                return false;
            }
            else
            {
                LogInfo($"Input amount <{amount}>, {amountId}");
            }

            // Input Description
            var msgId = transType == TransferType.Internal ?
                "techcom-internal-transfer textarea[formcontrolname=\"description\"]" :
                "techcom-otherbank-transfer textarea[formcontrolname=\"description\"]";
            var msgExists = await _browser.JsElementExists(msgId, 2);
            if (!msgExists)
            {
                LogError($"Message input not exists, {msgId}");
                return false;
            }
            var inputMsg = await _browser.JsInputElementValue(msgId, remarks, 2);
            if (!inputMsg)
            {
                LogError($"Error input Message <{remarks}>, {msgId}");
                return false;
            }
            else
            {
                LogInfo($"Input Message <{remarks}>, {msgId}");
            }

            // Click on amount again to triger validation
            var clickAmount2 = await _browser.MouseClickEvent(amountId, 2);
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
            var nextId = transType == TransferType.Internal ?
                "techcom-internal-transfer bb-load-button-ui[tcbtracker=\"btn_next\"] button" :
                "techcom-otherbank-transfer bb-load-button-ui[tcbtracker=\"btn_next\"] button";
            var nextExists = await _browser.JsElementExists(nextId, 2);
            if (!nextExists)
            {
                LogError($"Next button not exists, {nextId}");
                return false;
            }
            var nextEnabled = !await _browser.JsElementDisabled(nextId, 1);
            if (!nextEnabled)
            {
                LogError($"Next button not enabled, {nextId}");
                return false;
            }
            else
            {
                LogInfo($"Next button enabled, {nextId}");
            }
            var clickNext = await _browser.JsClickElement(nextId, 2);
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

            return true;
        }

        private async Task<bool> TcbConfirmTransfer()
        {
            // Wait confirm page loaded
            var confirmPageId = "techcom-transfer-confirmation";
            var confirmPageExists = await _browser.JsElementExists(confirmPageId, 15);
            if (!confirmPageExists)
            {
                LogError($"Confirm page not exists, timeout, {confirmPageId}");
                return false;
            }

            // Click Confirm
            var confirmId = "techcom-transfer-confirmation bb-load-button-ui[tcbtracker=\"btn_confirm\"]";
            var confirmExists = await _browser.JsElementExists(confirmId, 2);
            if (!confirmExists)
            {
                LogError($"Confirm button not exists, {confirmId}");
                return false;
            }
            var clickConfirm = await _browser.JsClickElement(confirmId, 2);
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

            TcbSetTransferOrder(_user);

            return true;
        }

        private bool TcbSetTransferOrder(string user)
        {
            // Set redis - pass to mobile to update transfer status
            TcbTransferOrder model = new TcbTransferOrder()
            {
                OrderId = OrderId,
                DeviceId = DeviceId,
                Phone = user,
            };
            try
            {
                //_redisService.SetTcbTransferOrder(user, model);
                //LogInfo($"TcbSetTransferOrder, {JsonConvert.SerializeObject(model)}");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error TcbSetTransferOrder, {JsonConvert.SerializeObject(model)}, {ex}",
                    $"Error TcbSetTransferOrder, {JsonConvert.SerializeObject(model)}, {ex.Message}");
            }
            return false;
        }

        private int TcbGetTransferOrderStatus(string user)
        {
            try
            {
                //var model = _redisService.GeTcbTransferOrder(user);
                //if (model == null)
                //    return 0;

                //return model.Status;
            }
            catch (Exception ex)
            {
                LogError($"Error TcbGetTransferOrderStatus, {user}, {ex}", $"Error TcbGetTransferOrderStatus, {user}, {ex.Message}");
            }
            return 0;
        }

        private void TcbRemoveTransferOrder(string user)
        {
            try
            {
                //_redisService.RemoveTcbTransferOrder(user);
            }
            catch (Exception ex)
            {
                LogError($"Error TcbRemoveTransferOrder, {user}, {ex}", $"Error TcbRemoveTransferOrder, {user}, {ex.Message}");
            }
        }

        private async Task<bool> TcbTransferAuthentication()
        {
            var rejectId = "techcom-transaction-signing-error";
            var isRejected = false;

            // Wait until authentication available
            var authId = "techcom-transaction-signing-oob-device";
            var authExists = await _browser.JsElementExists(authId, 10);
            if (!authExists)
            {
                // Posible user rejected before OTP detected by CEF on browser?
                isRejected = await _browser.JsElementExists(rejectId, 1);
                if (isRejected)
                {
                    LogInfo($"Transfer rejected, {rejectId}");
                    return false;
                }

                // Posible user approved before OTP detected by CEF on browser
                var successId = "techcom-transfer-successful";
                var isSuccess = await _browser.JsElementExists(successId, 1);
                if (isSuccess)
                {
                    LogInfo($"Transfer success, {successId}");
                    return true;
                }

                LogError($"Transfer authentication not exists, timeout, {authId}");
                CheckHistory = true;
                return true; // Proceed with check history
            }

            TcbShowLoginAuth?.Invoke(this, true);
            await ShowScreenshot();

            var expiredId = "techcom-transfer-failure";
            var isExpired = false;
            var resendId = "techcom-transaction-signing-oob-device button[tcbtracker=\"btn_resend\"]";
            var timeoutId = "#base-timer-label";
            var resendExists = false;
            while (true)
            {
                isRejected = await _browser.JsElementExists(rejectId, 1);
                if (isRejected)
                {
                    LogInfo($"Transfer rejected, {rejectId}");
                    return false;
                }

                isExpired = await _browser.JsElementExists(expiredId, 1);
                if (isExpired)
                {
                    LogInfo($"Transfer session expired, {expiredId}");
                    CheckHistory = true;
                    return true; // Proceed with check history
                }

                resendExists = await _browser.JsElementExists(resendId, 1);
                if (resendExists)
                {
                    var resendDisabled = await _browser.JsElementDisabled(resendId, 1);
                    if (resendDisabled)
                    {
                        var timeout = await _browser.JsGetElementInnerText(timeoutId, 2);
                        SetCountdown?.Invoke(this, timeout.StrToInt());
                        LogInfo($"Transfer authentication timeout <{timeout}>");
                        await Task.Delay(1000);
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

        private async Task<bool> TcbTransferStatus(string remarks)
        {
            var successId = "techcom-transfer-successful";
            var success = await _browser.JsElementExists(successId, 15);
            if (!success)
            {
                // Check redis
                var redisStatus = TcbGetTransferOrderStatus(_user);
                LogInfo($"Redis status is <{redisStatus}>");
                success = redisStatus == 1;
                if (success)
                {
                    TcbRemoveTransferOrder(_user);

                    Status = TransferStatus.Success;
                    TransactionId = $"PH{_orderNo}";
                    LogInfo($"Transfer success with redis");
                    return true;
                }
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

                    foundHistory = await TcbCheckTransactionHistory(iTry, remarks);
                    if (foundHistory) break;
                }
                if (!foundHistory)
                {
                    LogError($"Transfer failed, transaction history not found.");
                    return false;
                }

                Status = TransferStatus.Success;
                LogInfo($"Transfer success with history");
                return true;
            }

            Status = TransferStatus.Success;
            LogInfo($"Transfer success, {successId}");

            // Retrieve transaction id
            var transId = await _browser.JsGetTcbTransactionId(_lang, 2);
            if (transId == null)
            {
                TransactionId = _orderNo;
                LogError($"Transaction id not exists");
            }
            else
            {
                TransactionId = transId;
                LogInfo($"Transaction id is <{transId}>");
            }
            return true;
        }

        private async Task<bool> TcbCheckTransactionHistory(int iTry, string remarks)
        {
            var loadBank = await TcbLoadBank();
            if (!loadBank) return false;

            // Click to go Account Details
            var accountId = "techcom-account-summary-quick-view-widget .current-account__item";
            var accountExists = await _browser.JsElementExists(accountId, 5);
            if (!accountExists)
            {
                LogError($"{iTry}|Current account not exists, {accountId}");
                return false;
            }
            var clickAccount = await _browser.MouseClickEvent(accountId, 1);
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
            var transListExists = await _browser.JsElementExists(transListId, 2);
            if (!transListExists)
            {
                LogError($"{iTry}|Transaction history not exists, {transListId}");
                return false;
            }
            var transCount = await _browser.JsGetElementCount(transListId, 1);
            if (transCount == null || transCount.StrToInt() <= 0)
            {
                LogError($"{iTry}|Transaction history has no record, {transListId}");
                return false;
            }
            else
            {
                LogInfo($"{iTry}|Transaction history has record <{transCount}>, {transListId}");
            }
            var clickFirst = await _browser.JsClickElementAtIndex(transListId, 0, 1);
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

            var transMessage = await _browser.JsGetTcbMessageFromTransHistory(_lang, 2); // remarks
            if (transMessage == null || transMessage.Trim().ToLower() != remarks.Trim().ToLower())
            {
                LogError($"{iTry}|Transaction message not match <{transMessage}>, {remarks}");
                return false;
            }
            else
            {
                LogInfo($"{iTry}|Transaction message matched <{transMessage}>, {remarks}");
            }

            var transIdText = await _browser.JsGetTcbTransIdFromTransHistory(_lang, 2);
            TransactionId = transIdText;
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

        private async Task<bool> TcbTransferAsync(TransferType transType, string user, string password,
            TcbExtBank toBank, string toAccNum, string toAccName, decimal toAmount, string remarks)
        {
            var canRefresh = await TcbRefreshAsync(user, password);
            if (!canRefresh)
                return false;

            StartReportStopWatch();
            var goTransferScreen = await TcbGoTransferScreen(transType);
            TcbReport.GoTransfer = StopReportStopWatch();
            if (!goTransferScreen)
            {
                LogError($"Error go transfer screen.");
                return false;
            }

            StartReportStopWatch();
            var inputTransferInfo = await TcbInputTransferInfo(transType, toBank, toAccNum, toAccName, toAmount, remarks);
            TcbReport.InputTransfer = StopReportStopWatch();
            if (!inputTransferInfo)
            {
                LogError($"Input transfer failed.");
                return false;
            }

            // Verify Transfer Info and Confirm Transfer
            StartReportStopWatch();
            var confirmTransfer = await TcbConfirmTransfer();
            TcbReport.ConfirmTransfer = StopReportStopWatch();
            if (!confirmTransfer)
            {
                LogError($"Confirm transfer failed.");
                return false;
            }

            StartReportStopWatch();
            var authTransfer = await TcbTransferAuthentication();
            TcbReport.TransferAuth = StopReportStopWatch();
            TcbShowLoginAuth?.Invoke(this, false);
            if (!authTransfer && !CheckHistory)
            {
                LogError($"Authenticate transfer failed.");
                return false;
            }

            StartReportStopWatch();
            var transferSuccess = await TcbTransferStatus(remarks);
            TcbReport.TransferStatus = StopReportStopWatch();
            if (!transferSuccess)
            {
                LogError($"Transfer failed.");
                return false;
            }

            return true;
        }

        #endregion
    }
}
