using DohFlo.Data;
using DohFlo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DohFlo.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly DohFloContext _db;
        private const int BoolieUserId = 1;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(DohFloContext db, ILogger<CategoriesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categories
                .AsNoTracking()
                .Include(category => category.ParentCategory)
                .Where(category => category.UserId == BoolieUserId)
                .OrderBy(category => category.CatType)
                .ThenBy(category => category.Name)
                .ToListAsync();

            return View(categories);
        }

        private async Task PopulateParentCategories(CategoryFormViewModel viewModel, int? excludeId = null)
        {
            viewModel.ParentCategories = await _db.Categories
                .AsNoTracking()
                .Where(category =>
                    category.UserId == BoolieUserId &&
                    category.ParentCategoryId == null &&
                    (!excludeId.HasValue || category.Id != excludeId.Value))
                .OrderBy(category => category.Name)
                .Select(category => new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name
                }).ToListAsync();
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CategoryFormViewModel();
            await PopulateParentCategories(viewModel);

            return View(viewModel);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (CategoryFormViewModel viewModel)
        {
            if (viewModel.ParentCategoryId.HasValue)
            {
                var validParent = await _db.Categories.AnyAsync(category =>
                    category.Id == viewModel.ParentCategoryId.Value &&
                    category.UserId == BoolieUserId &&
                    category.ParentCategoryId == null);

                if (!validParent)
                {
                    ModelState.AddModelError(nameof(viewModel.ParentCategoryId),
                        "Please select a valid parent category.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateParentCategories(viewModel);
                    
                return View(viewModel);
            }

            var name = viewModel.Name.Trim();

            var duplicateExists = await _db.Categories.AnyAsync(category =>
                category.UserId == BoolieUserId &&
                category.Name == name);

            if (duplicateExists)
            {
                ModelState.AddModelError(nameof(viewModel.Name),
                    "You already have a category with this name.");
                await PopulateParentCategories(viewModel);

                return View(viewModel);
            }

            var category = new Category
            {
                UserId = BoolieUserId,
                Name = name,
                CatType = viewModel.CatType,
                ParentCategoryId = viewModel.ParentCategoryId
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category '{category.Name}' was created.";

            return RedirectToAction(nameof(Index));
        }

        // Edit
        [HttpGet]
        public async Task<IActionResult> Edit (int id)
        {
            var category = await _db.Categories
                .AsNoTracking()
                .SingleOrDefaultAsync(category =>
                category.Id == id &&
                category.UserId == BoolieUserId);

            if (category is null)
            {
                _logger.LogWarning("Category edit failed. Category #{Id}", id);

                return NotFound("The requested category culd not be found.");
            }

            var viewModel = new CategoryFormViewModel
            {
                Id = category.Id,
                Name = category.Name,
                CatType = category.CatType,
                ParentCategoryId = category.ParentCategoryId
            };

            await PopulateParentCategories(viewModel, id);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                _logger.LogWarning("Category edit ID mismatch. Route ID: {RouteId}, Category ID: {FormId}", id, viewModel.Id);

                return BadRequest();
            }

            var category = await _db.Categories.SingleOrDefaultAsync(category =>
                category.Id == id &&
                category.UserId == BoolieUserId);

            if (category is null)
            {
                _logger.LogWarning("Category edit failed. Category #{Id} was not found for the current user.", id);

                return NotFound("The requested category could not be found.");
            }

            if (viewModel.ParentCategoryId.HasValue)
            {
                var validParent = await _db.Categories.AnyAsync(parent =>
                    parent.Id == viewModel.ParentCategoryId.Value &&
                    parent.UserId == BoolieUserId &&
                    parent.Id != id &&
                    parent.ParentCategoryId == null);

                if (!validParent)
                {
                    ModelState.AddModelError(nameof(viewModel.ParentCategoryId),
                        "Please select a valid parent category.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateParentCategories(viewModel, id);

                return View(viewModel);
            }

            var name = viewModel.Name.Trim();

            var duplicateExists = await _db.Categories.AnyAsync(other =>
                other.UserId == BoolieUserId &&
                other.Id != id &&
                other.Name == name);

            if (duplicateExists)
            {
                ModelState.AddModelError(nameof(viewModel.Name),
                    "You already have another category with this name.");
                await PopulateParentCategories(viewModel, id);

                return View(viewModel);
            }

            category.Name = name;
            category.CatType = viewModel.CatType;
            category.ParentCategoryId = viewModel.ParentCategoryId;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category '{category.Name}' was updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete (int id)
        {
            var category = await _db.Categories.SingleOrDefaultAsync(category =>
                category.Id == id &&
                category.UserId == BoolieUserId);

            if (category is null)
            {
                _logger.LogWarning("Category delete failed. Category Id #{Id}", id);

                return NotFound("The requested category could not be found.");
            }

            var hasChildren = await _db.Categories.AnyAsync(child =>
                child.ParentCategoryId == id);

            var usedByTransaction = await _db.Transactions.AnyAsync(transaction =>
                transaction.CategoryId == id);

            var usedBySplit = await _db.TransactionCatSplits.AnyAsync(split =>
                split.CategoryId == id);

            if (hasChildren || usedByTransaction || usedBySplit)
            {
                TempData["ErrorMessage"] = $"'{category.Name}' is in use and cannot be deleted.";

                return RedirectToAction(nameof(Index));
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category '{category.Name}' was deleted.";

            return RedirectToAction(nameof(Index));
        }
    }
}
