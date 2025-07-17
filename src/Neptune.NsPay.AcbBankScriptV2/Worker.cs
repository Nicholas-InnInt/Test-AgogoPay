using Neptune.NsPay.Commons;

namespace Neptune.NsPay.AcbBankScriptV2
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
            AcbAppiumHelper acbAppiumHelper = new AcbAppiumHelper();
            var init = acbAppiumHelper.SetUp(deviceName);
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
                    var timespan= tempTime - dateTime;
                    if (timespan.TotalSeconds >= 45)
                    {
                        dateTime = DateTime.Now;
                        acbAppiumHelper.KeepSessionAlive(deviceName);
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
                            var timeSpan= tempTime - order.CreateTime;
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
                                    var result = await HttpHelper.UpdateOrderOtpAsync(deviceInfo.DeviceId, otp);
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
    }
}
