using Dohflo.Data;
using DohFlo.Data;
using DohFlo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DohFlo.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly DohFloContext _db;
        private const int BoolieUserId = 1; // The first seeded user

        public TransactionsController(DohFloContext db) => _db = db;

        // GET: /Transactions/Create
        public async Task<IActionResult> Create()
        {
            var vm = new CreateTransactionViewModel
            {
                Accounts = await _db.Accounts
                    .Where(a => a.UserId == BoolieUserId && !a.IsClosed)
                    .OrderBy(a => a.Name)
                    .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
                    .ToListAsync(),

            };

            return View(vm);
        }

        // POST: /Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTransactionViewModel vm)
        { 
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
                CurrencyCode = vm.CurrencyCode,
                Date = vm.Date,
                Notes = vm.Notes,
                IsPending = vm.IsPending
            };

            _db.Transactions.Add(tx);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Create)); // back to the form or go to Details/Index
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
