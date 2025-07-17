using Neptune.NsPay.Commons;
using System;
using System.Diagnostics;

namespace Neptune.NsPay.TcbBankScript
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
                var port =Convert.ToInt32(AppSettings.Configuration["APPIUM_HOST"]);
                TcbAppiumHelper acbAppiumHelper = new TcbAppiumHelper();
                var startInfo = TcbAppiumHelper.StartAppiumService(port);
                process = new Process { StartInfo = startInfo };

                process.OutputDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                process.ErrorDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine("ERROR: " + e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // �ȴ���������ȷ�� Appium �������
                System.Threading.Thread.Sleep(5000);

                var init = acbAppiumHelper.SetUp(port, deviceName);
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
                            acbAppiumHelper.KeepSessionAlive(port, deviceName);
                        }
                    }

                    try
                    {
                        //����Ƿ��е���
                        acbAppiumHelper.ClickAlert();

                        //�ж��Ƿ���ⵯ��
                        acbAppiumHelper.ClickErrorAlert();

                        //�ж��Ƿ��¼
                        var islogin = acbAppiumHelper.IsLogin();
                        if (islogin)
                        {
                            //�����ť
                            acbAppiumHelper.ClickBtnAccept();
                            //�������룬ȷ�ϵ�¼
                            var havePwd = acbAppiumHelper.CheckPassword();
                            if (havePwd)
                            {
                                acbAppiumHelper.InputPassword(deviceInfo.Otp);
                            }
                        }
                        else
                        {
                            //����Ƿ�����ҳ
                            var isMain = acbAppiumHelper.IsMain();
                            if (!isMain)
                            {
                                //�Ѿ���¼������Ҫ��������
                                var checkBtn = acbAppiumHelper.CheckBtnAccept();
                                if (checkBtn)
                                {
                                    acbAppiumHelper.ClickBtnAccept();
                                    var havepwd2 = acbAppiumHelper.CheckPassword(2000);
                                    if (havepwd2)
                                    {
                                        acbAppiumHelper.InputPassword(deviceInfo.Otp);
                                    }
                                }
                                else
                                {
                                    //�Ƿ�ת��
                                    var havepwd = acbAppiumHelper.CheckPassword(2000);
                                    if (havepwd)
                                    {
                                        acbAppiumHelper.InputPassword(deviceInfo.Otp);

                                        //�����ύ
                                        acbAppiumHelper.ClickBtnAccept();
                                        var havepwd2 = acbAppiumHelper.CheckPassword(2000);
                                        if (havepwd2)
                                        {
                                            acbAppiumHelper.InputPassword(deviceInfo.Otp);
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
            catch (Exception ex)
            {

            }
            finally
            {
                process?.Kill();
            }
        }
    }
}
