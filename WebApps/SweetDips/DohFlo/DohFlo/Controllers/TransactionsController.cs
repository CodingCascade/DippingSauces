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
