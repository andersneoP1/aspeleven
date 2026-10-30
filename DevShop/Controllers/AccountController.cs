using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevShop.Models;
using DevShop.Models.ViewModels;

namespace DevShop.Controllers;

public class AccountController : Controller
{
    private readonly ShopDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AccountController(ShopDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
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
                if (!user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Учетная запись деактивирована администратором.");
                    return View(model);
                }

                bool isPasswordValid = false;
                try
                {
                    isPasswordValid = _passwordHasher.VerifyHashedPassword(user, user.UserPasswordHash, model.Password) != PasswordVerificationResult.Failed;
                }
                catch
                {
                    try
                    {
                        isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.UserPasswordHash);
                    }
                    catch { }
                }

                if (isPasswordValid)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.IdUsers.ToString()),
                        new Claim(ClaimTypes.Name, user.UserLogin),
                        new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User"),
                        new Claim("isActive", user.IsActive.ToString().ToLower())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : (DateTimeOffset?)null
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

            ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
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
            var existingUser = await _context.Users.AnyAsync(u => u.UserLogin == model.Login);
            if (existingUser)
            {
                ModelState.AddModelError("Login", "Пользователь с таким логином уже существует.");
                return View(model);
            }

            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            int roleId = defaultRole?.IdRoles ?? 1;

            var newUser = new User
            {
                UserLogin = model.Login,
                RoleId = roleId,
                IsActive = true,
                UserCreatedAt = DateTime.UtcNow,
                UserPasswordHash = string.Empty
            };
            newUser.UserPasswordHash = _passwordHasher.HashPassword(newUser, model.Password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, newUser.IdUsers.ToString()),
                new Claim(ClaimTypes.Name, newUser.UserLogin),
                new Claim(ClaimTypes.Role, defaultRole?.RoleName ?? "User"),
                new Claim("isActive", "true")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Products");
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
