using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using DevShop.Models;

namespace DevShop.Attributes;

/// <summary>
/// Проверка зависимости остатка на складе от стоимости товара.
/// Для премиального оборудования и кастомных рабочих станций остаток строго лимитирован.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PriceStockDependencyAttribute : ValidationAttribute
{
    private readonly decimal _luxuryThreshold;
    private readonly int _maxStockForLuxury;

    public PriceStockDependencyAttribute(double luxuryThreshold, int maxStockForLuxury)
    {
        _luxuryThreshold = (decimal)luxuryThreshold;
        _maxStockForLuxury = maxStockForLuxury;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is Product product)
        {
            if (product.ProductPrice >= _luxuryThreshold && product.ProductStockQuantity > _maxStockForLuxury)
            {
                return new ValidationResult(
                    ErrorMessage ?? $"Флагманские устройства (цена от {_luxuryThreshold:N0} ₽) поставляются ограниченной партией. Остаток на складе не может превышать {_maxStockForLuxury} шт.");
            }
        }
        return ValidationResult.Success;
    }
}

/// <summary>
/// Защита от XSS-инъекций: запрет любых HTML-тегов в строковых полях.
/// </summary>
public class NoHtmlTagsAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string str && !string.IsNullOrWhiteSpace(str))
        {
            if (Regex.IsMatch(str, @"<[^>]+>"))
            {
                return new ValidationResult(ErrorMessage ?? "Поле не должно содержать HTML-теги в целях безопасности.");
            }
        }
        return ValidationResult.Success;
    }
}

/// <summary>
/// Запрет использования нежелательных слов (спам, скам, некачественный товар, реплики).
/// </summary>
public class ForbiddenWordsAttribute : ValidationAttribute
{
    private readonly string[] _forbiddenWords;

    public ForbiddenWordsAttribute(params string[] forbiddenWords)
    {
        _forbiddenWords = forbiddenWords;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string str && !string.IsNullOrWhiteSpace(str))
        {
            var lower = str.ToLower();
            foreach (var word in _forbiddenWords)
            {
                if (lower.Contains(word.ToLower()))
                {
                    return new ValidationResult(ErrorMessage ?? $"Обнаружено недопустимое слово: '{word}'. Товар должен соответствовать стандартам качества DevHub.");
                }
            }
        }
        return ValidationResult.Success;
    }
}

/// <summary>
/// Проверка уникальности наименования товара в каталоге магазина.
/// </summary>
public class UniqueProductNameAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string name)
        {
            var dbContext = (ShopDbContext?)validationContext.GetService(typeof(ShopDbContext));
            if (dbContext != null)
            {
                var currentProduct = (Product)validationContext.ObjectInstance;
                bool exists = dbContext.Products.Any(p => 
                    p.ProductName.ToLower() == name.ToLower() && 
                    p.IdProducts != currentProduct.IdProducts);

                if (exists)
                {
                    return new ValidationResult(ErrorMessage ?? "Товар с таким наименованием уже зарегистрирован в каталоге.");
                }
            }
        }
        return ValidationResult.Success;
    }
}

/// <summary>
/// Минимальное количество слов в описании товара для подробного описания характеристик.
/// </summary>
public class MinWordsAttribute : ValidationAttribute
{
    private readonly int _minWords;

    public MinWordsAttribute(int minWords)
    {
        _minWords = minWords;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string text && !string.IsNullOrWhiteSpace(text))
        {
            var words = text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < _minWords)
            {
                return new ValidationResult(ErrorMessage ?? $"Описание слишком краткое. Укажите ключевые спецификации (минимум {_minWords} слов).");
            }
        }
        return ValidationResult.Success;
    }
}

/// <summary>
/// Валидация разрешенных расширений загружаемого медиафайла изображения товара.
/// </summary>
public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public AllowedExtensionsAttribute(string extensions)
    {
        _extensions = extensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim().ToLower())
                                .ToArray();
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            var extension = System.IO.Path.GetExtension(file.FileName).ToLower();
            if (!_extensions.Contains(extension))
            {
                return new ValidationResult(ErrorMessage ?? $"Разрешены только графические форматы: {string.Join(", ", _extensions)}");
            }
        }
        return ValidationResult.Success;
    }
}

/// <summary>
/// Валидация максимального размера файла изображения.
/// </summary>
public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSizeMb;

    public MaxFileSizeAttribute(int maxFileSizeInMb)
    {
        _maxFileSizeMb = maxFileSizeInMb;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            if (file.Length > _maxFileSizeMb * 1024 * 1024)
            {
                return new ValidationResult(ErrorMessage ?? $"Превышен лимит размера файла. Максимум: {_maxFileSizeMb} МБ.");
            }
        }
        return ValidationResult.Success;
    }
}

/// <summary>
/// Проверка минимального возраста пользователя при регистрации.
/// </summary>
public class MinimumAgeAttribute : ValidationAttribute
{
    private readonly int _minimumAge;

    public MinimumAgeAttribute(int minimumAge)
    {
        _minimumAge = minimumAge;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateOfBirth)
        {
            if (dateOfBirth.AddYears(_minimumAge) > DateTime.Now)
            {
                return new ValidationResult(ErrorMessage ?? $"Регистрация доступна лицам не моложе {_minimumAge} лет.");
            }
        }
        return ValidationResult.Success;
    }
}

/// <summary>
/// Валидация артикула оборудования (SKU) по стандарту DEV-00000.
/// </summary>
public class SkuAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string sku && !string.IsNullOrWhiteSpace(sku))
        {
            if (!Regex.IsMatch(sku, @"^[A-Z]{3}-\d{5}$"))
            {
                return new ValidationResult(ErrorMessage ?? "Артикул должен соответствовать формату 'DEV-00000' (3 латинские буквы, дефис, 5 цифр).");
            }
        }
        return ValidationResult.Success;
    }
}

/// <summary>
/// Условная обязательность заполнения поля.
/// </summary>
public class RequiredIfAttribute : ValidationAttribute
{
    private readonly string _otherProperty;
    private readonly object _targetValue;

    public RequiredIfAttribute(string otherProperty, object targetValue)
    {
        _otherProperty = otherProperty;
        _targetValue = targetValue;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(_otherProperty);
        if (property == null)
            return new ValidationResult($"Свойство {_otherProperty} не найдено.");

        var otherValue = property.GetValue(validationContext.ObjectInstance);

        if (otherValue?.ToString() == _targetValue.ToString())
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(ErrorMessage ?? $"Поле обязательно для заполнения при значении {_otherProperty} = {_targetValue}.");
            }
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Проверка уникальности логина пользователя при регистрации и редактировании.
/// </summary>
public class UniqueUserLoginAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string login)
        {
            var dbContext = (ShopDbContext?)validationContext.GetService(typeof(ShopDbContext));
            if (dbContext != null)
            {
                var currentUser = (User)validationContext.ObjectInstance;
                bool loginExists = dbContext.Users.Any(u => 
                    u.UserLogin.ToLower() == login.ToLower() && 
                    u.IdUsers != currentUser.IdUsers);

                if (loginExists)
                {
                    return new ValidationResult(ErrorMessage ?? "Пользователь с таким логином уже существует в системе.");
                }
            }
        }
        return ValidationResult.Success;
    }
}

/// <summary>
/// Проверка уникальности названия категории.
/// </summary>
public class UniqueCategoryNameAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string name)
        {
            var dbContext = (ShopDbContext?)validationContext.GetService(typeof(ShopDbContext));
            if (dbContext != null)
            {
                var currentCategory = (Category)validationContext.ObjectInstance;
                bool nameExists = dbContext.Categories.Any(c => 
                    c.CategoryName.ToLower() == name.ToLower() && 
                    c.IdCategories != currentCategory.IdCategories);

                if (nameExists)
                {
                    return new ValidationResult(ErrorMessage ?? "Категория оборудования с таким названием уже существует.");
                }
            }
        }
        return ValidationResult.Success;
    }
}

/// <summary>
/// Проверка уникальности названия роли.
/// </summary>
public class UniqueRoleNameAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string name)
        {
            var dbContext = (ShopDbContext?)validationContext.GetService(typeof(ShopDbContext));
            if (dbContext != null)
            {
                var currentRole = (Role)validationContext.ObjectInstance;
                bool nameExists = dbContext.Roles.Any(r => 
                    r.RoleName.ToLower() == name.ToLower() && 
                    r.IdRoles != currentRole.IdRoles);

                if (nameExists)
                {
                    return new ValidationResult(ErrorMessage ?? "Системная роль с таким наименованием уже существует.");
                }
            }
        }
        return ValidationResult.Success;
    }
}
