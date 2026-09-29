using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DevShop.Models;

public partial class Cart
{
    public int IdCarts { get; set; }

    public int? UserId { get; set; }

    public DateTime? CartCreatedAt { get; set; }

    [ValidateNever]
    public virtual User? User { get; set; }

    [ValidateNever]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
