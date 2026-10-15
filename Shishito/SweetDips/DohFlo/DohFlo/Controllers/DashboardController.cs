using System.Text.Json;
using DohFlo.Security;
using DohFlo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DohFlo.Controllers;

[Authorize(Policy = FinanceAccess.ReadPolicy)]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class DashboardController(IWebHostEnvironment environment) : Controller
{
    [HttpGet("/dashboard")]
    public async Task<IActionResult> Index()
    {
        var manifestPath = Path.Combine(
            environment.ContentRootPath,
            "DashboardAssets",
            "dashboard-assets.json");

        if (!System.IO.File.Exists(manifestPath))
        {
            return StatusCode(503, "Build and copy the Angular dashboard first.");
        }

        DashboardAssetsViewModel? model;

        try
        {
            var json = await System.IO.File.ReadAllTextAsync(manifestPath);
            model = JsonSerializer.Deserialize<DashboardAssetsViewModel>(json);
        }
        catch (IOException)
        {
            return StatusCode(503, "Rebuild and copy the Angular dashboard assets.");
        }
        catch (JsonException)
        {
            return StatusCode(503, "Rebuild the Angular dashboard asset manifest.");
        }

        if (model is null || model.Scripts is null || model.Scripts.Length == 0
            || model.Stylesheets is null)
        {
            return StatusCode(503, "The dashboard asset manifest is incomplete. Rebuild Angular.");
        }

        return View(model);
    }
}