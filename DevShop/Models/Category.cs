using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DevShop.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DevShop.Models;

public partial class Category
{
    public int IdCategories { get; set; }

    [Required(ErrorMessage = "Введите название категории!")]
    [StringLength(100, ErrorMessage = "Название категории не может превышать 100 символов.")]
    [NoHtmlTags(ErrorMessage = "Название не должно содержать HTML-теги.")]
    [UniqueCategoryName]
    [Display(Name = "Название категории")]
    public string CategoryName { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов.")]
    [NoHtmlTags(ErrorMessage = "Описание не должно содержать HTML-теги.")]
    [Display(Name = "Описание")]
    public string? CategoryDescription { get; set; }

    [ValidateNever]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
