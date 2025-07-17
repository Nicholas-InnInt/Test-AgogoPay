namespace Neptune.NsPay.Commons
{
    public class KafkaTopics
    {
        // 添加前错避免与其他项目同名
        public static readonly string PayOrder = "Nspay-PayOrder";
        public static readonly string PayOrderCallBack = "Nspay-PayOrderCallBack";

        public static readonly string PayOrderCrypto = "Nspay-PayOrderCrypto";
        public static readonly string PayOrderCryptoCallBack = "Nspay-PayOrderCryptoCallBack";

        public static readonly string TransferOrder = "Nspay-TransferOrder";
        public static readonly string TransferOrderCallBack = "Nspay-TransferOrderCallBack";

        public static readonly string MerchantWithdrawal = "Nspay-MerchantWithdrawal";

        public static readonly string MerchantBalance = "Nspay-MerchantBalance";
    }
}