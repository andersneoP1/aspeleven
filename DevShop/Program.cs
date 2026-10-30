using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using DevShop.Authorization;
using DevShop.Mapping;
using DevShop.Models;
using DevShop.Repositories;
using DevShop.Services;
using DevShop.Swagger;

var builder = WebApplication.CreateBuilder(args);

// 1. Подключение сервисов MVC и Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 2. Swagger / OpenAPI с поддержкой JWT Bearer и динамических замочков
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DevShop REST API",
        Version = "v1",
        Description = "ASP.NET Core 8.0 Web API интернет-магазина оборудования разработчика с JWT, Policy-based авторизацией, CORS и Rate Limiting"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization заголовок с использованием схемы Bearer. Пример: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // Динамический замочек только для [Authorize] методов
    options.OperationFilter<AuthorizeCheckOperationFilter>();
});

// 3. AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// 4. Слой репозиториев (Scoped)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// 5. Слой сервисов (Scoped)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 6. Хеширование паролей PasswordHasher<User>
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// 7. Resource-Based Authorization Handlers
builder.Services.AddSingleton<IAuthorizationHandler, OrderAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CartAuthorizationHandler>();

// 8. Настройка контекста Entity Framework Core с поддержкой MS SQL Server и автоматическим SQLite-fallback
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
    builder.Services.AddDbContext<ShopDbContext>(options =>
        options.UseSqlite("Data Source=devshop.db"));
}

// 9. CORS: Чтение списка разрешенных origin фронтенда из appsettings.json
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                     ?? new[] { "http://localhost:5173", "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-Total-Count");
    });
});

// 10. Rate Limiting с партиционированием по IP-адресу клиента на эндпоинте входа (5 запросов в минуту)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"message\":\"Превышен лимит запросов на вход (Rate Limit: 5 запросов в минуту). Повторите попытку позже.\",\"statusCode\":429}",
            cancellationToken: token);
    };

    options.AddPolicy("login-limiter", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});

// 11. Аутентификация: JWT Bearer (по умолчанию для API) + Cookie (для MVC)
var jwtKey = builder.Configuration["Jwt:Key"] 
             ?? "VeryStrongAndSecretKeyForJwtAuthenticationDevShop2026!MustBe32BytesOrLonger";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DevShopApiServer";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DevShopApiClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Строго возвращаем JSON со статусами 401 и 403 (без 302 Redirect)
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"message\":\"Требуется валидный JWT access-токен (401 Unauthorized)\",\"statusCode\":401}");
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"message\":\"Недостаточно прав для доступа к ресурсу (403 Forbidden)\",\"statusCode\":403}");
        }
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// 12. Политики авторизации (Role-Based и Resource-Based)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireActiveUser", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(ctx =>
        {
            var isActiveClaim = ctx.User.FindFirst("isActive")?.Value;
            return string.Equals(isActiveClaim, "true", StringComparison.OrdinalIgnoreCase);
        });
    });

    options.AddPolicy("RequireAdminRole", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
        policy.RequireAssertion(ctx =>
        {
            var isActiveClaim = ctx.User.FindFirst("isActive")?.Value;
            return string.Equals(isActiveClaim, "true", StringComparison.OrdinalIgnoreCase);
        });
    });

    options.AddPolicy("OwnerOrAdminPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new SameOwnerOrAdminRequirement());
    });
});

// 13. Кэширование и сессии
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// UTF-8 для Razor
builder.Services.AddSingleton(System.Text.Encodings.Web.HtmlEncoder.Create(System.Text.Unicode.UnicodeRanges.All));

var app = builder.Build();

// 14. Инициализация базы данных, миграция структуры и сидинг пользователей
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

    try
    {
        context.Database.EnsureCreated();

        if (context.Database.IsSqlServer())
        {
            try
            {
                context.Database.ExecuteSqlRaw("IF COL_LENGTH('Users', 'is_active') IS NULL ALTER TABLE Users ADD is_active bit NOT NULL DEFAULT 1");
            }
            catch { }
        }

        // Гарантируем роли "User" и "Admin"
        if (!context.Roles.Any(r => r.RoleName == "User"))
        {
            context.Roles.Add(new Role { RoleName = "User" });
            context.SaveChanges();
        }

        if (!context.Roles.Any(r => r.RoleName == "Admin"))
        {
            context.Roles.Add(new Role { RoleName = "Admin" });
            context.SaveChanges();
        }

        var adminRole = context.Roles.First(r => r.RoleName == "Admin");
        var userRole = context.Roles.First(r => r.RoleName == "User");

        // Сидирование / миграция хеша PasswordHasher<User> для admin (admin123)
        var adminUser = context.Users.FirstOrDefault(u => u.UserLogin == "admin");
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserLogin = "admin",
                RoleId = adminRole.IdRoles,
                IsActive = true,
                UserCreatedAt = DateTime.UtcNow,
                UserPasswordHash = string.Empty
            };
            adminUser.UserPasswordHash = passwordHasher.HashPassword(adminUser, "admin123");
            context.Users.Add(adminUser);
        }
        else
        {
            adminUser.UserPasswordHash = passwordHasher.HashPassword(adminUser, "admin123");
            adminUser.RoleId = adminRole.IdRoles;
            adminUser.IsActive = true;
        }

        // Сидирование / миграция хеша PasswordHasher<User> для user (user123)
        var regularUser = context.Users.FirstOrDefault(u => u.UserLogin == "user");
        if (regularUser == null)
        {
            regularUser = new User
            {
                UserLogin = "user",
                RoleId = userRole.IdRoles,
                IsActive = true,
                UserCreatedAt = DateTime.UtcNow,
                UserPasswordHash = string.Empty
            };
            regularUser.UserPasswordHash = passwordHasher.HashPassword(regularUser, "user123");
            context.Users.Add(regularUser);
        }
        else
        {
            regularUser.UserPasswordHash = passwordHasher.HashPassword(regularUser, "user123");
            regularUser.RoleId = userRole.IdRoles;
            regularUser.IsActive = true;
        }

        context.SaveChanges();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DevShop Init Error]: {ex.Message}");
    }
}

// 15. Middleware конвейер
app.Use((context, next) => 
{ 
    if (!context.Request.Path.StartsWithSegments("/api") && !context.Request.Path.StartsWithSegments("/swagger"))
    {
        context.Response.Headers.ContentType = "text/html; charset=utf-8"; 
    }
    return next(); 
});

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevShop API v1");
    c.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// СТРОГИЙ ПОРЯДОК MIDDLEWARE: CORS ДО UseRateLimiter, UseAuthentication и UseAuthorization
app.UseCors("FrontendCorsPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllers();

app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

public partial class Program { }
