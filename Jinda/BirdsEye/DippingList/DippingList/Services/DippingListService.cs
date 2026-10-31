using DippingList.Models;

namespace DippingList.Services
{
    public sealed class DippingListService
    {
        private readonly object _gate = new();
        private List<DippingItem> _items = new();

        public IReadOnlyList<DippingItem> GetAll()
        {
            lock (_gate)
            {
                return _items.ToArray();
            }
        }

        private void Commit(List<DippingItem> next)
        {
            _items = next;
        }

    }
}
