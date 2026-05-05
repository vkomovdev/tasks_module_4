using CartService.BLL.Models;

namespace CartService.DAL.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(string cartId);
    Task SaveAsync(Cart cart);
    Task DeleteAsync(string cartId);
}
