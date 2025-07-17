using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using PayPalCheckoutSdk.Orders;
using System.IO;
using System.Text.RegularExpressions;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Twilio.TwiML.Messaging;

namespace Neptune.NsPay.TelegramReceipt
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IBinaryObjectManagerService _binaryObjectManagerService;

        public Worker(ILogger<Worker> logger,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            IBinaryObjectManagerService binaryObjectManagerService)
        {
            _logger = logger;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _binaryObjectManagerService = binaryObjectManagerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    if (_logger.IsEnabled(LogLevel.Information))
            //    {
            //        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    }
            //    await Task.Delay(1000, stoppingToken);
            //}

            var botId = AppSettings.Configuration["BotId"];
            var chatId = AppSettings.Configuration["ChatId"];
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
                            var photos = update.Message.Photo;
                            if (photos == null)
                            { continue; }
                            if(update.Message.Chat.Id.ToString() != chatId)
                            {
                                continue;
                            }
                            var telegrammessages = "";
                            if (photos != null)
                            {
                                telegrammessages = update.Message.Caption;
                            }
                            else
                            {
                                telegrammessages = update.Message.Text;
                            }
                            if (!string.IsNullOrEmpty(telegrammessages) && Regex.IsMatch(telegrammessages, @"\d"))
                            {
                                var messages = telegrammessages.Split(" ");
                                if (messages.Count() >= 3 && photos.Count()>=3)
                                {
                                    //提取订单号
                                    var orderNumber = messages[0];
                                    if (!string.IsNullOrEmpty(orderNumber))
                                    {
                                        var orderInfo = await _withdrawalOrdersMongoService.GetWithdrawOrderByOrderNumber(orderNumber);
                                        if (orderInfo != null)
                                        {
                                            try
                                            {
                                                //更新收据状态
                                                var lastPhoto = update.Message.Photo.Last();
                                                var fileInfo = bot.GetFile(lastPhoto.FileId);
                                                var imageUrl = $"https://api.telegram.org/file/bot{botId}/{fileInfo.FilePath}";
                                                var imageBytes = await ConvertImageUrlToBase64(imageUrl);
                                                var storageInfo = new Storage.BinaryObject() { TenantId = 1, Bytes = imageBytes, Description = "Upload Receipt (" + orderInfo.ID + ")" };
                                                var haveInsert = await _binaryObjectManagerService.AddAsync(storageInfo);
                                                if (haveInsert > 0)
                                                {
                                                    orderInfo.BinaryContentId = storageInfo.Id;
                                                    orderInfo.ContentMIMEType = DetectMimeType(storageInfo.Bytes);
                                                    var result = await _withdrawalOrdersMongoService.UpdateReceipt(orderInfo);
                                                    if (result)
                                                    {
                                                        //发送确认消息
                                                        var replyParams = new ReplyParameters();
                                                        replyParams.MessageId = update.Message.MessageId;
                                                        var messageArgs = new SendMessageArgs(update.Message.Chat.Id, $"Mã đơn hàng: {orderInfo.OrderNumber}  Biên lai đã được tải lên thành công！");
                                                        messageArgs.ReplyParameters = replyParams;
                                                        await bot.SendMessageAsync(messageArgs);
                                                        NlogLogger.Info("Telegram Bot: Order {OrderNumber} receipt uploaded successfully.", orderNumber);
                                                    }
                                                    else
                                                    {
                                                        NlogLogger.Warn("Telegram Bot: Failed to update receipt for order {OrderNumber}.", orderNumber);
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                NlogLogger.Error("Telegram Bot: Error processing order {OrderNumber} receipt upload: {Message}", orderNumber, ex.Message);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        updates = await bot.GetUpdatesAsync(offset: updates.Max(u => u.UpdateId) + 1);
                    }
                    else
                    {
                        updates = await bot.GetUpdatesAsync();
                    }
                }catch(Exception ex)
                {
                    NlogLogger.Error("Telegram Bot Error: {Message}", ex);
                    updates = await bot.GetUpdatesAsync(offset: updates.Max(u => u.UpdateId) + 1);
                }
            }
        }

        async Task<byte[]> ConvertImageUrlToBase64(string imageUrl)
        {
            using (var httpClient = new HttpClient())
            {
                // 下载图片
                var response = await httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                // 获取图片的字节数据
                byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                return imageBytes;
            }
        }

        public static string DetectMimeType(byte[] data)
        {
            // Check for PNG (starts with 89 50 4E 47)
            if (data.Length >= 4 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            {
                return "image/png";
            }
            // Check for JPEG (starts with FF D8 FF)
            else if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            {
                return "image/jpeg";
            }
            // Check for GIF (starts with 47 49 46 38)
            else if (data.Length >= 4 && data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && (data[3] == 0x38 || data[3] == 0x39))
            {
                return "image/gif";
            }
            // Check for WebP (starts with 52 49 46 46)
            else if (data.Length >= 4 && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46)
            {
                return "image/webp";
            }
            // Default MIME type if not recognized
            else
            {
                return "application/octet-stream";
            }
        }
    }
}
