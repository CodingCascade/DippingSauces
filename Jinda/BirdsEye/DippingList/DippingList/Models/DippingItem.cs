namespace DippingList.Models
{
    public sealed class DippingItem
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public bool IsPurchased { get; init; }
    }
}
