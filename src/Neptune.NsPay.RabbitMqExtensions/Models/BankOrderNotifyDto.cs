using Neptune.NsPay.RedisExtensions.Models;

namespace Neptune.NsPay.RabbitMqExtensions.Models
{
    public class BankOrderNotifyDto
    {
        public string? Title { get; set; }
        public List<BankOrderNotifyModel>? NotifyDetail {  get; set; }
        public string? TelegramNotifyBotId { get; set; }
        public string? TelegramNotifyChatId { get; set; }
        public string? MerchantCode { get; set; }
        public string? BankNotifyText { get; set; }

    }
}
