using Neptune.NsPay.CallBackSubRedis;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.PlatfromServices.AppServices;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.MongoDbExtensions;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddMongoSetup();
builder.Services.AddSingleton<IPayOrdersMongoService, PayOrdersMongoService>();

builder.Services.AddSingleton<ICallBackService, CallBackService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
