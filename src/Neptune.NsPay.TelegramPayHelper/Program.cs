using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.TelegramPayHelper;
using Abp.Domain.Repositories;
using Neptune.NsPay.MerchantWithdrawBanks;
using Neptune.NsPay.SqlSugarExtensions;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.Storage;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<NotifyConsumer>();

builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();


builder.Services.AddMongoSetup();
builder.Services.AddSingleton<IPayOrdersMongoService, PayOrdersMongoService>();
builder.Services.AddSingleton<IWithdrawalOrdersMongoService, WithdrawalOrdersMongoService>();
builder.Services.AddSingleton<IMerchantFundsMongoService, MerchantFundsMongoService>();
builder.Services.AddSingleton<IWithdrawalOrdersMongoService, WithdrawalOrdersMongoService>();

builder.Services.AddSqlsugarSetup();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IMerchantWithdrawService, MerchantWithdrawService>();
builder.Services.AddSingleton<IBinaryObjectManagerService, BinaryObjectManager>();

var host = builder.Build();
host.Run();
