using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevShop.Models;
using DevShop.Models.ViewModels;
using BCrypt.Net;

namespace DevShop.Controllers;

public class AccountController : Controller
{
    private readonly ShopDbContext _context;

    public AccountController(ShopDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Products");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserLogin == model.Login);

            if (user != null)
            {
                if (BCrypt.Net.BCrypt.Verify(model.Password, user.UserPasswordHash))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.IdUsers.ToString()),
                        new Claim(ClaimTypes.Name, user.UserLogin),
                        new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Products");
                }
            }
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль разработчика.");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Products");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserLogin.ToLower() == model.Login.ToLower());
            if (existingUser != null)
            {
                ModelState.AddModelError("Login", "Данный логин уже занят другим разработчиком.");
                return View(model);
            }

            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            int roleId = defaultRole?.IdRoles ?? 1;

            var user = new User
            {
                UserLogin = model.Login,
                UserPasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                RoleId = roleId,
                UserCreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Создаем корзину для нового пользователя
            var cart = new Cart
            {
                UserId = user.IdUsers,
                CartCreatedAt = DateTime.Now
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Учетная запись успешно зарегистрирована! Теперь выполните вход.";
            return RedirectToAction("Login");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Products");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
