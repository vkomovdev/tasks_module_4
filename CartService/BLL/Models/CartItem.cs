using System.ComponentModel.DataAnnotations;

namespace CartService.BLL.Models;

public class CartItem
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public CartItemImage? Image { get; set; }

    [Required]
    public Money Price { get; set; } = new(0);

    public int Quantity { get; set; }
}
