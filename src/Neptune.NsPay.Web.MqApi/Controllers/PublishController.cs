using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;

namespace Neptune.NsPay.Web.MqApi.Controllers
{
    public class PublishController : Controller
    {
        [CapSubscribe("xxx.services.show.time")]
        public void CheckReceivedMessage(DateTime datetime)
        {
            var dete = datetime;
            //Console.WriteLine(datetime);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
