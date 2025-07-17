using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.Localization;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Neptune.NsPay.Commons;
using PayPalCheckoutSdk.Orders;
using Neptune.NsPay.Merchants;
using Newtonsoft.Json;

namespace Neptune.NsPay.TelegramNotify
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMerchantService _merchantService;
        private readonly IMerchantSettingsService _merchantSettingsService;
        private readonly IRedisService _redisService;
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IPayMentService _paymentService;
        private readonly IMerchantWithdrawBankService _merchantWithdrawBankService;

        public Worker(ILogger<Worker> logger,
            IMerchantService merchantService,
            IMerchantSettingsService merchantSettingsService,
            IRedisService redisService,
            IPayGroupMentService payGroupMentService,
            IPayMentService paymentService,
            IMerchantWithdrawBankService merchantWithdrawBankService)
        {
            _logger = logger;
            _merchantService = merchantService;
            _merchantSettingsService = merchantSettingsService;
            _redisService = redisService;
            _payGroupMentService = payGroupMentService;
            _paymentService = paymentService;
            _merchantWithdrawBankService = merchantWithdrawBankService;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    if (_logger.IsEnabled(LogLevel.Information))
            //    {
            //        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    }

            //    var merchantConfigs = _merchantSettingsService.GetAll();
            //    var merchants = _merchantService.GetAll();
            //    List<PayMent> list = new List<PayMent>();
            //    list = _paymentService.GetWhere(r => r.IsDeleted == false);
            //    var paygroups = _payGroupMentService.GetWhere(r => r.IsDeleted == false).ToList();
            //    foreach (var item in merchantConfigs)
            //    {
            //        var merchant = merchants.FirstOrDefault(r => r.MerchantCode == item.MerchantCode);
            //        var paygroupInfo = paygroups.Where(r => r.GroupId == merchant.PayGroupId && r.IsDeleted == false).Select(r => r.PayMentId).ToList();
            //        if (!string.IsNullOrEmpty(item.TelegramNotifyBotId) && !string.IsNullOrEmpty(item.BankNotifyText) && !string.IsNullOrEmpty(item.TelegramNotifyChatId))
            //        {
            //            _logger.LogInformation("商户：{merchatcode},开启通知", item.MerchantCode);
            //            //检查是否配置下发银行
            //            var banks = _merchantWithdrawBankService.GetWhere(r => r.MerchantCode == merchant.MerchantCode);
            //            if (banks.Count > 0)
            //            {
            //                //获取缓存数据
            //                List<BankOrderNotifyModel> bankOrderNotifies = new List<BankOrderNotifyModel>();
            //                var bankOrderNotifiesAcb = _redisService.GetListBankOrderNotifyByAcb();
            //                var bankOrderNotifiesBidv = _redisService.GetListBankOrderNotifyByBidv();
            //                var bankOrderNotifiesMB = _redisService.GetListBankOrderNotifyByMB();
            //                var bankOrderNotifiesTcb = _redisService.GetListBankOrderNotifyByTcb();
            //                var bankOrderNotifiesVcb = _redisService.GetListBankOrderNotifyByVcb();
            //                var bankOrderNotifiesVtb = _redisService.GetListBankOrderNotifyByVtb();
            //                var bankOrderNotifiesPvcom = _redisService.GetListBankOrderNotifyByPVcom();
            //                bankOrderNotifies.AddRange(bankOrderNotifiesAcb);
            //                bankOrderNotifies.AddRange(bankOrderNotifiesBidv);
            //                bankOrderNotifies.AddRange(bankOrderNotifiesMB);
            //                bankOrderNotifies.AddRange(bankOrderNotifiesTcb);
            //                bankOrderNotifies.AddRange(bankOrderNotifiesVcb);
            //                bankOrderNotifies.AddRange(bankOrderNotifiesVtb);
            //                bankOrderNotifies.AddRange(bankOrderNotifiesPvcom);

            //                if (bankOrderNotifies.Count > 0)
            //                {
            //                    List<string> messages = new List<string>();
            //                    bankOrderNotifies = bankOrderNotifies.Distinct().ToList();
            //                    _logger.LogInformation("商户：{merchatcode},收款数据：{count}", item.MerchantCode, bankOrderNotifies.Count());
            //                    var bankNotifies = bankOrderNotifies.Where(r => paygroupInfo.Contains(r.PayMentId));
            //                    if (bankNotifies.Count() > 0)
            //                    {
            //                        _logger.LogInformation("商户：{merchatcode},提示数据：{count}", item.MerchantCode, bankNotifies.Count());
            //                        bankNotifies = bankNotifies.OrderBy(r => r.TransferTime).ToList();
            //                        foreach (var notify in bankNotifies)
            //                        {
            //                            var culInfo = CultureHelper.GetCultureInfoByChecking("vi-VN");
            //                            var paymentInfo = list.FirstOrDefault(r => r.Id == notify.PayMentId);
            //                            if (paymentInfo != null)
            //                            {
            //                                var message = item.BankNotifyText.Replace("#cardno", paymentInfo.CardNumber).Replace("#phone", paymentInfo.Phone).Replace("#money", notify.Money.ToString("C0", culInfo)).Replace("#remark", notify.Remark).Replace("#time", notify.TransferTime.ToString("yyyy-MM-dd HH:mm:ss"));
            //                                messages.Add(message);
            //                                _logger.LogInformation("商户：{merchatcode},添加数据", item.MerchantCode);
            //                            }
            //                        }

            //                        if (messages.Count > 0)
            //                        {
            //                            var bot = new TelegramBotClient(item.TelegramNotifyBotId);
            //                            var chats = item.TelegramNotifyChatId.Split(';');
            //                            foreach (var chatId in chats)
            //                            {
            //                                var message = string.Join("\r\n ", messages);
            //                                try
            //                                {
            //                                    await bot.SendMessageAsync(chatId, message);
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    NlogLogger.Error("发送消息异常");
            //                                }
            //                            }
            //                        }

            //                    }
            //                }
            //            }
            //        }
            //    }


            //    await Task.Delay(1000, stoppingToken);
            //}

            var reliableQueue = _redisService.GetTransferOrderQueue();
            reliableQueue.RetryInterval = 60;
            reliableQueue.RetryTimesWhenSendFailed = 1;

            while (!stoppingToken.IsCancellationRequested)
            {
                var transferOrder = await reliableQueue.TakeOneAsync(-1);
                if(transferOrder != null)
                {
                    if (!string.IsNullOrEmpty(transferOrder.Remark))
                    {
                        try
                        {
                            _logger.LogInformation("获取消息：{time}--Account:{orderid}", DateTimeOffset.Now, transferOrder.RefNo);
                            var merchantConfigs = _merchantSettingsService.GetAll();
                            var merchants = _merchantService.GetAll();
                            List<PayMent> payMents = _paymentService.GetWhere(r => r.IsDeleted == false);
                            var payGroupMent = await _payGroupMentService.GetFirstAsync(r => r.PayMentId == transferOrder.PayMentId && r.IsDeleted == false);
                            if (payGroupMent != null)
                            {
                                //var merchant = merchants.FirstOrDefault(r => r.PayGroupId == payGroupMent.GroupId);
                                var merchant = merchants.FirstOrDefault(r => r.PayGroupId == 5);
                                if (merchant != null && merchant.MerchantType== MerchantTypeEnum.Internal)
                                {
                                    var flag = 0;
                                    var checkCardResult = false;
                                    var checkNameResult = false;
                                    //判断记录是否在下发银行
                                    var banks = _merchantWithdrawBankService.GetWhere(r => r.MerchantCode == merchant.MerchantCode && r.IsDeleted == false);
                                    if (banks.Count > 0)
                                    {
                                        var checkCard = banks.FirstOrDefault(r => transferOrder.Remark.Trim().Replace(" ", "").Contains(r.ReceivCard.Trim().Replace(" ", "")));
                                        if (checkCard == null)
                                        {
                                            checkCardResult = true;
                                        }
                                        //var checkName = banks.FirstOrDefault(r => transferOrder.Remark.Trim().Replace(" ", "").Contains(r.ReceivName.Trim().Replace(" ", "")));
                                        //if (checkName == null)
                                        //{
                                        //    flag = 1;
                                        //    checkNameResult = true;
                                        //}
                                        if (checkCardResult)
                                        {
                                            var merchantConfig = merchantConfigs.FirstOrDefault(r => r.MerchantCode == merchant.MerchantCode);
                                            if (merchantConfig != null)
                                            {
                                                _logger.LogInformation("商户：{merchatcode},添加数据", merchant.MerchantCode);
                                                if (!string.IsNullOrEmpty(merchantConfig.TelegramNotifyBotId) && !string.IsNullOrEmpty(merchantConfig.BankNotifyText) && !string.IsNullOrEmpty(merchantConfig.TelegramNotifyChatId))
                                                {
                                                    var culInfo = CultureHelper.GetCultureInfoByChecking("vi-VN");
                                                    var paymentInfo = payMents.FirstOrDefault(r => r.Id == transferOrder.PayMentId);
                                                    if (paymentInfo != null)
                                                    {
                                                        _logger.LogInformation("商户：{merchatcode},发送信息", merchant.MerchantCode);
                                                        var message = merchantConfig.BankNotifyText.Replace("#cardno", paymentInfo.CardNumber).Replace("#phone", paymentInfo.Phone).Replace("#money", transferOrder.Money.ToString("C0", culInfo)).Replace("#remark", transferOrder.Remark).Replace("#time", transferOrder.TransferTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                                        var bot = new TelegramBotClient(merchantConfig.TelegramNotifyBotId);
                                                        var chats = merchantConfig.TelegramNotifyChatId.Split(';');
                                                        foreach (var chatId in chats)
                                                        {
                                                            try
                                                            {
                                                                //var flagMessage = "Card\r\n";
                                                                //if (flag == 1)
                                                                //{
                                                                //    flagMessage = "Name\r\n";
                                                                //}
                                                                await bot.SendMessageAsync(chatId, message);
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                NlogLogger.Error("发送消息异常:" + ex);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            var isKnow = reliableQueue.Acknowledge(JsonConvert.SerializeObject(transferOrder));
                            if (isKnow > 0)
                            {
                                _logger.LogInformation("获取消息：OrderId:{orderid}，完成消费", transferOrder.RefNo);
                            }
                            else
                            {
                                _logger.LogInformation("获取消息：OrderId:{orderid}，失败消费", transferOrder.RefNo);
                            }
                        }
                        catch (Exception ex)
                        {
                            NlogLogger.Error("Telegram消息异常：" + ex);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}
