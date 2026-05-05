namespace CartService.DAL.Models;

internal class CartDocument
{
    public string Id { get; set; } = string.Empty;
    public List<CartItemDocument> Items { get; set; } = new();
}

internal class CartItemDocument
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageAlt { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int Quantity { get; set; }
}
