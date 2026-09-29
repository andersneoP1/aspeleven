using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using DevShop.Attributes;

namespace DevShop.Models;

[PriceStockDependency(100000, 5)]
public partial class Product
{
    [Display(Name = "Идентификатор")]
    public int IdProducts { get; set; }

    [Required(ErrorMessage = "Введите наименование девайса/оборудования!")]
    [StringLength(200, ErrorMessage = "Длина наименования не должна превышать 200 символов.")]
    [NoHtmlTags(ErrorMessage = "Наименование не должно содержать HTML-теги.")]
    [ForbiddenWords("fake", "scam", "подделка", "майнинг-б/у", "реплика", "noname", ErrorMessage = "В наименовании обнаружены недопустимые или сомнительные термины.")]
    [UniqueProductName]
    [Display(Name = "Наименование товара")]
    public string ProductName { get; set; } = null!;

    [Required(ErrorMessage = "Введите описание технических характеристик.")]
    [MinWords(3, ErrorMessage = "Описание слишком короткое. Расскажите о спецификациях девайса подробнее (минимум 3 слова).")]
    [NoHtmlTags(ErrorMessage = "Описание не должно содержать HTML-теги.")]
    [Display(Name = "Технические характеристики")]
    public string? ProductDescription { get; set; }

    [Required(ErrorMessage = "Укажите стоимость товара.")]
    [Range(0.01, 2000000, ErrorMessage = "Укажите корректную стоимость от 0.01 ₽ до 2 000 000 ₽.")]
    [Display(Name = "Цена (₽)")]
    public decimal ProductPrice { get; set; }

    [Required(ErrorMessage = "Укажите количество на складе.")]
    [Range(0, 100000, ErrorMessage = "Количество на складе не может быть отрицательным.")]
    [Display(Name = "Остаток на складе")]
    public int ProductStockQuantity { get; set; }

    [Required(ErrorMessage = "Выберите категорию девайса.")]
    [Range(1, int.MaxValue, ErrorMessage = "Выберите корректную категорию из справочника.")]
    [Display(Name = "Категория")]
    public int CategoryId { get; set; }

    [Display(Name = "Изображение")]
    public string? ProductImageUrl { get; set; }

    [NotMapped]
    [AllowedExtensions(".jpg,.jpeg,.png,.webp,.gif", ErrorMessage = "Разрешены только графические форматы: JPG, JPEG, PNG, WEBP, GIF")]
    [MaxFileSize(2, ErrorMessage = "Максимальный объем загружаемого файла - 2 МБ.")]
    [Display(Name = "Загрузить фото")]
    public IFormFile? ImageFile { get; set; }

    [ValidateNever]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [ValidateNever]
    [Display(Name = "Категория")]
    public virtual Category Category { get; set; } = null!;

    [ValidateNever]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
