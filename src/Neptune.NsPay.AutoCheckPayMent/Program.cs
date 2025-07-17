using Neptune.NsPay.AutoCheckPayMent;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.SqlSugarExtensions;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Helpers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddMongoSetup();
builder.Services.AddSingleton<IPayOrdersMongoService, PayOrdersMongoService>();

builder.Services.AddSqlsugarSetup();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IPayGroupMentService, PayGroupMentService>();

builder.Services.AddSingleton<IBankBalanceService, BankBalanceService>();
builder.Services.AddSingleton<IBankStateHelper, BankStateHelper>();

var host = builder.Build();
host.Run();
