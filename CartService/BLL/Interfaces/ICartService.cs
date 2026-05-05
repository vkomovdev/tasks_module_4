using CartService.BLL.Models;

namespace CartService.BLL.Interfaces;

public interface ICartService
{
    Task<IEnumerable<CartItem>> GetItemsAsync(string cartId);
    Task AddItemAsync(string cartId, CartItem item);
    Task RemoveItemAsync(string cartId, int itemId);
}
