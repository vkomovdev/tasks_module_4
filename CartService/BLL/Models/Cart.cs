namespace CartService.BLL.Models;

public class Cart
{
    public string Id { get; set; } = string.Empty;

    public List<CartItem> Items { get; set; } = new();
}
