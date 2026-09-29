using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using DevShop.Models;

var builder = WebApplication.CreateBuilder(args);

// Подключение сервисов MVC и Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Swagger / OpenAPI для практической работы №9
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DevHub Gear REST API",
        Version = "v1",
        Description = "Полноценный RESTful Web API для управления каталогом оборудования разработчика (Практическая работа №9)"
    });
});

// Настройка контекста Entity Framework Core с поддержкой MS SQL Server и автоматическим SQLite-fallback
var sqlServerConnection = builder.Configuration.GetConnectionString("DefaultConnection");
bool useSqlServer = false;

if (!string.IsNullOrWhiteSpace(sqlServerConnection) && 
    !sqlServerConnection.Contains(".db") && 
    !sqlServerConnection.Contains(".sqlite"))
{
    try
    {
        using var testConn = new Microsoft.Data.SqlClient.SqlConnection(sqlServerConnection);
        testConn.Open();
        useSqlServer = true;
    }
    catch
    {
        useSqlServer = false;
    }
}

if (useSqlServer)
{
    builder.Services.AddDbContext<ShopDbContext>(options =>
        options.UseSqlServer(sqlServerConnection));
}
else
{
    Console.WriteLine("[DevShop Database]: SQL Server не обнаружен. Автоматически используется локальная база данных SQLite (devshop.db).");
    builder.Services.AddDbContext<ShopDbContext>(options =>
        options.UseSqlite("Data Source=devshop.db"));
}

// Настройка аутентификации на основе Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// Кэширование в оперативной памяти и сессии
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// Принудительное включение поддержки всех символов Unicode для Razor
builder.Services.AddSingleton(System.Text.Encodings.Web.HtmlEncoder.Create(System.Text.Unicode.UnicodeRanges.All));

var app = builder.Build();

// Инициализация базы данных и сидинг демонстрационных данных
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    
    try
    {
        // Создание базы данных и таблиц, если они еще не существуют
        context.Database.EnsureCreated();

        // 1. Гарантируем системные роли
        if (!context.Roles.Any(r => r.RoleName == "Admin"))
        {
            context.Roles.Add(new Role { RoleName = "Admin" });
            context.SaveChanges();
        }

        if (!context.Roles.Any(r => r.RoleName == "User"))
        {
            context.Roles.Add(new Role { RoleName = "User" });
            context.SaveChanges();
        }

        var adminRole = context.Roles.First(r => r.RoleName == "Admin");
        var userRole = context.Roles.First(r => r.RoleName == "User");

        // 2. Гарантируем учетную запись администратора (admin / admin123)
        var adminUser = context.Users.FirstOrDefault(u => u.UserLogin == "admin");
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserLogin = "admin",
                UserPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                RoleId = adminRole.IdRoles,
                UserCreatedAt = DateTime.Now
            };
            context.Users.Add(adminUser);
            context.SaveChanges();

            context.Carts.Add(new Cart { UserId = adminUser.IdUsers, CartCreatedAt = DateTime.Now });
            context.SaveChanges();
        }

        // 3. Гарантируем тестового пользователя (developer / dev123)
        var devUser = context.Users.FirstOrDefault(u => u.UserLogin == "developer");
        if (devUser == null)
        {
            devUser = new User
            {
                UserLogin = "developer",
                UserPasswordHash = BCrypt.Net.BCrypt.HashPassword("dev123"),
                RoleId = userRole.IdRoles,
                UserCreatedAt = DateTime.Now
            };
            context.Users.Add(devUser);
            context.SaveChanges();

            context.Carts.Add(new Cart { UserId = devUser.IdUsers, CartCreatedAt = DateTime.Now });
            context.SaveChanges();
        }

        // 4. Гарантируем категории оборудования DevHub
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { CategoryName = "Механические клавиатуры", CategoryDescription = "Кастомные клавиатуры с поддержкой QMK/VIA, горячей заменой свитчей и алюминиевым корпусом." },
                new Category { CategoryName = "Мониторы и экраны", CategoryDescription = "Профессиональные 4K/5K IPS мониторы с USB-C Power Delivery для мультимониторных рабочих станций." },
                new Category { CategoryName = "Микрокомпьютеры и IoT", CategoryDescription = "Одноплатные компьютеры, микроконтроллеры и платы разработки для embedded-инженеров." },
                new Category { CategoryName = "Студийное аудио", CategoryDescription = "Микрофоны студийного качества, звуковые карты и закрытые наушники для созвонов и код-ревью." },
                new Category { CategoryName = "Сетевое оборудование", CategoryDescription = "Управляемые коммутаторы, маршрутизаторы и роутеры с поддержкой PoE и Wi-Fi 6/7." },
                new Category { CategoryName = "Эргономика и аксессуары", CategoryDescription = "Кронштейны для мониторов, макропады, кабели в оплетке и подставки для правильной осанки." }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();
        }

        // 5. Гарантируем первоначальный каталог товаров
        if (!context.Products.Any())
        {
            var catKeyboards = context.Categories.First(c => c.CategoryName == "Механические клавиатуры");
            var catMonitors = context.Categories.First(c => c.CategoryName == "Мониторы и экраны");
            var catIoT = context.Categories.First(c => c.CategoryName == "Микрокомпьютеры и IoT");
            var catAudio = context.Categories.First(c => c.CategoryName == "Студийное аудио");
            var catNetwork = context.Categories.First(c => c.CategoryName == "Сетевое оборудование");
            var catErgo = context.Categories.First(c => c.CategoryName == "Эргономика и аксессуары");

            var products = new List<Product>
            {
                new Product
                {
                    ProductName = "Keychron Q1 Pro Wireless (Red Switches)",
                    ProductDescription = "Премиальная кастомная клавиатура на алюминиевом шасси с поддержкой Bluetooth 5.1 и проводного Type-C подключения. Совместима с QMK/VIA для переназначения макросов.",
                    ProductPrice = 18990.00m,
                    ProductStockQuantity = 12,
                    CategoryId = catKeyboards.IdCategories,
                    ProductImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=800&auto=format&fit=crop&q=80"
                },
                new Product
                {
                    ProductName = "Dell UltraSharp 32 4K USB-C Hub (U3223QE)",
                    ProductDescription = "31.5-дюймовый 4K IPS Black монитор с контрастностью 2000:1, охватом 100% sRGB и 98% DCI-P3. Встроенный сетевой порт RJ45 и зарядка ноутбука через USB-C 90W.",
                    ProductPrice = 84500.00m,
                    ProductStockQuantity = 7,
                    CategoryId = catMonitors.IdCategories,
                    ProductImageUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=800&auto=format&fit=crop&q=80"
                },
                new Product
                {
                    ProductName = "Raspberry Pi 5 8GB RAM Starter Kit",
                    ProductDescription = "Четырехъядерный 64-битный процессор Arm Cortex-A76 2.4 ГГц, два выхода micro-HDMI 4Kp60, скоростной интерфейс PCIe 2.0 для подключения NVMe SSD накопителей.",
                    ProductPrice = 12400.00m,
                    ProductStockQuantity = 24,
                    CategoryId = catIoT.IdCategories,
                    ProductImageUrl = "https://images.unsplash.com/photo-1517055729445-fa7d27394b48?w=800&auto=format&fit=crop&q=80"
                },
                new Product
                {
                    ProductName = "Shure MV7X XLR Podcast & Streaming Mic",
                    ProductDescription = "Динамический микрофон с кардиоидной диаграммой направленности и технологией изоляции голоса. Идеальное решение для технических докладов, стримов и командных митингов.",
                    ProductPrice = 21900.00m,
                    ProductStockQuantity = 15,
                    CategoryId = catAudio.IdCategories,
                    ProductImageUrl = "https://images.unsplash.com/photo-1590602847861-f357a9332bbc?w=800&auto=format&fit=crop&q=80"
                },
                new Product
                {
                    ProductName = "Sony WH-1000XM5 Noise Cancelling Headphones",
                    ProductDescription = "Флагманские полноразмерные наушники с лучшей в индустрии системой активного шумоподавления ANC, 30 часами автономной работы и комфортной посадкой для длительного кодинга.",
                    ProductPrice = 36990.00m,
                    ProductStockQuantity = 18,
                    CategoryId = catAudio.IdCategories,
                    ProductImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800&auto=format&fit=crop&q=80"
                },
                new Product
                {
                    ProductName = "Ubiquiti UniFi Switch 8 PoE (60W Gigabit)",
                    ProductDescription = "Управляемый гигабитный 8-портовый коммутатор с 4 портами 802.3af PoE. Централизованное управление через UniFi Controller для домашней лаборатории разработчика.",
                    ProductPrice = 14800.00m,
                    ProductStockQuantity = 9,
                    CategoryId = catNetwork.IdCategories,
                    ProductImageUrl = "https://images.unsplash.com/photo-1544197150-b99a580bb7a8?w=800&auto=format&fit=crop&q=80"
                },
                new Product
                {
                    ProductName = "Ergotron LX Dual Stacking Arm",
                    ProductDescription = "Надежный настольный газлифтный кронштейн из полированного алюминия для независимого крепления двух мониторов весом до 9.1 кг каждый с возможностью портретной ориентации.",
                    ProductPrice = 39900.00m,
                    ProductStockQuantity = 6,
                    CategoryId = catErgo.IdCategories,
                    ProductImageUrl = "https://images.unsplash.com/photo-1593062096033-9a26b09da705?w=800&auto=format&fit=crop&q=80"
                },
                new Product
                {
                    ProductName = "Elgato Stream Deck MK.2 (15 LCD Keys)",
                    ProductDescription = "Контроллер с 15 настраиваемыми ЖК-клавишами для запуска Docker-контейнеров, сборки проектов в CI/CD, переключения веток Git и автоматизации рутинных сценариев.",
                    ProductPrice = 17500.00m,
                    ProductStockQuantity = 14,
                    CategoryId = catErgo.IdCategories,
                    ProductImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=800&auto=format&fit=crop&q=80"
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DevShop Init Warning]: {ex.Message}");
    }
}

// Принудительный заголовок Content-Type для страниц Razor
app.Use((context, next) => 
{ 
    if (!context.Request.Path.StartsWithSegments("/api") && !context.Request.Path.StartsWithSegments("/swagger"))
    {
        context.Response.Headers.ContentType = "text/html; charset=utf-8"; 
    }
    return next(); 
});

// Middleware для Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevHub Gear API v1");
    c.RoutePrefix = "swagger";
});

// Режим обработки исключений
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Регистрация эндпоинтов API (ProductsApiController)
app.MapControllers();

// Маршрут для административной зоны (Areas/Admin)
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Products}/{action=Index}/{id?}");

// Основной публичный маршрут (Каталог товаров)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
