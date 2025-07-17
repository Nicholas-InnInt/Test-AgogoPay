using Abp.Collections.Extensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Localization;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Newtonsoft.Json;
using System.Dynamic;
using System.Globalization;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using AutoMapper;
using Neptune.NsPay.Merchants.Dtos;
using Neptune.NsPay.PayGroupMents.Dtos;
using Neptune.NsPay.PayMents.Dtos;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services;

namespace Neptune.NsPay.TelegramPayHelper
{

  
    public class NotifyConsumer : BackgroundService
    {
        private readonly ILogger<NotifyConsumer> _logger;
        private readonly IRedisService _redisService;
        private static TelegramBotClient _telegramClient;
        private readonly List<NameValueRedisModel> _merchantChatId = new List<NameValueRedisModel>();
        private readonly CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking("vi-VN");
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;

        public NotifyConsumer(ILogger<NotifyConsumer> logger, IRedisService redisService, IWithdrawalOrdersMongoService withdrawalOrdersMongoService)
        {
            _logger = logger;
            _redisService = redisService;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;

        }

        private async Task SendNotify (string merchantCode , TelegramNotifyModel notifyModel)
        {
            if(_telegramClient!=null && _merchantChatId.Any(x=>x.Name == merchantCode))
            {
                foreach (var chatId in _merchantChatId.Where(x => x.Name == merchantCode))
                {
                    var reasonMessage = string.Empty;
                    try
                    {
                        string notifyMessage = string.Empty;
                        var failedReasonStr = string.Empty;
                        switch (notifyModel.Type)
                        {

                            case NotifyTypeEnum.BankMaintenance:
                                {
                                    reasonMessage = "银行维护中 / Ngân hàng đang bảo trì";
                                    break;
                                }
                            case NotifyTypeEnum.ErrorBankcard:
                                {
                                    reasonMessage = "银行卡号错误";
                                    break;
                                }
                            case NotifyTypeEnum.BankNotSupported:
                                {
                                    reasonMessage = "银行名称错误 / Tên ngân hàng không đúng";
                                    break;
                                }
                            case NotifyTypeEnum.ErrorHolderName:
                                {
                                    reasonMessage = "收款人姓名错误 / Tên người nhận không đúng";
                                    break;
                                }
                            case NotifyTypeEnum.HolderInfoInvalid:
                                {
                                    reasonMessage = "收款人信息错误 / Thông tin người nhận không đúng";
                                    break;
                                }
                            case NotifyTypeEnum.TransactionError:
                                {
                                    reasonMessage = "转账异常 / Giao dịch chuyển tiền bất thường";
                                    break;
                                }
                        }

                        if (!reasonMessage.IsNullOrEmpty())
                        {
                            var merchantName = _redisService.GetMerchantKeyValue(notifyModel.MerchantCode)?.Name ?? string.Empty;
                            DateTimeOffset dateTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(notifyModel.OrderTimeUnix);
                            var orderTime = dateTimeUtc.UtcDateTime.AddHours(7);
                            var message = $"🔹 平台单号 / Mã đơn nền tảng : {notifyModel.OrderNumber} \n🏷 商户名称 / Tên thương nhân : {merchantName} \n 🆔 商户编码 / Mã thương nhân : {notifyModel.MerchantCode} \n💰 金额 / Số tiền : {notifyModel.OrderAmount.ToString("C0", culInfo)} \n 🕒 创建时间 / Thời gian tạo : {orderTime}\n ❌ 状态 / Trạng thái : Fail \n ⚠️ 原因 / Lý do : {reasonMessage}";
                            message += $"\n \n 系统已自动退回此订单，烦请贵商户自行处理 / \n Hệ thống tự động trả lại đơn hàng này, mong quý đối tác vui lòng tự xử lý giúp cảm ơn!!";
                            await _telegramClient.SendMessageAsync(chatId.Value, message );
                        }

                    }
                    catch (Exception ex)
                    {
                        NlogLogger.Error("发送消息异常");
                    }
                }
            }
        }

        private static T ParseStreamBody<T>(string[] body)
        {
            try
            {
                var dict = new Dictionary<string, string>();

                for (int i = 0; i < body.Length - 1; i += 2)
                {
                    var key = body[i];
                    var value = body[i + 1];
                    if (!string.IsNullOrEmpty(key))
                        dict[key] = value;
                }

                var json = JsonConvert.SerializeObject(dict);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
            }

            return default(T);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var stream = _redisService.GetFullRedis().GetStream<TelegramNotifyModel>(NsPayRedisKeyConst.TelegramNotify);
            try
            {
                stream.GroupCreate(NsPayRedisKeyConst.CommonConsumerGroup);
            }
            catch (Exception ex)
            {
            }

            var botId = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.TelegramPayHelperBotId);

            if (!botId.IsNullOrEmpty())
            {
                _telegramClient = new TelegramBotClient(botId);
                var merchantChatIds = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.TelegramPayHelperChatId);

                if (merchantChatIds != null)
                {
                    var chatIds = JsonConvert.DeserializeObject<List<NameValueRedisModel>>(merchantChatIds);
                    _merchantChatId.AddRange(chatIds);
                }

            }


             int counter = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                var queue = await stream.ReadGroupAsync(NsPayRedisKeyConst.CommonConsumerGroup, "TelegramBot", 10, 5000);

                foreach (var item in queue)
                {
                    var obj = ParseStreamBody<TelegramNotifyModel>(item.Body);
                    Console.WriteLine( JsonConvert.SerializeObject( item) +" Delay : "+ (DateTime.UtcNow - obj.TriggerDate).TotalMilliseconds +" ms");
                    await SendNotify(obj.MerchantCode, obj);
                    stream.Acknowledge(item.Id);
                }

                if(counter >100)
                {
                    _redisService.TrimRedisStreamIfNeeded(NsPayRedisKeyConst.TelegramNotify);
                    counter = 0;
                }

                if(counter%5 == 0)
                {
                    _merchantChatId.Clear();
                    var merchantChatIds = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.TelegramPayHelperChatId);

                    if (merchantChatIds != null)
                    {
                        var chatIds = JsonConvert.DeserializeObject<List<NameValueRedisModel>>(merchantChatIds);
                        _merchantChatId.AddRange(chatIds);
                    }
                }

                counter++;
            }
        }

    }
}
