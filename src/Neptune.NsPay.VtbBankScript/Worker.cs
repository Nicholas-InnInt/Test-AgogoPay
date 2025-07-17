using Neptune.NsPay.Commons;
using System.Diagnostics;

namespace Neptune.NsPay.VtbBankScript
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
                //var startInfo = VtbAppiumHelper.StartAppiumService(port);
                //process = new Process { StartInfo = startInfo };

                //process.OutputDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                //process.ErrorDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine("ERROR: " + e.Data); };

                //process.Start();
                //process.BeginOutputReadLine();
                //process.BeginErrorReadLine();

                //// �ȴ���������ȷ�� Appium �������
                //System.Threading.Thread.Sleep(5000);

                var init = vtbAppiumHelper.SetUp(port, deviceName);
                if (!init)
                {
                    _logger.LogInformation("init error");
                    return;
                }
                //��������
                //vtbAppiumHelper.InputPassword(deviceInfo.Otp);

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    }

                    //�Ƿ���ת�˵���
                    var transWindow = vtbAppiumHelper.IsTransAlert();
                    if(transWindow)
                    {
                        //�������
                        vtbAppiumHelper.ClickAlert();
                        var pwd = vtbAppiumHelper.CheckPassword();
                        if (pwd)
                        {
                            //��������,�Զ��ύ
                            vtbAppiumHelper.InputPassword(deviceInfo.Otp);

                            //��ѯ�Ƿ�ɹ�����תȷ��ҳ��
                            var isComfirm= vtbAppiumHelper.IsComfirm();
                            if(isComfirm)
                            {
                                vtbAppiumHelper.ComfirmTransfer();
                            }
                        }
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
