using DippingList.Services;
using Xunit;

namespace DippingList.Tests;

public class DippingListServiceTests
{
    [Fact]
    public void Add_TrimsAndStartsUnpurchased()
    {
        var service = new DippingListService();

        Guid id = service.Add("  Milk  ");

        var item = Assert.Single(service.GetAll());
        Assert.Equal("Milk", item.Name);
        Assert.Equal(id, item.Id);
        Assert.NotEqual(Guid.Empty, id);
        Assert.False(item.IsPurchased);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void Add_RejectsBlankWithoutChangingList(string? name)
    {
        var service = new DippingListService();
        Guid existing = service.Add("Eggs");

        Assert.Throws<ArgumentException>(() => service.Add(name));

        Assert.Equal(existing, Assert.Single(service.GetAll()).Id);
    }

    [Fact]
    public void Add_ChecksLengthAfterTrimming()
    {
        var service = new DippingListService();
        string accepted = new string('a', 80);

        service.Add(" " + accepted + " ");
        Assert.Throws<ArgumentException>(
            () => service.Add(new string('b', 81)));

        Assert.Equal(accepted, Assert.Single(service.GetAll()).Name);
    }

    [Fact]
    public void Add_AllowsDuplicatesWithDifferentIdsInOrder()
    {
        var service = new DippingListService();

        Guid first = service.Add("Milk");
        Guid second = service.Add("Milk");

        Assert.NotEqual(first, second);
        Assert.Equal(new[] { first, second },
            service.GetAll().Select(x => x.Id).ToArray());
    }

    [Fact]
    public void SetPurchased_UsesIdAndSetsBothStates()
    {
        var service = new DippingListService();
        Guid firstMilkId = service.Add("Milk");
        Guid secondMilkId = service.Add("Milk");

        Assert.True(service.SetPurchased(secondMilkId, true));
        Assert.True(service.SetPurchased(secondMilkId, true));
        Assert.False(service.GetAll().Single(
            x => x.Id == firstMilkId).IsPurchased);
        Assert.True(service.GetAll().Single(
            x => x.Id == secondMilkId).IsPurchased);

        Assert.True(service.SetPurchased(secondMilkId, false));
        Assert.False(service.GetAll().Single(
            x => x.Id == secondMilkId).IsPurchased);
    }

    [Fact]
    public void Remove_DeletesOnlyRequestedDuplicate()
    {
        var service = new DippingListService();
        Guid first = service.Add("Milk");
        Guid second = service.Add("Milk");

        bool removed = service.Remove(second);

        Assert.True(removed);
        Assert.Equal(first, Assert.Single(service.GetAll()).Id);
    }

    [Fact]
    public void MissingIds_DoNotChangeExistingItem()
    {
        var service = new DippingListService();
        Guid existing = service.Add("Bread");
        Guid missing = Guid.NewGuid();

        Assert.False(service.Remove(missing));
        Assert.False(service.SetPurchased(missing, true));

        var item = Assert.Single(service.GetAll());
        Assert.Equal(existing, item.Id);
        Assert.Equal("Bread", item.Name);
        Assert.False(item.IsPurchased);
    }
}
