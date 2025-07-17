using Abp.Extensions;
using Abp.Json;
using DotNetCore.CAP;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantConfig.Dto;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RabbitMqExtensions.Models;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;

namespace Neptune.NsPay.MqPublishBankOrderNotify.Jobs
{
    public class PublishNotifyByTcb : BackgroundService
    {
        private readonly ICapPublisher _capBus;
        private readonly IRedisService _redisService;
        private readonly ILogger<PublishNotifyByTcb> _logger;
        private readonly IMerchantSettingService _merchantSettingService;
        private readonly IMerchantService _merchantService;
        private readonly IPayGroupMentService _payGroupMentService;
        public PublishNotifyByTcb(ILogger<PublishNotifyByTcb> logger,
            ICapPublisher capBus,
            IRedisService redisService,
            IMerchantSettingService merchantSettingService,
            IMerchantService merchantService,
            IPayGroupMentService payGroupMentService)
        {
            _logger = logger;
            _capBus = capBus;
            _redisService = redisService;
            _merchantSettingService = merchantSettingService;
            _merchantService = merchantService;
            _payGroupMentService = payGroupMentService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) {
                _logger.LogInformation("PublishNotifyByTcb Worker running at: {time}", DateTimeOffset.Now);
                var merchantConfigs = _merchantSettingService.GetAll()
                                                .Where(e =>
                                                !e.TelegramNotifyBotId.IsNullOrEmpty()
                                                & !e.BankNotifyText.IsNullOrEmpty()
                                                & !e.TelegramNotifyChatId.IsNullOrEmpty()
                                                & !e.BankNotify.IsNullOrEmpty());

                var merchants = _merchantService.GetAll();
                var paygroups = _payGroupMentService.GetWhere(r => !r.IsDeleted).ToList();
                int notifyCount = AppSettings.Configuration["BankOrderNotifyCount"].ToInt();
                try
                {
                    foreach (var item in merchantConfigs)
                    {
                        var merchant = merchants.FirstOrDefault(r => r.MerchantCode == item.MerchantCode);
                        var paygroupInfo = paygroups.Where(r => r.GroupId == merchant.PayGroupId && !r.IsDeleted).Select(r => r.PayMentId).ToList();

                        _logger.LogInformation("商户：{merchatcode},开启通知", item.MerchantCode);
                        var banks = item.BankNotify.FromJsonString<List<MerchantConfigBank>>();
                        if (banks != null)
                        {
                            var tcbBankInfo = banks.Where(r => r.IsOpen & r.Type == PayMentTypeEnum.TechcomBank).FirstOrDefault();
                            if (tcbBankInfo != null)
                            {
                                var bankOrderNotifysList = new List<BankOrderNotifyModel>();
                                while (true)
                                {
                                    var bankOrderNotifys = _redisService.GetTcbBankOrderNotifyMqPublish();
                                    if (bankOrderNotifys != null)
                                    {
                                        bankOrderNotifysList.Add(bankOrderNotifys);
                                    }

                                    if (bankOrderNotifysList.Count >= notifyCount || bankOrderNotifys == null)
                                    {
                                        bankOrderNotifysList = bankOrderNotifysList.Distinct().ToList();
                                        _logger.LogInformation("商户：{merchantcode},收款数据：{count}", item.MerchantCode, bankOrderNotifysList.Count());
                                        var bankNotifies = bankOrderNotifysList.Where(r => paygroupInfo.Contains(r.PayMentId) && r.Money >= tcbBankInfo.Money).ToList();
                                        if (bankNotifies.Any())
                                        {
                                            _logger.LogInformation("商户：{merchantcode},提示数据：{count}", item.MerchantCode, bankNotifies.Count());
                                            bankNotifies = bankNotifies.OrderBy(r => r.TransferTime).ToList();
                                            await _capBus.PublishAsync(MQSubscribeStaticConsts.BankOrderNotify,
                                                                new BankOrderNotifyDto
                                                                {
                                                                    Title = BankNameConst.TCB,
                                                                    NotifyDetail = bankNotifies,
                                                                    TelegramNotifyBotId = item.TelegramNotifyBotId,
                                                                    TelegramNotifyChatId = item.TelegramNotifyChatId,
                                                                    MerchantCode = item.MerchantCode,
                                                                    BankNotifyText = item.BankNotifyText
                                                                });

                                            _logger.LogInformation("商户：{merchantcode},Tcb发送mq数据：{data}", item.MerchantCode, bankNotifies.ToJsonString());
                                            bankOrderNotifysList.Clear();
                                        }
                                    }

                                    if (bankOrderNotifys == null) break;

                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("PublishNotifyByTcb 队列错误：" + ex);
                }

                await Task.Delay(1000, stoppingToken);

            }
        }
    }
}
