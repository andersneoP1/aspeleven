using System;
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
public class UsersController : Controller
{
    private readonly ShopDbContext _context;

    public UsersController(ShopDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Users
    public async Task<IActionResult> Index()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .OrderByDescending(u => u.IdUsers)
            .ToListAsync();
        return View(users);
    }

    // GET: Admin/Users/Create
    public IActionResult Create()
    {
        ViewData["RoleId"] = new SelectList(_context.Roles, "IdRoles", "RoleName");
        return View();
    }

    // POST: Admin/Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user, string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            ModelState.AddModelError("", "Пароль должен содержать не менее 6 символов.");
        }

        if (ModelState.IsValid)
        {
            user.UserPasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.UserCreatedAt = DateTime.Now;

            _context.Add(user);
            await _context.SaveChangesAsync();

            // Создаем корзину для созданного пользователя
            var cart = new Cart { UserId = user.IdUsers, CartCreatedAt = DateTime.Now };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Пользователь {user.UserLogin} успешно добавлен.";
            return RedirectToAction(nameof(Index));
        }
        ViewData["RoleId"] = new SelectList(_context.Roles, "IdRoles", "RoleName", user.RoleId);
        return View(user);
    }

    // GET: Admin/Users/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        ViewData["RoleId"] = new SelectList(_context.Roles, "IdRoles", "RoleName", user.RoleId);
        return View(user);
    }

    // POST: Admin/Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User user, string? newPassword)
    {
        if (id != user.IdUsers) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsers == id);
                if (existingUser == null) return NotFound();

                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    user.UserPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                }
                else
                {
                    user.UserPasswordHash = existingUser.UserPasswordHash;
                }

                user.UserCreatedAt = existingUser.UserCreatedAt;

                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Данные пользователя {user.UserLogin} обновлены.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.IdUsers == user.IdUsers))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["RoleId"] = new SelectList(_context.Roles, "IdRoles", "RoleName", user.RoleId);
        return View(user);
    }

    // POST: Admin/Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            if (user.UserLogin == "admin" && User.Identity?.Name == "admin")
            {
                TempData["Error"] = "Невозможно удалить активную учетную запись главного администратора.";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Пользователь удален из системы.";
        }
        return RedirectToAction(nameof(Index));
    }
}
