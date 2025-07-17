using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.TelegramNotify;
using Neptune.NsPay.SqlSugarExtensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddSqlsugarSetup();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IMerchantService, MerchantService>();
builder.Services.AddSingleton<IMerchantSettingsService, MerchantSettingsService>();
builder.Services.AddSingleton<IPayGroupMentService, PayGroupMentService>();
builder.Services.AddSingleton<IPayMentService, PayMentService>();
builder.Services.AddSingleton<IMerchantWithdrawBankService, MerchantWithdrawBankService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
