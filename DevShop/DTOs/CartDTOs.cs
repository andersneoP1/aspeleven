using System.ComponentModel.DataAnnotations;

namespace DevShop.DTOs;

/// <summary>
/// DTO для чтения данных корзины (GET).
/// Содержит Id и дату создания CreatedAt.
/// </summary>
public class CartDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO для создания новой корзины (POST).
/// Не содержит Id и CreatedAt (формируются на сервере).
/// </summary>
public class CreateCartDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор пользователя.")]
    public int UserId { get; set; }
}

/// <summary>
/// DTO для полного обновления корзины (PUT).
/// Все поля обязательны.
/// </summary>
public class UpdateCartDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор пользователя.")]
    public int UserId { get; set; }
}

/// <summary>
/// DTO для частичного обновления корзины (PATCH).
/// Все поля nullable.
/// </summary>
public class PatchCartDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный идентификатор пользователя.")]
    public int? UserId { get; set; }
}
