using DippingList.Models;

namespace DippingList.Services
{
    public interface IDippingListStore
    {
        IReadOnlyList<DippingItem> Load();
        void Save(IReadOnlyList<DippingItem> items);
    }
}
