using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.TransferApi.SignalR;

namespace Neptune.NsPay.Web.TransferApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("GetNotificationMerchant")]
        public IEnumerable<string> GetABC()
        {
            var onlineMerchant = NotificationHub.OnlineMerchant();
            return onlineMerchant.ToArray();
        }

        [HttpGet("GetConnectedDevice")]
        public IEnumerable<DeviceConnection> GetDevice()
        {
            var onlineMerchant = OrderHub.GetConnectedDeviceConnection();
            return onlineMerchant.Select(x=> new DeviceConnection() { DeviceId = x.Item1 , SignalRConnection = x.Item2 }).ToArray();
        }


        public class DeviceConnection
        {
            public int DeviceId { get; set; }
            public string SignalRConnection { get; set; }
        }
    }
}
