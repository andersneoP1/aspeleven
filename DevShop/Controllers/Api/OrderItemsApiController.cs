using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevShop.DTOs;
using DevShop.Models;

namespace DevShop.Controllers.Api;

[ApiController]
[Route("api/order-items")]
[Produces("application/json")]
[AllowAnonymous]
public class OrderItemsApiController : ControllerBase
{
    private readonly ShopDbContext _context;

    public OrderItemsApiController(ShopDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Получение списка позиций заказов с пагинацией и сортировкой по Id.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderItemDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var orderItems = await _context.OrderItems
            .AsNoTracking()
            .OrderBy(oi => oi.IdOrderItems)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = orderItems.Select(OrderItemMapper.ToDto).ToList();
        return Ok(result);
    }

    /// <summary>
    /// Получение позиции заказа по идентификатору.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderItemDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderItemDTO>> GetById(int id)
    {
        var orderItem = await _context.OrderItems
            .AsNoTracking()
            .FirstOrDefaultAsync(oi => oi.IdOrderItems == id);

        if (orderItem == null)
        {
            return NotFound(new { message = $"Позиция заказа с ID {id} не найдена." });
        }

        return Ok(OrderItemMapper.ToDto(orderItem));
    }

    /// <summary>
    /// Создание новой позиции заказа.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderItemDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderItemDTO>> Create([FromBody] CreateOrderItemDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var orderExists = await _context.Orders.AnyAsync(o => o.IdOrders == dto.OrderId);
        if (!orderExists)
        {
            ModelState.AddModelError(nameof(dto.OrderId), $"Заказ с ID {dto.OrderId} не существует.");
            return BadRequest(ModelState);
        }

        var productExists = await _context.Products.AnyAsync(p => p.IdProducts == dto.ProductId);
        if (!productExists)
        {
            ModelState.AddModelError(nameof(dto.ProductId), $"Товар с ID {dto.ProductId} не существует.");
            return BadRequest(ModelState);
        }

        var orderItem = OrderItemMapper.ToEntity(dto);
        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();

        var createdDto = OrderItemMapper.ToDto(orderItem);
        return CreatedAtAction(nameof(GetById), new { id = orderItem.IdOrderItems }, createdDto);
    }

    /// <summary>
    /// Полное обновление позиции заказа (PUT).
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderItemDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var orderItem = await _context.OrderItems.FindAsync(id);
        if (orderItem == null)
        {
            return NotFound(new { message = $"Позиция заказа с ID {id} не найдена." });
        }

        var orderExists = await _context.Orders.AnyAsync(o => o.IdOrders == dto.OrderId);
        if (!orderExists)
        {
            ModelState.AddModelError(nameof(dto.OrderId), $"Заказ с ID {dto.OrderId} не существует.");
            return BadRequest(ModelState);
        }

        var productExists = await _context.Products.AnyAsync(p => p.IdProducts == dto.ProductId);
        if (!productExists)
        {
            ModelState.AddModelError(nameof(dto.ProductId), $"Товар с ID {dto.ProductId} не существует.");
            return BadRequest(ModelState);
        }

        OrderItemMapper.ApplyUpdate(orderItem, dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Частичное обновление позиции заказа (PATCH).
    /// </summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchOrderItemDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var orderItem = await _context.OrderItems.FindAsync(id);
        if (orderItem == null)
        {
            return NotFound(new { message = $"Позиция заказа с ID {id} не найдена." });
        }

        if (dto.OrderId.HasValue)
        {
            var orderExists = await _context.Orders.AnyAsync(o => o.IdOrders == dto.OrderId.Value);
            if (!orderExists)
            {
                ModelState.AddModelError(nameof(dto.OrderId), $"Заказ с ID {dto.OrderId.Value} не существует.");
                return BadRequest(ModelState);
            }
        }

        if (dto.ProductId.HasValue)
        {
            var productExists = await _context.Products.AnyAsync(p => p.IdProducts == dto.ProductId.Value);
            if (!productExists)
            {
                ModelState.AddModelError(nameof(dto.ProductId), $"Товар с ID {dto.ProductId.Value} не существует.");
                return BadRequest(ModelState);
            }
        }

        OrderItemMapper.ApplyPatch(orderItem, dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Удаление позиции заказа по идентификатору.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var orderItem = await _context.OrderItems.FindAsync(id);
        if (orderItem == null)
        {
            return NotFound(new { message = $"Позиция заказа с ID {id} не найдена." });
        }

        _context.OrderItems.Remove(orderItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
