using Neptune.NsPay.AutoTransferWorker;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices;
using Neptune.NsPay.RedisExtensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddMongoSetup();
builder.Services.AddSingleton<IWithdrawalOrdersMongoService, WithdrawalOrdersMongoService>();
builder.Services.AddSingleton<ITransferCallBackService, TransferCallBackService>();

var host = builder.Build();
host.Run();
