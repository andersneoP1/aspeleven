# Практическая работа №11
## JWT-аутентификация, Policy-Based авторизация, CORS, Rate Limiting, AutoMapper и клиентский фронтенд на React в ASP.NET Core 8.0

Полноценное веб-приложение интернет-магазина оборудования разработчика **DevHub Gear** на платформе **ASP.NET Core 8.0** со слоистой архитектурой (*Controllers / Services / Repositories / AutoMapper / EF Core*), расширенное комплексной системой безопасности: **JWT Bearer аутентификацией**, **Role-Based** и **Resource-Based политиками авторизации**, **CORS**, защитой от перебора через **Rate Limiting**, селективной документацией **Swagger/OpenAPI**, набором из 22 тестов на **xUnit** и отдельным клиентским приложением на **React (Vite + TypeScript)**.

Интерфейс серверных Razor-представлений выполнен в аутентичной эстетике **GitHub Primer Dark** (slate canvas, crisp borders, monospace badges, octicons style).

---

## 🚀 Основной стек технологий
* **Платформа:** .NET 8.0 (C# 12)
* **Веб-фреймворк:** ASP.NET Core 8.0 (Web API + MVC / Razor Views)
* **База данных:** MS SQL Server (LocalDB / Express) с поддержкой автоматического fallback на SQLite (`devshop.db`)
* **ORM:** Entity Framework Core 8.0 (`Microsoft.EntityFrameworkCore.SqlServer`, `Sqlite`, `Tools`, `Design`)
* **Слой доступа к данным:** Generic Repository (`IRepository<T>`) + специализированный `OrderRepository`
* **Слой бизнес-логики:** Сервисы `ProductService`, `CategoryService`, `OrderService`, `AuthService`, `JwtTokenService` с унифицированными результатами `ServiceResult<T>` и `PagedResult<T>`
* **Маппинг DTO:** `AutoMapper` 16.2.0 (`MappingProfile`) для всех доменных сущностей
* **Аутентификация:** `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.0 (по умолчанию для API), Cookie Authentication (для Razor Views)
* **Хеширование паролей:** `Microsoft.AspNetCore.Identity.PasswordHasher<User>`
* **Ограничение запросов (Rate Limiting):** `Microsoft.AspNetCore.RateLimiting` (.NET 8 built-in)
* **Документация API:** Swagger / OpenAPI (`Swashbuckle.AspNetCore` 10.2.3) с фильтром `AuthorizeCheckOperationFilter`
* **Тестирование:** xUnit 2.9, `Microsoft.AspNetCore.Mvc.Testing` 8.0 (22 теста: маппинг, авторизация, интеграция API)
* **Клиентский фронтенд:** React 18, TypeScript, Vite 5 (Origin: `http://localhost:5173`)

---

## ⚙️ Реализованный функционал

### 1. Архитектурное разделение и сервисный слой
* **Паттерн Репозиторий:** `IRepository<T>` инкапсулирует базовые CRUD операции, `IOrderRepository` предоставляет методы для подгрузки связанных данных позиций заказов и пользователей.
* **Сервисный слой:** Контроллеры API освобождены от прямой работы с БД и оперируют интерфейсами сервисов (`IProductService`, `ICategoryService`, `IOrderService`, `IAuthService`, `IJwtTokenService`).
* **AutoMapper:** Конфигурация в `MappingProfile.cs` обеспечивает автоматическое сопоставление сущностей и DTO (включая условное частичное обновление полей Patch DTO через `opt.Condition`).

### 2. Безопасность и хранение учетных записей (`PasswordHasher<T>`)
* В модель `User` добавлено поле активности `public bool IsActive { get; set; } = true;` с маппингом на колонку `is_active` в БД.
* Хеширование паролей реализовано через стандартный `Microsoft.AspNetCore.Identity.PasswordHasher<User>`.
* **Автоматическая миграция тестовых пользователей**: При старте приложения в `Program.cs` учетные записи `admin` (`admin123`) и `user` (`user123`) автоматически обновляются валидными хешами `PasswordHasher<User>`.

### 3. JWT Bearer аутентификация и разделение схем
* Подключен `AddJwtBearer` со строгой валидацией токена (`Issuer`, `Audience`, `Lifetime`, `IssuerSigningKey`, `ClockSkew = TimeSpan.Zero`).
* Схема `JwtBearerDefaults.AuthenticationScheme` установлена схемой по умолчанию для API.
* При неавторизованном доступе к API возвращается строго статус `401 Unauthorized` с JSON-ответом (без редиректа 302 на страницу логина).
* В `AuthApiController` (`/api/auth`) реализованы методы:
  * `POST /api/auth/register` — регистрация нового пользователя (роль `User`, `IsActive = true`, возврат токена);
  * `POST /api/auth/login` — вход по логину/паролю (под защитой Rate Limiter, возврат токена);
  * `GET /api/auth/me` — получение профиля текущего пользователя по JWT-токену.

### 4. Policy-Based авторизация (Role-based & Resource-based)
* **Проверка активности**: В политики авторизации добавлена проверка claim `isActive == "true"`. Деактивированный пользователь получает `403 Forbidden` даже при валидном токене.
* **Role-Based Policy (`RequireAdminRole`)**: Административные операции каталога и пользователей доступны только для роли `Admin`.
* **Resource-Based Policy (`OwnerOrAdminPolicy`)**:
  * Реализовано требование `SameOwnerOrAdminRequirement` и обработчики `OrderAuthorizationHandler`, `CartAuthorizationHandler`.
  * Администратор имеет доступ к любым ресурсам, обычный пользователь — **только к своим собственным заказам и корзине** (`order.UserId == currentUserId`). При попытке доступа к чужому ресурсу возвращается `403 Forbidden`.

### 5. CORS и экспонирование заголовков
* Настроена именованная политика `"FrontendCorsPolicy"` для origin фронтенда (`http://localhost:5173`, `http://localhost:3000`).
* Экспонирован заголовок пагинации `X-Total-Count` (`WithExposedHeaders("X-Total-Count")`), позволяющий клиентам читать общее количество записей.

### 6. Rate Limiting на эндпоинте входа
* Политика `"login-limiter"` с лимитом **5 запросов в минуту** с партиционированием по IP (`RemoteIpAddress`).
* При превышении возвращается код `429 Too Many Requests` с информативным JSON-ответом.
* Строгий порядок Middleware:
  ```csharp
  app.UseCors("FrontendCorsPolicy"); // Размещен ДО UseRateLimiter, чтобы 429 содержал CORS-заголовки
  app.UseRateLimiter();
  app.UseAuthentication();
  app.UseAuthorization();
  ```

### 7. Swagger UI с динамическими замками
* Подключен `AuthorizeCheckOperationFilter : IOperationFilter`.
* Иконка замка отображается только у методов, защищенных атрибутом `[Authorize]`. Общедоступные эндпоинты отображаются без замков.

### 8. Отдельный клиентский React-фронтенд (`client-app`)
* SPA-приложение на React (Vite + TypeScript) с интерактивным тестовым пультом (Testing Sandbox) для демонстрации 6 сценариев:
  1. **Тест 1 (200 OK — свой ресурс)**: Запрос `GET /api/auth/me`.
  2. **Тест 2 (403 Forbidden — чужой ресурс)**: Запрос `GET /api/orders/9999` через Resource-based policy.
  3. **Тест 3 (403 Forbidden — Role-based policy)**: Запрос `GET /api/users` (403 для `user`, 200 для `admin`).
  4. **Тест 4 (401 Unauthorized)**: Запрос к защищенному эндпоинту без токена.
  5. **Тест 5 (429 Too Many Requests)**: Серия из 6 запросов на вход для демонстрации Rate Limiter.
  6. **Тест 6 (CORS + Exposed Header)**: Запрос `GET /api/products` с чтением заголовка `X-Total-Count`.

---

## 🧪 Автоматизированное тестирование (`DevShop.Tests`)

Проект содержит **22 автоматизированных теста** на фреймворке **xUnit**, покрывающих 100% критических сценариев:
1. `MappingProfileTests` (10 тестов): тестирование всех проекций AutoMapper.
2. `AuthAndPolicyTests` (6 тестов): модульные тесты хеширования, клеймов токена и политик доступа.
3. `ApiEndpointIntegrationTests` (6 тестов): сквозные интеграционные тесты с реальным веб-сервером через `WebApplicationFactory<Program>`.

Запуск тестов:
```powershell
dotnet test pr11ASP_Af.sln
```

---

## 🚀 Инструкция по запуску

### Запуск серверной части (Web API & MVC)
```powershell
dotnet run --project DevShop/DevShop.csproj
```
- Витрина магазина: `http://localhost:5214/`
- Документация Swagger UI: `http://localhost:5214/swagger`

### Запуск клиентского приложения React (Sandbox)
```powershell
cd client-app
npm run dev
```
- Клиентский пульт: `http://localhost:5173/`

### Тестовые учетные записи:
* **Администратор:** логин `admin`, пароль `admin123`
* **Пользователь:** логин `user`, пароль `user123`
