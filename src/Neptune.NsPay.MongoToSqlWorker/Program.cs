using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.MongoToSqlWorker.Jobs;
using Quartz.Impl;
using Quartz.Spi;
using Quartz;
using Neptune.NsPay.MongoToSqlWorker.QuartzFactory;
using Neptune.NsPay.MongoToSqlWorker.QuartzFactory.Model;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Models;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMongoSetup();
builder.Services.AddSqlsugarSetup();
builder.Configuration.AddJsonFile("settings.json", optional: true, reloadOnChange: true);

builder.Services.AddSingleton<IJobFactory, QuartzJobFactory>();
builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

#region register mongodb service
builder.Services.AddSingleton<IMerchantBillsMongoService, MerchantBillsMongoService>();
builder.Services.AddSingleton<IMerchantFundsMongoService, MerchantFundsMongoService>();
builder.Services.AddSingleton<IPayOrdersMongoService, PayOrdersMongoService>();
builder.Services.AddSingleton<IPayOrderDepositsMongoService, PayOrderDepositsMongoService>();
builder.Services.AddSingleton<IWithdrawalOrdersMongoService, WithdrawalOrdersMongoService>();
#endregion

builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
#region register sqlsugar service
builder.Services.AddSingleton<IMerchantBillService, MerchantBillService>();
builder.Services.AddSingleton<IPayOrderService, PayOrderService>();
builder.Services.AddSingleton<IPayOrderDepositService, PayOrderDepositService>();
builder.Services.AddSingleton<IWithdrawalOrdersService, WithdrawalOrdersService>();
#endregion

#region register jobs
builder.Services.AddSingleton<ProcessSyncMerchantBills>();
builder.Services.AddSingleton<ProcessSyncPayOrderDeposits>();
builder.Services.AddSingleton<ProcessSyncPayOrders>();
builder.Services.AddSingleton<ProcessSyncWithdrawalOrders>();
#endregion

var quartzJobs = builder.Configuration.GetSection("QuartzJobs").Get<QuartzJob[]>();

quartzJobs.ToList().ForEach(x =>
{
    Type type = Type.GetType(x.JobType);
    builder.Services.AddSingleton(new JobSchedule(type, x));
});

builder.Services.AddHostedService<QuartzHostedService>();
var host = builder.Build();
host.Run();
