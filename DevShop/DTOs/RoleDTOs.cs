using System.ComponentModel.DataAnnotations;

namespace DevShop.DTOs;

/// <summary>
/// DTO для чтения роли (GET).
/// </summary>
public class RoleReadDto
{
    public int Id { get; set; }
    public string RoleName { get; set; } = null!;
}

/// <summary>
/// DTO для чтения роли (GET) - обратная совместимость.
/// </summary>
public class RoleDTO : RoleReadDto { }

/// <summary>
/// DTO для создания новой роли (POST).
/// </summary>
public class CreateRoleDTO
{
    [Required(ErrorMessage = "Название роли обязательно.")]
    [StringLength(50, ErrorMessage = "Длина названия роли не должна превышать 50 символов.")]
    public string RoleName { get; set; } = null!;
}

/// <summary>
/// DTO для полного обновления роли (PUT).
/// </summary>
public class UpdateRoleDTO
{
    [Required(ErrorMessage = "Название роли обязательно.")]
    [StringLength(50, ErrorMessage = "Длина названия роли не должна превышать 50 символов.")]
    public string RoleName { get; set; } = null!;
}

/// <summary>
/// DTO для частичного обновления роли (PATCH).
/// </summary>
public class PatchRoleDTO
{
    [StringLength(50, ErrorMessage = "Длина названия роли не должна превышать 50 символов.")]
    public string? RoleName { get; set; }
}

public class RoleCreateDto : CreateRoleDTO { }
public class RoleUpdateDto : UpdateRoleDTO { }
