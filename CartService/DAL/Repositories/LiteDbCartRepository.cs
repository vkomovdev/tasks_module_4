using CartService.BLL.Models;
using CartService.DAL.Interfaces;
using CartService.DAL.Models;
using LiteDB;

namespace CartService.DAL.Repositories;

public class LiteDbCartRepository : ICartRepository
{
    private readonly ILiteCollection<CartDocument> _collection;

    public LiteDbCartRepository(ILiteDatabase database)
    {
        _collection = database.GetCollection<CartDocument>("carts");
        _collection.EnsureIndex(x => x.Id, unique: true);
    }

    public Task<Cart?> GetByIdAsync(string cartId)
    {
        var doc = _collection.FindById(cartId);
        return Task.FromResult(doc is null ? null : MapToDomain(doc));
    }

    public Task SaveAsync(Cart cart)
    {
        _collection.Upsert(MapToDocument(cart));
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string cartId)
    {
        _collection.Delete(cartId);
        return Task.CompletedTask;
    }

    private static Cart MapToDomain(CartDocument doc) => new()
    {
        Id = doc.Id,
        Items = doc.Items.Select(i => new CartItem
        {
            Id = i.Id,
            Name = i.Name,
            Image = i.ImageUrl is not null
                ? new CartItemImage { Url = i.ImageUrl, AltText = i.ImageAlt ?? string.Empty }
                : null,
            Price = new Money(i.Price, i.Currency),
            Quantity = i.Quantity
        }).ToList()
    };

    private static CartDocument MapToDocument(Cart cart) => new()
    {
        Id = cart.Id,
        Items = cart.Items.Select(i => new CartItemDocument
        {
            Id = i.Id,
            Name = i.Name,
            ImageUrl = i.Image?.Url,
            ImageAlt = i.Image?.AltText,
            Price = i.Price.Amount,
            Currency = i.Price.Currency,
            Quantity = i.Quantity
        }).ToList()
    };
}
