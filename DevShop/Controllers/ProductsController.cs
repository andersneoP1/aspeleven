using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using DevShop.Models;
using DevShop.Models.ViewModels;

namespace DevShop.Controllers;

public class ProductsController : Controller
{
    private readonly ShopDbContext _context;
    private readonly IMemoryCache _cache;

    public ProductsController(ShopDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IActionResult> Index(string? searchString, int? categoryId, string? sortBy, int page = 1)
    {
        int pageSize = 6;
        
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var search = searchString.Trim();
            query = query.Where(p => p.ProductName.Contains(search) || 
                                     (p.ProductDescription != null && p.ProductDescription.Contains(search)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        query = sortBy switch
        {
            "price_desc" => query.OrderByDescending(p => p.ProductPrice),
            "price_asc" => query.OrderBy(p => p.ProductPrice),
            "name_desc" => query.OrderByDescending(p => p.ProductName),
            _ => query.OrderBy(p => p.ProductName),
        };

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (totalPages < 1) totalPages = 1;
        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages;

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Кэширование списка категорий в памяти на 10 минут
        var categories = await _cache.GetOrCreateAsync("categories_list", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _context.Categories.AsNoTracking().ToListAsync();
        });

        var productViewModels = products.Select(p => new ProductViewModel
        {
            Id = p.IdProducts,
            Name = p.ProductName,
            Description = p.ProductDescription,
            Price = p.ProductPrice,
            StockQuantity = p.ProductStockQuantity,
            CategoryName = p.Category?.CategoryName ?? "Без категории",
            ImagePath = p.ProductImageUrl
        }).ToList();

        var viewModel = new CatalogViewModel
        {
            Products = productViewModels,
            Categories = categories ?? new List<Category>(),
            Page = page,
            TotalPages = totalPages,
            SearchString = searchString,
            CategoryId = categoryId,
            SortBy = sortBy
        };

        return View(viewModel);
    }
}
