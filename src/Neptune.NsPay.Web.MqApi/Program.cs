
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.RabbitMqExtensions;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.SqlSugarExtensions;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices;
using Neptune.NsPay.BillingExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddMongoSetup();
builder.Services.AddSingleton<IPayOrdersMongoService, PayOrdersMongoService>();
builder.Services.AddSingleton<IWithdrawalOrdersMongoService, WithdrawalOrdersMongoService>();
builder.Services.AddSingleton<IMerchantFundsMongoService, MerchantFundsMongoService>();
builder.Services.AddSingleton<IMerchantBillsMongoService, MerchantBillsMongoService>();

builder.Services.AddSqlsugarSetup();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMerchantWithdrawService, MerchantWithdrawService>();

builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();


builder.Services.AddSingleton<IMerchantBillsHelper, MerchantBillsHelper>();

builder.Services.AddCAPMQSetup();

builder.Services.AddCap(cap =>
{
    cap.UseDashboard();
    cap.UseConsulDiscovery(_ =>
    {
        _.DiscoveryServerHostName = "localhost";
        _.DiscoveryServerPort = 8500;
        _.CurrentNodeHostName = AppSettings.Configuration["Cap:CurrentNodeHostName"];
        _.CurrentNodePort = Convert.ToInt32(AppSettings.Configuration["Cap:CurrentNodePort"]);
        _.NodeId = AppSettings.Configuration["Cap:NodeId"];
        _.NodeName = AppSettings.Configuration["Cap:NodeName"];
    });
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
    pattern: "{controller}/{action}/{id?}");

app.Run();
