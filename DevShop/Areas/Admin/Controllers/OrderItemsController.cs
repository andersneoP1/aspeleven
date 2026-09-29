using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DevShop.Models;

namespace DevShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrderItemsController : Controller
{
    private readonly ShopDbContext _context;

    public OrderItemsController(ShopDbContext context)
    {
        _context = context;
    }

    // GET: Admin/OrderItems
    public async Task<IActionResult> Index()
    {
        var orderItems = await _context.OrderItems
            .Include(o => o.Order)
            .Include(o => o.Product)
            .OrderByDescending(o => o.IdOrderItems)
            .ToListAsync();
        return View(orderItems);
    }

    // GET: Admin/OrderItems/Create
    public IActionResult Create()
    {
        ViewData["OrderId"] = new SelectList(_context.Orders, "IdOrders", "IdOrders");
        ViewData["ProductId"] = new SelectList(_context.Products, "IdProducts", "ProductName");
        return View();
    }

    // POST: Admin/OrderItems/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderItem orderItem)
    {
        if (ModelState.IsValid)
        {
            var product = await _context.Products.FindAsync(orderItem.ProductId);
            if (product != null && orderItem.OrderItemPriceAtPurchase == 0)
            {
                orderItem.OrderItemPriceAtPurchase = product.ProductPrice;
            }

            _context.Add(orderItem);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Позиция заказа добавлена.";
            return RedirectToAction(nameof(Index));
        }
        ViewData["OrderId"] = new SelectList(_context.Orders, "IdOrders", "IdOrders", orderItem.OrderId);
        ViewData["ProductId"] = new SelectList(_context.Products, "IdProducts", "ProductName", orderItem.ProductId);
        return View(orderItem);
    }

    // GET: Admin/OrderItems/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var orderItem = await _context.OrderItems.FindAsync(id);
        if (orderItem == null) return NotFound();

        ViewData["OrderId"] = new SelectList(_context.Orders, "IdOrders", "IdOrders", orderItem.OrderId);
        ViewData["ProductId"] = new SelectList(_context.Products, "IdProducts", "ProductName", orderItem.ProductId);
        return View(orderItem);
    }

    // POST: Admin/OrderItems/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, OrderItem orderItem)
    {
        if (id != orderItem.IdOrderItems) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(orderItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Позиция заказа обновлена.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.OrderItems.Any(e => e.IdOrderItems == orderItem.IdOrderItems))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["OrderId"] = new SelectList(_context.Orders, "IdOrders", "IdOrders", orderItem.OrderId);
        ViewData["ProductId"] = new SelectList(_context.Products, "IdProducts", "ProductName", orderItem.ProductId);
        return View(orderItem);
    }

    // POST: Admin/OrderItems/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var orderItem = await _context.OrderItems.FindAsync(id);
        if (orderItem != null)
        {
            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Позиция заказа удалена.";
        }
        return RedirectToAction(nameof(Index));
    }
}
