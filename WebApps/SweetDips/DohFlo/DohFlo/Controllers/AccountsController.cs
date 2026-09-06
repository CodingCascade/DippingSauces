using DohFlo.Data;
using DohFlo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DohFlo.Controllers
{
    public class AccountsController : Controller
    {
        private readonly DohFloContext _db;
        private const int BoolieUserId = 1;

        public AccountsController(DohFloContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accounts = await _db.Accounts
                .AsNoTracking()
                .Where(account => account.UserId == BoolieUserId)
                .OrderBy(account => account.IsClosed)
                .ThenBy(account => account.Name)
                .ToListAsync();

            return View(accounts);
        }

        [HttpGet]
        public IActionResult Create() =>
            View(new CreateAccountViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
           CreateAccountViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var accountName = viewModel.Name.Trim();  

            var duplicateExists = await _db.Accounts.AnyAsync(account =>
                account.UserId == BoolieUserId &&
                account.Name == accountName);

            if (duplicateExists)
            {
                ModelState.AddModelError(nameof(viewModel.Name),
                    "You already have an account with this name.");

                return View(viewModel);
            }

            var account = new Account
            {
                UserId = BoolieUserId,
                Name = accountName,
                Type = viewModel.Type,
                Institution = viewModel.Institution?.Trim() ?? "",
                CurrencyCode = viewModel.CurrencyCode
                    .Trim()
                    .ToUpperInvariant()
            };

            _db.Accounts.Add(account);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Account '{account.Name}' was created.";

            return RedirectToAction(nameof(Index));
        }
    }
}
