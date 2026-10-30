using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevShop.DTOs;
using DevShop.Models;

namespace DevShop.Controllers.Api;

[ApiController]
[Route("api/roles")]
[Produces("application/json")]
[AllowAnonymous]
public class RolesApiController : ControllerBase
{
    private readonly ShopDbContext _context;

    public RolesApiController(ShopDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Получение списка ролей с пагинацией и сортировкой по Id.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoleDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var roles = await _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.IdRoles)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = roles.Select(RoleMapper.ToDto).ToList();
        return Ok(result);
    }

    /// <summary>
    /// Получение роли по идентификатору.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RoleDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleDTO>> GetById(int id)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdRoles == id);

        if (role == null)
        {
            return NotFound(new { message = $"Роль с ID {id} не найдена." });
        }

        return Ok(RoleMapper.ToDto(role));
    }

    /// <summary>
    /// Создание новой роли.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoleDTO>> Create([FromBody] CreateRoleDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var role = RoleMapper.ToEntity(dto);
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var createdDto = RoleMapper.ToDto(role);
        return CreatedAtAction(nameof(GetById), new { id = role.IdRoles }, createdDto);
    }

    /// <summary>
    /// Полное обновление роли (PUT).
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound(new { message = $"Роль с ID {id} не найдена." });
        }

        RoleMapper.ApplyUpdate(role, dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Частичное обновление роли (PATCH).
    /// </summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchRoleDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound(new { message = $"Роль с ID {id} не найдена." });
        }

        RoleMapper.ApplyPatch(role, dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Удаление роли по идентификатору.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound(new { message = $"Роль с ID {id} не найдена." });
        }

        try
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new { message = "Невозможно удалить роль, так как она назначена пользователям." });
        }

        return NoContent();
    }
}
