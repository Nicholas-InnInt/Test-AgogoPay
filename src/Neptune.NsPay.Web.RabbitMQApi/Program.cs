using MassTransit;
using Neptune.NsPay.BillingExtensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.ELKLogExtension;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.SqlSugarExtensions;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Web.RabbitMQApi.Services;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddMongoSetup();
builder.Services.AddScoped<IPayOrderDepositsMongoService, PayOrderDepositsMongoService>();
builder.Services.AddScoped<IPayOrdersMongoService, PayOrdersMongoService>();
builder.Services.AddScoped<IWithdrawalOrdersMongoService, WithdrawalOrdersMongoService>();
builder.Services.AddScoped<IMerchantFundsMongoService, MerchantFundsMongoService>();
builder.Services.AddScoped<IMerchantBillsMongoService, MerchantBillsMongoService>();

builder.Services.AddSqlsugarSetup();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMerchantWithdrawService, MerchantWithdrawService>();

builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddScoped<IMerchantBalanceHelper, MerchantBalanceHelper>();
builder.Services.AddScoped<IMerchantBillsHelper, MerchantBillsHelper>();
builder.Services.AddScoped<ITransferCallBackService, TransferCallBackService>();
builder.Services.AddScoped<ICallBackService, CallBackService>();

builder.Services.SetupELKLog();

builder.Services.AddMassTransit(x =>
{
    var part = AppSettings.Configuration["Part"];
    var queueName = $"{RabbitMqTopics.MerchantOrderSub}.p{part}";

    // 注册所有消费者
    x.AddConsumer<MerchantOrderConsumer>();
    x.AddConsumer<TransferOrderCallbackConsumer>();
    x.AddConsumer<PayOrderCallbackConsumer>();
    x.AddConsumer<PayOrderCryptoConsumer>();

    // RabbitMQ 配置
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(AppSettings.Configuration["RabbitMQ:HostName"], h =>
        {
            h.Username(AppSettings.Configuration["RabbitMQ:UserName"]);
            h.Password(AppSettings.Configuration["RabbitMQ:Password"]);
        });

        // ========== 1. MerchantOrder/PayOrder/Withdraw/Balance 共用队列 ==========
        cfg.ReceiveEndpoint(queueName, e =>
        {
            e.UseMessageRetry(r => r.None());
            e.SetQueueArgument("x-single-active-consumer", true);
            e.PrefetchCount = 1;
            e.UseConcurrencyLimit(1);
            e.DiscardFaultedMessages();
            e.ConfigureConsumeTopology = false;

            // === 加上这句，显式绑定队列到指定 routingKey（否则收不到消息）===
            e.Bind(RabbitMqTopics.MerchantOrderTopic, s =>
            {
                s.RoutingKey = queueName;
                s.ExchangeType = ExchangeType.Topic;
            });

            e.ConfigureConsumer<MerchantOrderConsumer>(context);
        });

        // ========== 2. CallbackTransferOrder 独立队列、并发消费 ==========
        cfg.ReceiveEndpoint(RabbitMqTopics.TransferOrderCallBackSub, e =>
        {
            e.PrefetchCount = 10; // 批量并发
            e.ConfigureConsumer<TransferOrderCallbackConsumer>(context);
            e.UseConcurrencyLimit(10);
        });

        // ========== 3. CallbackPayOrder 独立队列、并发消费 ==========
        cfg.ReceiveEndpoint(RabbitMqTopics.PayOrderCallBackSub, e =>
        {
            e.PrefetchCount = 10;
            e.ConfigureConsumer<PayOrderCallbackConsumer>(context);
            e.UseConcurrencyLimit(10);
        });

        cfg.ReceiveEndpoint(RabbitMqTopics.PayOrderCryptoQueue, e =>
        {
            e.ConfigureConsumer<PayOrderCryptoConsumer>(context);
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();