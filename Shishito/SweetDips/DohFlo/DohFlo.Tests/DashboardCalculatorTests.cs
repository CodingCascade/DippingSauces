using DohFlo.Data;
using DohFlo.Services;
using Xunit;

namespace DohFlo.Tests;

public class DashboardCalculatorTests
{
    private static readonly DateTime Start = new(2026, 10, 1);
    private static Category Category(string kind, string name = "Food") =>
        new() { Id = 1, UserId = 1, CatType = kind, Name = name };
    private static Transaction Row(int id, decimal amount, string kind,
        DateTime? date = null) => new()
        {
            Id = id,
            UserId = 1,
            Amount = amount,
            CurrencyCode = "USD",
            Date = date ?? Start,
            Account = new() { UserId = 1 },
            Category = Category(kind),
            Status = TransactionStatus.Pending
        };

    [Fact]
    public void TotalsHonorDatesCurrencyDeletionAndOwnership()
    {
        var earlier = Row(1, 100, "Income", Start.AddDays(-1));
        var salary = Row(2, 200, "Income");
        var food = Row(3, 30, "Expense");
        var transfer = Row(4, 50, "Transfer");
        var deleted = Row(5, 999, "Expense"); deleted.IsDeleted = true;
        var otherUser = Row(6, 999, "Expense"); otherUser.UserId = 2;
        var otherCurrency = Row(7, 999, "Expense"); otherCurrency.CurrencyCode = "THB";
        var nextMonth = Row(8, 999, "Expense", Start.AddMonths(1));
        var foreignAccount = Row(9, 999, "Expense"); foreignAccount.Account!.UserId = 2;
        var result = DashboardCalculator.Build([
            earlier, salary, food, transfer, deleted, otherUser,
            otherCurrency, nextMonth, foreignAccount], 1, Start, "USD");
        Assert.Equal(200m, result.Income);
        Assert.Equal(30m, result.Expenses);
        Assert.Equal(270m, result.RecordedBalance);
        Assert.Equal(3, result.Recent.Count);
        Assert.Equal(31, result.Trend.Count);
        Assert.Equal(result.Expenses, result.Spending.Sum(s => s.Amount));
        Assert.Equal(result.RecordedBalance, result.Trend.Last().Balance);
    }

    [Fact]
    public void SplitsReplaceTheMainCategoryWithoutDoubleCounting()
    {
        var row = Row(1, 100, "Expense");
        row.Splits = [
            new() { Amount = 40, Category = Category("Expense", "Housing") },
            new() { Amount = 60, Category = Category("Expense", "Food") }
        ];
        var result = DashboardCalculator.Build([row], 1, Start, "USD");
        Assert.Equal(100m, result.Expenses);
        Assert.Equal(2, result.Spending.Count);
        Assert.Equal(-100m, result.RecordedBalance);
    }

    [Fact]
    public void EmptyDataProducesZerosAndNoSpendingSlices()
    {
        var result = DashboardCalculator.Build([], 1, Start, "USD");
        Assert.Equal(0m, result.RecordedBalance);
        Assert.Empty(result.Spending); Assert.Empty(result.Recent);
    }

    [Fact]
    public void InvalidSplitTotalIsRejected()
    {
        var row = Row(1, 100, "Expense");
        row.Splits = [new() { Amount = 90, Category = Category("Expense") }];
        Assert.Throws<InvalidOperationException>(() =>
            DashboardCalculator.Build([row], 1, Start, "USD"));
    }

    [Fact]
    public void ForeignCategoryIsRejected()
    {
        var row = Row(1, 100, "Expense"); row.Category!.UserId = 2;
        Assert.Throws<InvalidOperationException>(() =>
            DashboardCalculator.Build([row], 1, Start, "USD"));
    }
}