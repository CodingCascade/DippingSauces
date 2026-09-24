using Microsoft.AspNetCore.Mvc;

namespace DohFlo.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Index.cshtml");
        }

        public IActionResult PrivacyPolicy()
        {
            return View("~/Views/PrivacyPolicy.cshtml");
        }

        public IActionResult CookiePolicy()
        {
            return View("~/Views/CookiePolicy.cshtml");
        }

        public IActionResult SiteDisclaimer()
        {
            return View("~/Views/SiteDisclaimer.cshtml");
        }

        public IActionResult SiteMap()
        {
            return View("~/Views/SiteMap.cshtml");
        }

        public IActionResult ContactUs()
        {
            return View("~/Views/ContactUs.cshtml");
        }
    }
}
