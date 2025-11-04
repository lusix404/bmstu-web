using CoffeeShops.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using CoffeeShops.DataAccess.Extensions;
using System;
using Microsoft.AspNetCore.Cors.Infrastructure;
using CoffeeShops.DataAccess.Repositories;
using CoffeeShops.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Services.Services;
using CoffeeShops.Domain.Models;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


try
{
    Log.Logger = new LoggerConfiguration()
         .MinimumLevel.Information()
         .WriteTo.Console()
         .WriteTo.File("logs/coffeeshops-.txt", rollingInterval: RollingInterval.Day)
         .CreateLogger();

    Log.Information("Log started.");
}
catch (Exception ex)
{
    Console.WriteLine($"Logger error: {ex.Message}");
    Environment.Exit(1);
}

builder.Host.UseSerilog();
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Configure(builder.Configuration.GetSection("Kestrel"));
});

builder.Services.AddDbContext<CoffeeShopsContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("coffeeshopconnection"));
});

builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
        Description = "API для управления кофейнями",
    });

    c.EnableAnnotations();
});

// Регистрация репозиториев и сервисов
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();
builder.Services.AddTransient<ICoffeeShopRepository, CoffeeShopRepository>();
builder.Services.AddTransient<ICompanyRepository, CompanyRepository>();
builder.Services.AddTransient<IDrinksCategoryRepository, DrinksCategoryRepository>();
builder.Services.AddTransient<IDrinkRepository, DrinkRepository>();
builder.Services.AddTransient<IFavCoffeeShopsRepository, FavCoffeeShopsRepository>();
builder.Services.AddTransient<IFavDrinksRepository, FavDrinksRepository>();
builder.Services.AddTransient<IMenuRepository, MenuRepository>();

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<ICoffeeShopService, CoffeeShopService>();
builder.Services.AddTransient<ICompanyService, CompanyService>();
builder.Services.AddTransient<IDrinksCategoryService, DrinksCategoryService>();
builder.Services.AddTransient<IDrinkService, DrinkService>();
builder.Services.AddTransient<IFavCoffeeShopsService, FavCoffeeShopsService>();
builder.Services.AddTransient<IFavDrinksService, FavDrinksService>();
builder.Services.AddTransient<IMenuService, MenuService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<CoffeeShopsContext>();
    var logger = services.GetRequiredService<ILogger<Program>>(); 

    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogError("ERROR: Cannot connect to database");
            throw new InvalidOperationException("Database connection failed");
        }

        logger.LogInformation("Database connection successful!");

        var dbContext = scope.ServiceProvider.GetRequiredService<CoffeeShopsContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogError($"ERROR: {ex.Message}");
        throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI();

//app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
//    c.RoutePrefix = "swagger"; // URL: https://localhost:7081/swagger
//    c.ConfigObject.AdditionalItems["persistAuthorization"] = "true";
//});

app.UseHttpsRedirection();

app.UseCors(b =>
{
    b.AllowAnyOrigin();
    b.AllowAnyHeader();
    b.AllowAnyMethod();
});

app.UseAuthorization();

app.MapControllers();

app.Run();



//using CoffeeShops.DataAccess.Context;
//using Microsoft.EntityFrameworkCore;
//using CoffeeShops.DataAccess.Repositories;
//using CoffeeShops.Domain.Interfaces.Repositories;
//using CoffeeShops.Services.Interfaces.Services;
//using CoffeeShops.Services.Services;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;
//using Serilog;
//using CoffeeShops.Services.Interfaces.Services;
//using CoffeeShops.Services.Services;

//var builder = WebApplication.CreateBuilder(args);

//try
//{
//    Log.Logger = new LoggerConfiguration()
//         .MinimumLevel.Information()
//         .WriteTo.Console()
//         .WriteTo.File("logs/coffeeshops-.txt", rollingInterval: RollingInterval.Day)
//         .CreateLogger();

//    Log.Information("Log started.");
//}
//catch (Exception ex)
//{
//    Console.WriteLine($"Logger error: {ex.Message}");
//    Environment.Exit(1);
//}

//builder.Host.UseSerilog();

//// Добавляем поддержку Razor Pages
//builder.Services.AddRazorPages();

//// Настройка контекстов базы данных
//builder.Services.AddDbContext<UserDbContext>(
//    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("UserConnection")));
//builder.Services.AddDbContext<AdminDbContext>(
//    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("AdminConnection")));
//builder.Services.AddDbContext<ModerDbContext>(
//    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("ModerConnection")));
//builder.Services.AddDbContext<GuestDbContext>(
//    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("GuestConnection")));
//builder.Services.AddTransient<IDbContextFactory, DbContextFactory>();

//// Регистрация репозиториев
//builder.Services.AddTransient<IUserRepository, UserRepository>();
//builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();
//builder.Services.AddTransient<ICoffeeShopRepository, CoffeeShopRepository>();
//builder.Services.AddTransient<ICompanyRepository, CompanyRepository>();
//builder.Services.AddTransient<IDrinksCategoryRepository, DrinksCategoryRepository>();
//builder.Services.AddTransient<IDrinkRepository, DrinkRepository>();
//builder.Services.AddTransient<IFavCoffeeShopsRepository, FavCoffeeShopsRepository>();
//builder.Services.AddTransient<IFavDrinksRepository, FavDrinksRepository>();
//builder.Services.AddTransient<IMenuRepository, MenuRepository>();

//// Регистрация сервисов
//builder.Services.AddTransient<IUserService, UserService>();
//builder.Services.AddTransient<ICategoryService, CategoryService>();
//builder.Services.AddTransient<ICoffeeShopService, CoffeeShopService>();
//builder.Services.AddTransient<ICompanyService, CompanyService>();
//builder.Services.AddTransient<IDrinkService, DrinkService>();
//builder.Services.AddTransient<IDrinksCategoryService, DrinksCategoryService>();
//builder.Services.AddTransient<IMenuService, MenuService>();
//builder.Services.AddTransient<IFavDrinksService, FavDrinksService>();
//builder.Services.AddTransient<IFavCoffeeShopsService, FavCoffeeShopsService>();

//// Настройка аутентификации и авторизации
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options => options.LoginPath = "/login");
//builder.Services.AddAuthorization();

//// Настройка сессий и TempData
//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession();
//builder.Services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

//builder.Host.UseSerilog();
//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.Configure(builder.Configuration.GetSection("Kestrel"));
//});

//var app = builder.Build();

//// Проверка подключения к базе данных
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        var userContext = services.GetRequiredService<UserDbContext>();
//        var canConnect = await userContext.Database.CanConnectAsync();
//        if (!canConnect)
//        {
//            Log.Error("Cannot connect to User database");
//            throw new InvalidOperationException("Database connection failed");
//        }
//        Log.Information("Database connection successful!");
//    }
//    catch (Exception ex)
//    {
//        Log.Error($"Database connection error: {ex.Message}");
//        throw;
//    }
//}

//// Конфигурация pipeline
//app.UseDeveloperExceptionPage();
//app.UseStatusCodePages();
//app.UseStaticFiles();

//app.UseAuthentication();
//app.UseAuthorization();
//app.UseSession();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=User}/{action=Login}");

//app.Run();
