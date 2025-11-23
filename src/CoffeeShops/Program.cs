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
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

var readOnlyMode = builder.Configuration.GetValue<bool>("ReadOnlyMode");
var instanceName = builder.Configuration.GetValue<string>("InstanceName") ?? "main";
var pathBase = builder.Configuration["ASPNETCORE_PATHBASE"] ?? builder.Configuration["PathBase"];

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                              ForwardedHeaders.XForwardedProto;
    // ��� ���������� ������ (Nginx �� localhost)
    options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
    options.KnownProxies.Add(IPAddress.Parse("::1"));
});

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


builder.Services.AddControllersWithViews();  //��� ���

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CoffeeShops API",
        Version = "v1",
        Description = "REST API ��� ������� ���������� ���������, ��������� � ��������������",
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
app.UseForwardedHeaders();


//применение префикса mirror
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
}
//применение префикса mirror
string WithBase(string relativePath) =>
    string.IsNullOrEmpty(pathBase) ? relativePath : $"{pathBase}{relativePath}";

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

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Instance-Name"] = instanceName;
    context.Response.Headers["Server"] = "CoffeeShops";

    var isWriteMethod = !HttpMethods.IsGet(context.Request.Method)
        && !HttpMethods.IsHead(context.Request.Method)
        && !HttpMethods.IsOptions(context.Request.Method);

    if (readOnlyMode && isWriteMethod)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Instance runs in read-only mode",
            instance = instanceName
        });
        return;
    }

    await next();
});

// app.UseSwagger();
// app.UseSwaggerUI(c =>
// {
//     c.SwaggerEndpoint("/api/v1/swagger/v1/swagger.json", "CoffeeShops API v1");
//     c.RoutePrefix = "api/v1"; // Swagger ����� �� /api/v1
// });
app.UseSwagger(c =>
{
    c.RouteTemplate = "api/v1/swagger/{documentname}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint(WithBase("/api/v1/swagger/v1/swagger.json"), "CoffeeShops API v1");
    c.RoutePrefix = "api/v1/swagger"; // Теперь Swagger будет по /api/v1/swagger
});

app.UseStaticFiles();
app.UseRouting();

app.UseCors(b =>
{
    b.AllowAnyOrigin();
    b.AllowAnyHeader();
    b.AllowAnyMethod();
});


app.UseAuthentication(); 
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    // API �������� (��� Swagger)
    endpoints.MapControllers();

    // GUI �������� (��� ������������ ����������)
    endpoints.MapControllerRoute(
        name: "gui",
        pattern: "{controller=UserGui}/{action=Login}/{id?}"); 

    // ����������� MVC ������� ��� fallback
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=UserGui}/{action=Login}/{id?}"); 
});

app.MapGet("/", () => Results.Redirect(WithBase("/usergui/login"))) 
   .ExcludeFromDescription();

// �������� ��� SWAGGER
app.MapGet("/api/v1", () => Results.Redirect(WithBase("/api/v1/swagger")));
app.MapGet("/swagger", () => Results.Redirect(WithBase("/api/v1/swagger")));

app.Run();
