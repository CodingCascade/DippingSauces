namespace DohFlo.Models
{
    public sealed record DashboardDto(
        string Month,
        string Currency,
        decimal RecordedBalance,
        decimal Income,
        decimal Expenses,
        List<SpendingDto> Spending,
        List<TrendDto> Trend,
        List<RecentTransactionDto> Recent,
        int UnclassifiedCount);
}
