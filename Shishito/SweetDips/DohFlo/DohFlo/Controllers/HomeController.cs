using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DohFlo.Models;

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("~/Views/Error.cshtml", new ErrorViewModel {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        public IActionResult ContactUs()
        {
            return View("~/Views/ContactUs.cshtml");
        }
    }
}
