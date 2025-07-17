using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Neptune.NsPay.MerchantBillsRecoverWorker;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.SqlSugarExtensions;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices;
using Neptune.NsPay.BillingExtensions;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();


builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddMongoSetup();

#region register mongodb service
builder.Services.AddSingleton<IPayOrdersMongoService, PayOrdersMongoService>();
builder.Services.AddSingleton<IMerchantBillsMongoService, MerchantBillsMongoService>();
builder.Services.AddSingleton<IWithdrawalOrdersMongoService, WithdrawalOrdersMongoService>();
#endregion

#region register sql service
builder.Services.AddSqlsugarSetup();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IMerchantWithdrawService, MerchantWithdrawService>();

#endregion

builder.Services.AddSingleton<IMerchantBillsHelper, MerchantBillsHelper>();


var host = builder.Build();
host.Run();