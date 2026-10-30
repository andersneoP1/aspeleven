using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DevShop.Common;
using DevShop.DTOs;
using DevShop.Models;
using DevShop.Repositories;

namespace DevShop.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IMapper _mapper;

        public OrderService(
            IOrderRepository orderRepository,
            IRepository<Product> productRepository,
            IRepository<User> userRepository,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<PagedResult<OrderReadDto>>> GetPagedOrdersAsync(int page, int pageSize, int? userId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var pagedOrders = await _orderRepository.GetOrdersWithDetailsPagedAsync(page, pageSize, userId);
            var dtos = _mapper.Map<IEnumerable<OrderReadDto>>(pagedOrders.Items);

            var result = new PagedResult<OrderReadDto>(dtos, pagedOrders.TotalCount, page, pageSize);
            return ServiceResult<PagedResult<OrderReadDto>>.Success(result);
        }

        public async Task<ServiceResult<OrderReadDto>> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order == null)
            {
                return ServiceResult<OrderReadDto>.NotFound($"Заказ с Id={orderId} не найден.");
            }

            var dto = _mapper.Map<OrderReadDto>(order);
            return ServiceResult<OrderReadDto>.Success(dto);
        }

        /// <summary>
        /// Оформление заказа (Бизнес-операция):
        /// 1. Проверяем наличие пользователя.
        /// 2. Проверяем остатки каждого товара и списываем их (без сохранения).
        /// 3. Формируем сущность Order и OrderItem.
        /// 4. Вызываем SaveChangesAsync() ОДИН РАЗ в конце операции через общий Scoped-контекст.
        /// </summary>
        public async Task<ServiceResult<OrderReadDto>> CreateOrderAsync(CreateOrderDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
            {
                return ServiceResult<OrderReadDto>.Failed("В заказе должна быть указана хотя бы одна позиция товара.", ServiceErrorType.Validation);
            }

            // 1. Проверяем пользователя
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                return ServiceResult<OrderReadDto>.NotFound($"Пользователь с Id={dto.UserId} не найден.");
            }

            // 2. Валидация остатков товаров и списание
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var itemDto in dto.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    return ServiceResult<OrderReadDto>.NotFound($"Товар с Id={itemDto.ProductId} не найден.");
                }

                if (product.ProductStockQuantity < itemDto.Quantity)
                {
                    return ServiceResult<OrderReadDto>.Failed(
                        $"Недостаточно товара '{product.ProductName}' (ID: {product.IdProducts}) на складе. Запрошено: {itemDto.Quantity}, в наличии: {product.ProductStockQuantity}.",
                        ServiceErrorType.Validation);
                }

                // Уменьшаем остаток товара (Change Tracker EF Core зафиксирует изменения)
                product.ProductStockQuantity -= itemDto.Quantity;
                _productRepository.Update(product);

                // Формируем позицию заказа с фиксацией цены покупки
                var orderItem = new OrderItem
                {
                    ProductId = product.IdProducts,
                    OrderItemQuantity = itemDto.Quantity,
                    OrderItemPriceAtPurchase = product.ProductPrice
                };
                orderItems.Add(orderItem);

                totalAmount += product.ProductPrice * itemDto.Quantity;
            }

            // 3. Формируем сущность заказа
            var order = new Order
            {
                UserId = dto.UserId,
                OrderDate = DateTime.UtcNow,
                OrderTotalAmount = totalAmount,
                OrderStatus = "Pending",
                OrderItems = orderItems
            };

            await _orderRepository.AddAsync(order);

            // 4. ЕДИНЫЙ вызов SaveChangesAsync(): 
            // Все репозитории в рамках одного HTTP-запроса используют один экземпляр ShopDbContext (Scoped),
            // поэтому EF Core сохраняет заказ, его позиции и обновленные остатки товаров атомарно в одной SQL-транзакции.
            await _orderRepository.SaveChangesAsync();

            // 5. Загружаем сохраненный заказ с навигационными свойствами для возврата полного DTO
            var createdOrder = await _orderRepository.GetOrderWithDetailsAsync(order.IdOrders);
            var resultDto = _mapper.Map<OrderReadDto>(createdOrder ?? order);

            return ServiceResult<OrderReadDto>.Success(resultDto);
        }

        /// <summary>
        /// Обратная бизнес-операция (Отмена заказа):
        /// 1. Ищем заказ с позициями.
        /// 2. Проверяем статус (не отменен ли уже).
        /// 3. Возвращаем списанные товары на склад.
        /// 4. Меняем статус на 'Cancelled'.
        /// 5. Вызываем SaveChangesAsync() ОДИН РАЗ в конце.
        /// </summary>
        public async Task<ServiceResult<OrderReadDto>> CancelOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order == null)
            {
                return ServiceResult<OrderReadDto>.NotFound($"Заказ с Id={orderId} не найден.");
            }

            if (order.OrderStatus == "Cancelled")
            {
                return ServiceResult<OrderReadDto>.Failed("Заказ уже был отменен ранее.", ServiceErrorType.Conflict);
            }

            // 1. Возвращаем остатки товаров
            foreach (var item in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.ProductStockQuantity += item.OrderItemQuantity;
                    _productRepository.Update(product);
                }
            }

            // 2. Обновляем статус
            order.OrderStatus = "Cancelled";
            _orderRepository.Update(order);

            // 3. ЕДИНЫЙ вызов SaveChangesAsync()
            await _orderRepository.SaveChangesAsync();

            var resultDto = _mapper.Map<OrderReadDto>(order);
            return ServiceResult<OrderReadDto>.Success(resultDto);
        }

        public async Task<ServiceResult<bool>> DeleteOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return ServiceResult<bool>.NotFound($"Заказ с Id={orderId} не найден.");
            }

            _orderRepository.Delete(order);
            await _orderRepository.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }
    }
}
