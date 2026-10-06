using Microsoft.AspNetCore.Mvc;

namespace DohFlo.Controllers
{
    public class AccountsLabController : Controller
    {
        [HttpGet]
        public IActionResult Index() => View();
    }
}
