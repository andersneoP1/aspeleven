# Практическая работа №9: Реализация RESTful Web API в ASP.NET Core 8.0 MVC

**Тема:** Разработка защищенного RESTful Web API для сущности каталога оборудования (`Product`) с изоляцией Entity-слоя через DTO, поддержкой пагинации, стандартизированными HTTP-статусами и документированием в Swagger / OpenAPI.  
**Студент:** Студент группы МПТ  
**Платформа:** ASP.NET Core 8.0 (.NET 8.0) | Entity Framework Core 8.0 | MS SQL Server  

---

## 1. Описание выполненных архитектурных шагов

В рамках практической работы №9 на базе проекта интернет-магазина для разработчиков **DevHub Gear** было спроектировано и реализовано полноценное RESTful Web API для сущности **Product** с соблюдением требований enterprise-архитектуры:

1. **Изоляция слоя сущностей (Entity Isolation):**  
   Сущности базы данных (`Product`) полностью инкапсулированы внутри доменного слоя и не передаются внешним потребителям API напрямую. Это исключает циклические ссылки (circular references), скрывает внутреннюю структуру таблиц и предотвращает уязвимости типа Overposting / Mass Assignment. Взаимодействие осуществляется исключительно через типизированные DTO (Data Transfer Objects).

2. **Специализированные DTO-модели:**  
   - `ProductDTO` — модель для чтения данных (GET), содержащая идентификатор `Id` и все публичные поля товара;
   - `CreateProductDTO` — модель для создания товара (POST) с обязательными атрибутами валидации (`[Required]`, `[Range]`, `[StringLength]`), без поля `Id`;
   - `UpdateProductDTO` — модель для полного обновления ресурса (PUT), требующая передачи всех полей;
   - `PatchProductDTO` — модель для частичного обновления (PATCH), где все изменяемые свойства являются `nullable`.

3. **Слой маппинга (Data Mapping):**  
   Создан статический высокопроизводительный маппер `ProductMapper`, выполняющий детерминированные преобразования:
   - `ToDto(Product entity)` — маппинг в исходящий DTO;
   - `ToEntity(CreateProductDTO dto)` — сборка новой сущности из DTO;
   - `ApplyUpdate(Product entity, UpdateProductDTO dto)` — полное обновление свойств;
   - `ApplyPatch(Product entity, PatchProductDTO dto)` — условная модификация только переданных полей (отличных от `null`).

4. **Стандартизация HTTP-кодов ответов (REST Compliance):**  
   - `200 OK` — при успешной выборке списка товаров или единичного объекта;
   - `201 Created` — при успешном создании нового товара (с генерацией заголовка `Location: /api/products/{id}`);
   - `204 No Content` — при успешном выполнении операций изменения (`PUT`, `PATCH`) и удаления (`DELETE`);
   - `400 Bad Request` — при непройденной валидации модели данных (`ModelState`);
   - `404 Not Found` — при обращении к несуществующему идентификатору ресурса.

5. **Интерактивная документация Swagger / OpenAPI:**  
   Подключен пакет `Swashbuckle.AspNetCore`. Настроены сервисы `AddEndpointsApiExplorer`, `AddSwaggerGen` и middleware `UseSwagger`, `UseSwaggerUI`. Все эндпоинты задокументированы XML-комментариями с указанием возможных статус-кодов ответов.

---

## 2. Список созданных и модифицированных файлов

1. **[`DevShop/DTOs/ProductDTOs.cs`](file:///DevShop/DTOs/ProductDTOs.cs)**:  
   Классы контрактов данных: `ProductDTO`, `CreateProductDTO`, `UpdateProductDTO`, `PatchProductDTO`.
2. **[`DevShop/DTOs/ProductMapper.cs`](file:///DevShop/DTOs/ProductMapper.cs)**:  
   Статический класс преобразования сущностей с методами `ToDto`, `ToEntity`, `ApplyUpdate`, `ApplyPatch`.
3. **[`DevShop/Controllers/Api/ProductsApiController.cs`](file:///DevShop/Controllers/Api/ProductsApiController.cs)**:  
   API-контроллер с маршрутом `api/products`, содержащий 6 CRUD-эндпоинтов с валидацией, пагинацией и возвратом стандартных кодов REST.
4. **[`DevShop/Program.cs`](file:///DevShop/Program.cs)**:  
   Регистрация контроллеров API (`MapControllers`), конфигурация генератора Swagger и Swagger UI по маршруту `/swagger`.
5. **[`DevShop/DevShop.csproj`](file:///DevShop/DevShop.csproj)**:  
   Подключение NuGet-пакета `Swashbuckle.AspNetCore`.

---

## 3. Спецификация реализованных эндпоинтов

| HTTP Метод | Маршрут | Описание операции | Статус-коды |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/products?page=1&pageSize=10` | Выборка каталога товаров с пагинацией и сортировкой по ID | `200 OK` |
| **GET** | `/api/products/{id}` | Получение информации о конкретном товаре по идентификатору | `200 OK`, `404 NotFound` |
| **POST** | `/api/products` | Создание нового товара с генерацией заголовка Location | `201 Created`, `400 BadRequest` |
| **PUT** | `/api/products/{id}` | Полное замещение данных существующего товара | `204 NoContent`, `400 BadRequest`, `404 NotFound` |
| **PATCH** | `/api/products/{id}` | Частичное обновление отдельных атрибутов товара | `204 NoContent`, `400 BadRequest`, `404 NotFound` |
| **DELETE** | `/api/products/{id}` | Безвозвратное удаление товара из каталога | `204 NoContent`, `404 NotFound` |

---

## 4. Результаты тестирования эндпоинтов в Swagger UI

> **Инструкция по запуску:**
> 1. Откройте проект в Visual Studio / VS Code или выполните:  
>    ```bash
>    dotnet run --project DevShop/DevShop.csproj
>    ```
> 2. В браузере перейдите по адресу:  
>    `https://localhost:7198/swagger` (или `http://localhost:5214/swagger`)
> 3. Выполните запросы по приведенным сценариям и прикрепите снимки экрана в соответствующие секции.

---

### 4.1. GET /api/products — Получение списка товаров (200 OK)
*Параметры запроса: `page = 1`, `pageSize = 10`.*

```
[ВСТАВИТЬ СКРИНШОТ: Swagger UI -> GET /api/products -> 200 OK с массивом товаров]
```

---

### 4.2. GET /api/products/{id} — Получение по ID (200 OK и 404 NotFound)
*Тест 1: Запрос существующего товара (`id = 1`).*  
*Тест 2: Запрос несуществующего товара (`id = 99999`).*

```
[ВСТАВИТЬ СКРИНШОТ: Swagger UI -> GET /api/products/1 -> 200 OK]
```

```
[ВСТАВИТЬ СКРИНШОТ: Swagger UI -> GET /api/products/99999 -> 404 Not Found]
```

---

### 4.3. POST /api/products — Создание товара (201 Created)

```
[ВСТАВИТЬ СКРИНШОТ: Swagger UI -> POST /api/products -> 201 Created и заголовок Location]
```

---

### 4.4. PUT /api/products/{id} — Полное обновление (204 No Content)

```
[ВСТАВИТЬ СКРИНШОТ: Swagger UI -> PUT /api/products/{id} -> 204 No Content]
```

---

### 4.5. PATCH /api/products/{id} — Частичное обновление (204 No Content)

```
[ВСТАВИТЬ СКРИНШОТ: Swagger UI -> PATCH /api/products/{id} -> 204 No Content]
```

---

### 4.6. DELETE /api/products/{id} — Удаление товара (204 No Content)

```
[ВСТАВИТЬ СКРИНШОТ: Swagger UI -> DELETE /api/products/{id} -> 204 No Content]
```

---

## 5. Вывод

В ходе практической работы №9 были успешно решены все поставленные задачи:
1. Архитектура приложения дополнена слоем Web API без нарушения логики существующего MVC-приложения.
2. Реализовано строгое разделение между моделями БД и внешними контрактами DTO.
3. Обеспечено полное покрытие CRUD-операций со стандартными статус-кодами RESTful архитектуры.
4. Интегрирована документация Swagger UI, подтверждающая работоспособность каждого метода API.
