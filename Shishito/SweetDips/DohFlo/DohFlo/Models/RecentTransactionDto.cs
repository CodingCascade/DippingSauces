namespace DohFlo.Models
{
    public sealed record RecentTransactionDto(
        int Id,
        string Date,
        string Name,
        decimal Amount,
        decimal SignedAmount,
        string Kind);
}
