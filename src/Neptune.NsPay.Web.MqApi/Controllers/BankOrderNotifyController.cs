using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.RabbitMqExtensions.Models;
using Neptune.NsPay.RedisExtensions;
using Telegram.BotAPI;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Localization;
using Telegram.BotAPI.AvailableMethods;

namespace Neptune.NsPay.Web.MqApi.Controllers
{
    public class BankOrderNotifyController : Controller
    {
        private readonly IRedisService _redisService;
        public BankOrderNotifyController(
            IRedisService redisService
            )
        {
            _redisService = redisService;
        }

        [CapSubscribe(MQSubscribeStaticConsts.BankOrderNotify)]
        public async Task BankOrderNotify(BankOrderNotifyDto mqDto)
        {
            try 
            {
                var bot = new BotClient(mqDto?.TelegramNotifyBotId);
                var chats = mqDto.TelegramNotifyChatId?.Split(';');
                var culInfo = CultureHelper.GetCultureInfoByChecking("vi-VN");

                foreach (var chatId in chats)
                {
                    List<string> messageList = new List<string>();
                    var cache = _redisService.GetTelegramMessage(mqDto.MerchantCode);
                    if (cache == null)
                    {
                        try
                        {
                            foreach (var info in mqDto.NotifyDetail)
                            {
                                var message = mqDto.BankNotifyText
                                                //.Replace("#cardno", info.CardNo)
                                                //.Replace("#phone", info.Phone)
                                                .Replace("#money", info.Money.ToString("C0", culInfo))
                                                .Replace("#remark", info.Remark)
                                                .Replace("#time", info.TransferTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                messageList.Add(message);
                                _redisService.SetTelegramMessage(mqDto.MerchantCode, info);
                            }

                            if (messageList.Any())
                            {
                                var bankStr = string.Join("\r\n ", messageList);
                                var tempStr = mqDto.Title + "\r\n" + bankStr;
                                await bot.SendMessageAsync(chatId, tempStr);
                            }
                            NlogLogger.Info("商户：{merchatcode},群组：{chatid},发送完成", mqDto.MerchantCode, chatId);
                        }
                        catch (Exception ex)
                        {
                            NlogLogger.Info("商户：{merchatcode},群组：{chatid},发送失败", mqDto.MerchantCode, chatId);
                        }

                    }
                    else 
                    {
                        try
                        {
                            foreach (var info in mqDto.NotifyDetail)
                            {
                                //var check = cache.FirstOrDefault(r => r.Type == info.Type && r.RefNo == info.RefNo && r.Phone == info.Phone && r.CardNo == info.CardNo && r.Remark == info.Remark && r.Money == r.Money);
                                //if (check == null)
                                //{
                                //    var message = mqDto.BankNotifyText
                                //                   .Replace("#cardno", info.CardNo)
                                //                   .Replace("#phone", info.Phone)
                                //                   .Replace("#money", info.Money.ToString("C0", culInfo))
                                //                   .Replace("#remark", info.Remark)
                                //                   .Replace("#time", info.TransferTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                //    messageList.Add(message);
                                //    _redisService.SetTelegramMessage(mqDto.MerchantCode, info);
                                //}
                            }
                            if (messageList.Any())
                            {
                                var bankStr = string.Join("\r\n ", messageList);
                                var tempStr = mqDto.Title + "\r\n" + bankStr;
                                await bot.SendMessageAsync(chatId, tempStr);
                            }
                            NlogLogger.Info("商户：{merchatcode},群组：{chatid},发送完成", mqDto.MerchantCode, chatId);
                        }
                        catch (Exception ex)
                        {
                            NlogLogger.Info("商户：{merchatcode},群组：{chatid},发送失败", mqDto.MerchantCode, chatId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("BankOrderNotify mq异常:" + ex);
            }
        }
    }
}
