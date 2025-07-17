using SqlSugar;

namespace Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces
{
    public interface IUnitOfWork
    {
        void BeginTran();
        void CommitTran();
        void RollbackTran();

        SqlSugarScope CurrentDb();
    }
}
