namespace DohFlo.Models
{
    public sealed record TrendDto(
        string Date,
        decimal Balance,
        decimal Income,
        decimal Expenses);
}
