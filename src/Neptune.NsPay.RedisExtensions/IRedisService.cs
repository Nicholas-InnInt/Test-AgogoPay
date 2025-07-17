using MongoDB.Bson;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.WithdrawalOrders;
using NewLife.Caching;
using NewLife.Caching.Queues;

namespace Neptune.NsPay.RedisExtensions
{
    public interface IRedisService
    {
        FullRedis GetFullRedis();
        List<MerchantRedisModel>? GetMerchantRedis();
		MerchantRedisModel? GetMerchantKeyValue(string merchantCode);
        IDisposable? GetMerchantBalanceLock(string MerchantCode);

        PayGroupMentRedisModel? GetPayGroupMentRedisValue(string cacheKey);
        PayGroupMentRedisModel? GetPayGroupMentByGroupName(string cacheKey);
        PayGroupMentRedisModel? GetPayGroupMentByPayMentId(int cacheKey);
        List<string> GetPayGroupMentIdByPayMentId(int paymentId);
        void AddPayGroupMentRedisValue(string cacheKey, PayGroupMentRedisModel payGroupMentRedisModel);
        void DeletePayGroupMentRedisValue(string cacheKey);
        string GetNsPaySystemSettingKeyValue(string cacheKey);
        List<MerchantBankJobApiModel>? GetMerchantTcbBankJobApi();

        #region 基础

        T GetRedisValue<T>(string cacheKey);

        void AddRedisValue<T>(string cacheKey, T value);

        void RemoveRedisValue(string cacheKey);

        void RemoveRedisValueByPattern(string cacheKey);
        #endregion

        #region 银行组
        List<NaPasBankModel> GetTcbBankCodeCaches();
        List<BankOrderNotifyModel> GetListBankOrderNotifyByAcb();
        void SetBankOrderNotifyByAcb(BankOrderNotifyModel bankOrderNotify);
        List<BankOrderNotifyModel> GetListBankOrderNotifyByBidv();
        void SetBankOrderNotifyByBidv(BankOrderNotifyModel bankOrderNotify);
        List<BankOrderNotifyModel> GetListBankOrderNotifyByMB();
        void SetBankOrderNotifyByMB(BankOrderNotifyModel bankOrderNotify);
        List<BankOrderNotifyModel> GetListBankOrderNotifyByPVcom();
        void SetBankOrderNotifyByPVcom(BankOrderNotifyModel bankOrderNotify);
        List<BankOrderNotifyModel> GetListBankOrderNotifyByVcb();
        void SetBankOrderNotifyByVcb(BankOrderNotifyModel bankOrderNotify);
        List<BankOrderNotifyModel> GetListBankOrderNotifyByTcb();
        void SetBankOrderNotifyByTcb(BankOrderNotifyModel bankOrderNotify);
        List<BankOrderNotifyModel> GetListBankOrderNotifyByVtb();
        void SetBankOrderNotifyByVtb(BankOrderNotifyModel bankOrderNotify);
        #endregion

        #region 缓存成功订单
        string GetSuccessOrder(string orderId);
        bool SetSuccessOrder(string orderId);
        string GetSuccessBankOrder(string orderId, string type);
        void SetSuccessBankOrder(string orderId, string type);

        string GetWithdrawalSuccessOrder(string orderId);
        void SetWithdrawalSuccessOrder(string orderId);
        #endregion

        #region 缓存支付方式
        List<PayMentRedisModel> GetPayMents();
        PayMentRedisModel? GetPayMentInfo(string phone,string cardNumber, PayMentTypeEnum typeEnum);
        PayMentRedisModel? GetPayMentInfoById(int id);
        PayMentRedisModel? GetPayMentTcbInfo(string phone, string cardNumber);
        void SetPayMentCaches(PayMentRedisModel payMentRedis);
        void DeletePayMentCaches(string cacheKey);
        #endregion

        #region 收款订单队列
        void AddOrderQueueList(string channel, BankOrderPubModel bankOrder);
        RedisReliableQueue<BankOrderPubModel> GetOrderQueue(string channel);
        #endregion

        #region 提现订单队列
        void AddWithdrawalOrderQueueList(string channel, WithdrawalOrderPubModel bankOrder);
        RedisReliableQueue<WithdrawalOrderPubModel> GetWithdrawalOrderQueue(string channel);
        #endregion

        #region 下发订单队列
        void AddTransferOrderQueueList(BankOrderNotifyModel bankOrder);
        RedisReliableQueue<BankOrderNotifyModel> GetTransferOrderQueue();
        #endregion

        #region 交易流水
        string GetMerchantBillOrder(string merchantCode, string payorderId);
        void SetMerchantBillOrder(string merchantCode, string payorderId);

        string GetMerchantMqBillOrder(string merchantCode, string payorderId);
        void SetMerchantMqBillOrder(string merchantCode, string payorderId);
        #endregion

        #region 缓存回调订单
        void AddCallBackOrderQueueList(string channel, string orderId);
        RedisReliableQueue<string> GetCallBackOrderQueue(string channel);
        #endregion

        #region 添加缓存队列
        void SetPayOrderDepositSuccessCache(PayOrderDepositSuccessModel successModel);
        List<PayOrderDepositSuccessModel> GetPayOrderDepositSuccessCache();
        bool PayOrderDepositSuccessCheckExist(string orderId);
        #endregion

        #region 添加Mq缓存队列
        RedisReliableQueue<PayMerchantRedisMqDto> GetMerchantMqPublish();
        void SetMerchantMqPublish(PayMerchantRedisMqDto redisMqDto);
        BankOrderNotifyModel GetAcbBankOrderNotifyMqPublish();
        BankOrderNotifyModel GetBidvBankOrderNotifyMqPublish();
        BankOrderNotifyModel GetMBBankOrderNotifyMqPublish();
        BankOrderNotifyModel GetPVcomBankOrderNotifyMqPublish();
        BankOrderNotifyModel GetVcbBankOrderNotifyMqPublish();
        BankOrderNotifyModel GetTcbBankOrderNotifyMqPublish();
        BankOrderNotifyModel GetVtbBankOrderNotifyMqPublish();
        #endregion

        #region 缓存支付方式余额
        BankBalanceModel GetBalance(int paymentId, PayMentTypeEnum paytype);
        BankBalanceModel? GetBalanceByPaymentId(int paymentId);
        void SetBalance(int paymentId, BankBalanceModel bankBalanceModel);

        #endregion

        #region 出款设备

        List<WithdrawalDeviceRedisModel> GetListRangeDevice(string merchantCode);
        void SetRPushWithdrawDevice(string merchantCode,WithdrawalDeviceRedisModel withdrawalDevice);

        void UpdateWithdrawDevice(string merchantCode, WithdrawalDeviceRedisModel withdrawalDevice);

        void DeleteWithdrawDevice(string merchantCode, WithdrawalDeviceRedisModel withdrawalDevice);


        void EditProcessWithdrawDevice(string merchantCode, WithdrawalDeviceRedisModel withdrawalDevice);

        WithdrawalDeviceRedisModel GetLPushWithdrawDevice(string merchantCode);

        List<WithdrawalDeviceRedisModel> GethWithdrawDeviceAll();

        #endregion

        #region 出款订单

        void SetRPushTransferOrder(string bankType, string phone, WithdrawalOrderRedisModel withdrawalOrderId);
        bool RemoveTransferOrder(string bankType, string phone, WithdrawalOrderRedisModel withdrawalOrderId);
        void SetVcbTransferOrder(string devicePhone, VcbTransferOrder transferOrder);
        VcbTransferOrder GetVcbTransferOrder(string devicePhone);
        void RemoveVcbTransferOrder(string devicePhone);
        void SetTcbTransferOrder(string devicePhone, TcbTransferOrder transferOrder);
        TcbTransferOrder GeTcbTransferOrder(string devicePhone);
        void RemoveTcbTransferOrder(string devicePhone);
        WithdrawalOrderRedisModel GetLPushTransferOrder(string bankType, string phone);

        #endregion


        #region 出款账号登陆操作

        void SetAccountLoginAction(string bankCard, PayMentTypeEnum payType, string merchantCode, AccountLoginActionRedisModel model );

        void RemoveAccountLoginAction(string bankCard, PayMentTypeEnum payType, string merchantCode);

        AccountLoginActionRedisModel GetAccountLoginAction(string bankCard, PayMentTypeEnum payType, string merchantCode);

  
        #endregion

        #region 缓存正在使用支付方式

        int? GetPayUseMent(int paymentId);

        void SetPayUseMent(int paymentId);

        void RemovePayUseMent(int paymentId);

        #endregion

        #region 缓存出款流水编号

        string GetMerchantBillTrasferOrder(string merchantCode, string payorderId);

        void SetMerchantBillTrasferOrder(string merchantCode, string payorderId);
        #endregion

        #region Telegram
        void SetTelegramMessage(string merchantCode, BankOrderNotifyModel bankOrderNotify);
        List<BankOrderNotifyModel> GetTelegramMessage(string merchantCode);
        #endregion

        #region Excel
        void SetPayDepositExcel(string username, string input);
        string GetPayDepositExcel(string username);
        void SetPayOrderExcel(string username, string input);
        string GetPayOrderExcel(string username);
        void SetMerchantBillsExcel(string username, string input);
        string GetMerchantBillsExcel(string username);
        string GetWithdrawalOrderExcel(string username);
        void SetWithdrawalOrderExcel(string username, string input);
        #endregion

        WithdrawBalanceModel? GetWitdrawDeviceBalance(int deivceId);

        void SetWitdrawDeviceBalance(int deivceId, WithdrawBalanceModel bankBalanceModel);

        TransferOrderOtpModel? GetTransferOrder(string orderId);

        void SetGetTransferOrderOtp(string orderId, TransferOrderOtpModel transferOrder);

        void RemoveGetTransferOrderOtp(string orderId);

        void CheckAndDeleteTransferOrder(string bankType, string phone, string orderId);

        void SetMolibeSuccessTransferOrder(int deviceId, string orderId);

        string GetMolibeSuccessTransferOrder(string orderId);

        bool? AddWithdrawalOrderProcessLock(string orderId);

        bool IsWithdrawalOrderProcessing(string orderId);
        void TrimRedisStreamIfNeeded(string streamKey, int maxLen = 100);

        bool AddTelegramNotify(TelegramNotifyModel notifyModel);
    }
}
