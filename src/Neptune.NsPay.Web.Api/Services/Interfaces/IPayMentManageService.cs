using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions.Models;

namespace Neptune.NsPay.Web.Api.Services.Interfaces
{
    public interface IPayMentManageService
    {
        Task<List<PayMentRedisModel>> CheckBankPayMents(List<PayMentRedisModel> payMents, decimal orderMoney);
        string GetColorByPayType(PayMentTypeEnum paytype);
    }
}
