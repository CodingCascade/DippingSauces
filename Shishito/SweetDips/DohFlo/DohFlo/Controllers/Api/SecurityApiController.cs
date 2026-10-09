using DohFlo.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace DohFlo.Controllers.Api
{
    [ApiController, Route("api/security")]
    [Authorize(Policy = FinanceAccess.ReadPolicy)]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public sealed class SecurityApiController : ControllerBase
    {
        [HttpGet("antiforgery")]
        public IActionResult AntiForgery([FromServices] IAntiforgery antiforgery)
        {
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);

            return Ok(new { requestToken = tokens.RequestToken });
        }
    }
}
