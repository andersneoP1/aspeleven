using DevShop.Models;

namespace DevShop.DTOs;

/// <summary>
/// Статический класс-маппер для сущности OrderItem.
/// </summary>
public static class OrderItemMapper
{
    public static OrderItemDTO ToDto(OrderItem entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new OrderItemDTO
        {
            Id = entity.IdOrderItems,
            OrderId = entity.OrderId,
            ProductId = entity.ProductId,
            OrderItemQuantity = entity.OrderItemQuantity,
            OrderItemPriceAtPurchase = entity.OrderItemPriceAtPurchase
        };
    }

    public static OrderItem ToEntity(CreateOrderItemDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new OrderItem
        {
            OrderId = dto.OrderId,
            ProductId = dto.ProductId,
            OrderItemQuantity = dto.OrderItemQuantity,
            OrderItemPriceAtPurchase = dto.OrderItemPriceAtPurchase
        };
    }

    public static void ApplyUpdate(OrderItem entity, UpdateOrderItemDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        entity.OrderId = dto.OrderId;
        entity.ProductId = dto.ProductId;
        entity.OrderItemQuantity = dto.OrderItemQuantity;
        entity.OrderItemPriceAtPurchase = dto.OrderItemPriceAtPurchase;
    }

    public static void ApplyPatch(OrderItem entity, PatchOrderItemDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        if (dto.OrderId.HasValue)
        {
            entity.OrderId = dto.OrderId.Value;
        }

        if (dto.ProductId.HasValue)
        {
            entity.ProductId = dto.ProductId.Value;
        }

        if (dto.OrderItemQuantity.HasValue)
        {
            entity.OrderItemQuantity = dto.OrderItemQuantity.Value;
        }

        if (dto.OrderItemPriceAtPurchase.HasValue)
        {
            entity.OrderItemPriceAtPurchase = dto.OrderItemPriceAtPurchase.Value;
        }
    }
}
