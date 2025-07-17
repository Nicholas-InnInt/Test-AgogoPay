using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Localization;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.RedisExtensions;
using System.Globalization;
using System.Text;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;

namespace Neptune.NsPay.TelegramMidStopHelper
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRedisService _redisService;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;

        public Worker(ILogger<Worker> logger,
            IRedisService redisService,
            IMerchantFundsMongoService merchantFundsMongoService
            )
        {
            _logger = logger;
            _redisService = redisService;
            _merchantFundsMongoService = merchantFundsMongoService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
          
            var botId = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.TelegramMidStopHelperBotId); ;
            CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking("vi-VN");
            if (string.IsNullOrEmpty(botId))
            {
                return;
            }
            var bot = new TelegramBotClient(botId);
            var updates = await bot.GetUpdatesAsync();
            while (true)
            {
         
                try
                {
                    if (updates.Count() > 0)
                    {
                        foreach (var update in updates)
                        {
                            if (update.Message == null)
                            { continue; }
                            if (string.IsNullOrEmpty(update.Message.Text))
                            {
                                continue;
                            }
                            var time = StringToDatetime(update.Message.Date);
                            var datetime = DateTime.Now;
                            var timespan = datetime - time;
                            if (timespan.TotalMinutes > 10)
                            {
                                continue;
                            }

                            NlogLogger.Info("[TELEGRAM CHAT ID] [" + update.Message.Chat.Id.ToString() + "]");

                            string replyContent = string.Empty;
                            if (update.Message.Text.ToLower().StartsWith("/getallmerchant"))
                            {
                                var allExtMerchant =  _redisService.GetMerchantRedis()?.Where(x=>x.MerchantType == Merchants.MerchantTypeEnum.External);

                                StringBuilder content = new StringBuilder();
                                decimal totalBalance = 0m;
                                int merchantCount = 0;
                                if (allExtMerchant != null && allExtMerchant.Any())
                                {
                                    foreach(var extMerchant in allExtMerchant)
                                    {
                                        var merchantBalance = await _merchantFundsMongoService.GetFundsByMerchantCode(extMerchant.MerchantCode);
                                        var merchantBalanceDec = merchantBalance?.Balance ?? 0;
                                        content.AppendLine($" {extMerchant.Name} | {extMerchant.MerchantCode} | {merchantBalanceDec.ToString("C0", culInfo)}");
                                        totalBalance += merchantBalanceDec;
                                        merchantCount++;
                                    }
                                    
                                }

                                content.AppendLine(string.Empty);
                                content.AppendLine($"总商户：{merchantCount}");
                                content.AppendLine($"总金额： {totalBalance.ToString("C0", culInfo)} ");

                                replyContent = content.ToString();
                            }

                            if(!string.IsNullOrEmpty(replyContent))
                            {
                                await bot.SendMessageAsync(update.Message.Chat.Id, replyContent);
                            }
                       
                        }
                        updates = await bot.GetUpdatesAsync(offset: updates.Max(u => u.UpdateId) + 1);
                    }
                    else
                    {
                        updates = await bot.GetUpdatesAsync();
                    }
                }
                catch (Exception ex)
                {
                    NlogLogger.Error("中转机器人异常：" + ex);
                }
            }
        }

        public static DateTime StringToDatetime(long unixTimeStamp)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            DateTime dt = startTime.AddSeconds(unixTimeStamp);
            return dt;
        }

    }
}
