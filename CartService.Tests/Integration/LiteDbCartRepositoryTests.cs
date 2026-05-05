using CartService.BLL.Models;
using CartService.DAL.Repositories;
using LiteDB;

namespace CartService.Tests.Integration;

public class LiteDbCartRepositoryTests : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly LiteDbCartRepository _sut;

    public LiteDbCartRepositoryTests()
    {
        _db = new LiteDatabase(new MemoryStream());
        _sut = new LiteDbCartRepository(_db);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCartDoesNotExist()
    {
        var result = await _sut.GetByIdAsync("nonexistent");
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAsync_ThenGetByIdAsync_RoundtripsAllFields()
    {
        var cart = new Cart
        {
            Id = "cart-abc",
            Items = new List<CartItem>
            {
                new()
                {
                    Id = 42,
                    Name = "Mechanical Keyboard",
                    Image = new CartItemImage { Url = "https://example.com/kb.jpg", AltText = "keyboard" },
                    Price = new Money(129.99m, "USD"),
                    Quantity = 2
                }
            }
        };

        await _sut.SaveAsync(cart);
        var loaded = await _sut.GetByIdAsync("cart-abc");

        Assert.NotNull(loaded);
        Assert.Equal("cart-abc", loaded.Id);
        Assert.Single(loaded.Items);

        var item = loaded.Items[0];
        Assert.Equal(42, item.Id);
        Assert.Equal("Mechanical Keyboard", item.Name);
        Assert.Equal(129.99m, item.Price.Amount);
        Assert.Equal("USD", item.Price.Currency);
        Assert.Equal(2, item.Quantity);
        Assert.NotNull(item.Image);
        Assert.Equal("https://example.com/kb.jpg", item.Image.Url);
        Assert.Equal("keyboard", item.Image.AltText);
    }

    [Fact]
    public async Task SaveAsync_UpdatesExistingCart_OnSecondSave()
    {
        var cart = new Cart { Id = "upd-cart", Items = new List<CartItem>() };
        await _sut.SaveAsync(cart);

        cart.Items.Add(new CartItem { Id = 1, Name = "USB Hub", Price = new Money(19m), Quantity = 1 });
        await _sut.SaveAsync(cart);

        var loaded = await _sut.GetByIdAsync("upd-cart");
        Assert.Single(loaded!.Items);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCart()
    {
        var cart = new Cart { Id = "del-cart", Items = new List<CartItem>() };
        await _sut.SaveAsync(cart);
        await _sut.DeleteAsync("del-cart");

        var result = await _sut.GetByIdAsync("del-cart");
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAsync_HandlesOptionalImageCorrectly_WhenImageIsNull()
    {
        var cart = new Cart
        {
            Id = "no-img-cart",
            Items = new List<CartItem>
            {
                new() { Id = 99, Name = "Cable", Price = new Money(5m), Quantity = 3 }
            }
        };

        await _sut.SaveAsync(cart);
        var loaded = await _sut.GetByIdAsync("no-img-cart");

        Assert.Null(loaded!.Items[0].Image);
    }

    public void Dispose() => _db.Dispose();
}
