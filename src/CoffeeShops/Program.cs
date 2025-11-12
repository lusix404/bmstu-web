//using CoffeeShops.DataAccess.Context;
//using Microsoft.EntityFrameworkCore;
//using CoffeeShops.DataAccess.Extensions;
//using System;
//using Microsoft.AspNetCore.Cors.Infrastructure;
//using CoffeeShops.DataAccess.Repositories;
//using CoffeeShops.Domain.Interfaces.Repositories;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.OpenApi.Models;
//using System.Reflection;
//using CoffeeShops.Services.Interfaces.Services;
//using CoffeeShops.Services.Services;
//using CoffeeShops.Domain.Models;
//using Microsoft.Extensions.Logging;
//using Serilog;
//using Microsoft.Extensions.Configuration;

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
//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.Configure(builder.Configuration.GetSection("Kestrel"));
//});

//builder.Services.AddDbContext<UserDbContext>(
//    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("UserConnection")));
//builder.Services.AddDbContext<AdminDbContext>(
//    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("AdminConnection")));
//builder.Services.AddDbContext<ModerDbContext>(
//    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("ModerConnection")));
//builder.Services.AddTransient<IDbContextFactory, DbContextFactory>();

//builder.Services.AddCors();
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "CoffeeShops API",
//        Version = "v1",
//        Description = "REST API для системы управления кофейнями, напитками и пользователями",
//    });

//    // Группировать только по именам контроллеров (без суффикса Controller)
//    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"]?.Replace("Controller", "") });

//    // Убрать тэг по умолчанию
//    c.DocInclusionPredicate((name, api) => true);

//    c.EnableAnnotations();
//});

//builder.Services.AddTransient<IUserRepository, UserRepository>();
//builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();
//builder.Services.AddTransient<ICoffeeShopRepository, CoffeeShopRepository>();
//builder.Services.AddTransient<ICompanyRepository, CompanyRepository>();
//builder.Services.AddTransient<IDrinksCategoryRepository, DrinksCategoryRepository>();
//builder.Services.AddTransient<IDrinkRepository, DrinkRepository>();
//builder.Services.AddTransient<IFavCoffeeShopsRepository, FavCoffeeShopsRepository>();
//builder.Services.AddTransient<IFavDrinksRepository, FavDrinksRepository>();
//builder.Services.AddTransient<IMenuRepository, MenuRepository>();

//builder.Services.AddTransient<IUserService, UserService>();
//builder.Services.AddTransient<ICategoryService, CategoryService>();
//builder.Services.AddTransient<ICoffeeShopService, CoffeeShopService>();
//builder.Services.AddTransient<ICompanyService, CompanyService>();
//builder.Services.AddTransient<IDrinksCategoryService, DrinksCategoryService>();
//builder.Services.AddTransient<IDrinkService, DrinkService>();
//builder.Services.AddTransient<IFavCoffeeShopsService, FavCoffeeShopsService>();
//builder.Services.AddTransient<IFavDrinksService, FavDrinksService>();
//builder.Services.AddTransient<IMenuService, MenuService>();
//builder.Services.AddTransient<IJwtService, JwtService>();

//builder.Services.AddDbContext<CoffeeShopsContext>(
//    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));



//var app = builder.Build();

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

//app.UseSwagger();
//app.UseSwaggerUI();

//app.UseHttpsRedirection();

//app.UseRouting();

//app.UseCors(b =>
//{
//    b.AllowAnyOrigin();
//    b.AllowAnyHeader();
//    b.AllowAnyMethod();
//});

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//app.MapGet("/", () => Results.Redirect("/swagger"))
//   .ExcludeFromDescription();

//app.Run();

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
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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


builder.Services.AddDbContext<UserDbContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("UserConnection")));
builder.Services.AddDbContext<AdminDbContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("AdminConnection")));
builder.Services.AddDbContext<ModerDbContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("ModerConnection")));
builder.Services.AddTransient<IDbContextFactory, DbContextFactory>();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])
            )
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CoffeeShops API",
        Version = "v1",
        Description = "REST API для системы управления кофейнями, напитками и пользователями",
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http, 
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.EnableAnnotations();
});

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
builder.Services.AddTransient<IJwtService, JwtService>();

builder.Services.AddDbContext<CoffeeShopsContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userContext = services.GetRequiredService<UserDbContext>();
        var canConnect = await userContext.Database.CanConnectAsync();
        if (!canConnect)
        {
            Log.Error("Cannot connect to User database");
            throw new InvalidOperationException("Database connection failed");
        }
        Log.Information("Database connection successful!");
    }
    catch (Exception ex)
    {
        Log.Error($"Database connection error: {ex.Message}");
        throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(b =>
{
    b.AllowAnyOrigin();
    b.AllowAnyHeader();
    b.AllowAnyMethod();
});


app.UseAuthentication(); // Аутентификация
app.UseAuthorization();  // Авторизация

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

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
