using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DevShop.Models;

public partial class OrderItem
{
    public int IdOrderItems { get; set; }

    [Display(Name = "Заказ")]
    public int OrderId { get; set; }

    [Display(Name = "Товар")]
    public int ProductId { get; set; }

    [Display(Name = "Количество")]
    public int OrderItemQuantity { get; set; }

    [Display(Name = "Цена при покупке")]
    public decimal OrderItemPriceAtPurchase { get; set; }

    [ValidateNever]
    public virtual Order Order { get; set; } = null!;

    [ValidateNever]
    [Display(Name = "Товар")]
    public virtual Product Product { get; set; } = null!;
}
