using DohFlo.Data;
using DohFlo.Models;

namespace DohFlo.Services;

public static class DashboardCalculator
{
    private record Part(string Name, string Kind, decimal Amount);
    private record Row(Transaction Transaction, List<Part> Parts)
    {
        public decimal Income => Parts.Where(p => p.Kind == "Income")
            .Sum(p => p.Amount);
        public decimal Expenses => Parts.Where(p => p.Kind == "Expense")
            .Sum(p => p.Amount);
        public decimal Net => Income - Expenses;
    }

    public static DashboardDto Build(IEnumerable<Transaction> transactions,
        int userId, DateTime start, string currency)
    {
        var end = start.AddMonths(1);
        var rows = transactions
            .Where(t => t.UserId == userId && !t.IsDeleted && t.Date < end
                && t.CurrencyCode == currency
                && t.Account?.UserId == userId)
            .Select(t => new Row(t, Parts(t, userId))).ToList();
        var selected = rows.Where(r => r.Transaction.Date >= start).ToList();
        var expenses = selected.Sum(r => r.Expenses);
        var spending = selected.SelectMany(r => r.Parts)
            .Where(p => p.Kind == "Expense")
            .GroupBy(p => p.Name)
            .Select(g => new SpendingDto(g.Key, g.Sum(p => p.Amount)))
            .OrderByDescending(p => p.Amount).ThenBy(p => p.Name).ToList();
        var balance = rows.Where(r => r.Transaction.Date < start)
            .Sum(r => r.Net);
        decimal income = 0, spent = 0;
        var trend = new List<TrendDto>();
        
        for (var date = start; date < end; date = date.AddDays(1))
        {
            var daily = selected.Where(r => r.Transaction.Date.Date == date);
            income += daily.Sum(r => r.Income);
            spent += daily.Sum(r => r.Expenses);
            balance += daily.Sum(r => r.Net);
            trend.Add(new(date.ToString("yyyy-MM-dd"), balance, income, spent));
        }

        var recent = selected.OrderByDescending(r => r.Transaction.Date)
            .ThenByDescending(r => r.Transaction.Id).Take(6)
            .Select(r => new RecentTransactionDto(r.Transaction.Id,
                r.Transaction.Date.ToString("yyyy-MM-dd"),
                r.Transaction.Payee?.UserId == userId
                    ? r.Transaction.Payee.Name : "Transaction",
                Math.Abs(r.Transaction.Amount), r.Net,
                r.Parts.Select(p => p.Kind).Distinct().Count() == 1
                    ? r.Parts[0].Kind : "Mixed")).ToList();

        return new(start.ToString("yyyy-MM"), currency, balance,
            selected.Sum(r => r.Income), expenses, spending, trend, recent,
            selected.Count(r => r.Parts.Any(p => p.Name == "Uncategorized")));
    }

    private static List<Part> Parts(Transaction t, int userId)
    {
        Part Make(Category? category, decimal amount)
        {
            if (category is not null && category.UserId != userId)
            {
                throw new InvalidOperationException("Check category ownership.");
            }

            var kind = category?.CatType ?? "Expense";

            if (kind is not ("Income" or "Expense" or "Transfer"))
            {
                throw new InvalidOperationException("Check category types.");
            }

            return new(category?.Name ?? "Uncategorized", kind, Math.Abs(amount));
        }

        if (t.Splits.Count == 0)
        {
            return [Make(t.Category, t.Amount)];
        }

        if (t.Splits.Any(s => s.Amount <= 0 || s.Category is null)
            || t.Splits.Sum(s => s.Amount) != Math.Abs(t.Amount))
        {
            throw new InvalidOperationException("Check transaction splits.");
        }

        return t.Splits.Select(s => Make(s.Category, s.Amount)).ToList();
    }
}