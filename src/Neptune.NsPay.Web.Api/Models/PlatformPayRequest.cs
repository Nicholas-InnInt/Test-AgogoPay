namespace Neptune.NsPay.Web.Api.Models
{
    public class PlatformPayRequest
    {
        public string MerchNo { get; set; }

        /// <summary>
        /// 会员账户
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 会员手机号
        /// </summary>

        public string UserNo { get; set; }

        public string OrderNo { get; set; }

        /// <summary>
        /// 回调地址
        /// </summary>
        public string NotifyUrl { get; set; }

        public decimal Money { get; set; }

        /// <summary>
        /// 支付类型，扫码，转卡，电子钱包
        /// sc:刮刮卡
        /// momopay:MoMo支付
        /// scanbank:扫码银联支付
        /// onlinebank:在线网银支付
        /// directbank:银行直连
        /// crypto:加密货币支付
        /// </summary>
        public string PayType { get; set; }

        public string Sign { get; set; }
    }

    public class PlatformSCPayRequest : PlatformPayRequest
    {
        public string TelcoName { get; set; }
        public string Code { get; set; }
        public string Seri { get; set; }
    }
}