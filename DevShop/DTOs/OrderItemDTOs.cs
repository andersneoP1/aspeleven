using System.ComponentModel.DataAnnotations;

namespace DevShop.DTOs;

/// <summary>
/// DTO для чтения позиции заказа (GET).
/// Содержит денормализованное имя товара и вычисляемую сумму.
/// </summary>
public class OrderItemReadDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int OrderItemQuantity { get; set; }
    public decimal OrderItemPriceAtPurchase { get; set; }
    public decimal Subtotal => OrderItemQuantity * OrderItemPriceAtPurchase;
}

/// <summary>
/// DTO для добавления позиции в заказ при создании (POST).
/// Содержит только Id товара и количество.
/// </summary>
public class CreateOrderItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор товара.")]
    public int ProductId { get; set; }

    [Range(1, 1000, ErrorMessage = "Количество должно быть от 1 до 1 000.")]
    public int Quantity { get; set; }
}

/// <summary>
/// DTO для создания позиции через отдельный эндпоинт OrderItems.
/// </summary>
public class CreateOrderItemFullDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор заказа.")]
    public int OrderId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор товара.")]
    public int ProductId { get; set; }

    [Range(1, 1000, ErrorMessage = "Количество должно быть от 1 до 1 000.")]
    public int OrderItemQuantity { get; set; }

    [Range(0.01, 1000000.00, ErrorMessage = "Цена товара должна быть в диапазоне от 0.01 до 1 000 000.")]
    public decimal OrderItemPriceAtPurchase { get; set; }
}

/// <summary>
/// DTO для полного обновления позиции заказа (PUT).
/// </summary>
public class UpdateOrderItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор заказа.")]
    public int OrderId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор товара.")]
    public int ProductId { get; set; }

    [Range(1, 1000, ErrorMessage = "Количество должно быть от 1 до 1 000.")]
    public int OrderItemQuantity { get; set; }

    [Range(0.01, 1000000.00, ErrorMessage = "Цена товара должна быть в диапазоне от 0.01 до 1 000 000.")]
    public decimal OrderItemPriceAtPurchase { get; set; }
}

/// <summary>
/// DTO для частичного обновления позиции заказа (PATCH).
/// </summary>
public class PatchOrderItemDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор заказа.")]
    public int? OrderId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор товара.")]
    public int? ProductId { get; set; }

    [Range(1, 1000, ErrorMessage = "Количество должно быть от 1 до 1 000.")]
    public int? OrderItemQuantity { get; set; }

    [Range(0.01, 1000000.00, ErrorMessage = "Цена товара должна быть в диапазоне от 0.01 до 1 000 000.")]
    public decimal? OrderItemPriceAtPurchase { get; set; }
}

// Алиасы для обратной совместимости
public class OrderItemDTO : OrderItemReadDto { }
public class CreateOrderItemDTO : CreateOrderItemFullDto { }
public class UpdateOrderItemDTO : UpdateOrderItemDto { }
