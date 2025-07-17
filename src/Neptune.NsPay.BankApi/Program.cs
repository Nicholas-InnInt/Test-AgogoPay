using Neptune.NsPay.HttpExtensions.Bank.Helpers;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Crypto.Helpers;
using Neptune.NsPay.HttpExtensions.Crypto.Helpers.Interfaces;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.SqlSugarExtensions;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSqlsugarSetup();
builder.Services.AddMongoSetup();
builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();

#region register helper service
builder.Services.AddSingleton<IAcbBankHelper, AcbBankHelper>();
builder.Services.AddSingleton<IBidvBankHelper, BidvBankHelper>();
builder.Services.AddSingleton<IBusinessMBBankHelper, BusinessMBBankHelper>();
builder.Services.AddSingleton<IMBBankHelper, MBBankHelper>();
builder.Services.AddSingleton<IPVcomBankHelper, PVcomBankHelper>();
builder.Services.AddSingleton<ITechcomBankHelper, TechcomBankHelper>();
builder.Services.AddSingleton<IVietcomBankHelper, VietcomBankHelper>();
builder.Services.AddSingleton<IVietinBankHelper, VietinBankHelper>();
builder.Services.AddSingleton<IBusinessVtbBankHelper, BusinessVtbBankHelper>();
builder.Services.AddSingleton<ITronCryptoHelper, TronCryptoHelper>();
#endregion

#region register mongodb service
builder.Services.AddSingleton<IPayOrdersMongoService, PayOrdersMongoService>();
builder.Services.AddSingleton<IPayOrderDepositsMongoService, PayOrderDepositsMongoService>();
#endregion

#region register sqlsugar
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<INsPayBackgroundJobsService, NsPayBackgroundJobsService>();
builder.Services.AddSingleton<IPayGroupMentService, PayGroupMentService>();
builder.Services.AddSingleton<IPayMentService, PayMentService>();
#endregion

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

app.Run();
