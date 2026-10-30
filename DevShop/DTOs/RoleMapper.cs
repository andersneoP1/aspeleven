using DevShop.Models;

namespace DevShop.DTOs;

/// <summary>
/// Статический класс-маппер для сущности Role.
/// </summary>
public static class RoleMapper
{
    public static RoleDTO ToDto(Role entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new RoleDTO
        {
            Id = entity.IdRoles,
            RoleName = entity.RoleName
        };
    }

    public static Role ToEntity(CreateRoleDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new Role
        {
            RoleName = dto.RoleName
        };
    }

    public static void ApplyUpdate(Role entity, UpdateRoleDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        entity.RoleName = dto.RoleName;
    }

    public static void ApplyPatch(Role entity, PatchRoleDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        if (dto.RoleName != null)
        {
            entity.RoleName = dto.RoleName;
        }
    }
}
