using System.ComponentModel.DataAnnotations;

namespace DevShop.DTOs;

/// <summary>
/// DTO для чтения позиции корзины (GET).
/// </summary>
public class CartItemDTO
{
    public int Id { get; set; }
    public int? CartId { get; set; }
    public string UserId { get; set; } = null!;
    public int ProductId { get; set; }
    public int CartItemQuantity { get; set; }
}

/// <summary>
/// DTO для создания позиции корзины (POST).
/// </summary>
public class CreateCartItemDTO
{
    public int? CartId { get; set; }

    [Required(ErrorMessage = "Идентификатор пользователя обязателен.")]
    [StringLength(450, ErrorMessage = "Длина идентификатора пользователя не должна превышать 450 символов.")]
    public string UserId { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор товара.")]
    public int ProductId { get; set; }

    [Range(1, 10000, ErrorMessage = "Количество товара должно быть от 1 до 10 000.")]
    public int CartItemQuantity { get; set; }
}

/// <summary>
/// DTO для полного обновления позиции корзины (PUT).
/// Все поля обязательны.
/// </summary>
public class UpdateCartItemDTO
{
    public int? CartId { get; set; }

    [Required(ErrorMessage = "Идентификатор пользователя обязателен.")]
    [StringLength(450, ErrorMessage = "Длина идентификатора пользователя не должна превышать 450 символов.")]
    public string UserId { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор товара.")]
    public int ProductId { get; set; }

    [Range(1, 10000, ErrorMessage = "Количество товара должно быть от 1 до 10 000.")]
    public int CartItemQuantity { get; set; }
}

/// <summary>
/// DTO для частичного обновления позиции корзины (PATCH).
/// Все поля nullable.
/// </summary>
public class PatchCartItemDTO
{
    public int? CartId { get; set; }

    [StringLength(450, ErrorMessage = "Длина идентификатора пользователя не должна превышать 450 символов.")]
    public string? UserId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор товара.")]
    public int? ProductId { get; set; }

    [Range(1, 10000, ErrorMessage = "Количество товара должно быть от 1 до 10 000.")]
    public int? CartItemQuantity { get; set; }
}
