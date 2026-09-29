using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevShop.Models;

namespace DevShop.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ShopDbContext _context;

    public OrdersController(ShopDbContext context)
    {
        _context = context;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> CheckoutSuccess(int orderId)
    {
        int userId = GetUserId();
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.IdOrders == orderId && o.UserId == userId);

        if (order == null)
        {
            return RedirectToAction("Index", "Products");
        }

        return View(order);
    }
}
