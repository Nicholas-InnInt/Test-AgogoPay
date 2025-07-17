using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Neptune.NsPay.SqlSugarExtensions;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.Sundial;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.Sundial.Services.Interfaces;
using Neptune.NsPay.Sundial.Services;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    services.AddSqlsugarSetup();

    #region register sqlsugar

    services.AddSingleton<IUnitOfWork, UnitOfWork>();
    services.AddSingleton<INsPayBackgroundJobsService, NsPayBackgroundJobsService>();

    #endregion register sqlsugar

    services.AddSingleton<IBankAutoService, BankAutoService>();

    services.AddHttpClient();
    services.AddSchedule(options =>
    {
        options.JobDetail.LogEnabled = true;
    });

    services.AddHostedService<Worker>();
});

builder.ConfigureWebHostDefaults(config =>
{
    config.ConfigureKestrel(options => options.ListenLocalhost(38999));
    config.Configure(app =>
    {
        app.UseStaticFiles();
        app.UseScheduleUI();
    });
});

var host = builder.Build();
host.Run();