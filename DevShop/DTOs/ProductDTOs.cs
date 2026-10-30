using System.ComponentModel.DataAnnotations;

namespace DevShop.DTOs;

/// <summary>
/// DTO для чтения данных о товаре (GET).
/// Включает связанные данные (название категории).
/// </summary>
public class ProductReadDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductDescription { get; set; }
    public decimal ProductPrice { get; set; }
    public int ProductStockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? ProductImageUrl { get; set; }
}

/// <summary>
/// DTO для создания нового товара (POST).
/// Содержит только Id связанных сущностей (CategoryId).
/// </summary>
public class ProductCreateDto
{
    [Required(ErrorMessage = "Название товара обязательно.")]
    [StringLength(200, ErrorMessage = "Длина названия не должна превышать 200 символов.")]
    public string ProductName { get; set; } = null!;

    public string? ProductDescription { get; set; }

    [Range(0.01, 1000000.00, ErrorMessage = "Цена товара должна быть в диапазоне от 0.01 до 1 000 000.")]
    public decimal ProductPrice { get; set; }

    [Range(0, 100000, ErrorMessage = "Количество товара на складе не может быть отрицательным.")]
    public int ProductStockQuantity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор категории.")]
    public int CategoryId { get; set; }

    [StringLength(500, ErrorMessage = "URL изображения не должен превышать 500 символов.")]
    public string? ProductImageUrl { get; set; }
}

/// <summary>
/// DTO для полного обновления товара (PUT).
/// </summary>
public class ProductUpdateDto
{
    [Required(ErrorMessage = "Название товара обязательно.")]
    [StringLength(200, ErrorMessage = "Длина названия не должна превышать 200 символов.")]
    public string ProductName { get; set; } = null!;

    public string? ProductDescription { get; set; }

    [Range(0.01, 1000000.00, ErrorMessage = "Цена товара должна быть в диапазоне от 0.01 до 1 000 000.")]
    public decimal ProductPrice { get; set; }

    [Range(0, 100000, ErrorMessage = "Количество товара на складе не может быть отрицательным.")]
    public int ProductStockQuantity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор категории.")]
    public int CategoryId { get; set; }

    [StringLength(500, ErrorMessage = "URL изображения не должен превышать 500 символов.")]
    public string? ProductImageUrl { get; set; }
}

/// <summary>
/// DTO для частичного обновления товара (PATCH).
/// </summary>
public class PatchProductDTO
{
    [StringLength(200, ErrorMessage = "Длина названия не должна превышать 200 символов.")]
    public string? ProductName { get; set; }

    public string? ProductDescription { get; set; }

    [Range(0.01, 1000000.00, ErrorMessage = "Цена товара должна быть в диапазоне от 0.01 до 1 000 000.")]
    public decimal? ProductPrice { get; set; }

    [Range(0, 100000, ErrorMessage = "Количество товара на складе не может быть отрицательным.")]
    public int? ProductStockQuantity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор категории.")]
    public int? CategoryId { get; set; }

    [StringLength(500, ErrorMessage = "URL изображения не должен превышать 500 символов.")]
    public string? ProductImageUrl { get; set; }
}

// Алиасы для обратной совместимости
public class ProductDTO : ProductReadDto { }
public class CreateProductDTO : ProductCreateDto { }
public class UpdateProductDTO : ProductUpdateDto { }
