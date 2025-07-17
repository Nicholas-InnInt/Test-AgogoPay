using Neptune.NsPay.BankAutoScript.Helper;
using Neptune.NsPay.Commons;
using System;
using System.Diagnostics;

namespace Neptune.NsPay.BankAutoScript
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Process? process = null;
            try
            {
                var bankType =Convert.ToInt32(AppSettings.Configuration["BankType"]);
                //Tcb
                #region
                if (bankType == 0)
                {
                    //��ʼ��
                    //����豸
                    var deviceInfo = await HttpHelper.GetDeviceIdAsync();
                    if (deviceInfo == null)
                    {
                        _logger.LogInformation("device error");
                        return;
                    }
                    if (deviceInfo.Otp.Length != 6)
                    {
                        _logger.LogInformation("Otp error");
                        return;
                    }
                    var deviceName = AppSettings.Configuration["DeviceName"];
                    var port = Convert.ToInt32(AppSettings.Configuration["APPIUM_HOST"]);
                    TcbAppiumHelper tcbAppiumHelper = new TcbAppiumHelper();
                    var startInfo = TcbAppiumHelper.StartAppiumService(port);
                    process = new Process { StartInfo = startInfo };

                    process.OutputDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                    process.ErrorDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine("ERROR: " + e.Data); };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // �ȴ���������ȷ�� Appium �������
                    System.Threading.Thread.Sleep(5000);

                    var init = tcbAppiumHelper.SetUp(port, deviceName);
                    if (!init)
                    {
                        _logger.LogInformation("init error");
                        return;
                    }

                    DateTime dateTime = DateTime.Now;
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                        }

                        var tempTime = DateTime.Now;
                        if (DateTime.Now.Minute % 2 == 0)
                        {
                            var timespan = tempTime - dateTime;
                            if (timespan.TotalSeconds >= 45)
                            {
                                dateTime = DateTime.Now;
                                tcbAppiumHelper.KeepSessionAlive(port, deviceName);
                            }
                        }

                        try
                        {
                            //����Ƿ��е���
                            tcbAppiumHelper.ClickAlert();

                            //�ж��Ƿ���ⵯ��
                            tcbAppiumHelper.ClickErrorAlert();

                            //�ж��Ƿ��¼
                            var islogin = tcbAppiumHelper.IsLogin();
                            if (islogin)
                            {
                                //�����ť
                                tcbAppiumHelper.ClickBtnAccept();
                                //�������룬ȷ�ϵ�¼
                                var havePwd = tcbAppiumHelper.CheckPassword();
                                if (havePwd)
                                {
                                    tcbAppiumHelper.InputPassword(deviceInfo.Otp);
                                }
                            }
                            else
                            {
                                //����Ƿ�����ҳ
                                var isMain = tcbAppiumHelper.IsMain();
                                if (!isMain)
                                {
                                    //�Ѿ���¼������Ҫ��������
                                    var checkBtn = tcbAppiumHelper.CheckBtnAccept();
                                    if (checkBtn)
                                    {
                                        tcbAppiumHelper.ClickBtnAccept();
                                        var havepwd2 = tcbAppiumHelper.CheckPassword(2000);
                                        if (havepwd2)
                                        {
                                            tcbAppiumHelper.InputPassword(deviceInfo.Otp);
                                        }
                                    }
                                    else
                                    {
                                        //�Ƿ�ת��
                                        var havepwd = tcbAppiumHelper.CheckPassword(2000);
                                        if (havepwd)
                                        {
                                            tcbAppiumHelper.InputPassword(deviceInfo.Otp);

                                            //�����ύ
                                            tcbAppiumHelper.ClickBtnAccept();
                                            var havepwd2 = tcbAppiumHelper.CheckPassword(2000);
                                            if (havepwd2)
                                            {
                                                tcbAppiumHelper.InputPassword(deviceInfo.Otp);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            NlogLogger.Error("tcb�ű��쳣:" + ex.ToString());
                        }

                        await Task.Delay(1000, stoppingToken);
                    }
                }
                #endregion

                //Acb
                #region
                if (bankType == 2)
                {
                    var deviceInfo = await HttpHelper.GetDeviceIdAsync();
                    if (deviceInfo == null)
                    {
                        _logger.LogInformation("device error");
                        return;
                    }
                    if (deviceInfo.Otp.Length != 4)
                    {
                        _logger.LogInformation("Otp error");
                        return;
                    }
                    var deviceName = AppSettings.Configuration["DeviceName"];
                    var port = Convert.ToInt32(AppSettings.Configuration["APPIUM_HOST"]);
                    AcbAppiumHelper acbAppiumHelper = new AcbAppiumHelper();
                    var startInfo = AcbAppiumHelper.StartAppiumService(port);
                    process = new Process { StartInfo = startInfo };

                    process.OutputDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                    process.ErrorDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine("ERROR: " + e.Data); };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // �ȴ���������ȷ�� Appium �������
                    System.Threading.Thread.Sleep(5000);


                    var init = acbAppiumHelper.SetUp(port,deviceName);
                    if (!init)
                    {
                        _logger.LogInformation("init error");
                        return;
                    }
                    DateTime dateTime = DateTime.Now;
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                        }

                        var tempTime = DateTime.Now;
                        if (DateTime.Now.Minute % 2 == 0)
                        {
                            var timespan = tempTime - dateTime;
                            if (timespan.TotalSeconds >= 45)
                            {
                                dateTime = DateTime.Now;
                                acbAppiumHelper.KeepSessionAlive(port,deviceName);
                            }
                        }

                        try
                        {
                            var order = await HttpHelper.GetOrderOtpAsync(deviceInfo.DeviceId);
                            if (order != null)
                            {
                                acbAppiumHelper.BackMain();
                                if (order.OrderStatus == 0)
                                {
                                    var timeSpan = tempTime - order.CreateTime;
                                    if (timeSpan.TotalSeconds >= 90)
                                    {
                                        await HttpHelper.RemoveOrderOtpAsync(deviceInfo.DeviceId, order.OrderId);
                                    }
                                    else
                                    {
                                        _logger.LogInformation("start otp: {time}", DateTimeOffset.Now);
                                        var otp = acbAppiumHelper.Start(deviceInfo.Otp);
                                        if (!string.IsNullOrEmpty(otp))
                                        {
                                            //�ص�otp,д�뻺��
                                            var result = await HttpHelper.UpdateOrderOtpAsync(deviceInfo.DeviceId, order.OrderId, otp);
                                            if (result)
                                            {
                                                //��ѯ�Ƿ�ת�����
                                                var flag = 0;
                                                for (var i = 0; i < 3; i++)
                                                {
                                                    var checkResult = await HttpHelper.GetOrderOtpAsync(deviceInfo.DeviceId);
                                                    if (checkResult != null)
                                                    {
                                                        if (checkResult.OrderStatus == 1)
                                                        {
                                                            flag = 1;
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            otp = acbAppiumHelper.GetOtp();
                                                        }
                                                    }
                                                }
                                                if (flag == 1)
                                                {
                                                    //�������
                                                    await HttpHelper.RemoveOrderOtpAsync(deviceInfo.DeviceId, order.OrderId);
                                                    //ת����ɹر�
                                                    acbAppiumHelper.BackMain();
                                                }
                                            }
                                            else
                                            {
                                                _logger.LogInformation("update otp error: {time}", DateTimeOffset.Now);
                                            }
                                        }
                                        else
                                        {
                                            //�ر����´�
                                            acbAppiumHelper.BackMain();
                                            _logger.LogInformation("get otp error: {time}", DateTimeOffset.Now);
                                        }
                                    }
                                }
                                else
                                {
                                    await HttpHelper.RemoveOrderOtpAsync(deviceInfo.DeviceId, order.OrderId);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            NlogLogger.Error("accb�ű��쳣:" + ex.ToString());
                        }

                        await Task.Delay(1000, stoppingToken);
                    }
                }
                #endregion

                //vtb
                #region
                if (bankType == 3)
                {
                    //����豸
                    var deviceInfo = await HttpHelper.GetDeviceIdAsync();
                    if (deviceInfo == null)
                    {
                        _logger.LogInformation("device error");
                        return;
                    }
                    if (deviceInfo.Otp.Length != 4)
                    {
                        _logger.LogInformation("Otp error");
                        return;
                    }
                    var deviceName = AppSettings.Configuration["DeviceName"];
                    var port = Convert.ToInt32(AppSettings.Configuration["APPIUM_HOST"]);
                    VtbAppiumHelper vtbAppiumHelper = new VtbAppiumHelper();
                    var startInfo = VtbAppiumHelper.StartAppiumService(port);
                    process = new Process { StartInfo = startInfo };

                    process.OutputDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                    process.ErrorDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine("ERROR: " + e.Data); };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // �ȴ���������ȷ�� Appium �������
                    System.Threading.Thread.Sleep(5000);

                    var init = vtbAppiumHelper.SetUp(port, deviceName);
                    if (!init)
                    {
                        _logger.LogInformation("init error");
                        return;
                    }
                    //��������
                    //vtbAppiumHelper.InputPassword(deviceInfo.Otp);

                    DateTime dateTime = DateTime.Now;
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                        }

                        var tempTime = DateTime.Now;
                        if (DateTime.Now.Minute % 2 == 0)
                        {
                            var timespan = tempTime - dateTime;
                            if (timespan.TotalSeconds >= 45)
                            {
                                dateTime = DateTime.Now;
                                vtbAppiumHelper.KeepSessionAlive(port, deviceName);
                            }
                        }

                        //�Ƿ���ת�˵���
                        var transWindow = vtbAppiumHelper.IsTransAlert();
                        if (transWindow)
                        {
                            //�������
                            vtbAppiumHelper.ClickAlert();
                            var pwd = vtbAppiumHelper.CheckPassword();
                            if (pwd)
                            {
                                //��������,�Զ��ύ
                                vtbAppiumHelper.InputPassword(deviceInfo.Otp);

                                //��ѯ�Ƿ�ɹ�����תȷ��ҳ��
                                var isComfirm = vtbAppiumHelper.IsComfirm();
                                if (isComfirm)
                                {
                                    vtbAppiumHelper.ComfirmTransfer();
                                }
                            }
                            else
                            {
                                //������Ҫɨ��
                                var isComfirm = vtbAppiumHelper.IsComfirm(20000);
                                if (isComfirm)
                                {
                                    vtbAppiumHelper.ComfirmTransfer();
                                }
                            }
                        }

                        await Task.Delay(1000, stoppingToken);
                    }
                }
                #endregion

                //msb
                #region
                if (bankType == 4)
                {
                    //����豸
                    var deviceInfo = await HttpHelper.GetDeviceIdAsync();
                    if (deviceInfo == null)
                    {
                        _logger.LogInformation("device error");
                        return;
                    }
                    if (deviceInfo.Otp.Length != 4)
                    {
                        _logger.LogInformation("Otp error");
                        return;
                    }
                    var deviceName = AppSettings.Configuration["DeviceName"];
                    var port = Convert.ToInt32(AppSettings.Configuration["APPIUM_HOST"]);
                    MsbAppiumHelper msbAppiumHelper = new MsbAppiumHelper();
                    var startInfo = TcbAppiumHelper.StartAppiumService(port);
                    process = new Process { StartInfo = startInfo };

                    process.OutputDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                    process.ErrorDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine("ERROR: " + e.Data); };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // �ȴ���������ȷ�� Appium �������
                    System.Threading.Thread.Sleep(5000);

                    var init = msbAppiumHelper.SetUp(port, deviceName);
                    if (!init)
                    {
                        _logger.LogInformation("init error");
                        return;
                    }


                    DateTime dateTime = DateTime.Now;
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                        }

                        var tempTime = DateTime.Now;
                        if (DateTime.Now.Minute % 2 == 0)
                        {
                            var timespan = tempTime - dateTime;
                            if (timespan.TotalSeconds >= 45)
                            {
                                dateTime = DateTime.Now;
                                msbAppiumHelper.KeepSessionAlive(port, deviceName);
                            }
                        }

                        try
                        {

                            //����Ƿ�����ҳ
                            var isMain = msbAppiumHelper.IsMain();
                            if (isMain)
                            {
                                msbAppiumHelper.ClickBtnAccept();
                            }
                            var isOtp = msbAppiumHelper.IsOtp();
                            if (isOtp) 
                            {
                                msbAppiumHelper.InputOtp(deviceInfo.Otp);
                            }
                            var order = await HttpHelper.GetOrderOtpAsync(deviceInfo.DeviceId);
                            if (order != null)
                            {
                                //�Ƿ�������otp
                                var isCheckOtp = msbAppiumHelper.IsCheckOtp();
                                if (isCheckOtp)
                                {
                                    msbAppiumHelper.BtnCheckOtp();
                                    var transferOtp = msbAppiumHelper.IsTransferOtp();
                                    if (transferOtp)
                                    {
                                        //��ȡת��otp
                                        msbAppiumHelper.InputTransferOtp(order.TransferOtp);
                                        //��鰴ť
                                        var next = msbAppiumHelper.ClickBtnNext();
                                        if (next)
                                        {
                                            //��ȡotp
                                            var otp = msbAppiumHelper.GetOtp();
                                            if (!string.IsNullOrEmpty(otp))
                                            {
                                                //�ϴ�otp
                                                var result = await HttpHelper.UpdateOrderOtpAsync(deviceInfo.DeviceId, order.OrderId, otp);
                                                if (result)
                                                {
                                                    //��ѯ�Ƿ�ת�����
                                                    var flag = 0;
                                                    for (var i = 0; i < 3; i++)
                                                    {
                                                        var checkResult = await HttpHelper.GetOrderOtpAsync(deviceInfo.DeviceId);
                                                        if (checkResult != null)
                                                        {
                                                            if (checkResult.OrderStatus == 1)
                                                            {
                                                                flag = 1;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    if (flag == 1)
                                                    {
                                                        //�������
                                                        await HttpHelper.RemoveOrderOtpAsync(deviceInfo.DeviceId, order.OrderId);
                                                        //ת����ɹر�
                                                        msbAppiumHelper.ClickBtnGoBack();
                                                    }
                                                }
                                                else
                                                {
                                                    _logger.LogInformation("update otp error: {time}", DateTimeOffset.Now);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            NlogLogger.Error("sea�ű��쳣:" + ex.ToString());
                        }

                        await Task.Delay(1000, stoppingToken);
                    }
                }
                #endregion

                //sea
                #region
                if (bankType == 5)
                {
                    //����豸
                    var deviceInfo = await HttpHelper.GetDeviceIdAsync();
                    if (deviceInfo == null)
                    {
                        _logger.LogInformation("device error");
                        return;
                    }
                    if (deviceInfo.Otp.Length != 4)
                    {
                        _logger.LogInformation("Otp error");
                        return;
                    }
                    var deviceName = AppSettings.Configuration["DeviceName"];
                    var port = Convert.ToInt32(AppSettings.Configuration["APPIUM_HOST"]);
                    SeaAppiumHelper seaAppiumHelper = new SeaAppiumHelper();
                    var startInfo = TcbAppiumHelper.StartAppiumService(port);
                    process = new Process { StartInfo = startInfo };

                    process.OutputDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                    process.ErrorDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine("ERROR: " + e.Data); };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // �ȴ���������ȷ�� Appium �������
                    System.Threading.Thread.Sleep(5000);

                    var init = seaAppiumHelper.SetUp(port, deviceName);
                    if (!init)
                    {
                        _logger.LogInformation("init error");
                        return;
                    }

                    DateTime dateTime = DateTime.Now;
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                        }

                        var tempTime = DateTime.Now;
                        if (DateTime.Now.Minute % 2 == 0)
                        {
                            var timespan = tempTime - dateTime;
                            if (timespan.TotalSeconds >= 45)
                            {
                                dateTime = DateTime.Now;
                                seaAppiumHelper.KeepSessionAlive(port, deviceName);
                            }
                        }

                        try
                        {

                            //����Ƿ�����ҳ
                            var isMain = seaAppiumHelper.IsMain();
                            if (isMain)
                            {
                                seaAppiumHelper.ClickBtnAccept();
                            }
                            var order = await HttpHelper.GetOrderOtpAsync(deviceInfo.DeviceId);
                            if (order != null)
                            {
                                //�Ƿ�������otp
                                var isOtp = seaAppiumHelper.IsOtp();
                                if (isOtp)
                                {
                                    seaAppiumHelper.InputOtp(deviceInfo.Otp);
                                    var transferOtp = seaAppiumHelper.IsTransferOtp();
                                    if (transferOtp)
                                    {
                                        //��ȡת��otp
                                        seaAppiumHelper.InputTransferOtp(order.TransferOtp);
                                        //��鰴ť
                                        var next = seaAppiumHelper.ClickBtnNext();
                                        if (next)
                                        {
                                            //��ȡotp
                                            var otp = seaAppiumHelper.GetOtp();
                                            if (!string.IsNullOrEmpty(otp))
                                            {
                                                //�ϴ�otp
                                                var result = await HttpHelper.UpdateOrderOtpAsync(deviceInfo.DeviceId, order.OrderId, otp);
                                                if (result)
                                                {
                                                    //��ѯ�Ƿ�ת�����
                                                    var flag = 0;
                                                    for (var i = 0; i < 3; i++)
                                                    {
                                                        var checkResult = await HttpHelper.GetOrderOtpAsync(deviceInfo.DeviceId);
                                                        if (checkResult != null)
                                                        {
                                                            if (checkResult.OrderStatus == 1)
                                                            {
                                                                flag = 1;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    if (flag == 1)
                                                    {
                                                        //�������
                                                        await HttpHelper.RemoveOrderOtpAsync(deviceInfo.DeviceId, order.OrderId);
                                                        //ת����ɹر�
                                                        seaAppiumHelper.ClickBtnGoBack();
                                                    }
                                                }
                                                else
                                                {
                                                    _logger.LogInformation("update otp error: {time}", DateTimeOffset.Now);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            NlogLogger.Error("sea�ű��쳣:" + ex.ToString());
                        }

                        await Task.Delay(1000, stoppingToken);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                NlogLogger.Error("�ű��쳣��" + ex.ToString());
            }
            finally
            {
                process?.Kill();
            }
        }
    }
}
