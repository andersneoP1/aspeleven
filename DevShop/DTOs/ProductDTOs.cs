using System.ComponentModel.DataAnnotations;

namespace DevShop.DTOs;

/// <summary>
/// DTO для чтения данных о товаре/оборудовании (GET).
/// Содержит все поля сущности, включая идентификатор.
/// </summary>
public class ProductDTO
{
    /// <summary>
    /// Уникальный идентификатор товара.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Наименование товара/оборудования.
    /// </summary>
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// Подробные технические характеристики.
    /// </summary>
    public string? ProductDescription { get; set; }

    /// <summary>
    /// Стоимость товара в рублях.
    /// </summary>
    public decimal ProductPrice { get; set; }

    /// <summary>
    /// Доступное количество на складе.
    /// </summary>
    public int ProductStockQuantity { get; set; }

    /// <summary>
    /// Идентификатор категории оборудования.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// URL-адрес или путь к изображению товара.
    /// </summary>
    public string? ProductImageUrl { get; set; }
}

/// <summary>
/// DTO для создания нового товара/оборудования (POST).
/// Не содержит Id, так как он генерируется базой данных.
/// </summary>
public class CreateProductDTO
{
    /// <summary>
    /// Наименование товара.
    /// </summary>
    [Required(ErrorMessage = "Наименование товара обязательно.")]
    [StringLength(200, ErrorMessage = "Длина наименования не должна превышать 200 символов.")]
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// Описание спецификаций товара.
    /// </summary>
    public string? ProductDescription { get; set; }

    /// <summary>
    /// Стоимость товара.
    /// </summary>
    [Range(0.01, 2000000.00, ErrorMessage = "Цена товара должна быть в диапазоне от 0.01 до 2 000 000 ₽.")]
    public decimal ProductPrice { get; set; }

    /// <summary>
    /// Количество на складе.
    /// </summary>
    [Range(0, 100000, ErrorMessage = "Количество товара на складе не может быть отрицательным.")]
    public int ProductStockQuantity { get; set; }

    /// <summary>
    /// Идентификатор категории.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор категории оборудования.")]
    public int CategoryId { get; set; }

    /// <summary>
    /// Ссылка на изображение товара.
    /// </summary>
    [StringLength(500, ErrorMessage = "URL изображения не должен превышать 500 символов.")]
    public string? ProductImageUrl { get; set; }
}

/// <summary>
/// DTO для полного обновления данных о товаре (PUT).
/// Все поля обязательны к заполнению.
/// </summary>
public class UpdateProductDTO
{
    /// <summary>
    /// Полное наименование товара.
    /// </summary>
    [Required(ErrorMessage = "Наименование товара обязательно.")]
    [StringLength(200, ErrorMessage = "Длина наименования не должна превышать 200 символов.")]
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// Полное описание спецификаций товара.
    /// </summary>
    [Required(ErrorMessage = "Описание спецификаций обязательно к заполнению.")]
    public string ProductDescription { get; set; } = null!;

    /// <summary>
    /// Обновленная стоимость товара.
    /// </summary>
    [Range(0.01, 2000000.00, ErrorMessage = "Цена товара должна быть в диапазоне от 0.01 до 2 000 000 ₽.")]
    public decimal ProductPrice { get; set; }

    /// <summary>
    /// Обновленный остаток на складе.
    /// </summary>
    [Range(0, 100000, ErrorMessage = "Количество товара на складе не может быть отрицательным.")]
    public int ProductStockQuantity { get; set; }

    /// <summary>
    /// Идентификатор категории.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор категории.")]
    public int CategoryId { get; set; }

    /// <summary>
    /// URL изображения товара.
    /// </summary>
    [StringLength(500, ErrorMessage = "URL изображения не должен превышать 500 символов.")]
    public string? ProductImageUrl { get; set; }
}

/// <summary>
/// DTO для частичного обновления товара (PATCH).
/// Все поля являются nullable (обновляются только фактически переданные значения).
/// </summary>
public class PatchProductDTO
{
    /// <summary>
    /// Новое наименование товара (опционально).
    /// </summary>
    [StringLength(200, ErrorMessage = "Длина наименования не должна превышать 200 символов.")]
    public string? ProductName { get; set; }

    /// <summary>
    /// Новое описание (опционально).
    /// </summary>
    public string? ProductDescription { get; set; }

    /// <summary>
    /// Новая цена (опционально).
    /// </summary>
    [Range(0.01, 2000000.00, ErrorMessage = "Цена товара должна быть в диапазоне от 0.01 до 2 000 000 ₽.")]
    public decimal? ProductPrice { get; set; }

    /// <summary>
    /// Новый остаток на складе (опционально).
    /// </summary>
    [Range(0, 100000, ErrorMessage = "Количество товара на складе не может быть отрицательным.")]
    public int? ProductStockQuantity { get; set; }

    /// <summary>
    /// Новая категория (опционально).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор категории.")]
    public int? CategoryId { get; set; }

    /// <summary>
    /// Новый URL изображения (опционально).
    /// </summary>
    [StringLength(500, ErrorMessage = "URL изображения не должен превышать 500 символов.")]
    public string? ProductImageUrl { get; set; }
}
