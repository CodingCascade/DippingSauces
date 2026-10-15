using DohFlo.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DohFlo.Controllers
{
    [Authorize(Policy = FinanceAccess.ReadPolicy)]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None,
    NoStore = true)]
    public class DashboardController(IWebHostEnvironment environment) : Controller
    {
        [HttpGet("/dashboard")]
        public IActionResult Index()
        {
            var path = Path.Combine(environment.ContentRootPath, "DashboardAssets", "index.html");

            if (!System.IO.File.Exists(path))
            {
                return StatusCode(503, "Build and copy the Angular dashboard first.");
            }

            return PhysicalFile(path, "text/html; charset=utf-8");
        }
    }
}
