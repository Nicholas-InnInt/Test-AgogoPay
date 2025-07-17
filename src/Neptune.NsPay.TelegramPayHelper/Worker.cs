using System.Globalization;
using Abp.Domain.Repositories;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Localization;
using Neptune.NsPay.MerchantWithdrawBanks;
using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Storage;
using Neptune.NsPay.WithdrawalOrders;
using Newtonsoft.Json;
using Stripe;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;


namespace Neptune.NsPay.TelegramPayHelper
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRedisService _redisService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;
        private readonly IMerchantWithdrawService _merchantWithdrawService;
        private readonly IBinaryObjectManagerService _binaryObjectManagerService;

        public Worker(ILogger<Worker> logger,
            IRedisService redisService,
            IPayOrdersMongoService payOrdersMongoService,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            IMerchantFundsMongoService merchantFundsMongoService,
             IMerchantWithdrawService merchantWithdrawService, IBinaryObjectManagerService binaryObjectManagerService)
        {
            _logger = logger;
            _redisService = redisService;
            _payOrdersMongoService = payOrdersMongoService;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _merchantFundsMongoService = merchantFundsMongoService;
            _merchantWithdrawService = merchantWithdrawService;
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
            var botId = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.TelegramPayHelperBotId); ;
            CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking("vi-VN");
            //var botId = "6506636745:AAHi0JIbFku0fNsXooIpUfPcE_XbMP8wxD8";
            if (string.IsNullOrEmpty(botId))
            {
                return;
            }
            var bot = new TelegramBotClient(botId);
            var updates = await bot.GetUpdatesAsync();
            while (true)
            {
                var chatStr = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.TelegramPayHelperChatId);
                if (string.IsNullOrEmpty(chatStr))
                {
                    continue;
                }
                var chatIds = JsonConvert.DeserializeObject<List<NameValueRedisModel>>(chatStr); 
                if (chatIds == null) 
                {
                    continue;
                }
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
                            var chatInfo = chatIds.FirstOrDefault(r => r.Value == update.Message.Chat.Id.ToString());
                            if (chatInfo == null)
                            {
                                continue;
                            }

                            var merchantCode = chatInfo.Name;
                            if (update.Message.Text.ToLower().StartsWith("/info"))
                            {
                                var message = "群组名称：" + update.Message.Chat.Title + "\r\n群组ID：" + update.Message.Chat.Id;
                                await bot.SendMessageAsync(update.Message.Chat.Id, message);
                            }
                            else if (update.Message.Text.ToLower().StartsWith("/searchpayorder") || (update.Message.Text.ToLower().StartsWith("/df")))
                            {
                                var orderNumber = string.Empty;

                                if (update.Message.Text.ToLower().StartsWith("/searchpayorder"))
                                {
                                    orderNumber = update.Message.Text.Replace("/searchpayorder", "").Replace(" ", "").Trim();
                                }
                                else if ((update.Message.Text.ToLower().StartsWith("/df")))
                                {
                                    orderNumber = update.Message.Text.Replace("/df", "").Replace(" ", "").Trim();
                                }


                                var orderInfo = await _payOrdersMongoService.GetPayOrderByOrderNumber(merchantCode, orderNumber);
                                if (orderInfo != null)
                                {
                                    var statusStr = "";
                                    var status = orderInfo.OrderStatus;
                                    if (status == PayOrderOrderStatusEnum.NotPaid)
                                    {
                                        statusStr = "等待支付【chờ thanh toán】";
                                    }
                                    if (status == PayOrderOrderStatusEnum.Paid)
                                    {
                                        statusStr = "支付中【thanh toán】";
                                    }
                                    if (status == PayOrderOrderStatusEnum.Failed)
                                    {
                                        statusStr = "支付失败【giao dịch không thành công】";
                                    }
                                    if (status == PayOrderOrderStatusEnum.TimeOut)
                                    {
                                        statusStr = "支付超时【Qúa hạn】";
                                    }
                                    if (status == PayOrderOrderStatusEnum.Completed)
                                    {
                                        statusStr = "支付完成【giao dịch thành công】";
                                    }
                                    var message = "订单号[mã đơn hàng]：" + orderNumber + "\r\n金额[Số tiền]:"+orderInfo.OrderMoney.ToString("C0", culInfo) + "\r\n订单状态[trạng thái đơn hàng]：" + statusStr;
                                    await bot.SendMessageAsync(update.Message.Chat.Id, message);
                                }
                                else
                                {
                                    var message = "查询不到该订单【Không thể tìm thấy đơn đặt hàng】";
                                    await bot.SendMessageAsync(update.Message.Chat.Id, message);
                                }
                            }
                            else if (update.Message.Text.ToLower().StartsWith("/searchwithdraworder")||(update.Message.Text.ToLower().StartsWith("/wf")))
                            {
                                var orderNumber = string.Empty;

                                if(update.Message.Text.ToLower().StartsWith("/searchwithdraworder"))
                                {
                                    orderNumber= update.Message.Text.Replace("/searchwithdraworder", "").Replace(" ", "").Trim();
                                }else if(update.Message.Text.ToLower().StartsWith("/wf"))
                                {
                                    orderNumber = update.Message.Text.Replace("/wf", "").Replace(" ", "").Trim();
                                }
                                    
                                var message = string.Empty;

                                var orderInfo = await _withdrawalOrdersMongoService.GetWithdrawOrderByOrderNumber(merchantCode, orderNumber);
                                if (orderInfo != null)
                                {
                                    var statusStr = "";
                                    var status = orderInfo.OrderStatus;
                                    if (status == WithdrawalOrderStatusEnum.Wait)
                                    {
                                        statusStr = "等待出款【CHỜ XUẤT KHOẢN】";
                                    }
                                    if (status == WithdrawalOrderStatusEnum.Pending || status == WithdrawalOrderStatusEnum.WaitPhone || status == WithdrawalOrderStatusEnum.PendingProcess)
                                    {
                                        statusStr = "转账中【ĐANG XUẤT KHOẢN】";
                                    }
                                    if (status == WithdrawalOrderStatusEnum.Success)
                                    {
                                        statusStr = "转账成功【XUẤT KHOẢN THÀNH CÔNG】";
                                    }
                                    if (status == WithdrawalOrderStatusEnum.Fail)
                                    {
                                        statusStr = "转账失败【XUẤT KHOẢN THẤT BẠI】";
                                    }
                                    if (status == WithdrawalOrderStatusEnum.ErrorCard)
                                    {
                                        statusStr = "卡错误【SAI SỐ THẺ】";
                                    }
                                    if (status == WithdrawalOrderStatusEnum.ErrorBank)
                                    {
                                        statusStr = "该银行无法转账【NGÂN HÀNG KHÔNG THỂ XUẤT KHOẢN】";
                                    }

                                    //var message = "";
                                    if (status != WithdrawalOrderStatusEnum.Success)
                                    {
                                         message = "出款单号[số lệnh xuất khoản]：" + orderNumber + "\r\n金额[Số tiền]:" + orderInfo.OrderMoney.ToString("C0", culInfo) + "\r\n订单状态[trạng thái xuất khoản]：" + statusStr;
                                    }
                                    else
                                    {
                                        var binaryInfo = await _binaryObjectManagerService.GetFirstAsync(x => x.Id == orderInfo.BinaryContentId.Value);

                                        if (binaryInfo != null)
                                        {
                                            // Step 2: Wrap image into InputFile using MemoryStream
                                            using (var stream = new MemoryStream(binaryInfo.Bytes))
                                            {
                                                stream.Position = 0;

                                                // Use file extension based on MIME type
                                                var extension = orderInfo.ContentMIMEType?.Contains("png") == true ? "png" : "jpg";
                                                var fileName = $"proof.{extension}";

                                                // Step 3: Create the InputFile using the MemoryStream
                                                var inputFile = new InputFile(stream, fileName);
                                                var caption = $"出款单号[số lệnh xuất khoản]: {orderNumber}\n金额[Số tiền]: {orderInfo.OrderMoney.ToString("C0", culInfo)}\n订单状态[trạng thái xuất khoản]: {statusStr}";

                                                var sendPhotoArgs = new SendPhotoArgs(update.Message.Chat.Id, inputFile);
                                                sendPhotoArgs.Caption = caption;

                                                // Step 5: Send the photo
                                                await bot.SendPhotoAsync(sendPhotoArgs);
                                            }
                                        }
                                        else
                                        {
                                            // Fallback to text if no binary data
                                             message = $"出款单号[số lệnh xuất khoản]: {orderNumber}\n金额[Số tiền]: {orderInfo.OrderMoney.ToString("C0", culInfo)}\n订单状态[trạng thái xuất khoản]: {statusStr}";
                                            //await bot.SendMessageAsync(update.Message.Chat.Id, message);
                                        }

                                    }
                                }
                                else
                                {
                                    message = "查询不到该订单【Không thể tìm thấy đơn đặt hàng】";
                                    //await bot.SendMessageAsync(update.Message.Chat.Id, message);
                                }

                                if(!string.IsNullOrEmpty(message))
                                {
                                    await bot.SendMessageAsync(update.Message.Chat.Id, message);
                                }
                            }
                            else if (update.Message.Text.ToLower().StartsWith("/bankbalance"))
                            {
                                var merchantInfo = await _merchantFundsMongoService.GetFundsByMerchantCode(merchantCode);
                                var hostiyLists =  _merchantWithdrawService.GetAll().
                                    Where(r => r.MerchantCode == merchantInfo.MerchantCode && r.Status == MerchantWithdrawStatusEnum.Pending).ToList();
                                var pendingWithdrwalMoney = Convert.ToDecimal(0);
                                if (hostiyLists.Count > 0)
                                {
                                    pendingWithdrwalMoney = hostiyLists.Sum(r => r.Money);
                                }
                                var funds = await _merchantFundsMongoService.GetFundsByMerchantCode(merchantInfo.MerchantCode);
                                var pendingWithdrawOrder = await _withdrawalOrdersMongoService.GetMerchantPendingOrder(merchantInfo.MerchantCode);

                                if (pendingWithdrawOrder.Count > 0)
                                {
                                    pendingWithdrwalMoney += pendingWithdrawOrder.Sum(x => x.OrderMoney);
                                }

                                if (merchantInfo != null)
                                {
                                    var message = "商户号[số tài khoản]：" + merchantCode + "\r\n商户余额[số dư tài khoản]：" + merchantInfo.Balance.ToString("C0", culInfo) +
                                        "\r\n锁定余额[Khóa số dư]：" + pendingWithdrwalMoney;
                                    await bot.SendMessageAsync(update.Message.Chat.Id, message);
                                }
                                else
                                {
                                    var message = "该群组暂不支持，请联系三方管理员";
                                    await bot.SendMessageAsync(update.Message.Chat.Id, message);
                                }
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
                    NlogLogger.Error("查单机器人异常：" + ex);
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
