namespace Neptune.NsPay.Commons
{
    public class NsPayRedisKeyConst
    {
        public readonly static string NsPay = "NsPay";

        #region Merchant
        public readonly static string NsPaySystemKey = "NsPaySystemSetting:";
        public readonly static string MerchantKey = "Merchants:";
        public readonly static string MerchantRateKey = "MerchantRates:";
        public readonly static string MerchantFundKey = "MerchantFunds:";
        public readonly static string MerchantSetting = "MerchantSettings:";
        #endregion

        #region PayGroup
        public readonly static string PayGroupMent = "PayGroupMents";
        public readonly static string PayMents = "PayMents";
        #endregion

        #region 银行 
        public readonly static string BankOrderMqPublish = "{BankOrderMqPublish}";

        public readonly static string BankOrderNotify = "{BankOrderNotify}";

        public readonly static string WithdrawalOrder = "{WithdrawalOrder}";

        public readonly static string TCBBankCode = "BankCodes:TCB";

        public readonly static string ACBBankOrderNotify = "BankOrderNotify:Acb";
        public readonly static string ACBBankOrder = "{BankAcbOrder}";
        public readonly static string BIDVBankOrderNotify = "BankOrderNotify:Bidv";
        public readonly static string BIDVBankOrder = "{BankBidvOrder}";
        public readonly static string TCBBankOrderNotify = "BankOrderNotify:Tcb";
        public readonly static string TCBBankOrder = "{BankTcbOrder}";
        public readonly static string MBBankOrderNotify = "BankOrderNotify:MB";
        public readonly static string MBBankOrder = "{BankMbOrder}";
        public readonly static string VCBBankOrderNotify = "BankOrderNotify:Vcb";
        public readonly static string VCBBankOrder = "{BankVcbOrder}";
        public readonly static string VTBBankOrderNotify = "BankOrderNotify:Vtb";
        public readonly static string VTBBankOrder = "{BankVtbOrder}";
        public readonly static string PVcomBankOrderNotify = "BankOrderNotify:PVcom";
        public readonly static string PVcomBankOrder = "{BankPVcomOrder}";
        public readonly static string BusinessMbBankOrder = "{BankBusinessMbOrder}";
        public readonly static string BusinessTcbBankOrder = "{BankBusinessTcbOrder}";
        public readonly static string BusinessVtbBankOrder = "{BankBusinessVtbOrder}";
        public readonly static string MsbBankOrderNotify = "BankOrderNotify:Msb";
        public readonly static string MsbBankOrder = "{BankMsbOrder}";
        public readonly static string SeaBankOrderNotify = "BankOrderNotify:Sea";
        public readonly static string SeaBankOrder = "{BankMsbOrder}";
        public readonly static string BvBankOrder = "{BankBvOrder}";
        public readonly static string NamaBankOrder = "{BankNamaOrder}";
        public readonly static string TPBankOrder = "{BankTPOrder}";
        public readonly static string VPBBankOrder = "{BankVPBOrder}";
        public readonly static string OCBBankOrder = "{BankOCBOrder}";
        public readonly static string EXIMBankOrder = "{BankEXIMOrder}";
        public readonly static string NCBBankOrder = "{BankNCBOrder}";
        public readonly static string HDBankOrder = "{BankHDOrder}";
        public readonly static string LPBankOrder = "{BankLPOrder}";
        public readonly static string PGBankOrder = "{BankPGOrder}";
        public readonly static string VietBankOrder = "{BankVietOrder}";
        public readonly static string BacaBankOrder = "{BankBacaOrder}";


        public readonly static string CallBackOrder = "{CallBackOrder}";

        public readonly static string SuccessOrder = "SuccessOrder:OrderId_";
        public readonly static string SuccessBankOrder = "SuccessBankOrder:BankOrderId_";
        public readonly static string WithdrawalSuccessOrder = "WithdrawalSuccessOrder:OrderId_";
        #endregion

        #region 出款

        public readonly static string WithdrawalDevices = "WithdrawalDevice:Devices";

        #endregion

        #region 添加缓存队列
        public readonly static string PayOrderDepositOrders = "PayOrderDeposit:Orders";
        #endregion

        #region 银行余额

        public readonly static string PayMentBalance = "BankBalance:Balance";

        #endregion

        #region 使用中

        public readonly static string PayUseMents = "PayUseMents:PayMent";

        #endregion

        #region Telegram
        public readonly static string TelegramMessage = "TelegramMessage:Message_";
        #endregion

        #region Excel
        public readonly static string PayOrderExcel = "PayOrderExcel:User_";
        public readonly static string PayDepositExcel = "PayDepositExcel:User_";
        public readonly static string MerchantBillsExcel = "MerchantBillsExcel:User_";
        public readonly static string PayWithdrawalOrderExcel = "WithdrawalOrderExcel:User_";
        #endregion

        #region 出款设备余额缓存

        public readonly static string WithdrawDeviceBalance = "WithdrawDeviceBalance:Balance_";
        public readonly static string TransferOrders = "TransferOtpOrders:Order_";
        public readonly static string MolibeSuccessTransferOrder = "MolibeSuccessTransferOrder:Order_";

        #endregion

        #region Redis 消费列队

        public readonly static string TelegramNotify = "TelegramNotify:Queue";

        public readonly static string CommonConsumerGroup = "NsPayConsumerGroup";

        #endregion
    }
}
