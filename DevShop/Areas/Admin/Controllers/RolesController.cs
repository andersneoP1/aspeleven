using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevShop.Models;

namespace DevShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private readonly ShopDbContext _context;

    public RolesController(ShopDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Roles
    public async Task<IActionResult> Index()
    {
        var roles = await _context.Roles
            .Include(r => r.Users)
            .OrderBy(r => r.IdRoles)
            .ToListAsync();
        return View(roles);
    }

    // GET: Admin/Roles/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/Roles/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Role role)
    {
        if (ModelState.IsValid)
        {
            _context.Add(role);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Роль «{role.RoleName}» успешно создана.";
            return RedirectToAction(nameof(Index));
        }
        return View(role);
    }

    // GET: Admin/Roles/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var role = await _context.Roles.FindAsync(id);
        if (role == null) return NotFound();

        return View(role);
    }

    // POST: Admin/Roles/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Role role)
    {
        if (id != role.IdRoles) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(role);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Роль успешно обновлена.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Roles.Any(e => e.IdRoles == role.IdRoles))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(role);
    }

    // POST: Admin/Roles/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var role = await _context.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.IdRoles == id);

        if (role != null)
        {
            if (role.RoleName == "Admin" || role.RoleName == "User")
            {
                TempData["Error"] = "Системные роли (Admin, User) не подлежат удалению.";
                return RedirectToAction(nameof(Index));
            }

            if (role.Users.Any())
            {
                TempData["Error"] = "Невозможно удалить роль, назначенную активным пользователям.";
                return RedirectToAction(nameof(Index));
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Роль удалена.";
        }
        return RedirectToAction(nameof(Index));
    }
}
