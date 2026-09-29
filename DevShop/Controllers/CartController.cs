using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevShop.Models;
using DevShop.Models.ViewModels;

namespace DevShop.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ShopDbContext _context;

    public CartController(ShopDbContext context)
    {
        _context = context;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // 1. Index: получение позиций корзины авторизованного пользователя
    public async Task<IActionResult> Index()
    {
        string userId = GetUserId();

        var items = await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        var viewModel = new CartViewModel
        {
            Items = items.Select(ci => new CartItemViewModel
            {
                Id = ci.IdCartItems,
                ProductId = ci.ProductId,
                ProductName = ci.Product.ProductName,
                ProductPrice = ci.Product.ProductPrice,
                Quantity = ci.CartItemQuantity,
                ImagePath = ci.Product.ProductImageUrl
            }).ToList()
        };

        return View(viewModel);
    }

    // 2. AddToCart: безопасное добавление товара в корзину
    public async Task<IActionResult> AddToCart(int productId)
    {
        string userId = GetUserId();

        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            TempData["Error"] = "Товар не найден в каталоге.";
            return RedirectToAction("Index", "Products");
        }

        if (product.ProductStockQuantity <= 0)
        {
            TempData["Error"] = "Товар временно закончился на складе.";
            return RedirectToAction("Index", "Products");
        }

        var item = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (item == null)
        {
            _context.CartItems.Add(new CartItem
            {
                UserId = userId,
                ProductId = productId,
                CartItemQuantity = 1
            });
        }
        else
        {
            if (item.CartItemQuantity < product.ProductStockQuantity)
            {
                item.CartItemQuantity++;
            }
            else
            {
                TempData["Warning"] = $"Достигнут максимальный доступный остаток товара на складе ({product.ProductStockQuantity} шт.).";
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Товар «{product.ProductName}» успешно добавлен в корзину.";
        return RedirectToAction("Index");
    }

    // 3. RemoveFromCart: удаление позиции корзины с проверкой владельца
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        string userId = GetUserId();

        var item = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.IdCartItems == id && ci.UserId == userId);

        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Позиция удалена из корзины.";
        }

        return RedirectToAction("Index");
    }

    // 4. UpdateQuantity: обновление количества товара в корзине
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int id, int quantity)
    {
        string userId = GetUserId();

        if (quantity <= 0)
        {
            return RedirectToAction("RemoveFromCart", new { id = id });
        }

        var item = await _context.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.IdCartItems == id && ci.UserId == userId);

        if (item != null)
        {
            if (quantity > item.Product.ProductStockQuantity)
            {
                item.CartItemQuantity = item.Product.ProductStockQuantity;
                TempData["Warning"] = $"Количество скорректировано до максимального остатка: {item.Product.ProductStockQuantity} шт.";
            }
            else
            {
                item.CartItemQuantity = quantity;
            }
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    // 5. Checkout: транзакционное оформление заказа с очисткой корзины
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        string userId = GetUserId();
        if (!int.TryParse(userId, out int userIdInt))
        {
            TempData["Error"] = "Ошибка идентификации учетной записи.";
            return RedirectToAction("Index");
        }

        var cartItems = await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        if (cartItems == null || !cartItems.Any())
        {
            TempData["Error"] = "Ваша корзина пуста. Невозможно оформить заказ.";
            return RedirectToAction("Index");
        }

        // Проверка наличия всех позиций на складе
        foreach (var ci in cartItems)
        {
            if (ci.CartItemQuantity > ci.Product.ProductStockQuantity)
            {
                TempData["Error"] = $"Товара «{ci.Product.ProductName}» недостаточно на складе (в наличии: {ci.Product.ProductStockQuantity} шт.). Измените количество в корзине.";
                return RedirectToAction("Index");
            }
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            decimal totalAmount = cartItems.Sum(ci => ci.Product.ProductPrice * ci.CartItemQuantity);

            // Создание записи заказа
            var order = new Order
            {
                UserId = userIdInt,
                OrderDate = DateTime.Now,
                OrderTotalAmount = totalAmount,
                OrderStatus = "Processing"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Создание позиций заказа и списание со склада
            var orderItems = cartItems.Select(ci =>
            {
                ci.Product.ProductStockQuantity -= ci.CartItemQuantity;
                return new OrderItem
                {
                    OrderId = order.IdOrders,
                    ProductId = ci.ProductId,
                    OrderItemQuantity = ci.CartItemQuantity,
                    OrderItemPriceAtPurchase = ci.Product.ProductPrice
                };
            }).ToList();

            _context.OrderItems.AddRange(orderItems);

            // Очистка корзины пользователя
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction("CheckoutSuccess", "Orders", new { orderId = order.IdOrders });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Произошла ошибка при проведении транзакции заказа: " + ex.Message;
            return RedirectToAction("Index");
        }
    }
}
