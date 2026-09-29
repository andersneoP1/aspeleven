using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DevShop.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DevShop.Models;

public partial class User
{
    public int IdUsers { get; set; }

    [Required(ErrorMessage = "Введите логин!")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 50 символов.")]
    [UniqueUserLogin]
    [Display(Name = "Логин")]
    public string UserLogin { get; set; } = null!;

    [Display(Name = "Хеш пароля")]
    public string UserPasswordHash { get; set; } = null!;

    [Display(Name = "Роль")]
    public int RoleId { get; set; }

    [Display(Name = "Дата регистрации")]
    public DateTime? UserCreatedAt { get; set; }

    [ValidateNever]
    public virtual Cart? Cart { get; set; }

    [ValidateNever]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [ValidateNever]
    [Display(Name = "Роль")]
    public virtual Role Role { get; set; } = null!;
}
