using Neptune.NsPay.AutoDepositWorker;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.RedisExtensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddMongoSetup();
builder.Services.AddSingleton<IPayOrdersMongoService, PayOrdersMongoService>();
builder.Services.AddSingleton<ICallBackService, CallBackService>();

var host = builder.Build();
host.Run();
