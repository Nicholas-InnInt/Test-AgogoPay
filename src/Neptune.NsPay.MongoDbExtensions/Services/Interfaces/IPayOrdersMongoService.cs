using Neptune.NsPay.MerchantDashboard.Dtos;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PayOrders.Dtos;

namespace Neptune.NsPay.MongoDbExtensions.Services.Interfaces
{
    public interface IPayOrdersMongoService : IMongoBaseService<PayOrdersMongoEntity>
    {
        Task UpdatePayOrderMentByOrderId(string id, int payMenyId, PayMentTypeEnum payType);

        Task UpdateSuccesByOrderId(string id, decimal tradeMoney);

        Task UpdateScInfoByOrderId(string id, string scCode, string scSeri);

        Task UpdateOrderStatusByOrderId(string id, PayOrderOrderStatusEnum status, decimal tradeMoney, string errorMsg);

        Task UpdateScoreStatus(PayOrdersMongoEntity payOrdersEntity);

        Task<PayOrdersMongoEntity?> GetPayOrderByRemark(string merchantCode, PayMentTypeEnum paytype, string remark, decimal money);

        Task<List<PayOrdersMongoEntity>> GetPayOrderByPayMentId(int PayMentId, PayOrderOrderStatusEnum orderStatus, DateTime creationTimeFrom);

        Task<PayOrdersMongoEntity?> GetPayOrderByOrderId(string orderId);

        Task<List<PayOrdersMongoEntity>> GetPayOrderByOrderIdList(List<string> orderIdList);

        Task<PayOrdersMongoEntity?> GetPayOrderByOrderNumber(string merchantCode, string orderNumber);

        Task<PayOrdersMongoEntity?> GetPayOrderByOrderNumber(int merchantCodeId, string orderNumber);

        Task<List<PayOrdersMongoEntity>?> GetPayOrderByCompletedList(string merchantCode, DateTime dateTime, MerchantTypeEnum merchantTypeEnum);

        Task<List<PayOrdersMongoEntity>> GetPayOrderByCompletedList(string merchantCode, DateTime dateTime);

        Task<long> UpdatePayOrderByFailedList(string merchantCode, DateTime dateTime, MerchantTypeEnum merchantTypeEnum);

        Task<List<PayOrdersMongoEntity>> GetPayOrderByDateRange(DateTime startDate, DateTime endDate, string merchantCode = "");

        Task<List<PayOrdersMongoEntity>> GetPayOrderProjectionsByDateRange(DateTime startDate, DateTime endDate, string merchantCode = "");

        Task<List<PayOrdersMongoEntity>> GetPayOrderProjectionsByCardNumberDateRange(DateTime startDate, DateTime endDate, List<int> cardNumbers);

        Task<List<PayOrdersMongoEntity>> GetPayOrderByDateRangeAndStatus(DateTime startDate, DateTime endDate, PayOrderOrderStatusEnum Status);

        Task<PayOrderPageResultDto<PayOrdersMongoEntity>> GetAllWithPagination(GetAllPayOrdersInput input, List<int> merchantIds, List<int> paymentIds = null);

        Task<List<PayOrdersMongoEntity>> GetAll(GetAllPayOrdersForExcelInput input, List<int> merchantIds);

        Task<PayOrdersMongoEntity> GetPayOrderByOrderNumber(List<string> merchantCode, string orderNumber);

        Task<List<string>> GetPayOrdersByFilter(string order, string userName, List<string> merchants);

        Task<IEnumerable<PayOrdersMongoEntity>> GetPayOrderListForExcelAsync(List<string> merchants, GetAllPayOrderDepositsForExcelInput input);

        Task<PayOrdersMongoEntity> GetPayOrderForExcelAsync(List<string> merchants, GetAllPayOrderDepositsForExcelInput input, string orderId);

        Task<List<CurrentPayOrderCashInByType>> GetAllforMerchantDashboardSummary(GetAllMerchantDashboardInput input, List<int> userMerchantIdList);

        Task<PayOrdersMongoEntity?> GetPayOrderByOrderNumberAmt(decimal amount, string orderNumber);

        Task<int> GetTotalExcelRecordCount(GetAllPayOrdersForExcelInput input, List<int> merchantIds);

        Task<List<PayOrdersMongoEntity>> GetBatchNotifcationCheckBox(List<string> ListOfPayOrderID);
    }
}