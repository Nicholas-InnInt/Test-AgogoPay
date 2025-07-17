namespace Neptune.NsPay.PayMents
{
    public enum PayMentMethodEnum
    {
        Manual = 0,

        /// <summary>
        /// 刮刮卡
        /// </summary>
        ScratchCards = 1,

        /// <summary>
        /// MoMo支付
        /// </summary>
        MoMoPay = 2,

        /// <summary>
        /// 扫码银联支付
        /// </summary>
        ScanBank = 3,

        /// <summary>
        /// 在线网银支付
        /// </summary>
        OnlineBank = 4,

        /// <summary>
        /// 银行直连
        /// </summary>
        DirectBank = 5,

        /// <summary>
        /// 加密货币支付
        /// </summary>
        USDTCrypto = 6,
    }
}