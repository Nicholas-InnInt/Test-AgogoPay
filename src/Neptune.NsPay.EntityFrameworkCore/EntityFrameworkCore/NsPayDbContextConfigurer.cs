using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neptune.NsPay.DataEvent;
using Neptune.NsPay.Interceptor;

namespace Neptune.NsPay.EntityFrameworkCore
{
    public static class NsPayDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<NsPayDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<NsPayDbContext> builder, ServiceProvider serviceProvider, string connectionString)
        {
            builder.UseSqlServer(connectionString);
         
        }

        public static void Configure(DbContextOptionsBuilder<NsPayDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);

        }

        public static void Configure(DbContextOptionsBuilder<NsPayDbContext> builder, IDataChangedEvent dataChangedEvent, DbConnection connection)
        {
            builder.UseSqlServer(connection);

            if (dataChangedEvent != null)
            {
                builder.AddInterceptors(new DataChangedInterceptor(dataChangedEvent));
            }
        }

        public static void Configure(DbContextOptionsBuilder<NsPayDbContext> builder, IDataChangedEvent dataChangedEvent, string connectionString)
        {
            builder.UseSqlServer(connectionString);

            if (dataChangedEvent != null)
            {
                builder.AddInterceptors(new DataChangedInterceptor(dataChangedEvent));
            }
        }
    }
}