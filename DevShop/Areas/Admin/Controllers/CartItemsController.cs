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
public class CartItemsController : Controller
{
    private readonly ShopDbContext _context;

    public CartItemsController(ShopDbContext context)
    {
        _context = context;
    }

    // GET: Admin/CartItems
    public async Task<IActionResult> Index()
    {
        var cartItems = await _context.CartItems
            .Include(c => c.Cart)
            .Include(c => c.Product)
            .OrderByDescending(c => c.IdCartItems)
            .ToListAsync();
        return View(cartItems);
    }

    // GET: Admin/CartItems/Create
    public IActionResult Create()
    {
        ViewData["CartId"] = new SelectList(_context.Carts, "IdCarts", "IdCarts");
        ViewData["ProductId"] = new SelectList(_context.Products, "IdProducts", "ProductName");
        return View();
    }

    // POST: Admin/CartItems/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CartItem cartItem)
    {
        if (ModelState.IsValid)
        {
            _context.Add(cartItem);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Позиция добавлена в корзину.";
            return RedirectToAction(nameof(Index));
        }
        ViewData["CartId"] = new SelectList(_context.Carts, "IdCarts", "IdCarts", cartItem.CartId);
        ViewData["ProductId"] = new SelectList(_context.Products, "IdProducts", "ProductName", cartItem.ProductId);
        return View(cartItem);
    }

    // GET: Admin/CartItems/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var cartItem = await _context.CartItems.FindAsync(id);
        if (cartItem == null) return NotFound();

        ViewData["CartId"] = new SelectList(_context.Carts, "IdCarts", "IdCarts", cartItem.CartId);
        ViewData["ProductId"] = new SelectList(_context.Products, "IdProducts", "ProductName", cartItem.ProductId);
        return View(cartItem);
    }

    // POST: Admin/CartItems/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CartItem cartItem)
    {
        if (id != cartItem.IdCartItems) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(cartItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Позиция корзины обновлена.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.CartItems.Any(e => e.IdCartItems == cartItem.IdCartItems))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["CartId"] = new SelectList(_context.Carts, "IdCarts", "IdCarts", cartItem.CartId);
        ViewData["ProductId"] = new SelectList(_context.Products, "IdProducts", "ProductName", cartItem.ProductId);
        return View(cartItem);
    }

    // POST: Admin/CartItems/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cartItem = await _context.CartItems.FindAsync(id);
        if (cartItem != null)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Позиция удалена из корзины.";
        }
        return RedirectToAction(nameof(Index));
    }
}
