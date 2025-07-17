using Neptune.NsPay.BankSubRedis;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.RedisExtensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddMongoSetup();

#region register mongodb service
builder.Services.AddSingleton<IPayOrdersMongoService, PayOrdersMongoService>();
builder.Services.AddSingleton<IPayOrderDepositsMongoService, PayOrderDepositsMongoService>();
#endregion

builder.Services.AddKafkaSetup();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();


var host = builder.Build();
host.Run();
