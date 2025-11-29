using CoffeeShops.DataAccess.Context;
using CoffeeShops.DataAccess.Repositories;
using CoffeeShops.Domain.Exceptions.CategoryServiceExceptions;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.DTOs.CoffeeShop;
using CoffeeShops.DTOs.Company;
using CoffeeShops.DTOs.Drink;
using CoffeeShops.DTOs.DrinksCategory;
using CoffeeShops.DTOs.FavCoffeeShops;
using CoffeeShops.DTOs.FavDrinks;
using CoffeeShops.DTOs.Menu;
using CoffeeShops.DTOs.Pagination;
using CoffeeShops.DTOs.User;
using CoffeeShops.DTOs.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var readOnlyMode = builder.Configuration.GetValue<bool>("ReadOnlyMode");
var instanceName = builder.Configuration.GetValue<string>("InstanceName") ?? "data-main";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "data-service")
    .Enrich.WithProperty("Instance", instanceName)
    .WriteTo.Console()
    .WriteTo.File($"logs/dataservice-{instanceName}-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<UserDbContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("UserConnection")));
builder.Services.AddDbContext<AdminDbContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("AdminConnection")));
builder.Services.AddDbContext<ModerDbContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("ModerConnection")));
builder.Services.AddTransient<IDbContextFactory, DbContextFactory>();

builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();
builder.Services.AddTransient<ICoffeeShopRepository, CoffeeShopRepository>();
builder.Services.AddTransient<ICompanyRepository, CompanyRepository>();
builder.Services.AddTransient<IDrinksCategoryRepository, DrinksCategoryRepository>();
builder.Services.AddTransient<IDrinkRepository, DrinkRepository>();
builder.Services.AddTransient<IFavCoffeeShopsRepository, FavCoffeeShopsRepository>();
builder.Services.AddTransient<IFavDrinksRepository, FavDrinksRepository>();
builder.Services.AddTransient<IMenuRepository, MenuRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Instance-Name"] = instanceName;
    var method = context.Request.Method;
    var isWrite = !HttpMethods.IsGet(method) &&
                  !HttpMethods.IsHead(method) &&
                  !HttpMethods.IsOptions(method);

    if (readOnlyMode && isWrite)
    {
        Log.Warning("Write attempt blocked on read-only data service instance {Instance}", instanceName);
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new Error(
            message: "Instance runs in read-only mode",
            code: 403
        ));
        return;
    }

    await next();
});

var data = app.MapGroup("/api/data");

data.MapGet("/health", () => Results.Ok(new { status = "ok", instance = instanceName }));

data.MapGet("/categories/{id:guid}", async Task<IResult> (Guid id, [FromQuery] int? role, ICategoryRepository repo) =>
{
    try
    {
        var category = await repo.GetCategoryByIdAsync(id, NormalizeRole(role));
        return category is null
            ? Results.NotFound(new Error("Category not found", 404))
            : Results.Ok(category);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/categories", async Task<IResult> ([AsParameters] PagedQuery query, ICategoryRepository repo) =>
{
    try
    {
        var page = NormalizePage(query.Page);
        var limit = NormalizeLimit(query.Limit);
        (var categories, var total) = await repo.GetAllCategoriesAsync(page, limit, NormalizeRole(query.Role));
        if (categories == null || categories.Count == 0)
        {
            return Results.NotFound(new Error("Categories not found", 404));
        }

        return Results.Ok(new PaginatedResponse<Category>
        {
            Data = categories,
            Total = total,
            Page = page,
            Limit = limit
        });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapPost("/categories", async Task<IResult> ([FromBody] Category category, [FromQuery] int? role, ICategoryRepository repo) =>
{
    try
    {
        var id = await repo.AddCategoryAsync(category, NormalizeRole(role));
        return Results.Created($"/api/data/categories/{id}", new CreateResponse { Id = id.ToString() });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/companies/{id:guid}", async Task<IResult> (Guid id, [FromQuery] int? role, ICompanyRepository repo) =>
{
    try
    {
        var company = await repo.GetCompanyByIdAsync(id, NormalizeRole(role));
        return company is null
            ? Results.NotFound(new Error("Company not found", 404))
            : Results.Ok(company);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/companies", async Task<IResult> ([AsParameters] CompanyPagedQuery query, ICompanyRepository repo) =>
{
    try
    {
        var page = NormalizePage(query.Page);
        var limit = NormalizeLimit(query.Limit);
        var filters = new CompanyFilters { Id_drink = query.DrinkId };

        (var companies, var total) = await repo.GetAllCompaniesAsync(filters, page, limit, NormalizeRole(query.Role));
        if (companies == null || companies.Count == 0)
        {
            return Results.NotFound(new Error("Companies not found", 404));
        }

        return Results.Ok(new PaginatedResponse<Company>
        {
            Data = companies,
            Total = total,
            Page = page,
            Limit = limit
        });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapPost("/companies", async Task<IResult> ([FromBody] Company company, [FromQuery] int? role, ICompanyRepository repo) =>
{
    try
    {
        var id = await repo.AddAsync(company, NormalizeRole(role));
        return Results.Created($"/api/data/companies/{id}", new CreateResponse { Id = id.ToString() });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapDelete("/companies/{id:guid}", async Task<IResult> (Guid id, [FromQuery] int? role, ICompanyRepository repo) =>
{
    try
    {
        await repo.RemoveAsync(id, NormalizeRole(role));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapDelete("/companies/{id:guid}/with-data", async Task<IResult> (Guid id, [FromQuery] int? role, ICompanyRepository repo) =>
{
    try
    {
        await repo.RemoveCompanyWithAllDataAsync(id, NormalizeRole(role));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/coffeeshops/{id:guid}", async Task<IResult> (Guid id, [FromQuery] int? role, ICoffeeShopRepository repo) =>
{
    try
    {
        var cs = await repo.GetCoffeeShopByIdAsync(id, NormalizeRole(role));
        return cs is null
            ? Results.NotFound(new Error("Coffee shop not found", 404))
            : Results.Ok(cs);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/coffeeshops", async Task<IResult> ([AsParameters] CoffeeShopPagedQuery query, ICoffeeShopRepository repo) =>
{
    try
    {
        var page = NormalizePage(query.Page);
        var limit = NormalizeLimit(query.Limit);
        var filters = new CoffeeShopFilters
        {
            Id_company = query.CompanyId,
            Id_user = query.UserId,
            OnlyFavorites = query.OnlyFavorites
        };

        (var items, var total) = await repo.GetAllCoffeeShopsAsync(filters, page, limit, NormalizeRole(query.Role));
        if (items == null || items.Count == 0)
        {
            return Results.NotFound(new Error("Coffee shops not found", 404));
        }

        return Results.Ok(new PaginatedResponse<CoffeeShop>
        {
            Data = items,
            Total = total,
            Page = page,
            Limit = limit
        });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/companies/{companyId:guid}/coffeeshops", async Task<IResult> (Guid companyId, [AsParameters] CoffeeShopCompanyQuery query, ICoffeeShopRepository repo) =>
{
    try
    {
        var page = NormalizePage(query.Page);
        var limit = NormalizeLimit(query.Limit);
        var filters = new CoffeeShopFilters
        {
            Id_company = companyId,
            Id_user = query.UserId,
            OnlyFavorites = query.OnlyFavorites
        };

        (var items, var total) = await repo.GetCoffeeShopsByCompanyIdAsync(companyId, filters, page, limit, NormalizeRole(query.Role));
        if (items == null || items.Count == 0)
        {
            return Results.NotFound(new Error("Coffee shops for company not found", 404));
        }

        return Results.Ok(new PaginatedResponse<CoffeeShop>
        {
            Data = items,
            Total = total,
            Page = page,
            Limit = limit
        });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapPost("/coffeeshops", async Task<IResult> ([FromBody] CoffeeShop shop, [FromQuery] int? role, ICoffeeShopRepository repo) =>
{
    try
    {
        var id = await repo.AddAsync(shop, NormalizeRole(role));
        return Results.Created($"/api/data/coffeeshops/{id}", new CreateResponse { Id = id.ToString() });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapDelete("/coffeeshops/{id:guid}", async Task<IResult> (Guid id, [FromQuery] int? role, ICoffeeShopRepository repo) =>
{
    try
    {
        await repo.RemoveAsync(id, NormalizeRole(role));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapDelete("/companies/{companyId:guid}/coffeeshops", async Task<IResult> (Guid companyId, [FromQuery] int? role, ICoffeeShopRepository repo) =>
{
    try
    {
        await repo.RemoveAllByCompanyIdAsync(companyId, NormalizeRole(role));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/drinks/{id:guid}", async Task<IResult> (Guid id, [FromQuery] int? role, IDrinkRepository repo) =>
{
    try
    {
        var drink = await repo.GetDrinkByIdAsync(id, NormalizeRole(role));
        return drink is null
            ? Results.NotFound(new Error("Drink not found", 404))
            : Results.Ok(drink);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/drinks", async Task<IResult> ([AsParameters] DrinkPagedQuery query, IDrinkRepository repo) =>
{
    try
    {
        var page = NormalizePage(query.Page);
        var limit = NormalizeLimit(query.Limit);
        var filters = new DrinkFilters
        {
            OnlyFavorites = query.OnlyFavorites,
            Id_user = query.UserId,
            DrinkName = query.DrinkName,
            CategoryName = query.CategoryName
        };

        (var drinks, var total) = await repo.GetAllDrinksAsync(filters, page, limit, NormalizeRole(query.Role));
        if (drinks == null || drinks.Count == 0)
        {
            return Results.NotFound(new Error("Drinks not found", 404));
        }

        return Results.Ok(new PaginatedResponse<Drink>
        {
            Data = drinks,
            Total = total,
            Page = page,
            Limit = limit
        });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapPost("/drinks", async Task<IResult> ([FromBody] Drink drink, [FromQuery] int? role, IDrinkRepository repo) =>
{
    try
    {
        var id = await repo.AddAsync(drink, NormalizeRole(role));
        return Results.Created($"/api/data/drinks/{id}", new CreateResponse { Id = id.ToString() });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapDelete("/drinks/{id:guid}", async Task<IResult> (Guid id, [FromQuery] int? role, IDrinkRepository repo) =>
{
    try
    {
        await repo.RemoveAsync(id, NormalizeRole(role));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/drinks/{drinkId:guid}/categories", async Task<IResult> (Guid drinkId, [FromQuery] int? role, IDrinksCategoryRepository repo) =>
{
    try
    {
        var categories = await repo.GetAllCategoriesByDrinkIdAsync(drinkId, NormalizeRole(role));
        if (categories == null || categories.Count == 0)
        {
            return Results.NotFound(new Error("Categories not found", 404));
        }

        return Results.Ok(categories);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapPost("/drinks-categories", async Task<IResult> ([FromBody] DrinksCategory drinksCategory, [FromQuery] int? role, IDrinksCategoryRepository repo) =>
{
    try
    {
        await repo.AddAsync(drinksCategory, NormalizeRole(role));
        return Results.Created($"/api/data/drinks-categories", new { drinksCategory.Id_drink, drinksCategory.Id_category });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/drinks-categories/record", async Task<IResult> ([FromQuery] Guid drinkId, [FromQuery] Guid categoryId, [FromQuery] int? role, IDrinksCategoryRepository repo) =>
{
    try
    {
        var record = await repo.GetRecordAsync(drinkId, categoryId, NormalizeRole(role));
        return record is null ? Results.NotFound() : Results.Ok(record);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapDelete("/drinks/{drinkId:guid}/drinks-categories", async Task<IResult> (Guid drinkId, [FromQuery] int? role, IDrinksCategoryRepository repo) =>
{
    try
    {
        await repo.RemoveByDrinkIdAsync(drinkId, NormalizeRole(role));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapDelete("/drinks-categories", async Task<IResult> ([FromQuery] Guid drinkId, [FromQuery] Guid categoryId, [FromQuery] int? role, IDrinksCategoryRepository repo) =>
{
    try
    {
        await repo.RemoveAsync(drinkId, categoryId, NormalizeRole(role));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapPost("/favorites/coffeeshops", async Task<IResult> ([FromBody] FavCoffeeShops fav, [FromQuery] int? role, IFavCoffeeShopsRepository repo) =>
{
    try
    {
        await repo.AddAsync(fav, NormalizeRole(role));
        return Results.Created($"/api/data/favorites/coffeeshops", new { fav.Id_user, fav.Id_coffeeshop });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/favorites/coffeeshops", async Task<IResult> ([AsParameters] FavoritesQuery query, IFavCoffeeShopsRepository repo) =>
{
    try
    {
        var page = NormalizePage(query.Page);
        var limit = NormalizeLimit(query.Limit);
        (var items, var total) = await repo.GetAllFavCoffeeShopsAsync(query.UserId, page, limit, NormalizeRole(query.Role));
        if (items == null || items.Count == 0)
        {
            return Results.NotFound(new Error("Favorite coffee shops not found", 404));
        }

        return Results.Ok(new PaginatedResponse<CoffeeShop>
        {
            Data = items,
            Total = total,
            Page = page,
            Limit = limit
        });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/favorites/coffeeshops/record", async Task<IResult> ([FromQuery] Guid userId, [FromQuery] Guid coffeeShopId, [FromQuery] int? role, IFavCoffeeShopsRepository repo) =>
{
    try
    {
        var record = await repo.GetRecordAsync(coffeeShopId, userId, NormalizeRole(role));
        return record is null ? Results.NotFound() : Results.Ok(record);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapDelete("/favorites/coffeeshops", async Task<IResult> ([FromQuery] Guid userId, [FromQuery] Guid coffeeShopId, [FromQuery] int? role, IFavCoffeeShopsRepository repo) =>
{
    try
    {
        await repo.RemoveAsync(userId, coffeeShopId, NormalizeRole(role));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapDelete("/favorites/coffeeshops/by-shop/{coffeeShopId:guid}", async Task<IResult> (Guid coffeeShopId, [FromQuery] int? role, IFavCoffeeShopsRepository repo) =>
{
    try
    {
        await repo.RemoveByCoffeeShopIdAsync(coffeeShopId, NormalizeRole(role));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapPost("/favorites/drinks", async Task<IResult> ([FromBody] FavDrinks fav, [FromQuery] int? role, IFavDrinksRepository repo) =>
{
    try
    {
        await repo.AddAsync(fav, NormalizeRole(role));
        return Results.Created($"/api/data/favorites/drinks", new { fav.Id_user, fav.Id_drink });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/favorites/drinks", async Task<IResult> ([AsParameters] FavoritesQuery query, IFavDrinksRepository repo) =>
{
    try
    {
        var page = NormalizePage(query.Page);
        var limit = NormalizeLimit(query.Limit);
        (var items, var total) = await repo.GetAllFavDrinksAsync(query.UserId, page, limit, NormalizeRole(query.Role));
        if (items == null || items.Count == 0)
        {
            return Results.NotFound(new Error("Favorite drinks not found", 404));
        }

        return Results.Ok(new PaginatedResponse<Drink>
        {
            Data = items,
            Total = total,
            Page = page,
            Limit = limit
        });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/favorites/drinks/record", async Task<IResult> ([FromQuery] Guid userId, [FromQuery] Guid drinkId, [FromQuery] int? role, IFavDrinksRepository repo) =>
{
    try
    {
        var record = await repo.GetRecordAsync(drinkId, userId, NormalizeRole(role));
        return record is null ? Results.NotFound() : Results.Ok(record);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapDelete("/favorites/drinks", async Task<IResult> ([FromQuery] Guid userId, [FromQuery] Guid drinkId, [FromQuery] int? role, IFavDrinksRepository repo) =>
{
    try
    {
        await repo.RemoveAsync(userId, drinkId, NormalizeRole(role));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapDelete("/favorites/drinks/by-drink/{drinkId:guid}", async Task<IResult> (Guid drinkId, [FromQuery] int? role, IFavDrinksRepository repo) =>
{
    try
    {
        await repo.RemoveByDrinkIdAsync(drinkId, NormalizeRole(role));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/menu/company/{companyId:guid}", async Task<IResult> (Guid companyId, [AsParameters] PagedQuery query, IMenuRepository repo) =>
{
    try
    {
        var page = NormalizePage(query.Page);
        var limit = NormalizeLimit(query.Limit);
        (var items, var total) = await repo.GetMenuByCompanyId(companyId, page, limit, NormalizeRole(query.Role));
        if (items == null || items.Count == 0)
        {
            return Results.NotFound(new Error("Menu not found", 404));
        }

        return Results.Ok(new PaginatedResponse<Menu>
        {
            Data = items,
            Total = total,
            Page = page,
            Limit = limit
        });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/menu/drink/{drinkId:guid}/companies", async Task<IResult> (Guid drinkId, [AsParameters] PagedQuery query, IMenuRepository repo) =>
{
    try
    {
        var page = NormalizePage(query.Page);
        var limit = NormalizeLimit(query.Limit);
        (var items, var total) = await repo.GetCompaniesByDrinkIdAsync(drinkId, page, limit, NormalizeRole(query.Role));
        if (items == null || items.Count == 0)
        {
            return Results.NotFound(new Error("Companies for drink not found", 404));
        }

        return Results.Ok(new PaginatedResponse<Company>
        {
            Data = items,
            Total = total,
            Page = page,
            Limit = limit
        });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapPost("/menu", async Task<IResult> ([FromBody] Menu menu, [FromQuery] int? role, IMenuRepository repo) =>
{
    try
    {
        await repo.AddAsync(menu, NormalizeRole(role));
        return Results.Created("/api/data/menu", new { ok = true });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapDelete("/menu", async Task<IResult> ([FromQuery] Guid drinkId, [FromQuery] Guid companyId, [FromQuery] int? role, IMenuRepository repo) =>
{
    try
    {
        await repo.RemoveAsync(drinkId, companyId, NormalizeRole(role));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/users/id/{id:guid}", async Task<IResult> (Guid id, [FromQuery] int? role, IUserRepository repo) =>
{
    try
    {
        var user = await repo.GetUserByIdAsync(id, NormalizeRole(role));
        return user is null
            ? Results.NotFound(new Error("User not found", 404))
            : Results.Ok(user);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/users/login/{login}", async Task<IResult> (string login, [FromQuery] int? role, IUserRepository repo) =>
{
    try
    {
        var user = await repo.GetUserByLoginAsync(login, NormalizeRole(role));
        return user is null
            ? Results.NotFound(new Error("User not found", 404))
            : Results.Ok(user);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapGet("/users", async Task<IResult> ([AsParameters] UserPagedQuery query, IUserRepository repo) =>
{
    try
    {
        var page = NormalizePage(query.Page);
        var limit = NormalizeLimit(query.Limit);
        var filters = new UserFilters
        {
            Login = query.Login,
            UserRole = query.UserRole
        };

        (var users, var total) = await repo.GetAllUsersAsync(filters, page, limit, NormalizeRole(query.Role));
        if (users == null || users.Count == 0)
        {
            return Results.NotFound(new Error("Users not found", 404));
        }

        return Results.Ok(new PaginatedResponse<User>
        {
            Data = users,
            Total = total,
            Page = page,
            Limit = limit
        });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapPost("/users", async Task<IResult> ([FromBody] User user, [FromQuery] int? role, IUserRepository repo) =>
{
    try
    {
        var id = await repo.AddUserAsync(user, NormalizeRole(role));
        return Results.Created($"/api/data/users/id/{id}", new CreateResponse { Id = id.ToString() });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapPut("/users/{id:guid}", async Task<IResult> (Guid id, [FromBody] User user, [FromQuery] int? role, IUserRepository repo) =>
{
    try
    {
        user.Id_user = id;
        await repo.UpdateUserAsync(user, NormalizeRole(role));
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapPatch("/users/{id:guid}", async Task<IResult> (Guid id, [FromBody] UserPartialUpdate payload, [FromQuery] int? role, IUserRepository repo) =>
{
    try
    {
        await repo.PartialUpdateUserAsync(id, payload.Login, payload.Password, payload.Email, NormalizeRole(role));
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapPost("/users/{id:guid}/role", async Task<IResult> (Guid id, [FromBody] UpdateUserRoleRequest payload, [FromQuery] int? role, IUserRepository repo) =>
{
    try
    {
        await repo.UpdateUserRightsAsync(id, payload.Role, NormalizeRole(role));
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

data.MapDelete("/users/{id:guid}", async Task<IResult> (Guid id, [FromQuery] int? role, IUserRepository repo) =>
{
    try
    {
        await repo.DeleteUserAsync(id, NormalizeRole(role));
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

app.Run();

static int NormalizePage(int page) => page < 1 ? 1 : page;
static int NormalizeLimit(int limit) => limit < 1 || limit > 100 ? 20 : limit;
static int NormalizeRole(int? role) => role is null or <= 0 ? 1 : role.Value;

static IResult MapException(Exception ex)
{
    return ex switch
    {
        CategoryUniqueException cue => Results.Conflict(new Error("Category already exists", 409, cue.Message)),
        DrinkNameAlreadyExistsException dne => Results.Conflict(new Error("Drink already exists", 409, dne.Message)),
        UserNotFoundException unf => Results.NotFound(new Error("User not found", 404, unf.Message)),
        UserLastAdminException ula => Results.BadRequest(new Error("Cannot remove last admin", 400, ula.Message)),
        _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
    };
}

record PagedQuery(int Page = 1, int Limit = 20, int? Role = 1);
record CompanyPagedQuery(Guid? DrinkId, int Page = 1, int Limit = 20, int? Role = 1);
record CoffeeShopPagedQuery(Guid? CompanyId, Guid? UserId, bool OnlyFavorites = false, int Page = 1, int Limit = 20, int? Role = 1);
record CoffeeShopCompanyQuery(Guid? UserId, bool OnlyFavorites = false, int Page = 1, int Limit = 20, int? Role = 1);
record DrinkPagedQuery(Guid? UserId, bool OnlyFavorites = false, string? DrinkName = null, string? CategoryName = null, int Page = 1, int Limit = 20, int? Role = 1);
record UserPagedQuery(string? Login, int? UserRole, int Page = 1, int Limit = 20, int? Role = 1);
record FavoritesQuery(Guid UserId, int Page = 1, int Limit = 20, int? Role = 1);
record UserPartialUpdate(string Login, string Password, string Email);
record UpdateUserRoleRequest(int Role);
