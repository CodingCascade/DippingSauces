namespace DohFlo.Models.Api
{
    public sealed record AccountDto(int Id, string Name, string Type, string Institution, string CurrencyCode, bool IsClosed);
}
