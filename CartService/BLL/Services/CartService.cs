using CartService.BLL.Interfaces;
using CartService.BLL.Models;
using CartService.DAL.Interfaces;

namespace CartService.BLL.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _repository;

    public CartService(ICartRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CartItem>> GetItemsAsync(string cartId)
    {
        var cart = await _repository.GetByIdAsync(cartId);
        return cart?.Items ?? Enumerable.Empty<CartItem>();
    }

    public async Task AddItemAsync(string cartId, CartItem item)
    {
        var cart = await _repository.GetByIdAsync(cartId) ?? new Cart { Id = cartId };

        var existing = cart.Items.FirstOrDefault(i => i.Id == item.Id);
        if (existing is not null)
            existing.Quantity += item.Quantity;
        else
            cart.Items.Add(item);

        await _repository.SaveAsync(cart);
    }

    public async Task RemoveItemAsync(string cartId, int itemId)
    {
        var cart = await _repository.GetByIdAsync(cartId);
        if (cart is null) return;

        cart.Items.RemoveAll(i => i.Id == itemId);
        await _repository.SaveAsync(cart);
    }
}
