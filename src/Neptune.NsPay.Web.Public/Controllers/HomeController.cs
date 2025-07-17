using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Controllers;

namespace Neptune.NsPay.Web.Public.Controllers
{
    public class HomeController : NsPayControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}