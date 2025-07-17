using Neptune.NsPay.MqPublish;
using Neptune.NsPay.RabbitMqExtensions;
using Neptune.NsPay.RedisExtensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddCAPMQSetup();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
