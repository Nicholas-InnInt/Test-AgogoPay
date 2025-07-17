using Microsoft.Extensions.DependencyInjection;
using Neptune.NsPay.Commons;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Neptune.NsPay.SqlSugarExtensions
{
    public static class SetupSqlSugar
    {
        public static void AddSqlsugarSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<ISqlSugarClient>(x =>
            {
                return new SqlSugarScope(new ConnectionConfig()
                {
                    ConnectionString = AppSettings.Configuration["ConnectionStrings:Default"],
                    DbType = DbType.SqlServer,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.SystemTable,
                    ConfigureExternalServices = new ConfigureExternalServices()
                    {
                        //DataInfoCacheService = new RedisCache()
                        EntityService = (property, column) =>
                        {
                            if (property.Name == "xxx")
                            {//根据列名    
                                column.IsIgnore = true;
                            }
                            var attributes = property.GetCustomAttributes(true);//get all attributes     
                            if (attributes.Any(it => it is KeyAttribute))//根据自定义属性    
                            {
                                column.IsPrimarykey = true;
                            }
                        },
                        EntityNameService = (type, entity) =>
                        {
                            var attributes = type.GetCustomAttributes(true);
                            if (attributes.Any(it => it is TableAttribute))
                            {
                                entity.DbTableName = (attributes.First(it => it is TableAttribute) as TableAttribute).Name;
                            }
                        }
                    },
                    MoreSettings = new ConnMoreSettings()
                    {
                        IsAutoRemoveDataCache = true
                    }
                });
            });
        }

    }
}
