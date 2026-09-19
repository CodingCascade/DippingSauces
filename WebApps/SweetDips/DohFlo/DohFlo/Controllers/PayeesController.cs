using DohFlo.Data;
using DohFlo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DohFlo.Controllers
{
    public class PayeesController : Controller
    {
        private readonly DohFloContext _db;
        private const int BoolieUserId = 1;

        private readonly ILogger<AccountsController> _logger;

        public PayeesController(DohFloContext db, ILogger<AccountsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var payees = await _db.Payees
                .AsNoTracking()
                .Where(payee => payee.UserId == BoolieUserId)
                .OrderBy(payee => payee.Name)
                .ToListAsync();

            return View(payees);
        }

        [HttpGet]
        public IActionResult Create() =>
            View(new PayeeFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PayeeFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var name = viewModel.Name.Trim();
            var normalizedName = name.ToUpperInvariant();

            var duplicateExists = await _db.Payees.AnyAsync(payee =>
                payee.UserId == BoolieUserId &&
                payee.NormalizedName == normalizedName);

            if (duplicateExists)
            {
                ModelState.AddModelError(nameof(viewModel.Name),
                    "You already have a payee with this name.");
                return View(viewModel);
            }

            var payee = new Payee
            {
                UserId = BoolieUserId,
                Name = name,
                NormalizedName = normalizedName
            };

            _db.Payees.Add(payee);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Payee '{payee.Name}' was created.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var payee = await _db.Payees
                .AsNoTracking()
                .SingleOrDefaultAsync(payee =>
                    payee.Id == id &&
                    payee.UserId == BoolieUserId);

            if (payee is null)
            {
                _logger.LogWarning("Payee edit failed. Payee {PayeeId} was not found for the current user.", id);

                return NotFound("The requested payee could not be found.");
            }

            return View(new PayeeFormViewModel
            {
                Id = payee.Id,
                Name = payee.Name
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PayeeFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                _logger.LogWarning("Payee edit ID mismatch. Route ID: {RouteId}, Form ID: {FormId}", id, viewModel.Id);
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Payee edit validation failed for payee {PayeeId}.", id);

                ModelState.AddModelError(string.Empty, "Please correct the errors below and try again");

                return View(viewModel);
            }

            var payee = await _db.Payees.SingleOrDefaultAsync(payee =>
                payee.Id == id &&
                payee.UserId == BoolieUserId);

            if (payee is null)
            {
                _logger.LogWarning("Payee edit failed. Payee {PayeeId} was not found for the current user.", id);

                return NotFound("The requested payee could not be found.");
            }

            var name = viewModel.Name.Trim();
            var normalizedName = name.ToUpperInvariant();

            var duplicateExists = await _db.Payees.AnyAsync(other =>
                other.UserId == BoolieUserId &&
                other.Id != id &&
                other.NormalizedName == normalizedName);

            if (duplicateExists)
            {
                ModelState.AddModelError(nameof(viewModel.Name),
                    "You already have another payee with this name.");

                return View(viewModel);
            }

            payee.Name = name;
            payee.NormalizedName = normalizedName;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Payee '{payee.Name}' was updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete (int id)
        {
            var payee = await _db.Payees.SingleOrDefaultAsync(payee =>
                payee.Id == id &&
                payee.UserId == BoolieUserId);

            if (payee is null)
            {
                _logger.LogWarning("Payee edit failed. Payee {PayeeId} was not found for the current user.", id);

                return NotFound("The requested payee could not be found.");
            }

            var isUsed = await _db.Transactions.AnyAsync(transaction =>
                transaction.PayeeId == id);

            if (isUsed)
            {
                TempData["ErrorMessage"] = $"'{payee.Name}' is used by transactions and cannot be deleted.";

                return RedirectToAction(nameof(Index));
            }

            _db.Payees.Remove(payee);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Payee '{payee.Name}' was deleted.";

            return RedirectToAction(nameof(Index));
        }
    }
}
