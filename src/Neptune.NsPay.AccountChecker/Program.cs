using Neptune.NsPay.AccountChecker;
using Neptune.NsPay.AccountChecker.BankChecker;
using Neptune.NsPay.HttpExtensions.Bank.Helpers;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.RedisExtensions;
using NLog.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddLogging(config =>
//{
//    config.AddDebug();
//});

builder.Services.AddLogging(configure => configure.AddNLog());

// Add services to the container.
builder.Services.AddStackExchangeRedisExtensions();
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IBankStateHelper, BankStateHelper>();
builder.Services.AddSingleton<IBankAccountService, BankAccountService>();
builder.Services.AddSingleton<LogHelper>();

//builder.Services.AddTransient<RequestResponseLoggingMiddleware>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
var bankAccountService = app.Services.GetRequiredService<IBankAccountService>();
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
//app.UseMiddleware<RequestResponseLoggingMiddleware>();
// Register the event handlers for application stop
lifetime.ApplicationStopping.Register(OnApplicationStopping);
lifetime.ApplicationStopped.Register(OnApplicationStopped);
lifetime.ApplicationStarted.Register(OnApplicationStarted);


// Event handler for when the application is stopping
void OnApplicationStarted()
{
}


// Event handler for when the application is stopping
void OnApplicationStopping()
{
 
}

// Event handler for when the application has stopped
void OnApplicationStopped()
{
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
