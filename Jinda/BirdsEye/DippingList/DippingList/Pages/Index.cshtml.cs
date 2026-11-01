using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DippingList.Models;
using DippingList.Services;

namespace DippingList.Pages;

public class IndexModel : PageModel
{
    private readonly DippingListService _service;

    public IndexModel(DippingListService service)
    {
        _service = service;
    }

    [BindProperty]
    public string? ItemName { get; set; }

    public IReadOnlyList<DippingItem> Items { get; private set; }
        = Array.Empty<DippingItem>();

    [TempData]
    public string? Message { get; set; }

    public void OnGet()
    {
        Items = _service.GetAll();
    }

    public IActionResult OnPostAdd()
    {
        return Apply(() =>
        {
            _service.Add(ItemName);
            return true;
        }, "Item added.");

    }

    public IActionResult OnPostSetPurchased(
        Guid id, bool purchased)
    {
        return Apply(() => _service.SetPurchased(id, purchased),
            "Item updated successfully!");
    }

    public IActionResult OnPostRemove(Guid id)
    {
        return Apply(() => _service.Remove(id), "Item removed successfully!");
    }

    private IActionResult Apply(Func<bool> action, string success)
    {
        if (!ModelState.IsValid)
        {
            Message = "That item request wasn't valid.";
            return RedirectToPage();
        }

        try
        {
            Message = action() ? success : "Item not found.";
        }
        catch (ArgumentException ex)
        {
            Message = ex.Message;
        }
        catch (IOException)
        {
            Message = "Couldn't save the list. Nothing was changed.";
        }
        catch (UnauthorizedAccessException)
        {
            Message = "Couldn't save the list. Nothing was changed.";
        }

        return RedirectToPage();
    }
}
