using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevShop.Models;

namespace DevShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CartsController : Controller
{
    private readonly ShopDbContext _context;

    public CartsController(ShopDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Carts
    public async Task<IActionResult> Index()
    {
        var carts = await _context.Carts
            .Include(c => c.User)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .OrderByDescending(c => c.IdCarts)
            .ToListAsync();
        return View(carts);
    }

    // GET: Admin/Carts/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var cart = await _context.Carts
            .Include(c => c.User)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(m => m.IdCarts == id);

        if (cart == null) return NotFound();

        return View(cart);
    }

    // POST: Admin/Carts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.IdCarts == id);

        if (cart != null)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Корзина удалена.";
        }
        return RedirectToAction(nameof(Index));
    }
}
