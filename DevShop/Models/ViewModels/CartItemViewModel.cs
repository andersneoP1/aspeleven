namespace DevShop.Models.ViewModels;

public class CartItemViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public string? ImagePath { get; set; }
    public decimal TotalPrice => ProductPrice * Quantity;
}
