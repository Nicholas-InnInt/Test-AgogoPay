using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using SqlSugar;

namespace Neptune.NsPay.SqlSugarExtensions.DbContext
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ISqlSugarClient _sqlSugarClient;

        public UnitOfWork(ISqlSugarClient sqlSugarClient)
        {
            _sqlSugarClient = sqlSugarClient;
        }

        public void BeginTran()
        {
            CurrentDb().BeginTran();
        }

        public void CommitTran()
        {
            try
            {
                CurrentDb().CommitTran();
            }
            catch (Exception ex)
            {
                CurrentDb().RollbackTran();
                throw ex;
            }
        }


        public void RollbackTran()
        {
            CurrentDb().RollbackTran();
        }

        public SqlSugarScope CurrentDb()
        {
            return _sqlSugarClient as SqlSugarScope;
        }
    }

}
