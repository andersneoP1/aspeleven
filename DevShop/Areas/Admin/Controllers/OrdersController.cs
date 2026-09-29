using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevShop.Models;

namespace DevShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly ShopDbContext _context;

    public OrdersController(ShopDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Orders
    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.IdOrders)
            .ToListAsync();
        return View(orders);
    }

    // GET: Admin/Orders/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(m => m.IdOrders == id);

        if (order == null) return NotFound();

        return View(order);
    }

    // GET: Admin/Orders/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.IdOrders == id);

        if (order == null) return NotFound();

        return View(order);
    }

    // POST: Admin/Orders/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string orderStatus)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.OrderStatus = orderStatus;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Статус заказа #{order.IdOrders} изменен на «{orderStatus}».";

        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Orders/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.IdOrders == id);

        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Заказ #{id} удален.";
        }
        return RedirectToAction(nameof(Index));
    }
}
