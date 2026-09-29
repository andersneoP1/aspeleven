using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DevShop.Models;

public partial class CartItem
{
    public int IdCartItems { get; set; }

    public int? CartId { get; set; }

    public int ProductId { get; set; }

    public int CartItemQuantity { get; set; }

    public string? UserId { get; set; }

    [ValidateNever]
    public virtual Cart? Cart { get; set; }

    [ValidateNever]
    public virtual Product Product { get; set; } = null!;
}
