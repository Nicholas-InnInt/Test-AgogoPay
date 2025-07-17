using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.PlatformWithdraw;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddMongoSetup();
builder.Services.AddSingleton<IWithdrawalOrdersMongoService, WithdrawalOrdersMongoService>();

builder.Services.AddSqlsugarSetup();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IMerchantService, MerchantService>();
builder.Services.AddSingleton<IWithdrawalDevicesService, WithdrawalDevicesService>();

var host = builder.Build();
host.Run();
