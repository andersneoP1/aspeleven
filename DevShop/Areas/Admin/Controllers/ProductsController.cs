using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DevShop.Models;

namespace DevShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly ShopDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductsController(ShopDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Admin/Products
    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .OrderByDescending(p => p.IdProducts)
            .ToListAsync();
        return View(products);
    }

    // GET: Admin/Products/Create
    public IActionResult Create()
    {
        ViewData["CategoryId"] = new SelectList(_context.Categories, "IdCategories", "CategoryName");
        return View();
    }

    // POST: Admin/Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(fileStream);
                }

                product.ProductImageUrl = "/images/" + uniqueFileName;
            }

            _context.Add(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Новый товар успешно добавлен в каталог DevHub.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["CategoryId"] = new SelectList(_context.Categories, "IdCategories", "CategoryName", product.CategoryId);
        return View(product);
    }

    // GET: Admin/Products/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        ViewData["CategoryId"] = new SelectList(_context.Categories, "IdCategories", "CategoryName", product.CategoryId);
        return View(product);
    }

    // POST: Admin/Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.IdProducts) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.IdProducts == id);
                if (existingProduct == null) return NotFound();

                if (product.ImageFile != null && product.ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(fileStream);
                    }

                    product.ProductImageUrl = "/images/" + uniqueFileName;
                }
                else
                {
                    product.ProductImageUrl = existingProduct.ProductImageUrl;
                }

                _context.Update(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Данные товара успешно обновлены.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.IdProducts == product.IdProducts))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["CategoryId"] = new SelectList(_context.Categories, "IdCategories", "CategoryName", product.CategoryId);
        return View(product);
    }

    // POST: Admin/Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Товар успешно удален из каталога.";
        }
        return RedirectToAction(nameof(Index));
    }
}
