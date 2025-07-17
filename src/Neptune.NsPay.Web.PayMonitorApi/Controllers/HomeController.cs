using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.PayMonitorApi.Models;
using Neptune.NsPay.Web.PayMonitorApi.Service;
using System.Diagnostics;

namespace Neptune.NsPay.Web.PayMonitorApi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPushUpdateService _pushUpdateService;
        public HomeController(ILogger<HomeController> logger, IPushUpdateService pushUpdateService)
        {
            _logger = logger;
            _pushUpdateService = pushUpdateService;

        }

        public IActionResult Index()
        {
            _pushUpdateService.BalanceChanged(new BalanceUpdateNotification() { Balance = 99999, PayMentId = 56, UpdatedTime = DateTime.Now });
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
