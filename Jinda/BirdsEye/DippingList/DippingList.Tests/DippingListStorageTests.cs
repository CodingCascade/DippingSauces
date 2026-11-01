using DippingList.Models;
using DippingList.Services;
using Xunit;

namespace DippingList.Tests;

public class DippingListStorageTests
{
    [Fact]
    public void Restart_RestoresIdNameOrderAndPurchasedState()
    {
        string folder = NewFolder();
        try
        {
            string path = Path.Combine(folder, "list.json");
            var firstRun = new DippingListService(
                new JsonDippingListStore(path));
            Guid milk = firstRun.Add("Milk");
            Guid eggs = firstRun.Add("Eggs");
            firstRun.SetPurchased(milk, true);

            var secondRun = new DippingListService(
                new JsonDippingListStore(path));
            var restored = secondRun.GetAll();

            Assert.Equal(new[] { milk, eggs },
                restored.Select(x => x.Id).ToArray());
            Assert.Equal("Milk", restored[0].Name);
            Assert.True(restored[0].IsPurchased);
            Assert.Equal("Eggs", restored[1].Name);
            Assert.False(restored[1].IsPurchased);

            secondRun.Remove(milk);
            var thirdRun = new DippingListService(
                new JsonDippingListStore(path));
            Assert.Equal(eggs,
                Assert.Single(thirdRun.GetAll()).Id);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void MissingFile_StartsEmpty()
    {
        string folder = NewFolder();
        try
        {
            var store = new JsonDippingListStore(
                Path.Combine(folder, "missing.json"));
            Assert.Empty(store.Load());
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Theory]
    [InlineData("{broken")]
    [InlineData("null")]
    [InlineData("[null]")]
    [InlineData("[{}]")]
    public void BadFile_IsRejectedAndNotOverwritten(string content)
    {
        string folder = NewFolder();
        try
        {
            string path = Path.Combine(folder, "list.json");
            File.WriteAllText(path, content);
            var store = new JsonDippingListStore(path);

            Assert.Throws<InvalidDataException>(() =>
            {
                _ = new DippingListService(store);
            });
            Assert.Equal(content, File.ReadAllText(path));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void FailedSaves_LeaveMemoryUnchanged()
    {
        Guid id = Guid.NewGuid();
        var service = new DippingListService(
            new FailingStore(id));

        Assert.Throws<IOException>(() => service.Add("Eggs"));
        Assert.Throws<IOException>(() =>
            service.SetPurchased(id, true));
        Assert.Throws<IOException>(() => service.Remove(id));

        var item = Assert.Single(service.GetAll());
        Assert.Equal(id, item.Id);
        Assert.Equal("Milk", item.Name);
        Assert.False(item.IsPurchased);
    }

    private static string NewFolder()
    {
        string folder = Path.Combine(Path.GetTempPath(),
            "DippingListTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    private sealed class FailingStore : IDippingListStore
    {
        private readonly Guid _id;

        public FailingStore(Guid id)
        {
            _id = id;
        }

        public IReadOnlyList<DippingItem> Load()
        {
            return new[]
            {
                new DippingItem { Id = _id, Name = "Milk" }
            };
        }

        public void Save(IReadOnlyList<DippingItem> items)
        {
            throw new IOException("Simulated save failure.");
        }
    }
}
