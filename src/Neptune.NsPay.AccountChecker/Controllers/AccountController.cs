using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.AccountChecker.BankChecker;
using System.Diagnostics;
using System.Text.Json;

namespace Neptune.NsPay.AccountChecker.Controllers
{
 
        [ApiController]
        [Route("[controller]")]
    public class AccountController : ControllerBase
    {


        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IBankAccountService _bankAccountService;

        public AccountController(ILogger<WeatherForecastController> logger, IBankAccountService bankAccountService)
        {
            _logger = logger;
            _bankAccountService = bankAccountService;
        }

        [HttpGet("BankDetails")]
        public async Task<BankAccountDetail> Get(string bankKey, string accountNumber)
        {
            var stopwatch = Stopwatch.StartNew();

            var processId = Guid.NewGuid().ToString();
            var result = await _bankAccountService.GetBankAccountDetails(bankKey, accountNumber, processId);

            stopwatch.Stop();

            _logger.LogInformation("BankDetails time take {time} ms , Request {req}  Responaw {res} ", stopwatch.ElapsedMilliseconds , (bankKey + "||"+ accountNumber), JsonSerializer.Serialize(result));

            return result;

        }


    }
}
