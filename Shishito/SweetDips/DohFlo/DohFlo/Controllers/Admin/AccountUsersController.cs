using DohFlo.Data;
using DohFlo.Models.Admin;
using DohFlo.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DohFlo.Controllers.Admin
{
    [Authorize(Policy = FinanceAccess.AdminPolicy)]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public sealed class AccountUsersController : Controller
    {
        private const string IndexViewPath = "~/Views/Admin/AccountUsers/Index.cshtml";
        private const string CreateViewPath = "~/Views/Admin/AccountUsers/Create.cshtml";
        private const string ResetPasswordViewPath = "~/Views/Admin/AccountUsers/ResetPassword.cshtml";

        private readonly UserManager<User> _users;

        public AccountUsersController(UserManager<User> users)
        {
            _users = users;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _users.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var rows = new List<AccountUserRowViewModel>();

            foreach (var user in users)
            {
                var roles = await _users.GetRolesAsync(user);

                rows.Add(new AccountUserRowViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    DisplayName = user.DisplayName,
                    Role = string.Join(", ", roles.Select(role =>
                        role == FinanceAccess.StandardUserRole ? "Standard User" : role)),
                    IsAdmin = roles.Contains(FinanceAccess.AdminRole),
                    IsBlocked = user.IsBlocked
                });
            }

            return View(IndexViewPath, rows);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(CreateViewPath, new CreateAccountUserViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccountUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(CreateViewPath, model);
            }

            var email = model.Email.Trim();

            if (await _users.FindByEmailAsync(email) is not null)
            {
                ModelState.AddModelError(nameof(model.Email),
                    "An account with this email already exists.");

                return View(CreateViewPath, model);
            }

            var user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = model.DisplayName.Trim(),
                IsBlocked = false,
                LockoutEnabled = true
            };

            var created = await _users.CreateAsync(user, model.Password);

            if (!created.Succeeded)
            {
                foreach (var error in created.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(CreateViewPath, model);
            }

            var roleResult = await _users.AddToRoleAsync(user, FinanceAccess.StandardUserRole);

            if (!roleResult.Succeeded)
            {
                var cleanupResult = await _users.DeleteAsync(user);
                if (!cleanupResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty,
                        "The login was created but its role could not be assigned. " +
                        "Automatic cleanup also failed. Review this account before retrying.");
                }

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(CreateViewPath, model);
            }

            TempData["SuccessMessage"] = "Standard user created successfully!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetBlocked(int id, [FromForm] bool? blocked)
        {
            if (!ModelState.IsValid || !blocked.HasValue)
            {
                TempData["ErrorMessage"] =
                    "The requested account status was missing or invalid. Please try again.";

                return RedirectToAction(nameof(Index));
            }

            var user = await _users.FindByIdAsync(id.ToString());

            if (user is null)
            {
                return NotFound();
            }

            if (await _users.IsInRoleAsync(user, FinanceAccess.AdminRole))
            {
                return BadRequest("The Admin account cannot be blocked here.");
            }

            bool shouldBlock = blocked.Value;

            user.IsBlocked = shouldBlock;

            var result = await _users.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(
                    "; ",
                    result.Errors.Select(error => error.Description));

                return RedirectToAction(nameof(Index));
            }

            if (shouldBlock)
            {
                var stampResult = await _users.UpdateSecurityStampAsync(user);

                if (!stampResult.Succeeded)
                {
                    TempData["ErrorMessage"] =
                        "The user is blocked, but existing sessions could not be invalidated.";

                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["SuccessMessage"] = shouldBlock
                ? "The user is blocked successfully!"
                : "The user is restored successfully!";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _users.FindByIdAsync(id.ToString());

            if (user is null)
            {
                return NotFound();
            }

            if (await _users.IsInRoleAsync(user, FinanceAccess.AdminRole))
            {
                return BadRequest("Use 'Change Password' for the Admin account.");
            }

            return View(ResetPasswordViewPath, new AdminResetPasswordViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? ""
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(AdminResetPasswordViewModel model)
        {
            var user = await _users.FindByIdAsync(model.UserId.ToString());

            if (user is null)
            {
                return NotFound();
            }

            if (await _users.IsInRoleAsync(user, FinanceAccess.AdminRole))
            {
                return BadRequest("Use 'Change Password' for the Admin account.");
            }

            model.Email = user.Email ?? "";
            ModelState.Remove(nameof(model.Email));

            if (!ModelState.IsValid)
            {
                return View(ResetPasswordViewPath, model);
            }

            var token = await _users.GeneratePasswordResetTokenAsync(user);
            var result = await _users.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(ResetPasswordViewPath, model);
            }

            TempData["SuccessMessage"] = "Password reset successfully!";

            return RedirectToAction(nameof(Index));
        }

    }
}