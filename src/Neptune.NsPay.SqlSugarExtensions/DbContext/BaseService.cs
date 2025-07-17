using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using SqlSugar;
using System.Linq.Expressions;


namespace Neptune.NsPay.SqlSugarExtensions.DbContext
{
    public class BaseService<T> : IBaseService<T> where T : class, new()
    {

        private ISqlSugarClient _currentDb;

        public ISqlSugarClient Db
        {
            get { return _currentDb; }
        }

        public BaseService(IUnitOfWork unitOfWork)
        {
            //_unitOfWork = unitOfWork;
            _currentDb = unitOfWork.CurrentDb();
        }

        #region 添加操作
        /// <summary>
        /// 添加一条数据
        /// </summary>
        /// <param name="parm">T</param>
        /// <returns></returns>
        public int Add(T parm)
        {
            return Db.Insertable(parm).RemoveDataCache().ExecuteCommand();
        }
        public async Task<int> AddAsync(T parm)
        {
            return await Db.CopyNew().Insertable(parm).RemoveDataCache().ExecuteCommandAsync();
        }

        public object AddGetId(T parm)
        {
            return Db.Insertable(parm).ExecuteReturnIdentity();
        }

        public async Task<object> AddGetIdAsync(T parm)
        {
            return await Db.CopyNew().Insertable(parm).ExecuteReturnBigIdentityAsync();
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="parm">List<T></param>
        /// <returns></returns>
        public int Add(List<T> parm)
        {
            return Db.Insertable(parm).RemoveDataCache().ExecuteCommand();
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="parm">List<T></param>
        /// <returns></returns>
        public async Task<int> AddAsync(List<T> parm)
        {
            return await Db.CopyNew().Insertable(parm).RemoveDataCache().ExecuteCommandAsync();
        }
        #endregion

        #region 查询操作

        /// <summary>
        /// 根据条件查询数据是否存在
        /// </summary>
        /// <param name="where">条件表达式树</param>
        /// <returns></returns>
        public bool Any(Expression<Func<T, bool>> where)
        {
            return Db.Queryable<T>().Any(where);
        }

        /// <summary>
        /// 根据条件合计字段
        /// </summary>
        /// <param name="where">条件表达式树</param>
        /// <returns></returns>
        public TResult Sum<TResult>(Expression<Func<T, bool>> where, Expression<Func<T, TResult>> field)
        {
            return Db.Queryable<T>().Where(where).Sum(field);
        }

        /// <summary>
        /// 根据主值查询单条数据
        /// </summary>
        /// <param name="pkValue">主键值</param>
        /// <returns>泛型实体</returns>
        public T GetId(object pkValue)
        {
            return Db.Queryable<T>().InSingle(pkValue);
        }

        /// <summary>
        /// 根据主键查询多条数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<T> GetIn(object[] ids)
        {
            return Db.Queryable<T>().In(ids).ToList();
        }

        /// <summary>
        /// 根据条件取条数
        /// </summary>
        /// <param name="where">条件表达式树</param>
        /// <returns></returns>
        public int GetCount(Expression<Func<T, bool>> where)
        {
            return Db.Queryable<T>().Count(where);

        }

        /// <summary>
        /// 查询所有数据(无分页,请慎用)
        /// </summary>
        /// <returns></returns>
        public List<T> GetAll(bool useCache = false, int cacheSecond = 3600)
        {
            return Db.Queryable<T>().WithCacheIF(useCache, cacheSecond).ToList();
        }

        /// <summary>
        /// 获得一条数据
        /// </summary>
        /// <param name="where">Expression<Func<T, bool>></param>
        /// <returns></returns>
        public T GetFirst(Expression<Func<T, bool>> where)
        {
            return Db.Queryable<T>().First(where);
            //return Db.Queryable<T>().Where(where).First();
        }

        public async Task<T> GetFirstAsync(Expression<Func<T, bool>> where)
        {
            try
            {
                return await Db.CopyNew().Queryable<T>().FirstAsync(where);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 获得一条数据
        /// </summary>
        /// <param name="parm">string</param>
        /// <returns></returns>
        //public T GetFirst(string parm)
        //{
        //    return Db.Queryable<T>().Where(parm).First();
        //}

        /// <summary>
        /// 根据条件查询数据
        /// </summary>
        /// <param name="where">条件表达式树</param>
        /// <returns></returns>
        public List<T> GetWhere(Expression<Func<T, bool>> where, bool useCache = false, int cacheSecond = 3600)
        {
            var query = Db.Queryable<T>().Where(where).WithCacheIF(useCache, cacheSecond);
            return query.ToList();
        }


        /// <summary>
		/// 根据条件查询数据
		/// </summary>
		/// <param name="where">条件表达式树</param>
		/// <returns></returns>
        public List<T> GetWhere(Expression<Func<T, bool>> where, Expression<Func<T, object>> order, string orderEnum = "Asc", bool useCache = false, int cacheSecond = 3600)
        {
            var query = Db.Queryable<T>().Where(where).OrderByIF(orderEnum == "Asc", order, OrderByType.Asc).OrderByIF(orderEnum == "Desc", order, OrderByType.Desc).WithCacheIF(useCache, cacheSecond);
            return query.ToList();
        }

        #endregion

        #region 修改操作

        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <param name="parm">T</param>
        /// <returns></returns>
        public int Update(T parm)
        {
            return Db.Updateable(parm).RemoveDataCache().ExecuteCommand();
        }

        public async Task<int> UpdateAsync(T parm)
        {
            return await Db.CopyNew().Updateable(parm).RemoveDataCache().ExecuteCommandAsync();
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="parm">T</param>
        /// <returns></returns>
        public async Task<int> UpdateAsync(List<T> parm)
        {
            return await Db.CopyNew().Updateable(parm).RemoveDataCache().ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <param name="parm">T</param>
        /// <returns></returns>
        public int Update(T parm, Expression<Func<T, object>> columns)
        {
            return Db.Updateable(parm).WhereColumns(columns).RemoveDataCache().ExecuteCommand();
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="parm">T</param>
        /// <returns></returns>
        public int Update(List<T> parm)
        {
            return Db.Updateable(parm).RemoveDataCache().ExecuteCommand();
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="parm">T</param>
        /// <returns></returns>
        public int Update(List<T> parm, Expression<Func<T, object>> columns)
        {
            return Db.Updateable(parm).WhereColumns(columns).RemoveDataCache().ExecuteCommand();
        }

        /// <summary>
        /// 按查询条件更新
        /// </summary>
        /// <param name="where"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public int Update(Expression<Func<T, bool>> where, Expression<Func<T, T>> columns)
        {
            return Db.Updateable<T>().SetColumns(columns).Where(where).RemoveDataCache().ExecuteCommand();
        }

        #endregion

        #region 删除操作

        /// <summary>
        /// 删除一条或多条数据
        /// </summary>
        /// <param name="parm">string</param>
        /// <returns></returns>
        public int Delete(object id)
        {
            return Db.Deleteable<T>(id).RemoveDataCache().ExecuteCommand();
        }

        /// <summary>
        /// 删除一条或多条数据
        /// </summary>
        /// <param name="parm">string</param>
        /// <returns></returns>
        public int Delete(object[] ids)
        {
            return Db.Deleteable<T>().In(ids).RemoveDataCache().ExecuteCommand();
        }

        /// <summary>
        /// 根据条件删除一条或多条数据
        /// </summary>
        /// <param name="where">过滤条件</param>
        /// <returns></returns>
        public int Delete(Expression<Func<T, bool>> where)
        {
            return Db.Deleteable<T>().Where(where).RemoveDataCache().ExecuteCommand();
        }

        public StorageableResult<T> Saveable(List<T> parm, Expression<Func<T, object>> where)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 添加或更新

        /// <summary>
        /// 添加或更新数据
        /// </summary>
        /// <param name="parm">List<T></param>
        /// <returns></returns>
        public T Saveable(T parm, Expression<Func<T, object>> uClumns = null, Expression<Func<T, object>> iColumns = null)
        {
            var command = Db.Saveable(parm);

            if (uClumns != null)
            {
                command = command.UpdateIgnoreColumns(uClumns);
            }

            if (iColumns != null)
            {
                command = command.InsertIgnoreColumns(iColumns);
            }

            return command.RemoveDataCache().ExecuteReturnEntity();
        }

        /// <summary>
        /// 批量添加或更新数据
        /// </summary>
        /// <param name="parm">List<T></param>
        /// <returns></returns>
        public List<T> Saveable(List<T> parm, Expression<Func<T, object>> uClumns = null, Expression<Func<T, object>> iColumns = null)
        {
            var command = Db.Saveable(parm);

            if (uClumns != null)
            {
                command = command.UpdateIgnoreColumns(uClumns);
            }

            if (iColumns != null)
            {
                command = command.InsertIgnoreColumns(iColumns);
            }

            return command.RemoveDataCache().ExecuteReturnList();
        }

        /// <summary>
        /// 无主键添加或更新数据 (切记该表若有缓存，请执行 RemoveDataCache())
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public StorageableResult<T> Storageable(T parm, Expression<Func<T, object>> where)
        {
            var command = Db.Storageable(parm).WhereColumns(where).ToStorage();

            return command;
        }

        /// <summary>
        /// 无主键添加或更新数据 (切记该表若有缓存，请执行 RemoveDataCache())
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public StorageableResult<T> Storageable(List<T> parm, Expression<Func<T, object>> where)
        {
            var command = Db.Storageable(parm).WhereColumns(where).ToStorage();

            return command;
        }
        #endregion

    }
}
