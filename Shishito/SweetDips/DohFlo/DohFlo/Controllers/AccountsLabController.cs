using DohFlo.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DohFlo.Controllers
{
    public class AccountsLabController : Controller
    {
        [HttpGet]
        [Authorize(Policy = FinanceAccess.AdminPolicy)]
        public IActionResult Index() => View();
    }
}
