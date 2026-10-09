using DohFlo.Data;
using DohFlo.Models.Auth;
using DohFlo.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DohFlo.Controllers.Auth
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public sealed class AuthController : Controller
    {
        private readonly SignInManager<User> _signIn;
        private readonly UserManager<User> _users;

        public AuthController(SignInManager<User> signIn, UserManager<User> users)
        {
            _signIn = signIn;
            _users = users;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
                
            var email = model.Email.Trim();
            var user = await _users.FindByEmailAsync(email);

            // Keep the visible failure generic for invalid or blocked accounts.
            if (user is null || user.IsBlocked)
            {
                ModelState.AddModelError(string.Empty, "Sign-in failed. Please check your credentials or try again later.");

                return View(model);
            }

            var result = await _signIn.PasswordSignInAsync(
                user.UserName!, model.Password  , isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) &&
                    Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }
                    
                return RedirectToAction("Index", "Accounts");
            }

            ModelState.AddModelError(string.Empty, "Sign-in failed. Please check your credentials or try again later.");

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;

            return View();
        }

        [Authorize(Policy = FinanceAccess.AdminPolicy)]
        [HttpGet]
        public IActionResult ChangePassword() 
        {
            return View(new ChangePasswordViewModel());
        } 

        [Authorize(Policy = FinanceAccess.AdminPolicy)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
               return View(model);
            }

            var user = await _users.GetUserAsync(User);

            if (user is null)
            {
                return Challenge();
            }

            var result = await _users.ChangePasswordAsync(
                user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                
                return View(model);
            }

            await _signIn.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Your password was changed successfully!";

            return RedirectToAction("Index", "Profile"); 
        }
        
    }
}
