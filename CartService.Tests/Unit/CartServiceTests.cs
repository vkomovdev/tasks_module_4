using CartService.BLL.Models;
using CartService.BLL.Services;
using CartService.DAL.Interfaces;
using Moq;

namespace CartService.Tests.Unit;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _repoMock = new();
    private readonly CartService.BLL.Services.CartService _sut;

    public CartServiceTests()
    {
        _sut = new CartService.BLL.Services.CartService(_repoMock.Object);
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsEmptyEnumerable_WhenCartDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync("unknown")).ReturnsAsync((Cart?)null);

        var result = await _sut.GetItemsAsync("unknown");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsItems_WhenCartExists()
    {
        var cart = new Cart
        {
            Id = "cart-1",
            Items = new List<CartItem>
            {
                new() { Id = 10, Name = "Laptop", Price = new Money(999.99m), Quantity = 1 }
            }
        };
        _repoMock.Setup(r => r.GetByIdAsync("cart-1")).ReturnsAsync(cart);

        var result = (await _sut.GetItemsAsync("cart-1")).ToList();

        Assert.Single(result);
        Assert.Equal(10, result[0].Id);
        Assert.Equal("Laptop", result[0].Name);
    }

    [Fact]
    public async Task AddItemAsync_CreatesNewCart_WhenCartDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync("new-cart")).ReturnsAsync((Cart?)null);

        var item = new CartItem { Id = 1, Name = "Mouse", Price = new Money(25m), Quantity = 1 };
        await _sut.AddItemAsync("new-cart", item);

        _repoMock.Verify(r => r.SaveAsync(It.Is<Cart>(c =>
            c.Id == "new-cart" &&
            c.Items.Count == 1 &&
            c.Items[0].Id == 1)), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_IncrementsQuantity_WhenItemAlreadyInCart()
    {
        var existingCart = new Cart
        {
            Id = "cart-1",
            Items = new List<CartItem>
            {
                new() { Id = 5, Name = "Keyboard", Price = new Money(59m), Quantity = 1 }
            }
        };
        _repoMock.Setup(r => r.GetByIdAsync("cart-1")).ReturnsAsync(existingCart);

        await _sut.AddItemAsync("cart-1", new CartItem { Id = 5, Name = "Keyboard", Price = new Money(59m), Quantity = 2 });

        _repoMock.Verify(r => r.SaveAsync(It.Is<Cart>(c =>
            c.Items.Single(i => i.Id == 5).Quantity == 3)), Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_DoesNothing_WhenCartDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync("ghost")).ReturnsAsync((Cart?)null);

        await _sut.RemoveItemAsync("ghost", 99);

        _repoMock.Verify(r => r.SaveAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemAsync_RemovesItem_WhenItemExists()
    {
        var cart = new Cart
        {
            Id = "cart-2",
            Items = new List<CartItem>
            {
                new() { Id = 3, Name = "Monitor", Price = new Money(299m), Quantity = 1 },
                new() { Id = 7, Name = "Webcam", Price = new Money(49m), Quantity = 1 }
            }
        };
        _repoMock.Setup(r => r.GetByIdAsync("cart-2")).ReturnsAsync(cart);

        await _sut.RemoveItemAsync("cart-2", 3);

        _repoMock.Verify(r => r.SaveAsync(It.Is<Cart>(c =>
            c.Items.Count == 1 &&
            c.Items[0].Id == 7)), Times.Once);
    }
}
