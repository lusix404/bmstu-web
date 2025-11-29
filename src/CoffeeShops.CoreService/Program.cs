using CoffeeShops.CoreService.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.DTOs.Auth;
using CoffeeShops.DTOs.Category;
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
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Services.Services;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.Domain.Converters;
using Microsoft.AspNetCore.Http.Json;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var instanceName = builder.Configuration.GetValue<string>("InstanceName") ?? "core-main";
var readOnlyMode = builder.Configuration.GetValue<bool>("ReadOnlyMode");
var dataBaseUrl = builder.Configuration.GetValue<string>("DataService:BaseUrl") ?? "http://data-main:8080";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "core-service")
    .Enrich.WithProperty("Instance", instanceName)
    .WriteTo.Console()
    .WriteTo.File($"logs/coreservice-{instanceName}-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddHttpClient("data", client =>
{
    client.BaseAddress = new Uri(dataBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<ICategoryRepository>(sp =>
    new HttpCategoryRepository(sp.GetRequiredService<IHttpClientFactory>().CreateClient("data"),
        sp.GetRequiredService<ILogger<HttpCategoryRepository>>()));
builder.Services.AddScoped<ICoffeeShopRepository>(sp =>
    new HttpCoffeeShopRepository(sp.GetRequiredService<IHttpClientFactory>().CreateClient("data"),
        sp.GetRequiredService<ILogger<HttpCoffeeShopRepository>>()));
builder.Services.AddScoped<ICompanyRepository>(sp =>
    new HttpCompanyRepository(sp.GetRequiredService<IHttpClientFactory>().CreateClient("data"),
        sp.GetRequiredService<ILogger<HttpCompanyRepository>>()));
builder.Services.AddScoped<IDrinkRepository>(sp =>
    new HttpDrinkRepository(sp.GetRequiredService<IHttpClientFactory>().CreateClient("data"),
        sp.GetRequiredService<ILogger<HttpDrinkRepository>>()));
builder.Services.AddScoped<IDrinksCategoryRepository>(sp =>
    new HttpDrinksCategoryRepository(sp.GetRequiredService<IHttpClientFactory>().CreateClient("data"),
        sp.GetRequiredService<ILogger<HttpDrinksCategoryRepository>>()));
builder.Services.AddScoped<IFavCoffeeShopsRepository>(sp =>
    new HttpFavCoffeeShopsRepository(sp.GetRequiredService<IHttpClientFactory>().CreateClient("data"),
        sp.GetRequiredService<ILogger<HttpFavCoffeeShopsRepository>>()));
builder.Services.AddScoped<IFavDrinksRepository>(sp =>
    new HttpFavDrinksRepository(sp.GetRequiredService<IHttpClientFactory>().CreateClient("data"),
        sp.GetRequiredService<ILogger<HttpFavDrinksRepository>>()));
builder.Services.AddScoped<IMenuRepository>(sp =>
    new HttpMenuRepository(sp.GetRequiredService<IHttpClientFactory>().CreateClient("data"),
        sp.GetRequiredService<ILogger<HttpMenuRepository>>()));
builder.Services.AddScoped<IUserRepository>(sp =>
    new HttpUserRepository(sp.GetRequiredService<IHttpClientFactory>().CreateClient("data"),
        sp.GetRequiredService<ILogger<HttpUserRepository>>()));

builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<ICoffeeShopService, CoffeeShopService>();
builder.Services.AddTransient<ICompanyService, CompanyService>();
builder.Services.AddTransient<IDrinkService, DrinkService>();
builder.Services.AddTransient<IDrinksCategoryService, DrinksCategoryService>();
builder.Services.AddTransient<IFavCoffeeShopsService, FavCoffeeShopsService>();
builder.Services.AddTransient<IFavDrinksService, FavDrinksService>();
builder.Services.AddTransient<IMenuService, MenuService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IJwtService, JwtService>();

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
        Log.Warning("Write attempt blocked on read-only core service instance {Instance}", instanceName);
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new Error("Instance runs in read-only mode", 403));
        return;
    }

    await next();
});

var core = app.MapGroup("/api/core");

core.MapGet("/health", () => Results.Ok(new { status = "ok", instance = instanceName, dataService = dataBaseUrl }));

core.MapPost("/auth/login", async Task<IResult> (LoginRequest request, IUserService userService) =>
{
    try
    {
        var result = await userService.Login(request.Login, request.Password);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapPost("/auth/register", async Task<IResult> (RegisterRequest request, IUserService userService) =>
{
    try
    {
        var id = await userService.Registrate(RegisterRequestConverter.ConvertToDomainModel(request));
        return Results.Created($"/api/core/users/{id}", new CreateResponse { Id = id.ToString() });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/users", async Task<IResult> (int page, int limit, string? login, int? userRole, int role, IUserService userService) =>
{
    try
    {
        var filters = new UserFilters { Login = login, UserRole = userRole };
        var result = await userService.GetAllUsersAsync(filters, page, limit, role);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/users/{id:guid}", async Task<IResult> (Guid id, int role, IUserService userService) =>
{
    try
    {
        var user = await userService.GetUserByIdAsync(id, role);
        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/users/by-login/{login}", async Task<IResult> (string login, int role, IUserService userService) =>
{
    try
    {
        var user = await userService.GetUserByLoginAsync(login, role);
        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapPut("/users/{id:guid}", async Task<IResult> (Guid id, UpdateUser update, int role, IUserService userService) =>
{
    try
    {
        var existing = await userService.GetUserByIdAsync(id, role);
        var domainUser = new User(id, existing!.Id_role, update.Login, update.Password, existing.BirthDate, update.Email);
        domainUser.SetPassword(domainUser.PasswordHash);
        await userService.UpdateUserAsync(domainUser, role);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapPatch("/users/{id:guid}/role", async Task<IResult> (Guid id, UpdateUserRole request, int role, IUserService userService) =>
{
    try
    {
        var newRole = UserRoleExtensions.ToRoleIntFromString(request.User_role);
        await userService.UpdateUserRightsAsync(id, newRole, role);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapDelete("/users/{id:guid}", async Task<IResult> (Guid id, int role, IUserService userService) =>
{
    try
    {
        await userService.DeleteUserAsync(id, role);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/categories", async Task<IResult> (int page, int limit, int role, ICategoryService categoryService) =>
{
    try
    {
        var result = await categoryService.GetAllCategoriesAsync(page, limit, role);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/categories/{id:guid}", async Task<IResult> (Guid id, int role, ICategoryService categoryService) =>
{
    try
    {
        var category = await categoryService.GetCategoryByIdAsync(id, role);
        return Results.Ok(category);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapPost("/categories", async Task<IResult> (CreateCategoryRequest request, int role, ICategoryService categoryService) =>
{
    try
    {
        var id = await categoryService.AddCategoryAsync(CreateCategoryRequestConverter.ConvertToDomainModel(request), role);
        return Results.Created($"/api/core/categories/{id}", new CreateResponse { Id = id.ToString() });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/companies", async Task<IResult> (int page, int limit, Guid? drinkId, int role, ICompanyService companyService) =>
{
    try
    {
        var filters = new CompanyFilters { Id_drink = drinkId };
        var result = await companyService.GetAllCompaniesAsync(filters, page, limit, role);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/companies/{id:guid}", async Task<IResult> (Guid id, int role, ICompanyService companyService) =>
{
    try
    {
        var company = await companyService.GetCompanyByIdAsync(id, role);
        return Results.Ok(company);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapPost("/companies", async Task<IResult> (CreateCompanyRequest request, int role, ICompanyService companyService) =>
{
    try
    {
        var id = await companyService.AddCompanyAsync(CreateCompanyRequestConverter.ConvertToDomainModel(request), role);
        return Results.Created($"/api/core/companies/{id}", new CreateResponse { Id = id.ToString() });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapDelete("/companies/{id:guid}", async Task<IResult> (Guid id, int role, ICompanyService companyService) =>
{
    try
    {
        await companyService.DeleteCompanyAsync(id, role);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapDelete("/companies/{id:guid}/with-data", async Task<IResult> (Guid id, int role, ICompanyService companyService) =>
{
    try
    {
        await companyService.DeleteCompanyAsync(id, role);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/coffeeshops", async Task<IResult> (int page, int limit, Guid? companyId, Guid? userId, ICoffeeShopService service, bool onlyFavorites = false, int role = 1) =>
{
    try
    {
        var filters = new CoffeeShopFilters
        {
            Id_company = companyId,
            Id_user = userId,
            OnlyFavorites = onlyFavorites
        };
        var result = await service.GetAllCoffeeShopsAsync(filters, page, limit, role);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/coffeeshops/{id:guid}", async Task<IResult> (Guid id, ICoffeeShopService service, int role = 1) =>
{
    try
    {
        var shop = await service.GetCoffeeShopByIdAsync(id, role);
        return Results.Ok(shop);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/companies/{companyId:guid}/coffeeshops", async Task<IResult> (Guid companyId, int page, int limit, Guid? userId, ICoffeeShopService service, bool onlyFavorites = false, int role = 1) =>
{
    try
    {
        var filters = new CoffeeShopFilters { Id_company = companyId, Id_user = userId, OnlyFavorites = onlyFavorites };
        var result = await service.GetCoffeeShopsByCompanyIdAsync(companyId, filters, page, limit, role);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapPost("/coffeeshops", async Task<IResult> (CreateCoffeeShopRequest request, ICoffeeShopService service, int role = 1) =>
{
    try
    {
        var id = await service.AddCoffeeShopAsync(CreateCoffeeShopRequestConverter.ConvertToDomainModel(request), role);
        return Results.Created($"/api/core/coffeeshops/{id}", new CreateResponse { Id = id.ToString() });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapDelete("/coffeeshops/{id:guid}", async Task<IResult> (Guid id, int role, ICoffeeShopService service) =>
{
    try
    {
        await service.DeleteCoffeeShopAsync(id, role);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/drinks", async Task<IResult> (int page, int limit, Guid? userId, IDrinkService service, bool onlyFavorites = false, string? drinkName = null, string? categoryName = null, int role = 1) =>
{
    try
    {
        var filters = new DrinkFilters
        {
            Id_user = userId,
            OnlyFavorites = onlyFavorites,
            DrinkName = drinkName,
            CategoryName = categoryName
        };
        var result = await service.GetAllDrinksAsync(filters, page, limit, role);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/drinks/{id:guid}", async Task<IResult> (Guid id, IDrinkService service, int role = 1) =>
{
    try
    {
        var drink = await service.GetDrinkByIdAsync(id, role);
        return Results.Ok(drink);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapPost("/drinks", async Task<IResult> (CreateDrinkRequest request, IDrinkService service, int role = 1) =>
{
    try
    {
        var id = await service.AddDrinkAsync(CreateDrinkRequestConverter.ConvertToDomainModel(request), role);
        return Results.Created($"/api/core/drinks/{id}", new CreateResponse { Id = id.ToString() });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapDelete("/drinks/{id:guid}", async Task<IResult> (Guid id, IDrinkService service, int role = 1) =>
{
    try
    {
        await service.DeleteDrinkAsync(id, role);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/drinks/{drinkId:guid}/categories", async Task<IResult> (Guid drinkId, IDrinksCategoryService service, int role = 1) =>
{
    try
    {
        var categories = await service.GetCategoryByDrinkIdAsync(drinkId, role);
        return Results.Ok(categories);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapPost("/drinks-categories", async Task<IResult> (DrinksCategory link, IDrinksCategoryService service, int role = 1) =>
{
    try
    {
        await service.AddAsync(link.Id_drink, link.Id_category, role);
        return Results.Created($"/api/core/drinks-categories", new { ok = true });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapDelete("/drinks/{drinkId:guid}/drinks-categories", async Task<IResult> (Guid drinkId, IDrinksCategoryService service, int role = 1) =>
{
    try
    {
        await service.RemoveAsync(drinkId, role);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapPost("/favorites/coffeeshops", async Task<IResult> (FavCoffeeShopsRequest request, int role, IFavCoffeeShopsService service) =>
{
    try
    {
        await service.AddCoffeeShopToFavsAsync(request.UserId, request.CoffeeShopId, role);
        return Results.Created("/api/core/favorites/coffeeshops", new { ok = true });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapDelete("/favorites/coffeeshops", async Task<IResult> (Guid userId, Guid coffeeShopId, int role, IFavCoffeeShopsService service) =>
{
    try
    {
        await service.RemoveCoffeeShopFromFavsAsync(userId, coffeeShopId, role);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/favorites/coffeeshops", async Task<IResult> (Guid userId, int page, int limit, int role, IFavCoffeeShopsService service) =>
{
    try
    {
        var result = await service.GetFavCoffeeShopsAsync(userId, page, limit, role);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapPost("/favorites/drinks", async Task<IResult> (FavDrinkRequest request, int role, IFavDrinksService service) =>
{
    try
    {
        await service.AddDrinkToFavsAsync(request.UserId, request.DrinkId, role);
        return Results.Created("/api/core/favorites/drinks", new { ok = true });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapDelete("/favorites/drinks", async Task<IResult> (Guid userId, Guid drinkId, int role, IFavDrinksService service) =>
{
    try
    {
        await service.RemoveDrinkFromFavsAsync(userId, drinkId, role);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/favorites/drinks", async Task<IResult> (Guid userId, int page, int limit, int role, IFavDrinksService service) =>
{
    try
    {
        var result = await service.GetFavDrinksAsync(userId, page, limit, role);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/menu/company/{companyId:guid}", async Task<IResult> (Guid companyId, int page, int limit, int role, IMenuService service) =>
{
    try
    {
        var result = await service.GetMenuByCompanyIdAsync(companyId, page, limit, role);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapGet("/menu/drink/{drinkId:guid}/companies", async Task<IResult> (Guid drinkId, int page, int limit, int role, IMenuService service) =>
{
    try
    {
        var result = await service.GetCompaniesByDrinkIdAsync(drinkId, page, limit, role);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapPost("/companies/{companyId:guid}/menu", async Task<IResult> (Guid companyId, CreateMenuRequest request, int role, IMenuService service) =>
{
    try
    {
        var menu = new Menu(request.Id_drink, companyId, request.Size, request.Price);
        await service.AddDrinkToMenuAsync(menu, role);
        return Results.Created($"/api/core/companies/{companyId}/menu", new { ok = true });
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

core.MapDelete("/companies/{companyId:guid}/menu/{drinkId:guid}", async Task<IResult> (Guid companyId, Guid drinkId, int role, IMenuService service) =>
{
    try
    {
        await service.DeleteDrinkFromMenuAsync(drinkId, companyId, role);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return MapException(ex);
    }
});

app.Run();

static IResult MapException(Exception ex)
{
    var status = ex switch
    {
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        ArgumentNullException => StatusCodes.Status400BadRequest,
        _ when ex.GetType().Name.Contains("NotFound", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status404NotFound,
        _ when ex.GetType().Name.Contains("Already", StringComparison.OrdinalIgnoreCase) ||
               ex.GetType().Name.Contains("Unique", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status409Conflict,
        _ when ex.GetType().Name.Contains("Incorrect", StringComparison.OrdinalIgnoreCase) ||
               ex.GetType().Name.Contains("Wrong", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };

    var error = new Error(ex.Message, status, ex.Message);
    return Results.Json(error, statusCode: status);
}

internal record FavCoffeeShopsRequest(Guid UserId, Guid CoffeeShopId);
internal record FavDrinkRequest(Guid UserId, Guid DrinkId);
