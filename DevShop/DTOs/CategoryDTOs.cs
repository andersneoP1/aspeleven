using System.ComponentModel.DataAnnotations;

namespace DevShop.DTOs;

/// <summary>
/// DTO для чтения данных категории (GET).
/// Содержит вложенную статистику (ProductsCount).
/// </summary>
public class CategoryReadDto
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? CategoryDescription { get; set; }
    public int ProductsCount { get; set; }
}

/// <summary>
/// DTO для создания новой категории (POST).
/// </summary>
public class CategoryCreateDto
{
    [Required(ErrorMessage = "Название категории обязательно.")]
    [StringLength(100, ErrorMessage = "Длина названия категории не должна превышать 100 символов.")]
    public string CategoryName { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Длина описания категории не должна превышать 500 символов.")]
    public string? CategoryDescription { get; set; }
}

/// <summary>
/// DTO для полного обновления категории (PUT).
/// </summary>
public class CategoryUpdateDto
{
    [Required(ErrorMessage = "Название категории обязательно.")]
    [StringLength(100, ErrorMessage = "Длина названия категории не должна превышать 100 символов.")]
    public string CategoryName { get; set; } = null!;

    [Required(ErrorMessage = "Описание категории обязательно при полном обновлении.")]
    [StringLength(500, ErrorMessage = "Длина описания категории не должна превышать 500 символов.")]
    public string CategoryDescription { get; set; } = null!;
}

/// <summary>
/// DTO для частичного обновления категории (PATCH).
/// </summary>
public class PatchCategoryDTO
{
    [StringLength(100, ErrorMessage = "Длина названия категории не должна превышать 100 символов.")]
    public string? CategoryName { get; set; }

    [StringLength(500, ErrorMessage = "Длина описания категории не должна превышать 500 символов.")]
    public string? CategoryDescription { get; set; }
}

// Алиасы для обратной совместимости
public class CategoryDTO : CategoryReadDto { }
public class CreateCategoryDTO : CategoryCreateDto { }
public class UpdateCategoryDTO : CategoryUpdateDto { }
