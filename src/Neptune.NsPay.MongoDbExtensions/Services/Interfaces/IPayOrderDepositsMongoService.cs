using Abp.Application.Services.Dto;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrderDeposits;
using Neptune.NsPay.PayOrderDeposits.Dtos;

namespace Neptune.NsPay.MongoDbExtensions.Services.Interfaces
{
    public interface IPayOrderDepositsMongoService : IMongoBaseService<PayOrderDepositsMongoEntity>
    {
        Task<PayOrderDepositsMongoEntity> GetPayOrderByBankNoTime(string refNo, int payMentId, string Description);

        Task<PayOrderDepositsMongoEntity> GetPayOrderByBankNoDesc(string refNo, int payMentId);

        Task<List<PayOrderDepositsMongoEntity>> GetPayOrdersByBankNosDesc(List<string> refNos, int payMentId);
        Task<List<PayOrderDepositsMongoEntity>> GetPayOrderByBankNosTime(List<string> refNos, int payMentId, List<string> Description);

        Task<PayOrderDepositsMongoEntity> GetPayOrderByBankInDesc(string description, int payMentId);
        Task<PayOrderDepositsMongoEntity> GetPayOrderFirstAsync(string refNo, string description, int paymemtId);
        Task<PayOrderDepositsMongoEntity> GetPayOrderByBank(DateTime dateNow, string bankId, int paymemtId);
        Task<PayOrderDepositsMongoEntity> GetPayOrderByBank(DateTime dateNow, string refNo, int paymemtId, string orderMark);
        Task<List<PayOrderDepositsMongoEntity>> GetPayOrderDepositByDateRange(DateTime startDate, DateTime endDate);
        Task<List<PayOrderDepositsMongoEntity>> GetPayOrderDepositByPaymentIdDateRange(DateTime startDate, DateTime endDate, List<int> payMentId);
        Task<PagedResultDto<PayOrderDepositsMongoEntity>> GetAllWithPagination(GetAllPayOrderDepositsInput input, List<int> payMentIds, List<string> orders, List<string> payOrderUserNos);
        Task<List<PayOrderDepositsMongoEntity>> GetAll(GetAllPayOrderDepositsForExcelInput input, List<int> payMentIds, List<string> orders, List<string> payOrderUserNos);
        Task<PayOrderDepositsMongoEntity?> GetPayOrderDepositsByBankOrderId(string orderId, PayMentTypeEnum payType);
        Task<PayOrderDepositsMongoEntity> GetPayOrderDepositsByBankOrderIdNoType(string orderId, PayMentTypeEnum payType);

        Task PayOrderDepositAssociated(string merchantCode, string payOrderId, decimal orderMoney, string transactionNo, int payMentId, PayMentTypeEnum payType, string remark, string bankId, int merchanrId, long userId);
        Task PayOrderDepositReject(string bankId, long userId, string rejectRemark);
        Task<bool> PayOrderDepositSubRedisUpdate(string orderId, string transactionNo, decimal orderMoney, string remark, string bankId, string merchantCode, int merchantId, int payMentId, PayMentTypeEnum payType);

        Task<int> GetTotalExcelRecordCount(GetAllPayOrderDepositsForExcelInput input, List<int> payMentIds, List<string> orders, List<string> payOrderUserNos);
    }
}
