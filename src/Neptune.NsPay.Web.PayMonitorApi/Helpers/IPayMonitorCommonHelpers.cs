using AutoMapper;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.SignalRClient;

namespace Neptune.NsPay.Web.PayMonitorApi.Helpers
{
    public interface IPayMonitorCommonHelpers
    {

        int GetLimitStatus(decimal balanceLimitMoney, decimal balance);
        DateTime? ConvertToStandardTime(string dateStr , string? dateTimeZone=null);
        Task<bool> NotifyMerchantPaymentChanged(int PaymentId );
        Task<bool> UpdatePaymentUseState(int PaymentId, decimal NewBalance);
        Task<bool> DetermineChangesAndNotify(MerchantPaymentChangedDto changes);
        Task<bool> InitialDataMerchant(string MerchantCode);
        Task<bool> NotifyAllMerchant();
        IMapper Mapper();
    }
}
