using Neptune.NsPay.CefTransfer.Common.Classes;
using Neptune.NsPay.CefTransfer.Common.Models;
using Neptune.NsPay.CefTransfer.Common.MyEnums;
using Neptune.NsPay.CefTransfer.Win.Classes;
using Neptune.NsPay.CefTransfer.Win.UserControls;
using Neptune.NsPay.Commons;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.WithdrawalOrders;
using System.Globalization;
//using Neptune.NsPay.MongoDbExtensions.Models;

namespace Neptune.NsPay.CefTransfer.Win.Forms
{
    public partial class fBrowser : Form
    {
        #region Variables
        private MyBrowser _browser;
        private bool _start;
        private bool _running;
        private int _iRefreshInterval;
        private System.Timers.Timer _refreshTimer;
        private int _iRefreshTime = 170;
        private bool _closeProgram;
        private int _splitterDistance = 400;
        #endregion

        #region Property
        public string Phone { get; set; }
        public WithdrawalDevicesBankTypeEnum BankType { get; set; }
        public string MerchantCode { get; set; }
        #endregion

        public fBrowser()
        {
            InitializeComponent();
        }

        #region Event
        private void fBrowser_Load(object sender, EventArgs e)
        {
            Phone = AppSettings.Configuration["Phone"];
            BankType = (WithdrawalDevicesBankTypeEnum)Enum.Parse(typeof(WithdrawalDevicesBankTypeEnum), AppSettings.Configuration["BankType"]);
            MerchantCode = AppSettings.Configuration["NsPayApi:MerchantCode"];

            this.InvokeOnUiThreadIfRequired(() => SetSpliterDistance(50));
            this.InvokeOnUiThreadIfRequired(() => this.Text = $"{BankType} - {Phone}");

            this.InvokeOnUiThreadIfRequired(() => SetScreenshot(null));
            this.InvokeOnUiThreadIfRequired(() => SetToolStripLayout());
            this.InvokeOnUiThreadIfRequired(async () => await Start());
        }

        private void fBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_closeProgram)
            {
                e.Cancel = true;
                this.InvokeOnUiThreadIfRequired(() => CloseProgram());
            }
        }

        private void fBrowser_SizeChanged(object sender, EventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => SetSpliterDistance(_splitterDistance, false));
        }

        private void toolStrip1_Layout(object sender, LayoutEventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => SetToolStripLayout());
        }

        private async void tsmiScrnshot_Click(object sender, EventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => SetScreenshotButton(false));
            this.InvokeOnUiThreadIfRequired(() => SetScreenshot(null));
            await _browser.ShowScreenshot();
            this.InvokeOnUiThreadIfRequired(() => SetScreenshotButton(true));
        }

        private void refreshTimer_Elapsed(object? source, System.Timers.ElapsedEventArgs e)
        {
            _iRefreshInterval++;
        }

        private void browser_IsBusy(object? sender, bool busy)
        {
            this.InvokeOnUiThreadIfRequired(() => SetBusy(busy));
            this.InvokeOnUiThreadIfRequired(() => SetUrl(_browser.Browser.Address));
            this.InvokeOnUiThreadIfRequired(() => SetScreenshotButton(_browser.Capturing));
        }

        private void browser_GetScreenshot(object? sender, ScreenshotModel? screenshot)
        {
            this.InvokeOnUiThreadIfRequired(() => SetScreenshot(screenshot));
        }

        private void browser_ShowLoginPage1(object? sender, bool show)
        {
            this.InvokeOnUiThreadIfRequired(() => ShowLogin1(show));
        }

        private void browser_ShowLoginPage3(object? sender, bool show)
        {
            this.InvokeOnUiThreadIfRequired(() => ShowLogin3(show));
        }

        private void browser_ShowTransferPage3(object? sender, bool show)
        {
            this.InvokeOnUiThreadIfRequired(() => ShowTransfer3(show));
        }

        private void browser_TcbShowLoginAuth(object? sender, bool show)
        {
            this.InvokeOnUiThreadIfRequired(() => TcbShowLoginAuth(show));
        }

        private void tsslblMsg_WriteMessage(object? sender, string message)
        {
            this.InvokeOnUiThreadIfRequired(() => WriteMessage(message));
        }

        private void tsslblBalance_SetBalance(object? sender, decimal balance)
        {
            this.InvokeOnUiThreadIfRequired(() => SetBalance(balance));
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => SaveScreenshot());
        }
        #endregion

        #region Public
        #endregion

        #region Private

        private async Task<bool> InitWorker()
        {
            try
            {
                _refreshTimer = new System.Timers.Timer();
                _refreshTimer.Interval = 1000;
                _refreshTimer.Elapsed += refreshTimer_Elapsed;
                _refreshTimer.Enabled = true;

                _browser = new MyBrowser();
                await _browser.Init();
                _browser.WriteUiMessage += tsslblMsg_WriteMessage;
                _browser.DoSetBalance += tsslblBalance_SetBalance;
                _browser.IsBusy += browser_IsBusy;
                _browser.GetScreenshot += browser_GetScreenshot;
                _browser.ShowLoginPage1 += browser_ShowLoginPage1;
                _browser.ShowLoginPage3 += browser_ShowLoginPage3;
                _browser.ShowTransferPage3 += browser_ShowTransferPage3;
                _browser.TcbShowLoginAuth += browser_TcbShowLoginAuth;

                return true;
            }
            catch (Exception ex)
            {
                _browser.LogError($"Error at: {DateTime.Now}, {ex}", $"Error at: {DateTime.Now}, {ex.Message}");
            }
            return false;
        }

        private async Task Start()
        {
            _start = true;
            var blnGo = await InitWorker();
            if (blnGo)
            {
                while (_start)
                {
                    _running = true;
                    _browser.LogInfo($"Worker running at: {DateTime.Now}");

                    await StartAsync();
                    //await TestVcbAsync();
                    //await TestTcbAsync();
                    //await TestTcbBizAsync();

                    _running = false;
                    if (!_start) break;
                    await Task.Delay(1000 * 10);
                }
            }
            CloseProgram();
        }

        private async Task StartAsync()
        {
            try
            {
                var blnUpdateOrder = false;
                var blnTransfer = false;
                var deviceInfo = await _browser.GetDevice(Phone, BankType);
                if (deviceInfo != null && BankType == WithdrawalDevicesBankTypeEnum.TCB)
                {
                    if (await _browser.TcbIsLoggedIn())
                    {
                        if (deviceInfo.Process == (int)WithdrawalDevicesProcessTypeEnum.Process)
                        {
                            _browser.LogInfo($"查询订单: {DateTime.Now}");

                            var checkOrder = await _browser.CheckWithdrawOrder(Phone, BankType);
                            if (checkOrder == null)
                            {
                                var withdrawalOrder = await _browser.GetWithdrawOrder(Phone, BankType);
                                if (withdrawalOrder != null)
                                {
                                    _browser.LogInfo($"获取消息：{DateTime.Now}--OrderId:{withdrawalOrder.OrderNo}");
                                    try
                                    {
                                        ////更新订单未转账中
                                        blnUpdateOrder = await _browser.UpdateWithdrawOrder(withdrawalOrder.Id, "", WithdrawalOrderStatusEnum.Pending, "");

                                        //WithdrawalOrdersMongoEntity order = null;
                                        //var orderTemp = await _withdrawalOrdersService.GetWithdrawOrderByOrderNumber(withdrawalOrder.MerchantCode, withdrawalOrder.OrderNo);
                                        //if (orderTemp == null)
                                        //{
                                        //    return;
                                        //}
                                        //if (orderTemp.OrderStatus == WithdrawalOrderStatusEnum.Wait)
                                        //{
                                        //    orderTemp.OrderStatus = WithdrawalOrderStatusEnum.Pending;
                                        //    await _withdrawalOrdersService.UpdateAsync(orderTemp);

                                        _browser.LogInfo($"进入转账：{DateTime.Now}--OrderId:{withdrawalOrder.OrderNo}");

                                        //创建出款订单
                                        var orderNo = withdrawalOrder.OrderNo;
                                        if (orderNo.Contains("_"))
                                        {
                                            orderNo = orderNo.Split("_")[0];
                                        }
                                        else
                                        {
                                            orderNo = orderNo.Substring(2);
                                        }

                                        var toBank = string.Empty;
                                        var tcbToBank = TcbExtBank.Unknown;
                                        if (withdrawalOrder.BenBankName.ToLower().Contains("TECHCOM".ToLower()))
                                        {
                                            _browser.LogInfo($"TCB转账：{DateTime.Now}--OrderId:{withdrawalOrder.OrderNo}");
                                            tcbToBank = TcbExtBank.Techcombank_TCB;
                                            toBank = "TCB";
                                        }
                                        else
                                        {
                                            _browser.LogInfo($"其他银行转账：{DateTime.Now}--OrderId:{withdrawalOrder.OrderNo}");
                                            tcbToBank = withdrawalOrder.BenBankName.Replace(" ", "").ToTcbExtBank();
                                            toBank = withdrawalOrder.BenBankName.Replace(" ", "");
                                        }

                                        if (tcbToBank == TcbExtBank.Unknown)
                                        {
                                            //订单状态修改
                                            blnUpdateOrder = await _browser.UpdateWithdrawOrder(withdrawalOrder.Id, "", WithdrawalOrderStatusEnum.ErrorBank, "");
                                            return;
                                        }

                                        var fromBank = Bank.TcbBank;
                                        var user = deviceInfo.Phone;
                                        var password = deviceInfo.LoginPassWord;
                                        var toAccNum = withdrawalOrder.BenAccountNo;
                                        var toAccName = withdrawalOrder.BenAccountName;
                                        int toAmount = withdrawalOrder.OrderMoney.ToString("F0").StrToInt(); // int
                                        var remarks = $"{withdrawalOrder.BenAccountName} {orderNo}";

                                        blnTransfer = true;
                                        _iRefreshInterval = 0;
                                        _browser.LogInfo($"Start transfer, {fromBank}, {orderNo}, {user}, {toBank}, {toAccNum}, {toAccNum}, {toAmount}, {remarks}");
                                        var transferWatch = System.Diagnostics.Stopwatch.StartNew();

                                        _browser.OrderId = withdrawalOrder.Id;
                                        //_browser.OrderId = orderTemp.ID;
                                        _browser.DeviceId = deviceInfo.DeviceId;
                                        var transfer = await _browser.TransferAsync(fromBank, orderNo, user, password, toBank, toAccNum, toAccName, toAmount, remarks);
                                        _browser.FirstTime = false;
                                        transferWatch.Stop();
                                        _browser.LogInfo($"End transfer. Total time: {transferWatch.ElapsedMilliseconds.MilisecondsToTimeTaken()}");
                                        _browser.LogInfo($"Report statistic|{transferWatch.Elapsed.TotalSeconds.To3Decimal()}|{_browser.TcbReportText}");

                                        if (_browser.Status == TransferStatus.Reprocess)
                                        {
                                            // Transfer due to authentication timeout, send to queue and reprocess
                                            blnUpdateOrder = await _browser.UpdateWithdrawOrder(withdrawalOrder.Id, "", WithdrawalOrderStatusEnum.Fail, "");
                                            return;
                                        }

                                        if (_browser.Status != TransferStatus.Success)
                                        {
                                            WithdrawalOrderStatusEnum orderStatus = WithdrawalOrderStatusEnum.Fail;
                                            if (_browser.Status == TransferStatus.Failed)
                                            {
                                                orderStatus = WithdrawalOrderStatusEnum.Fail;
                                            }
                                            if (_browser.Status == TransferStatus.ErrorBank)
                                            {
                                                orderStatus = WithdrawalOrderStatusEnum.ErrorBank;
                                            }
                                            if (_browser.Status == TransferStatus.ErrorCard)
                                            {
                                                orderStatus = WithdrawalOrderStatusEnum.ErrorCard;
                                            }
                                            blnUpdateOrder = await _browser.UpdateWithdrawOrder(withdrawalOrder.Id, "", orderStatus, _browser.TransferErrorMessage);
                                            _browser.LogInfo($"Withdrawal failed.");
                                            return;
                                        }

                                        blnUpdateOrder = await _browser.UpdateWithdrawOrder(withdrawalOrder.Id, _browser.TransactionId, WithdrawalOrderStatusEnum.Success, "");
                                        _browser.LogInfo($"Withdrawal success.");

                                        _browser.LogInfo($"Set balance, {Phone}, {_browser.BalanceAfterTransfer}");
                                        await _browser.UpdateBalance(deviceInfo.DeviceId, _browser.BalanceAfterTransfer);
                                        //TcbSetBalance(paymentInfo.Id, deviceInfo.phone, _browser.BalanceAfterTransfer);

                                        _browser.LogInfo($"转账完成：{DateTime.Now}--OrderId:{orderNo}");
                                        //}
                                    }
                                    catch (Exception ex)
                                    {
                                        blnUpdateOrder = await _browser.UpdateWithdrawOrder(withdrawalOrder.Id, "", WithdrawalOrderStatusEnum.Fail, $"出款队列异常: {ex.Message}");
                                        _browser.LogInfo($"Withdrawal error, {ex.Message}");

                                        NlogLogger.Error("tcb：" + Phone + "，出款队列异常：" + ex);
                                    }
                                }
                            }
                        }
                    }
                    // Refresh if not performing transfer and first launch
                    // Refresh if not performing transfer each 3 minutes (180 seconds)
                    if (!blnTransfer)
                    {
                        if (_browser.FirstTime || _iRefreshInterval >= _iRefreshTime)
                        {
                            var fromBank = Bank.TcbBank;
                            var user = deviceInfo.Phone;
                            var password = deviceInfo.LoginPassWord;

                            _browser.LogInfo($"Start refresh... {_iRefreshInterval}");
                            var refresh = await _browser.RefreshAsync(fromBank, user, password);
                            _browser.LogInfo($"End refresh... {_iRefreshInterval}");
                            _browser.LogInfo($"Report statistic||{_browser.TcbReportText}");
                            if (_browser.FirstTime && !refresh)
                            { _iRefreshInterval = _iRefreshTime; }// Quickly trigger another refresh if first time login failed
                            else
                            { _iRefreshInterval = 0; }
                            _browser.FirstTime = false;
                        }
                        else
                        { _browser.LogInfo($"Next refresh at {_iRefreshInterval}/{_iRefreshTime}"); }
                    }
                }
                else if (deviceInfo != null && BankType == WithdrawalDevicesBankTypeEnum.VCB)
                {
                    if (await _browser.VcbIsLoggedIn())
                    {
                        if (deviceInfo.Process == (int)WithdrawalDevicesProcessTypeEnum.Process)
                        {
                            _browser.LogInfo($"查询订单: {DateTime.Now}");

                            var checkOrder = await _browser.CheckWithdrawOrder(Phone, BankType);

                            if (checkOrder == null)
                            {
                                var withdrawalOrder = await _browser.GetWithdrawOrder(Phone, BankType);
                                if (withdrawalOrder != null)
                                {
                                    _browser.LogInfo($"获取消息：{DateTime.Now}--OrderId:{withdrawalOrder.OrderNo}");
                                    try
                                    {
                                        ////更新订单未转账中
                                        blnUpdateOrder = await _browser.UpdateWithdrawOrder(withdrawalOrder.Id, "", WithdrawalOrderStatusEnum.Pending, "");

                                        //WithdrawalOrdersMongoEntity order = null;
                                        //var orderTemp = await _withdrawalOrdersService.GetWithdrawOrderByOrderNumber(withdrawalOrder.MerchantCode, withdrawalOrder.OrderNo);
                                        //if (orderTemp == null)
                                        //{
                                        //    return;
                                        //}
                                        //if (orderTemp.OrderStatus == WithdrawalOrderStatusEnum.Wait)
                                        //{
                                        //    orderTemp.OrderStatus = WithdrawalOrderStatusEnum.Pending;
                                        //    await _withdrawalOrdersService.UpdateAsync(orderTemp);

                                        _browser.LogInfo($"进入转账：{DateTime.Now}--OrderId:{withdrawalOrder.OrderNo}");

                                        //创建出款订单
                                        var orderNo = withdrawalOrder.OrderNo;
                                        if (orderNo.Contains("_"))
                                        {
                                            orderNo = orderNo.Split("_")[0];
                                        }
                                        else
                                        {
                                            orderNo = orderNo.Substring(2);
                                        }

                                        var toBank = string.Empty;
                                        var vcbToBank = VcbExtBank.Unknown;
                                        if (withdrawalOrder.BenBankName.ToLower().Contains("VIETCOM".ToLower()))
                                        {
                                            _browser.LogInfo($"VCB转账：{DateTime.Now}--OrderId:{withdrawalOrder.OrderNo}");
                                            vcbToBank = VcbExtBank.VCB;
                                            toBank = "VCB";
                                        }
                                        else
                                        {
                                            _browser.LogInfo($"其他银行转账：{DateTime.Now}--OrderId:{withdrawalOrder.OrderNo}");
                                            vcbToBank = withdrawalOrder.BenBankName.Replace(" ", "").ToVcbExtBank();
                                            toBank = withdrawalOrder.BenBankName.Replace(" ", "");
                                        }

                                        if (vcbToBank == VcbExtBank.Unknown)
                                        {
                                            blnUpdateOrder = await _browser.UpdateWithdrawOrder(withdrawalOrder.Id, "", WithdrawalOrderStatusEnum.ErrorBank, "");
                                            return;
                                        }

                                        var fromBank = Bank.VcbBank;
                                        var user = deviceInfo.Phone;
                                        var password = deviceInfo.LoginPassWord;
                                        var toAccNum = withdrawalOrder.BenAccountNo;
                                        var toAccName = withdrawalOrder.BenAccountName;
                                        int toAmount = withdrawalOrder.OrderMoney.ToString("F0").StrToInt(); // int
                                        var remarks = $"{withdrawalOrder.BenAccountName} {orderNo}";
                                        _browser.DeviceId = deviceInfo.DeviceId;

                                        blnTransfer = true;
                                        _iRefreshInterval = 0;
                                        _browser.LogInfo($"Start transfer, {fromBank}, {orderNo}, {user}, {toBank}, {toAccNum}, {toAccNum}, {toAmount}, {remarks}");
                                        var transferWatch = System.Diagnostics.Stopwatch.StartNew();
                                        var transfer = await _browser.TransferAsync(fromBank, orderNo, user, password, toBank, toAccNum, toAccName, toAmount, remarks);
                                        _browser.FirstTime = false;
                                        transferWatch.Stop();
                                        _browser.LogInfo($"End transfer. Total time: {transferWatch.ElapsedMilliseconds.MilisecondsToTimeTaken()}");
                                        _browser.LogInfo($"Report statistic|{transferWatch.Elapsed.TotalSeconds.To3Decimal()}|{_browser.VcbReportText}");

                                        if (_browser.Status != TransferStatus.Success)
                                        {
                                            WithdrawalOrderStatusEnum orderStatus = WithdrawalOrderStatusEnum.Fail;
                                            if (_browser.Status == TransferStatus.Failed)
                                            {
                                                orderStatus = WithdrawalOrderStatusEnum.Fail;
                                            }
                                            if (_browser.Status == TransferStatus.ErrorBank)
                                            {
                                                orderStatus = WithdrawalOrderStatusEnum.ErrorBank;
                                            }
                                            if (_browser.Status == TransferStatus.ErrorCard)
                                            {
                                                orderStatus = WithdrawalOrderStatusEnum.ErrorCard;
                                            }
                                            blnUpdateOrder = await _browser.UpdateWithdrawOrder(withdrawalOrder.Id, "", WithdrawalOrderStatusEnum.Fail, "");
                                            _browser.LogInfo($"Withdrawal failed.");
                                            return;
                                        }

                                        blnUpdateOrder = await _browser.UpdateWithdrawOrder(withdrawalOrder.Id, _browser.TransactionId, WithdrawalOrderStatusEnum.Success, "");
                                        _browser.LogInfo($"Withdrawal success.");

                                        _browser.LogInfo($"Set balance, {Phone}, {_browser.BalanceAfterTransfer}");
                                        await _browser.UpdateBalance(deviceInfo.DeviceId, _browser.BalanceAfterTransfer);
                                        //VcbSetBalance(paymentInfo.Id, deviceInfo.Phone, _browser.BalanceAfterTransfer);

                                        _browser.LogInfo($"转账完成：{DateTime.Now}--OrderId:{orderNo}");
                                        //}
                                    }
                                    catch (Exception ex)
                                    {
                                        blnUpdateOrder = await _browser.UpdateWithdrawOrder(withdrawalOrder.Id, "", WithdrawalOrderStatusEnum.Fail, "");
                                        _browser.LogError($"vcb: {Phone}, 出款队列异常: {ex}", $"vcb: {Phone}, 出款队列异常: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                    // Refresh if not performing transfer and first launch
                    // Refresh if not performing transfer each 3 minutes (180 seconds)
                    if (!blnTransfer)
                    {
                        if (_browser.FirstTime || _iRefreshInterval >= _iRefreshTime)
                        {
                            var fromBank = Bank.VcbBank;
                            var user = deviceInfo.Phone;
                            var password = deviceInfo.LoginPassWord;
                            _browser.DeviceId = deviceInfo.DeviceId;

                            _browser.LogInfo($"Start refresh... {_iRefreshInterval}");
                            var refresh = await _browser.RefreshAsync(fromBank, user, password);
                            _browser.LogInfo($"End refresh... {_iRefreshInterval}");
                            _browser.LogInfo($"Report statistic||{_browser.VcbReportText}");
                            if (_browser.FirstTime && !refresh)
                            { _iRefreshInterval = _iRefreshTime; } // Quickly trigger another refresh if first time login failed
                            else { _iRefreshInterval = 0; }
                            _browser.FirstTime = false;
                        }
                        else
                        { _browser.LogInfo($"Next refresh at {_iRefreshInterval}/{_iRefreshTime}"); }
                    }
                }
            }
            catch (Exception ex)
            {
                _browser.LogError($"账户：{Phone}, 错误 {ex}", $"账户：{Phone}, 错误 {ex.Message}");
            }
        }

        private async Task TestVcbAsync()
        {
            try
            {
                var doTransfer = false;
                var blnTransfer = false;

                ////External Transfer
                var fromBank = Bank.VcbBank;
                var user = "0966493205";
                var password = "Win168168@";
                var toBank = "TCB";
                var toAccNum = "7703099999"; // 7703099999
                var toAccName = "NGUYEN VAN HUY";
                int toAmount = 2000; // int
                var remarks = "2000";

                ////// Internal Transfer
                //var fromBank = Bank.VcbBank;
                //var user = "0966493205"; // "0966493205";
                //var password = "Win168168@";
                //var toBank = "VCB";
                //var toAccNum = "336639128"; // 3366391283 // 0984875872
                //var toAccName = "HA THI PHUONG HAO";
                //int toAmount = 2000; // int
                //var remarks = "2000";

                if (doTransfer) // debug conditionals
                {
                    blnTransfer = true;
                    _browser.LogInfo($"Start test transfer, {fromBank}, [ORDERNO], {user}, {toBank}, {toAccNum}, {toAccNum}, {toAmount}, {remarks}");
                    var transferWatch = System.Diagnostics.Stopwatch.StartNew();
                    var result = await _browser.TransferAsync(fromBank, "[ORDERNO]", user, password, toBank, toAccNum, toAccName, toAmount, remarks);
                    _browser.FirstTime = false;
                    transferWatch.Stop();
                    _browser.LogInfo($"End test transfer. Total time: {transferWatch.ElapsedMilliseconds.MilisecondsToTimeTaken()}");
                    _browser.LogInfo($"Report statistic|{transferWatch.Elapsed.TotalSeconds.To3Decimal()}|{_browser.VcbReportText}");
                    _iRefreshInterval = 0;
                }

                // Refresh if not performing transfer and first launch
                // Refresh if not performing transfer each 3 minutes (180 seconds)
                if (!blnTransfer)
                {
                    if (_browser.FirstTime || _iRefreshInterval >= _iRefreshTime)
                    {
                        _browser.LogInfo($"Start refresh... {_iRefreshInterval}");
                        // Refresh Browser
                        var refresh = await _browser.RefreshAsync(fromBank, user, password);
                        _browser.LogInfo($"End refresh... {_iRefreshInterval}");
                        _browser.LogInfo($"Report statistic||{_browser.VcbReportText}");
                        if (_browser.FirstTime && !refresh)
                        {
                            _iRefreshInterval = _iRefreshTime; // Quickly trigger another refresh if first time login failed
                        }
                        else
                        {
                            _iRefreshInterval = 0;
                        }
                        _browser.FirstTime = false;
                    }
                    else
                    {
                        _browser.LogInfo($"Next refresh at {_iRefreshInterval}/{_iRefreshTime}");
                    }
                }
            }
            catch (Exception ex)
            {
                _browser.LogError($"Error, {ex}", $"Error, {ex.Message}");
            }
        }

        private async Task TestTcbAsync()
        {
            try
            {
                var doTransfer = false;
                var blnTransfer = false;

                //// External Transfer
                //var fromBank = Bank.TcbBank;
                //var user = "0966493205"; // "0966493205";
                //var password = "Win168168@"; // "Win168168@";
                //var toBank = "vietcombank"; // TcbExtBank.MB; // Bank.MbBank;
                //var toAccNum = "1042306840"; // "0984875872"; // 0984875872
                //var toAccName = "NGUYEN VAN HUY"; // "NONG VAN TUYEN";
                //int toAmount = 2000; // int
                //var remarks = "2000";

                // Internal Transfer
                var fromBank = Bank.TcbBank;
                var user = "0703805200"; // "0966493205"; // 
                var password = "Win168168@";
                var toBank = "tcb"; // Bank.TcbBank;
                var toAccNum = "8399599999"; // 0984875872
                var toAccName = "NGUYEN MINH THONG";
                int toAmount = 2000; // int
                var remarks = "2000";

                if (doTransfer) // debug conditionals
                {
                    blnTransfer = true;
                    _browser.LogInfo($"Start test transfer, {fromBank}, [ORDERNO], {user}, {toBank}, {toAccNum}, {toAccNum}, {toAmount}, {remarks}");
                    var transferWatch = System.Diagnostics.Stopwatch.StartNew();
                    var result = await _browser.TransferAsync(fromBank, "[ORDERNO]", user, password, toBank, toAccNum, toAccName, toAmount, remarks);
                    _browser.FirstTime = false;
                    transferWatch.Stop();
                    _browser.LogInfo($"End test transfer. Total time: {transferWatch.ElapsedMilliseconds.MilisecondsToTimeTaken()}");
                    _browser.LogInfo($"Report statistic|{transferWatch.Elapsed.TotalSeconds.To3Decimal()}|{_browser.TcbReportText}");
                    _iRefreshInterval = 0;
                }

                // Refresh if not performing transfer and first launch
                // Refresh if not performing transfer each 3 minutes (180 seconds)
                if (!blnTransfer)
                {
                    if (_browser.FirstTime || _iRefreshInterval >= _iRefreshTime)
                    {
                        _browser.LogInfo($"Start refresh... {_iRefreshInterval}");
                        // Refresh Browser
                        var refresh = await _browser.RefreshAsync(fromBank, user, password);
                        _browser.LogInfo($"End refresh... {_iRefreshInterval}");
                        _browser.LogInfo($"Report statistic||{_browser.TcbReportText}");
                        if (_browser.FirstTime && !refresh)
                        {
                            _iRefreshInterval = _iRefreshTime; // Quickly trigger another refresh if first time login failed
                        }
                        else
                        {
                            _iRefreshInterval = 0;

                        }
                        _browser.FirstTime = false;
                    }
                    else
                    {
                        _browser.LogInfo($"Next refresh at {_iRefreshInterval}/{_iRefreshTime}");
                    }
                }
            }
            catch (Exception ex)
            {
                _browser.LogError($"Error, {ex}", $"Error, {ex.Message}");
            }
        }

        private async Task TestTcbBizAsync()
        {
            try
            {
                var doTransfer = false;
                var blnTransfer = false;

                // External Transfer
                var fromBank = Bank.TcbBiz;
                var user = "ctyxnksgc";
                var password = "Bet168168@";
                var toBank = "tcb";
                var toAccNum = "7703099999";
                var toAccName = "NGUYEN VAN HUY";
                int toAmount = 2000; // int
                var remarks = "2000";

                //// Internal Transfer
                //var fromBank = Bank.TcbBank;
                //var user = "0966493205";
                //var password = "Win168168@";
                //var toBank = "tcb"; // Bank.TcbBank;
                //var toAccNum = "8399599999"; // 0984875872
                //var toAccName = "NGUYEN MINH THONG";
                //int toAmount = 2000; // int
                //var remarks = "2000";

                if (doTransfer) // debug conditionals
                {
                    blnTransfer = true;
                    _browser.LogInfo($"Start test transfer, {fromBank}, [ORDERNO], {user}, {toBank}, {toAccNum}, {toAccNum}, {toAmount}, {remarks}");
                    var transferWatch = System.Diagnostics.Stopwatch.StartNew();
                    var result = await _browser.TransferAsync(fromBank, "[ORDERNO]", user, password, toBank, toAccNum, toAccName, toAmount, remarks);
                    _browser.FirstTime = false;
                    transferWatch.Stop();
                    _browser.LogInfo($"End test transfer. Total time: {transferWatch.ElapsedMilliseconds.MilisecondsToTimeTaken()}");
                    _browser.LogInfo($"Report statistic|{transferWatch.Elapsed.TotalSeconds.To3Decimal()}|{_browser.TcbReportText}");
                    _iRefreshInterval = 0;
                }

                // Refresh if not performing transfer and first launch
                // Refresh if not performing transfer each 3 minutes (180 seconds)
                if (!blnTransfer)
                {
                    if (_browser.FirstTime || _iRefreshInterval >= _iRefreshTime)
                    {
                        _browser.LogInfo($"Start refresh... {_iRefreshInterval}");
                        // Refresh Browser
                        var refresh = await _browser.RefreshAsync(fromBank, user, password);
                        _browser.LogInfo($"End refresh... {_iRefreshInterval}");
                        _browser.LogInfo($"Report statistic||{_browser.TcbReportText}");
                        if (_browser.FirstTime && !refresh)
                        {
                            _iRefreshInterval = _iRefreshTime; // Quickly trigger another refresh if first time login failed
                        }
                        else
                        {
                            _iRefreshInterval = 0;

                        }
                        _browser.FirstTime = false;
                    }
                    else
                    {
                        _browser.LogInfo($"Next refresh at {_iRefreshInterval}/{_iRefreshTime}");
                    }
                }
            }
            catch (Exception ex)
            {
                _browser.LogError($"Error, {ex}", $"Error, {ex.Message}");
            }
        }

        private void SetToolStripLayout()
        {
            var width = toolStrip1.Width;
            foreach (ToolStripItem item in toolStrip1.Items)
            {
                if (item != tstxtUrl)
                {
                    width -= item.Width - item.Margin.Horizontal;
                }
            }
            tstxtUrl.Width = Math.Max(0, width - tstxtUrl.Margin.Horizontal - 18);
        }

        private void SetSpliterDistance(int distance, bool resetSize = true)
        {
            if (resetSize)
                _splitterDistance = distance;

            if (this.WindowState != FormWindowState.Minimized)
                this.splitContainer1.SplitterDistance = distance;
        }

        private void WriteMessage(string message)
        {
            this.tsslblMsg.Text = message;
        }

        private void SetBalance(decimal balance)
        {
            this.tsslblBalance.Text = double.Parse(balance.ToString()).ToString("C0", CultureInfo.GetCultureInfo("vi-VN"));
        }

        private void SetBusy(bool busy)
        {
            var cursor = Cursors.Default;
            if (busy)
                cursor = Cursors.WaitCursor;
            this.Cursor = cursor;
        }

        private void SetUrl(string url)
        {
            if (this.tstxtUrl.Text != url)
                this.tstxtUrl.Text = url;
        }

        private void SetScreenshotButton(bool capturing)
        {
            tsmiScrnshot.Enabled = !capturing;
        }

        private void SetScreenshot(ScreenshotModel? screenshot)
        {
            this.pbScrnshot.Image?.Dispose();
            this.pbScrnshot.Image = screenshot?.Image.ToBitmap();
            this.txtScrnTime.Text = screenshot?.Image == null ? "" : $"Screenshot at {screenshot?.CapturedAt}";
            this.btnSave.Enabled = screenshot != null;
        }

        private void SaveScreenshot()
        {
            _browser.SaveScreenshot();
        }

        private async void CloseProgram()
        {
            while (_running)
            {
                _browser.LogInfo("Stoping, please wait...");
                await Task.Delay(1000);
            }
            _closeProgram = true;
            this.Close();
        }

        private async void ShowLogin1(bool show)
        {
            await _browser.ShowScreenshot(false);
            this.splitContainer1.Panel1.Controls.Clear();
            SetSpliterDistance(50);

            if (show)
            {
                var login1 = new ucLogin1(_browser);
                SetSpliterDistance(login1.Width);
                this.splitContainer1.Panel1.Controls.Add(login1);
                login1.Focus();
            }
        }

        private async void ShowLogin3(bool show)
        {
            await _browser.ShowScreenshot(false);
            this.splitContainer1.Panel1.Controls.Clear();
            SetSpliterDistance(50);

            if (show)
            {
                var login3 = new ucLogin3(_browser);
                SetSpliterDistance(login3.Width);
                this.splitContainer1.Panel1.Controls.Add(login3);
                login3.Focus();
            }
        }

        private async void ShowTransfer3(bool show)
        {
            await _browser.ShowScreenshot(false);
            this.splitContainer1.Panel1.Controls.Clear();
            SetSpliterDistance(50);

            if (show)
            {
                var transfer3 = new ucTransfer3(_browser);
                SetSpliterDistance(transfer3.Width);
                this.splitContainer1.Panel1.Controls.Add(transfer3);
                transfer3.Focus();
            }
        }

        private async void TcbShowLoginAuth(bool show)
        {
            await _browser.ShowScreenshot(false);
            this.splitContainer1.Panel1.Controls.Clear();
            SetSpliterDistance(50);

            if (show)
            {
                var auth = new ucAuth(_browser);
                SetSpliterDistance(auth.Width);
                this.splitContainer1.Panel1.Controls.Add(auth);
                auth.Focus();
            }
        }

        //private void TcbSetBalance(int payMentId, string userName, decimal dBalance)
        //{
        //    var balance = new BankBalanceModel()
        //    {
        //        Type = PayMentTypeEnum.TechcomBank,
        //        UserName = userName,
        //        Balance = 0,
        //        Balance2 = dBalance,
        //        UpdateTime = DateTime.Now
        //    };
        //    //_redisService.SetBalance(payMentId, balance);
        //}

        //private void VcbSetBalance(int payMentId, string userName, decimal dBalance)
        //{
        //    var balance = new BankBalanceModel()
        //    {
        //        Type = PayMentTypeEnum.VietcomBank,
        //        UserName = userName,
        //        Balance = 0,
        //        Balance2 = dBalance,
        //        UpdateTime = DateTime.Now
        //    };
        //    //_redisService.SetBalance(payMentId, balance);
        //}

        #endregion
    }
}
