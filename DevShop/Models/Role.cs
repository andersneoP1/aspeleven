using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DevShop.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DevShop.Models;

public partial class Role
{
    public int IdRoles { get; set; }

    [Required(ErrorMessage = "Введите название роли!")]
    [StringLength(50, ErrorMessage = "Длина названия роли не должна превышать 50 символов.")]
    [UniqueRoleName]
    [Display(Name = "Название роли")]
    public string RoleName { get; set; } = null!;

    [ValidateNever]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
