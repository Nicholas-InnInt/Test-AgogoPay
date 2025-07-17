using Neptune.NsPay.MerchantDashboard.Dtos;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.WithdrawalOrders;
using Neptune.NsPay.WithdrawalOrders.Dtos;

namespace Neptune.NsPay.MongoDbExtensions.Services.Interfaces
{
    public interface IWithdrawalOrdersMongoService: IMongoBaseService<WithdrawalOrdersMongoEntity>
    {
        Task<bool> UpdateNotifyStatus(string Id, int number);
        Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByOrderId(string merchantCode, string orderId);
        Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByDevice(string merchantCode, int deviceId, DateTime startTime);
        Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByDevice(int deviceId, DateTime startTime);

        Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderCountByDevice(List<int> deviceIds);
        Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByDevice(List<string> merchantCode, int deviceId, DateTime startTime);
        Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByOrderNumber(int merchantCodeId, string orderNumber);
        Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByOrderNumber(string orderNumber);
        Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByOrderNumber(string merchantCode, string orderNumber);
        Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderByFk(string merchantCode);
        Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderProcess(string merchantCode);
        Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderByWaitPhone(DateTime startTime);
        Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderCallBack(DateTime startTime);
        Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderByDateRange(DateTime startDate, DateTime endDate, string merchantCode = "");

        Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderByDateRange(DateTime startDate, DateTime endDate, WithdrawalOrderStatusEnum orderStatus);

        Task<WithdrawalOrderPageResultDto<WithdrawalOrdersMongoEntity>> GetAllWithPagination(GetAllWithdrawalOrdersInput input, List<int> merchantIds, List<int> deviceIds);
        Task<List<WithdrawalOrdersMongoEntity>> GetAll(GetAllWithdrawalOrdersForExcelInput input, List<int> merchantIds, List<int> deviceIds);
        Task<List<MerchantDashboardDto>> GetAllforMerchantDashboardSummary(GetAllMerchantDashboardInput input, List<int> merchantIds);
        Task<List<WithdrawalOrdersMongoEntity>> GetMerchantPendingOrder(string merchantCode = "");

        Task<int> GetTotalRecordExcelCount(GetAllWithdrawalOrdersForExcelInput input, List<int> merchantIds, List<int> deviceIds);
        Task<bool> UpdateReceipt(WithdrawalOrdersMongoEntity newData);
        Task<bool> UpdateOrderStatusAsync(WithdrawalOrdersMongoEntity order);
        Task<bool> UpdateNotifyStatus(string Id, int number, WithdrawalNotifyStatusEnum status);
        Task<bool> ReleaseWithdrawal(string withdrawalId , string user);

        Task<decimal> GetAllPendingReleaseOrder(int MerchantId);
    }
}
