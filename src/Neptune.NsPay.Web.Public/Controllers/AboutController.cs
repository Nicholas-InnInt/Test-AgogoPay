using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Controllers;

namespace Neptune.NsPay.Web.Public.Controllers
{
    public class AboutController : NsPayControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}