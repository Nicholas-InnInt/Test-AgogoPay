using Abp.Application.Services.Dto;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MerchantBills.Dtos;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.PayMents;

namespace Neptune.NsPay.MongoDbExtensions.Services.Interfaces
{
    public interface IMerchantBillsMongoService : IMongoBaseService<MerchantBillsMongoEntity>
    {
        Task<MerchantBillsMongoEntity> GetMerchantBillByOrderNo(string merchantCode, string orderNumber, MerchantBillTypeEnum billType);

        Task<MerchantBillsMongoEntity> GetLastMerchantBillByMerchantCode(string merchantCode);

        Task<bool> UpdateWithRetryAddPayOrderBillAsync(string merchantCode, PayOrdersMongoEntity payOrder);

        Task<bool> UpdateWithRetryAddWithdrawalOrderBillAsync(string merchantCode, WithdrawalOrdersMongoEntity withdrawalOrder);

        Task<bool> UpdateWithRetryAddMerchantWithdrawBillBillAsync(string merchantCode, MerchantWithdrawMongoEntity merchantWithdraw);

        //Task<bool> AddPayOrderBill(string merchantCode, PayOrdersMongoEntity payOrder);
        //Task<bool> AddWithdrawalOrderBill(string merchantCode, WithdrawalOrdersMongoEntity withdrawalOrder);
        //Task<bool> AddMerchantWithdrawBill(string merchantCode, MerchantWithdrawMongoEntity merchantWithdraw);
        //Task<bool> ArtificialMerchantWithdrawBill(string merchantCode, MerchantBillsMongoEntity merchantBillsMongoEntity);

        Task<List<MerchantBillsMongoEntity>> GetMerchantBillByDateRange(DateTime startDate, DateTime endDate);

        Task<PagedResultDto<MerchantBillsMongoEntity>> GetAllWithPagination(GetAllMerchantBillsInput input, List<int> merchantIds, List<PayMentMethodEnum?> moneyTypes = null);

        Task<List<MerchantBillsMongoEntity>> GetAll(GetAllMerchantBillsForExcelInput input, List<int> merchantIds);

        Task<List<GetMerchatBillForDashboardDto>> GetMerchantBillSummaryByUserMerchantDateRange(List<int> merchantIds, DateTime startDate, DateTime endDate);

        Task<List<GetMerchatBillForDashboardSummaryDto>> GetMerchantBillByUserMerchantDateRange(List<int> merchantIds, DateTime startDate, DateTime endDate);

        Task<int> GetTotalExcelRecordCount(GetAllMerchantBillsForExcelInput input, List<int> merchantIds);

        Task<MerchantBillsMongoEntity> GetLastMerchantBillByMerchantId(int merchantId);
    }
}