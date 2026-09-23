using DohFlo.Data;
using DohFlo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DohFlo.Controllers
{
    public class TransactionsController : Controller
    {        private readonly DohFloContext _db;
        private const int BoolieUserId = 1; // The first seeded user

        public TransactionsController(DohFloContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var transactions = await _db.Transactions
                .AsNoTracking()
                .Include(transaction => transaction.Account)
                .Include(transaction => transaction.Payee)
                .Include(transaction => transaction.Category)
                .Where(transaction => 
                    transaction.UserId == BoolieUserId &&
                    !transaction.IsDeleted)
                .OrderByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Id)
                .ToListAsync();

            return View(transactions);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _db.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(transaction =>
                    transaction.Id == id &&
                    transaction.UserId == BoolieUserId &&
                    !transaction.IsDeleted);

            if (transaction is null)
            {
                return NotFound();
            }

            var viewModel = new EditTransactionViewModel
            {
                Id = transaction.Id,
                AccountId = transaction.AccountId,
                PayeeId = transaction.PayeeId,
                CategoryId = transaction.CategoryId,
                Amount = transaction.Amount,
                Date = transaction.Date,
                Notes = transaction.Notes,
                IsPending = transaction.IsPending,
                CurrencyCode = transaction.CurrencyCode
            };

            viewModel.Accounts = await _db.Accounts
                .AsNoTracking()
                .Where(account => account.UserId == BoolieUserId &&
                (!account.IsClosed || account.Id == transaction.AccountId))
                .OrderBy(account => account.Name)
                .Select(account => new SelectListItem
                {
                    Value = account.Id.ToString(),
                    Text = account.Name
                })
                .ToListAsync();

            viewModel.Payees = await _db.Payees
                .AsNoTracking()
                .Where(payee => payee.UserId == BoolieUserId)
                .OrderBy(payee => payee.Name)
                .Select(payee => new SelectListItem
                {
                    Value = payee.Id.ToString(),
                    Text = payee.Name
                })
                .ToListAsync();

            viewModel.Categories = await _db.Categories
                .AsNoTracking()
                .Where(category => category.UserId == BoolieUserId)
                .OrderBy(category => category.Name)
                .Select(category => new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name
                })
                .ToListAsync();

            return View(viewModel);
        }

        // POST: /Transactions/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTransactionViewModel viewModel)
        {
            var transaction = await _db.Transactions
                .FirstOrDefaultAsync(transaction =>
                    transaction.Id == viewModel.Id &&
                    transaction.UserId == BoolieUserId &&
                    !transaction.IsDeleted);

            if (transaction is null)
            {
                return NotFound();
            }

            var accountIsValid = await _db.Accounts.AnyAsync(account =>
                account.Id == viewModel.AccountId &&
                account.UserId == BoolieUserId &&
                (!account.IsClosed ||
                 account.Id == transaction.AccountId));

            if (!accountIsValid)
            {
                ModelState.AddModelError(
                    nameof(viewModel.AccountId),
                    "Please select a valid account.");
            }

            if (viewModel.PayeeId.HasValue &&
                !await _db.Payees.AnyAsync(payee =>
                    payee.Id == viewModel.PayeeId.Value &&
                    payee.UserId == BoolieUserId))
            {
                ModelState.AddModelError(
                    nameof(viewModel.PayeeId),
                    "Please select a valid payee.");
            }

            if (viewModel.CategoryId.HasValue &&
                !await _db.Categories.AnyAsync(category =>
                    category.Id == viewModel.CategoryId.Value &&
                    category.UserId == BoolieUserId))
            {
                ModelState.AddModelError(
                    nameof(viewModel.CategoryId),
                    "Please select a valid category.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateLists(viewModel, transaction.AccountId);
                return View(viewModel);
            }

            var wasPending = transaction.IsPending;

            transaction.AccountId = viewModel.AccountId;
            transaction.PayeeId = viewModel.PayeeId;
            transaction.CategoryId = viewModel.CategoryId;
            transaction.Amount = viewModel.Amount;
            transaction.CurrencyCode =
                viewModel.CurrencyCode.Trim().ToUpperInvariant();
            transaction.Date = viewModel.Date;
            transaction.Notes = viewModel.Notes?.Trim();
            transaction.IsPending = viewModel.IsPending;
            transaction.UpdatedAt = DateTime.UtcNow;

            if (viewModel.IsPending)
            {
                transaction.ClearedDate = null;
            }
            else if (wasPending || transaction.ClearedDate is null)
            {
                transaction.ClearedDate = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The transaction was updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateLists(EditTransactionViewModel viewModel, int currentAccountId)
        {
            viewModel.Accounts = await _db.Accounts
                .AsNoTracking()
                .Where(account =>
                    account.UserId == BoolieUserId &&
                    (!account.IsClosed ||
                     account.Id == currentAccountId))
                .OrderBy(account => account.Name)
                .Select(account => new SelectListItem
                {
                    Value = account.Id.ToString(),
                    Text = account.Name
                })
                .ToListAsync();

            viewModel.Payees = await _db.Payees
                .AsNoTracking()
                .Where(payee => payee.UserId == BoolieUserId)
                .OrderBy(payee => payee.Name)
                .Select(payee => new SelectListItem
                {
                    Value = payee.Id.ToString(),
                    Text = payee.Name
                })
                .ToListAsync();

            viewModel.Categories = await _db.Categories
                .AsNoTracking()
                .Where(category =>
                    category.UserId == BoolieUserId)
                .OrderBy(category => category.Name)
                .Select(category => new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name
                })
                .ToListAsync();
        }

        // GET: /Transactions/Create
        public async Task<IActionResult> Create()
        {
            var vm = new CreateTransactionViewModel();
            await PopulateLists(vm);
            return View(vm);
        }

        // POST: /Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTransactionViewModel vm)
        {
            if (vm.PayeeId.HasValue && !await _db.Payees.AnyAsync(payee =>
                payee.Id == vm.PayeeId.Value &&
                payee.UserId == BoolieUserId))
            {
                ModelState.AddModelError(
                    nameof(vm.PayeeId),
                    "Please select a valid payee.");
            }

            if (vm.CategoryId.HasValue && !await _db.Categories.AnyAsync(category =>
                    category.Id == vm.CategoryId.Value &&
                    category.UserId == BoolieUserId))
            {
                ModelState.AddModelError(
                    nameof(vm.CategoryId),
                    "Please select a valid category.");
            }

            //The server side validation can't trust the dropdown alone
            if (vm.AccountId > 0 && !await _db.Accounts.AnyAsync(account =>
            account.Id == vm.AccountId &&
            account.UserId == BoolieUserId &&
            !account.IsClosed))
            {
                ModelState.AddModelError(nameof(vm.AccountId), "Please select a valid open account.");
            }

            // Re-populate dropdowns if validation fails
            if(!ModelState.IsValid)
            {
                await PopulateLists(vm);
                return View(vm);
            }

            var tx = new Transaction
            {
                UserId = BoolieUserId,
                AccountId = vm.AccountId,
                PayeeId = vm.PayeeId,
                CategoryId = vm.CategoryId, // null is Ok when you'll add splits
                Amount = vm.Amount,
                CurrencyCode = vm.CurrencyCode.Trim().ToUpperInvariant(),
                Date = vm.Date,
                Notes = vm.Notes?.Trim(),
                IsPending = vm.IsPending
            };

            _db.Transactions.Add(tx);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Transaction saved successfully!";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Transactions/Delete/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _db.Transactions
                .FirstOrDefaultAsync(transaction =>
                    transaction.Id == id &&
                    transaction.UserId == BoolieUserId &&
                    !transaction.IsDeleted);

            if (transaction is null)
            {
                TempData["ErrorMessage"] = "The transaction could not be found.";
                return RedirectToAction(nameof(Index));
            }

            transaction.IsDeleted = true;
            transaction.DeletedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The transaction was deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, bool isPending)
        {
            var transaction = await _db.Transactions
                .FirstOrDefaultAsync(transaction =>
                    transaction.Id == id &&
                    transaction.UserId == BoolieUserId &&
                    !transaction.IsDeleted);

            if (transaction is null)
            {
                TempData["ErrorMessage"] = "The transaction could not be found.";

                return RedirectToAction(nameof(Index));
            }

            transaction.IsPending = isPending;

            transaction.ClearedDate = isPending ? null : DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = isPending ? "The transaction was marked as pending." : "The transaction was reconciled.";

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateLists(CreateTransactionViewModel vm)
        {
            vm.Accounts = await _db.Accounts
                .Where(a => a.UserId == BoolieUserId && !a.IsClosed)
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
                .ToListAsync();

            vm.Payees = await _db.Payees
                .Where(p => p.UserId == BoolieUserId)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToListAsync();

            vm.Categories = await _db.Categories
                .Where(c => c.UserId == BoolieUserId)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
        }
    }
}
