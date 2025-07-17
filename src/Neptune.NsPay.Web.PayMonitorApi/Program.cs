using Neptune.NsPay.HttpExtensions.Bank.Helpers;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Crypto.Helpers;
using Neptune.NsPay.HttpExtensions.Crypto.Helpers.Interfaces;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.SqlSugarExtensions;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Web.PayMonitorApi.Helpers;
using Neptune.NsPay.Web.PayMonitorApi.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddSqlsugarSetup();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPayGroupMentService, PayGroupMentService>();
builder.Services.AddScoped<IPayMentService, PayMentService>();
builder.Services.AddScoped<IAbpUserService, AbpUserService>();
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<IAbpUserMerchantService, AbpUserMerchantService>();
builder.Services.AddScoped<IPayMonitorCommonHelpers, PayMonitorCommonHelpers>();

builder.Services.AddScoped<IBankStateHelper, BankStateHelper>();
builder.Services.AddScoped<IMBBankHelper, MBBankHelper>();
builder.Services.AddScoped<IVietinBankHelper, VietinBankHelper>();
builder.Services.AddScoped<ITechcomBankHelper, TechcomBankHelper>();
builder.Services.AddScoped<IVietcomBankHelper, VietcomBankHelper>();
builder.Services.AddScoped<IBidvBankHelper, BidvBankHelper>();
builder.Services.AddScoped<IAcbBankHelper, AcbBankHelper>();
builder.Services.AddScoped<IPVcomBankHelper, PVcomBankHelper>();
builder.Services.AddScoped<IBusinessMBBankHelper, BusinessMBBankHelper>();
builder.Services.AddScoped<IBusinessVtbBankHelper, BusinessVtbBankHelper>();
builder.Services.AddScoped<IBankHelper, BankHelper>();
builder.Services.AddScoped<ITronCryptoHelper, TronCryptoHelper>();

builder.Services.AddMongoSetup();
builder.Services.AddScoped<IBankBalanceService, BankBalanceService>();
builder.Services.AddScoped<IPayOrdersMongoService, PayOrdersMongoService>();
builder.Services.AddScoped<IPayOrderDepositsMongoService, PayOrderDepositsMongoService>();
builder.Services.AddScoped<IPayOrderDepositsMongoService, PayOrderDepositsMongoService>();

builder.Services.AddSignalR();
builder.Services.AddScoped<IPushUpdateService, PushUpdateService>();
builder.Services.AddSingleton<SignalRPushService>();

builder.Services.AddKafkaSetup();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

builder.Services.AddHostedService(sp =>
{
    // Manually resolve and start the scoped background service
    var backgroundService = sp.GetRequiredService<SignalRPushService>();
    return backgroundService;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/notificationHub");

app.Run();