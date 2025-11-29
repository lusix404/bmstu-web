using CoffeeShops.CoreApi;
using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.DTOs.Utils;
using CoffeeShops.Services.Interfaces.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Collections.Generic;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var readOnlyMode = builder.Configuration.GetValue<bool>("ReadOnlyMode");
var instanceName = builder.Configuration.GetValue<string>("InstanceName") ?? "gateway-main";
var pathBase = builder.Configuration["ASPNETCORE_PATHBASE"] ?? builder.Configuration["PathBase"];
var coreBaseUrl = builder.Configuration.GetValue<string>("CoreService:BaseUrl") ?? "http://core-main:8080";

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
    options.KnownProxies.Add(IPAddress.Parse("::1"));
});

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File($"logs/gateway-{instanceName}-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Configure(builder.Configuration.GetSection("Kestrel"));
});

builder.Services.AddHttpClient("core", client =>
{
    client.BaseAddress = new Uri(coreBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddTransient<ICategoryService>(sp =>
    new CoreCategoryServiceClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("core"),
        sp.GetRequiredService<ILogger<CoreCategoryServiceClient>>()));
builder.Services.AddTransient<ICoffeeShopService>(sp =>
    new CoreCoffeeShopServiceClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("core"),
        sp.GetRequiredService<ILogger<CoreCoffeeShopServiceClient>>()));
builder.Services.AddTransient<ICompanyService>(sp =>
    new CoreCompanyServiceClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("core"),
        sp.GetRequiredService<ILogger<CoreCompanyServiceClient>>()));
builder.Services.AddTransient<IDrinkService>(sp =>
    new CoreDrinkServiceClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("core"),
        sp.GetRequiredService<ILogger<CoreDrinkServiceClient>>()));
builder.Services.AddTransient<IDrinksCategoryService>(sp =>
    new CoreDrinksCategoryServiceClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("core"),
        sp.GetRequiredService<ILogger<CoreDrinksCategoryServiceClient>>()));
builder.Services.AddTransient<IFavCoffeeShopsService>(sp =>
    new CoreFavCoffeeShopsServiceClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("core"),
        sp.GetRequiredService<ILogger<CoreFavCoffeeShopsServiceClient>>()));
builder.Services.AddTransient<IFavDrinksService>(sp =>
    new CoreFavDrinksServiceClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("core"),
        sp.GetRequiredService<ILogger<CoreFavDrinksServiceClient>>()));
builder.Services.AddTransient<IMenuService>(sp =>
    new CoreMenuServiceClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("core"),
        sp.GetRequiredService<ILogger<CoreMenuServiceClient>>()));
builder.Services.AddTransient<IUserService>(sp =>
    new CoreUserServiceClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("core"),
        sp.GetRequiredService<ILogger<CoreUserServiceClient>>()));

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
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CoffeeShops Gateway API",
        Version = "v1"
    });

    var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            securityScheme, new List<string>()
        }
    });
});

var app = builder.Build();
app.UseForwardedHeaders();

if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
}

string WithBase(string relativePath) =>
    string.IsNullOrEmpty(pathBase) ? relativePath : $"{pathBase}{relativePath}";

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Instance-Name"] = instanceName;
    context.Response.Headers["Server"] = "CoffeeShops-Gateway";

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

app.UseSwagger(c =>
{
    c.RouteTemplate = "api/v1/swagger/{documentname}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint(WithBase("/api/v1/swagger/v1/swagger.json"), "CoffeeShops API v1");
    c.RoutePrefix = "api/v1/swagger";
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
    endpoints.MapControllers();
    endpoints.MapControllerRoute(
        name: "gui",
        pattern: "{controller=UserGui}/{action=Login}/{id?}");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=UserGui}/{action=Login}/{id?}");
});

app.MapGet("/", () => Results.Redirect(WithBase("/usergui/login")))
   .ExcludeFromDescription();

app.MapGet("/api/v1", () => Results.Redirect(WithBase("/api/v1/swagger")));
app.MapGet("/swagger", () => Results.Redirect(WithBase("/api/v1/swagger")));

app.Run();
