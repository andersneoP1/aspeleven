using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevShop.Models;

namespace DevShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ShopDbContext _context;

    public CategoriesController(ShopDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Categories
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
        return View(categories);
    }

    // GET: Admin/Categories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Категория успешно создана.";
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // GET: Admin/Categories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        return View(category);
    }

    // POST: Admin/Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.IdCategories) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Категория успешно обновлена.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(e => e.IdCategories == category.IdCategories))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // POST: Admin/Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.IdCategories == id);

        if (category != null)
        {
            if (category.Products.Any())
            {
                TempData["Error"] = "Невозможно удалить категорию, содержащую привязанные товары.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Категория удалена.";
        }
        return RedirectToAction(nameof(Index));
    }
}
