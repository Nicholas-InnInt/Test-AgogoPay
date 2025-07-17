using Neptune.NsPay.Commons;
using Neptune.NsPay.Web.Api.Models;

namespace Neptune.NsPay.Web.Api.Helpers
{
    public class RequestSignHelper
    {
        public static bool CheckSign(PlatformPayRequest payRequest, string secret)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "MerchNo".ToLower(), payRequest.MerchNo.ToLower() },
                { "OrderNo".ToLower(), payRequest.OrderNo.ToLower() },
                { "Money".ToLower(), payRequest.Money.ToString().ToLower() },
                { "PayType".ToLower(), payRequest.PayType.ToLower() },
                { "NotifyUrl".ToLower(), payRequest.NotifyUrl.ToLower() }
            };
            var param = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
            string sign = SignHelper.GetSignContent(param) + "&secret=" + secret.ToLower();
            if (MD5Helper.MD5Encrypt32(sign).ToLower() == payRequest.Sign)
            {
                return true;
            }
            return false;
        }

        public static bool CheckScSign(PlatformSCPayRequest payRequest, string secret)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "MerchNo".ToLower(), payRequest.MerchNo.ToLower() },
                { "OrderNo".ToLower(), payRequest.OrderNo.ToLower() },
                { "Money".ToLower(), payRequest.Money.ToString().ToLower() },
                { "PayType".ToLower(), payRequest.PayType.ToLower() },
                { "NotifyUrl".ToLower(), payRequest.NotifyUrl.ToLower() },
                { "TelcoName".ToLower(), payRequest.TelcoName.ToLower() },
                { "Code".ToLower(), payRequest.Code.ToLower() },
                { "Seri".ToLower(), payRequest.Seri.ToLower() }
            };
            var param = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
            string sign = SignHelper.GetSignContent(param) + "&secret=" + secret.ToLower();
            if (MD5Helper.MD5Encrypt32(sign).ToLower() == payRequest.Sign)
            {
                return true;
            }
            return false;
        }

        public static bool CheckTransferSign(PlatformTransferRequest transferRequest, string secret)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "MerchNo".ToLower(), transferRequest.MerchNo.ToLower() },
                { "OrderNo".ToLower(), transferRequest.OrderNo.ToLower() },
                { "Money".ToLower(), transferRequest.Money.ToString().ToLower() },
                { "BankAccNo".ToLower(), transferRequest.BankAccNo.ToString().ToLower() },
                { "BankAccName".ToLower(), transferRequest.BankAccName.ToString().ToLower() },
                { "BankName".ToLower(), transferRequest.BankName.ToString().ToLower() },
                { "NotifyUrl".ToLower(), transferRequest.NotifyUrl.ToLower() }
            };
            var param = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
            string sign = SignHelper.GetSignContent(param) + "&secret=" + secret.ToLower();
            if (MD5Helper.MD5Encrypt32(sign).ToLower() == transferRequest.Sign)
            {
                return true;
            }
            return false;
        }
    }
}