using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DevShop.Models;

public partial class Order
{
    [Display(Name = "Номер заказа")]
    public int IdOrders { get; set; }

    [Display(Name = "Пользователь")]
    public int UserId { get; set; }

    [Display(Name = "Дата создания")]
    public DateTime? OrderDate { get; set; }

    [Display(Name = "Итоговая сумма")]
    [DisplayFormat(DataFormatString = "{0:N2} ₽")]
    public decimal OrderTotalAmount { get; set; }

    [Display(Name = "Статус")]
    public string? OrderStatus { get; set; }

    [ValidateNever]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [ValidateNever]
    [Display(Name = "Клиент")]
    public virtual User User { get; set; } = null!;
}
