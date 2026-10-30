using System.ComponentModel.DataAnnotations;

namespace DevShop.DTOs;

/// <summary>
/// DTO для чтения данных заказа (GET).
/// Содержит вложенный список позиций заказа с названиями товаров.
/// </summary>
public class OrderReadDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserLogin { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal OrderTotalAmount { get; set; }
    public string OrderStatus { get; set; } = null!;
    public List<OrderItemReadDto> Items { get; set; } = new();
}

/// <summary>
/// DTO для создания заказа с позициями (POST).
/// Не содержит Id и CreatedAt (формируются на сервере).
/// </summary>
public class CreateOrderDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор пользователя.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Заказ должен содержать хотя бы одну позицию.")]
    [MinLength(1, ErrorMessage = "Заказ должен содержать хотя бы одну позицию.")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO для создания плоского заказа (без позиций) - для обратной совместимости.
/// </summary>
public class CreateOrderFlatDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор пользователя.")]
    public int UserId { get; set; }

    [Range(0.00, 10000000.00, ErrorMessage = "Сумма заказа должна быть в диапазоне от 0 до 10 000 000.")]
    public decimal OrderTotalAmount { get; set; }

    [Required(ErrorMessage = "Статус заказа обязателен.")]
    [StringLength(50, ErrorMessage = "Длина статуса заказа не должна превышать 50 символов.")]
    public string OrderStatus { get; set; } = "Pending";
}

/// <summary>
/// DTO для полного обновления заказа (PUT).
/// </summary>
public class UpdateOrderDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор пользователя.")]
    public int UserId { get; set; }

    [Range(0.00, 10000000.00, ErrorMessage = "Сумма заказа должна быть в диапазоне от 0 до 10 000 000.")]
    public decimal OrderTotalAmount { get; set; }

    [Required(ErrorMessage = "Статус заказа обязателен.")]
    [StringLength(50, ErrorMessage = "Длина статуса заказа не должна превышать 50 символов.")]
    public string OrderStatus { get; set; } = null!;
}

/// <summary>
/// DTO для частичного обновления заказа (PATCH).
/// </summary>
public class PatchOrderDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор пользователя.")]
    public int? UserId { get; set; }

    [Range(0.00, 10000000.00, ErrorMessage = "Сумма заказа должна быть в диапазоне от 0 до 10 000 000.")]
    public decimal? OrderTotalAmount { get; set; }

    [StringLength(50, ErrorMessage = "Длина статуса заказа не должна превышать 50 символов.")]
    public string? OrderStatus { get; set; }
}

// Алиасы для совместимости
public class OrderDTO : OrderReadDto { }
public class CreateOrderDTO : CreateOrderFlatDto { }
