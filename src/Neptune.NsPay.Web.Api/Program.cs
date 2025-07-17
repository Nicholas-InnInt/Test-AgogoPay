using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.Web.Api.Services.Interfaces;
using Neptune.NsPay.Web.Api.Services;
using Neptune.NsPay.HttpExtensions.ScratchCard;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Helpers;
using Neptune.NsPay.HttpExtensions.PayOnline;
using Neptune.NsPay.Web.Api.Helpers;
using Neptune.NsPay.ELKLogExtension;
using Neptune.NsPay.BillingExtensions;
using Neptune.NsPay.AccountCheckerClientExtension;
using Neptune.NsPay.KafkaExtensions;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddMongoSetup();
builder.Services.AddSingleton<IPayOrdersMongoService, PayOrdersMongoService>();
builder.Services.AddSingleton<IWithdrawalOrdersMongoService, WithdrawalOrdersMongoService>();
builder.Services.AddSingleton<IMerchantFundsMongoService, MerchantFundsMongoService>();

builder.Services.AddSqlsugarSetup();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IPayGroupMentService, PayGroupMentService>();
builder.Services.AddSingleton<IWithdrawalDevicesService, WithdrawalDevicesService>();
builder.Services.AddSingleton<IBinaryObjectManagerService, BinaryObjectManager>();


builder.Services.AddSingleton<IPayMentManageService, PayMentManageService>();

builder.Services.AddSingleton<IScDoiTheCaoHelper, ScDoiTheCaoHelper>();
builder.Services.AddSingleton<IPayOnlineHelper, PayOnlineHelper>();

builder.Services.AddSingleton<ICallBackService, CallBackService>();

builder.Services.AddSingleton<IBankBalanceService, BankBalanceService>();
builder.Services.AddSingleton<IBankStateHelper, BankStateHelper>();

builder.Services.AddKafkaSetup();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

builder.Services.SetupClient(); // account checker service


builder.Services.AddSingleton<MerchantOrderCacheHelper>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.SetupELKLog();

var app = builder.Build();

app.UseCors("AllowAll");


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

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}");

app.Run();
