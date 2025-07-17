using MassTransit;
using Neptune.NsPay.Commons;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.Web.KafkaApi.Handler;
using Neptune.NsPay.Web.KafkaApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(20999); // ⬅️ Change the port here
});

// Add services to the container.
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddScoped<KafkaHandlerDispatcher>();
builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddScoped<IKafkaTopicHandler, TransferOrderEventHandler>();
builder.Services.AddScoped<IKafkaTopicHandler, TransferOrderCallBackEventHandler>();
builder.Services.AddScoped<IKafkaTopicHandler, PayOrderEventHandler>();
builder.Services.AddScoped<IKafkaTopicHandler, PayOrderCallBackEventHandler>();
builder.Services.AddScoped<IKafkaTopicHandler, PayOrderCryptoEventHandler>();
builder.Services.AddScoped<IKafkaTopicHandler, MerchantWithdrawEventHandler>();
builder.Services.AddScoped<IKafkaTopicHandler, MerchantBalanceEventHandler>();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(AppSettings.Configuration["RabbitMQ:HostName"], h =>
        {
            h.Username(AppSettings.Configuration["RabbitMQ:UserName"]);
            h.Password(AppSettings.Configuration["RabbitMQ:Password"]);
        });
    });
});

//builder.Services.AddSlimMessageBus((mbb) =>
//{
//    mbb
//        .Produce<TransferOrderPublishDto>(x => x.DefaultPath(RabbitMqTopics.TransferOrderTopic));
//    mbb.Produce<TransferOrderCallbackPublishDto>(x => x.DefaultPath(RabbitMqTopics.TransferOrderCallBackTopic));

//    mbb.Produce<PayOrderCallbackPublishDto>(x => x.DefaultPath(RabbitMqTopics.PayOrderCallBackTopic));

//    mbb.Produce<PayOrderPublishDto>(x => x.DefaultPath(RabbitMqTopics.PayOrderTopic));
//    mbb.Produce<MerchantWithdrawalPublishDto>(x => x.DefaultPath(RabbitMqTopics.MerchantWithdrawalTopic));

//    mbb
//        .WithProviderRabbitMQ(cfg =>
//        {
//            //cfg.Host = hostContext.Configuration["RabbitMQ:Host"];
//            //cfg.Username = hostContext.Configuration["RabbitMQ:Username"];
//            //cfg.Password = hostContext.Configuration["RabbitMQ:Password"];
//            //cfg.Exchange = "slim.exchange";
//            //cfg.PrefetchCount = 100;
//            // Alternatively, when not using AMQP URI:
//            cfg.ConnectionFactory.HostName = AppSettings.Configuration["RabbitMQ:HostName"];
//            // cfg.ConnectionFactory.VirtualHost = "..."
//            cfg.ConnectionFactory.UserName = AppSettings.Configuration["RabbitMQ:UserName"];
//            cfg.ConnectionFactory.Password = AppSettings.Configuration["RabbitMQ:Password"];
//            cfg.AcknowledgementMode(RabbitMqMessageAcknowledgementMode.ConfirmAfterMessageProcessingWhenNoManualConfirmMade);
//            // cfg.ConnectionFactory.Ssl.Enabled = true
//        });
//    mbb.AddJsonSerializer();
//});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();