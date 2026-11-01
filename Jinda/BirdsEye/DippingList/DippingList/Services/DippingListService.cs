using DippingList.Models;

namespace DippingList.Services
{
    public sealed class DippingListService
    {
        private readonly object _gate = new();
        private List<DippingItem> _items = new();
        private readonly IDippingListStore? _store;

        public IReadOnlyList<DippingItem> GetAll()
        {
            lock (_gate)
            {
                return _items.ToArray();
            }
        }

        public DippingListService(IDippingListStore? store = null)
        {
            _store = store;
            _items = store?.Load().ToList() ?? new List<DippingItem>();
        }

        /// <summary>
        /// Adds a new dipping item to the list.
        /// </summary>
        /// <param name="name">The item name (required, will be trimmed, max 80 chars after trim).</param>
        /// <returns>The Id of the newly created item.</returns>
        /// <exception cref="ArgumentException">Thrown if name is blank or exceeds 80 characters after trimming.</exception>
        public Guid Add(string? name)
        {
            // Validate before locking
            // Assumption: Validation before lock reduces lock contention; exceptions are fast-path failures
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Enter an item name.", nameof(name));
            }

            var cleanedName = name.Trim();

            // Assumption: 80-character limit is a reasonable constraint for UI/display
            if (cleanedName.Length > 80)
            {
                throw new ArgumentException("Use 80 characters or fewer.", nameof(name));
            }

            lock (_gate)
            {
                // Create new item with auto-generated Id, trimmed name, and unpurchased state
                // Assumption: Guid.NewGuid() is unique enough for this use case (no persistence guarantees needed)
                var item = new DippingItem
                {
                    Id = Guid.NewGuid(),
                    Name = cleanedName,           // Store trimmed version to prevent inconsistencies
                    IsPurchased = false            // All new items start unpurchased
                };

                // Build new list with existing items + new item
                // Assumption: ToList() creates a shallow copy, preserving insertion order
                var next = _items.ToList();
                next.Add(item);                   // Maintains insertion order (append to end)

                // Atomically replace old list with new one
                Commit(next);

                return item.Id;
            }
        }

        public bool SetPurchased(Guid id, bool purchased)
        {
            lock (_gate)
            {
                int index = _items.FindIndex(x => x.Id == id);

                if (index < 0)
                {
                    return false;
                }

                var current = _items[index];
                var next = _items.ToList();

                next[index] = new DippingItem
                {
                    Id = current.Id,
                    Name = current.Name,
                    IsPurchased = purchased
                };

                Commit(next);
                return true;
            }
        }

        public bool Remove(Guid id)
        {
            lock (_gate)
            {
                var next = _items.Where(x => x.Id != id).ToList();

                if (next.Count == _items.Count)
                {
                    return false;
                }

                Commit(next);
                return true;
            }
        }

        private void Commit(List<DippingItem> next)
        {
            _store?.Save(next);
            _items = next;
        }

    }
}
