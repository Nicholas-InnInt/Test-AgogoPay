namespace Neptune.NsPay.WithdrawalOrders
{
    public enum WithdrawalOrderStatusEnum
    {
        Wait = 0,
        Pending = 1,//转账中
        Success = 2,//转账成功
        Fail = 3,//转账失败
        ErrorCard = 4,//卡错误，无法转账
        ErrorBank = 5,//银行错误无法转账
        TimeOut = 6,//交易超时
        Cancel = 7,//取消订单
        WaitPhone = 8,//等待手机回调
        PendingProcess = 9,//待处理 ， 出款异常
    }
}
