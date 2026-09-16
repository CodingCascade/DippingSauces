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
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(DohFloContext db, ILogger<AccountsController> logger)
        {
            _db = db;
            _logger = logger;
        }

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

        // GET - Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var account = await _db.Accounts
                .AsNoTracking()
                .SingleOrDefaultAsync(account =>
                    account.Id == id &&
                    account.UserId == BoolieUserId);

            if(account is null)
            {
                return NotFound();
            }

            var viewModel = new EditAccountViewModel
            {
                Id = account.Id,
                Name = account.Name,
                Type = account.Type,
                Institution = account.Institution,
                CurrencyCode = account.CurrencyCode
            };

            return View(viewModel);
        }

        // POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditAccountViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                _logger.LogWarning("Account edit ID mismatch. Route ID: {RouteId}, Form ID: {FormId}", id, viewModel.Id);
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Account edit validation failed for account {AccountId}.", id);

                ModelState.AddModelError(string.Empty, "Please correct the errors below and try again");

                return View(viewModel);
            }

            var account = await _db.Accounts.SingleOrDefaultAsync(account =>
                account.Id == id &&
                account.UserId == BoolieUserId);

            if (account is null)
            {
                _logger.LogWarning("Account edit failed. Account {AccountId} was not found for the current user.", id);

                return NotFound("The requested account could not be found.");
            }

            var accountName = viewModel.Name.Trim();

            var duplicateExists = await _db.Accounts.AnyAsync(other =>
                other.UserId == BoolieUserId &&
                other.Id != id &&
                other.Name == accountName
            );

            if (duplicateExists)
            {
                account.Name = accountName;
                account.Type = viewModel.Type;
                account.Institution = viewModel.Institution?.Trim() ?? "";
                account.CurrencyCode = viewModel.CurrencyCode
                    .Trim()
                    .ToUpperInvariant();

                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Account '{account.Name}' was updated.";

            }

            return RedirectToAction(nameof(Index));

        }

        // ToggledClosed the closed/inactive account. We can use it as a reversable status instead of deleting the row.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleClosed(int id)
        {
            var account = await _db.Accounts.SingleOrDefaultAsync(account =>
                account.Id == id &&
                account.UserId == BoolieUserId);

            if (account is null)
            {
                _logger.LogWarning("Account edit failed. Account {AccountId} was not found for the current user.", id);

                return NotFound("The requested account could not be found.");
            }

            account.IsClosed = !account.IsClosed;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = account.IsClosed
                ? $"Account '{account.Name}' was closed."
                : $"Account '{account.Name}' was reopened.";

            return RedirectToAction(nameof(Index));
        }
    }
}
