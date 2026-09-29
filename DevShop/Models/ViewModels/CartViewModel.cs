using System.Collections.Generic;
using System.Linq;

namespace DevShop.Models.ViewModels;

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();

    public decimal GrandTotal => Items.Sum(i => i.TotalPrice);

    public int TotalCount => Items.Sum(i => i.Quantity);
}
