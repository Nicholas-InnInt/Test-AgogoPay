using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.Web.TransferApi;
using Neptune.NsPay.Web.TransferApi.SignalR;
using Neptune.NsPay.Web.TransferApi.Models;
using Neptune.NsPay.AccountCheckerClientExtension;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices;
using Neptune.NsPay.ELKLogExtension;

var builder = WebApplication.CreateBuilder(args);
// Get the application root path
string appRootPath = builder.Environment.ContentRootPath;
// Add services to the container.
builder.Services.AddStackExchangeRedisExtensions();
builder.Services.SetupClient();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddMongoSetup();
builder.Services.AddSingleton<IWithdrawalOrdersMongoService, WithdrawalOrdersMongoService>();
builder.Services.AddSingleton<IMerchantFundsMongoService, MerchantFundsMongoService>();
builder.Services.AddSingleton<IMerchantBillsMongoService, MerchantBillsMongoService>();

builder.Services.AddSqlsugarSetup();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IMerchantService, MerchantService>();
builder.Services.AddSingleton<IWithdrawalDevicesService, WithdrawalDevicesService>();
builder.Services.AddScoped<IAbpUserService, AbpUserService>();
builder.Services.AddScoped<IAbpUserMerchantService, AbpUserMerchantService>();
builder.Services.AddScoped<IBinaryObjectManagerService, BinaryObjectManager>();
builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();
builder.Services.AddSingleton<DeviceProcessLock>();



builder.Services.AddMemoryCache();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 2 * 1024 * 1024; // Set the limit to 2 MB (2 * 1024 * 1024 bytes)
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    // You can also set the client timeout to be longer if needed
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);  // Increase if needed

    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddSingleton<FileWatcherService>(sp =>
    new FileWatcherService(appRootPath + "\\data\\"));

builder.Services.AddSingleton<PushOrderWorkerService>();
builder.Services.AddSingleton<PushOrderHelper>();
builder.Services.AddTransient<PushOrderService>();
builder.Services.AddSingleton<ITransferCallBackService, TransferCallBackService>();

builder.Services.AddKafkaSetup();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddSingleton<WithdrawalOrderHelper>();

builder.Services.SetupELKLog();

builder.Services.AddHostedService(sp =>
{
    // Manually resolve and start the scoped background service
    var backgroundService = sp.GetRequiredService<PushOrderWorkerService>();
    return backgroundService;

});


builder.Services.AddSingleton<SignalRPushService>();

builder.Services.AddHostedService(sp =>
{
    // Manually resolve and start the scoped background service
    var backgroundService = sp.GetRequiredService<SignalRPushService>();
    return backgroundService;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
var fileWatcherService = app.Services.GetRequiredService<FileWatcherService>();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

// Register the event handlers for application stop
lifetime.ApplicationStopping.Register(OnApplicationStopping);
lifetime.ApplicationStopped.Register(OnApplicationStopped);
lifetime.ApplicationStarted.Register(OnApplicationStarted);


// Event handler for when the application is stopping
void OnApplicationStarted()
{
    fileWatcherService.Start();
    // Logic to execute just before the application stops
    Console.WriteLine("Application is Started...");
}


// Event handler for when the application is stopping
void OnApplicationStopping()
{
    fileWatcherService.Stop();
    // Logic to execute just before the application stops
    Console.WriteLine("Application is stopping...");
}

// Event handler for when the application has stopped
void OnApplicationStopped()
{
    // Logic to execute after the application has stopped
    Console.WriteLine("Application has stopped.");
}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<OrderHub>("/orderHub");

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
