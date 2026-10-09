using DohFlo.Data;
using DohFlo.Models.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DohFlo.Controllers.Profile;

[Authorize]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public sealed class ProfileController : Controller
{
    private readonly UserManager<User> _users;

    public ProfileController(UserManager<User> users)
    {
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _users.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var roles = await _users.GetRolesAsync(user);

        var model = new ProfileViewModel
        {
            Email = user.Email ?? "",
            DisplayName = user.DisplayName,
            Role = string.Join(", ", roles)
        };

        return View(model);
    }
}