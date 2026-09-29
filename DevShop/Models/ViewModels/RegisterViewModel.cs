using System.ComponentModel.DataAnnotations;
using DevShop.Attributes;

namespace DevShop.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите логин.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен содержать от 3 до 50 символов.")]
    [NoHtmlTags(ErrorMessage = "Логин не должен содержать HTML-теги.")]
    [Display(Name = "Логин разработчика")]
    public string Login { get; set; } = null!;

    [Required(ErrorMessage = "Введите пароль.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть длиной не менее 6 символов.")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Повторите пароль для подтверждения.")]
    [DataType(DataType.Password)]
    [Display(Name = "Подтверждение пароля")]
    [Compare("Password", ErrorMessage = "Введенные пароли не совпадают.")]
    public string ConfirmPassword { get; set; } = null!;
}
