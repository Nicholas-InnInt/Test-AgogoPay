using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neptune.NsPay.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DotNetCore.CAP.RabbitMQOptions;

namespace Neptune.NsPay.RabbitMqExtensions
{
    public static class SetupCAPMQ
    {
        public static void AddCAPMQSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddCap(x =>
            {
                //如果你使用的 EF 进行数据操作，你需要添加如下配置：
                //x.UseEntityFramework<AppDbContext>();  //可选项，你不需要再次配置 x.UseSqlServer 了

                //如果你使用的ADO.NET，根据数据库选择进行配置：
                x.UseSqlServer(AppSettings.Configuration["ConnectionStrings:Default"]);
                // x.UseMySql("数据库连接字符串");
                // x.UsePostgreSql("数据库连接字符串");

                //如果你使用的 MongoDB，你可以添加如下配置：
                // x.UseMongoDB("ConnectionStrings");  //注意，仅支持MongoDB 4.0+集群

                //CAP支持 RabbitMQ、Kafka、AzureServiceBus、AmazonSQS 等作为MQ，根据使用选择配置：
                //x.UseRabbitMQ("localhost:5672");

                x.UseStorageLock = true;
                x.FailedRetryCount = 2;
                x.FailedMessageExpiredAfter = 1 * 24 * 3600;
                x.SucceedMessageExpiredAfter = 1 * 1 * 300;

                x.UseRabbitMQ(rb =>
                {
                    rb.HostName = AppSettings.Configuration["RabbitMQ:HostName"];
                    rb.UserName = AppSettings.Configuration["RabbitMQ:UserName"];
                    rb.Password = AppSettings.Configuration["RabbitMQ:Password"];
                    rb.Port = 5672;
                    rb.BasicQosOptions = new BasicQos(1);
                });


                // x.UseKafka("ConnectionStrings");
                // x.UseAzureServiceBus("ConnectionStrings");
                // x.UseAmazonSQS();
                // x.UseDashboard();
            });
        }
    }
}
