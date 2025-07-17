using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RabbitMqExtensions;
using Neptune.NsPay.MqPublishBankOrderNotify.Jobs;
using Neptune.NsPay.SqlSugarExtensions;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSqlsugarSetup();
builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddCAPMQSetup();

builder.Services.AddHostedService<PublishNotifyByAcb>();
builder.Services.AddHostedService<PublishNotifyByBidv>();
builder.Services.AddHostedService<PublishNotifyByMB>();
builder.Services.AddHostedService<PublishNotifyByPVcom>();
builder.Services.AddHostedService<PublishNotifyByTcb>();
builder.Services.AddHostedService<PublishNotifyByVcb>();
builder.Services.AddHostedService<PublishNotifyByVtb>();

builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IMerchantSettingService, MerchantSettingService>();
builder.Services.AddSingleton<IMerchantService, MerchantService>();
builder.Services.AddSingleton<IPayGroupMentService, PayGroupMentService>();

var host = builder.Build();
host.Run();