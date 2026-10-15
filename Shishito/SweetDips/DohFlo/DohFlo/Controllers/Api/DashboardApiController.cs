using System.Globalization;
using System.Text.RegularExpressions;
using DohFlo.Data;
using DohFlo.Models;
using DohFlo.Security;
using DohFlo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DohFlo.Controllers.Api
{
    [ApiController, Route("api/dashboard")]
    [Authorize(Policy = FinanceAccess.ReadPolicy)]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class DashboardApiController(DohFloContext db) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<DashboardDto>> Get(string month, string currency = "USD", CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(month) || !Regex.IsMatch(month, @"^\d{4}-\d{2}$") || !DateTime.TryParseExact(month + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) || start.Year < 2000 || start.Year > 2100)
            {
                return BadRequest("Use a month between 2000-01 and 2100-12.");
            }

            if (string.IsNullOrWhiteSpace(currency))
            {
                return BadRequest("Use a three-letter currency code.");
            }

            currency = currency.Trim().ToUpperInvariant();

            if (!Regex.IsMatch(currency, "^[A-Z]{3}$"))
            {
                return BadRequest("Use a three-letter currency code.");
            }

            var userId = FinanceAccess.GetUserId(User);
            var end = start.AddMonths(1);
            var transactions = await db.Transactions.AsNoTracking()
                .Where(t => t.UserId == userId && !t.IsDeleted && t.Date < end
                && t.CurrencyCode == currency && t.Account!.UserId == userId)
                .Include(t => t.Account).Include(t => t.Payee)
                .Include(t => t.Category)
                .Include(t => t.Splits).ThenInclude(s => s.Category)
                .AsSplitQuery().ToListAsync(ct);

            try
            {
                return DashboardCalculator.Build(transactions, userId, start, currency);
            }
            catch (InvalidOperationException)
            {
                return Problem(statusCode: 422, title: "Check category types and split amounts in your records.");
            }
        }
    }
}