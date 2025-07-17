namespace Neptune.NsPay.Commons
{
    public class RabbitMqTopics
    {
        public static readonly string TransferOrderCallBackTopic = "transferorderscallback-topic";
        public static readonly string TransferOrderCallBackSub = "transferorderscallback-subscriber";

        public static readonly string PayOrderCallBackTopic = "payorderscallback-topic";
        public static readonly string PayOrderCallBackSub = "payorderscallback-subscriber";

        public static readonly string PayOrderCryptoQueue = "payordercrypto-queue";

        #region Obsolete

        public static readonly string PayOrderTopic = "payorders-topic";
        public static readonly string PayOrderSub = "payorders-subscriber";

        public static readonly string TransferOrderTopic = "transferorders-topic";
        public static readonly string TransferOrderSub = "transferorders-subscriber";

        public static readonly string MerchantWithdrawalTopic = "merchantwithdrawal-topic";
        public static readonly string MerchantWithdrawalSub = "merchantwithdrawal-subscriber";

        #endregion Obsolete

        public static readonly string MerchantOrderTopic = "merchantorders-topic";
        public static readonly string MerchantOrderSub = "merchantorders-subscriber";
    }
}