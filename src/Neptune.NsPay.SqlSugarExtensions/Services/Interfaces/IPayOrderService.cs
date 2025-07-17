using Neptune.NsPay.PayOrders;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;

namespace Neptune.NsPay.SqlSugarExtensions.Services.Interfaces
{
    public interface IPayOrderService : IBaseService<PayOrder>
    {
        Task<bool> UpsertPayOrder(List<PayOrder> datalist);
    }
}
