using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using CoffeeShops.CoreApi;
using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.Domain.Exceptions.CategoryServiceExceptions;
using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
using CoffeeShops.Domain.Exceptions.CoffeeShopServiceExceptions;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Exceptions.MenuServiceExceptions;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
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
using Microsoft.Extensions.Logging;

namespace CoffeeShops.CoreApi;

internal sealed class CoreCategoryServiceClient : CoreServiceClientBase, ICategoryService
{
    public CoreCategoryServiceClient(HttpClient client, ILogger<CoreCategoryServiceClient> logger) : base(client, logger) { }

    public async Task<Guid> AddCategoryAsync(Category category, int id_role)
    {
        var payload = new CreateCategoryRequest { CategoryName = category.Name };
        var response = await Client.PostAsJsonAsync($"/api/core/categories?role={id_role}", payload);
        await EnsureSuccess(response, status => status switch
        {
            HttpStatusCode.Conflict => new CategoryUniqueException("Category already exists"),
            HttpStatusCode.BadRequest => new CategoryIncorrectAtributeException("Invalid category data"),
            _ => null
        });
        var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
        return created?.Id is null ? Guid.Empty : Guid.Parse(created.Id);
    }

    public async Task<PaginatedResponse<Category>>? GetAllCategoriesAsync(int page, int limit, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/categories?page={page}&limit={limit}&role={id_role}");
        return await ReadAsync<PaginatedResponse<Category>>(response, status => status == HttpStatusCode.NotFound ? new CategoryNotFoundException("Categories not found") : null);
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid category_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/categories/{category_id}?role={id_role}");
        return await ReadAsync<Category>(response, status => status == HttpStatusCode.NotFound ? new CategoryNotFoundException("Category not found") : null);
    }
}

internal record FavCoffeeShopsRequest(Guid UserId, Guid CoffeeShopId);
internal record FavDrinkRequest(Guid UserId, Guid DrinkId);

internal sealed class CoreCompanyServiceClient : CoreServiceClientBase, ICompanyService
{
    public CoreCompanyServiceClient(HttpClient client, ILogger<CoreCompanyServiceClient> logger) : base(client, logger) { }

    public async Task<Guid> AddCompanyAsync(Company company, int id_role)
    {
        var payload = new CreateCompanyRequest { Name = company.Name, Website = company.Website };
        var response = await Client.PostAsJsonAsync($"/api/core/companies?role={id_role}", payload);
        await EnsureSuccess(response, status => status == HttpStatusCode.BadRequest
            ? new CompanyIncorrectAtributeException("Invalid company data")
            : null);
        var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
        return created?.Id is null ? Guid.Empty : Guid.Parse(created.Id);
    }

    public async Task<PaginatedResponse<Company>>? GetAllCompaniesAsync(CompanyFilters filters, int page, int limit, int id_role)
    {
        var url = $"/api/core/companies?page={page}&limit={limit}&role={id_role}";
        if (filters.Id_drink != null)
        {
            url += $"&drinkId={filters.Id_drink}";
        }

        var response = await Client.GetAsync(url);
        return await ReadAsync<PaginatedResponse<Company>>(response,
            status => status == HttpStatusCode.NotFound ? new NoCompaniesFoundException("Companies not found") : null);
    }

    public async Task<Company?> GetCompanyByIdAsync(Guid company_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/companies/{company_id}?role={id_role}");
        return await ReadAsync<Company>(response,
            status => status == HttpStatusCode.NotFound ? new CompanyNotFoundException("Company not found") : null);
    }

    public async Task RemoveCompanyAsync(Guid company_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/core/companies/{company_id}?role={id_role}");
        await EnsureSuccess(response,
            status => status == HttpStatusCode.NotFound ? new CompanyNotFoundException("Company not found") : null);
    }

    public async Task RemoveCompanyWithAllDataAsync(Guid company_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/core/companies/{company_id}/with-data?role={id_role}");
        await EnsureSuccess(response,
            status => status == HttpStatusCode.NotFound ? new CompanyNotFoundException("Company not found") : null);
    }

    public Task DeleteCompanyAsync(Guid companyId, int id_role) =>
        RemoveCompanyWithAllDataAsync(companyId, id_role);
}

internal sealed class CoreCoffeeShopServiceClient : CoreServiceClientBase, ICoffeeShopService
{
    public CoreCoffeeShopServiceClient(HttpClient client, ILogger<CoreCoffeeShopServiceClient> logger) : base(client, logger) { }

    public async Task<Guid> AddCoffeeShopAsync(CoffeeShop coffeeshop, int id_role)
    {
        var payload = new CreateCoffeeShopRequest { Address = coffeeshop.Address, Id_company = coffeeshop.Id_company, WorkingHours = coffeeshop.WorkingHours };
        var response = await Client.PostAsJsonAsync($"/api/core/coffeeshops?role={id_role}", payload);
        await EnsureSuccess(response, status => status == HttpStatusCode.BadRequest
            ? new CoffeeShopIncorrectAtributeException("Invalid coffee shop data")
            : null);
        var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
        return created?.Id is null ? Guid.Empty : Guid.Parse(created.Id);
    }

    public async Task<PaginatedResponse<CoffeeShop>>? GetAllCoffeeShopsAsync(CoffeeShopFilters filters, int page, int limit, int id_role)
    {
        var url = $"/api/core/coffeeshops?page={page}&limit={limit}&role={id_role}";
        if (filters.Id_company != null) url += $"&companyId={filters.Id_company}";
        if (filters.Id_user != null) url += $"&userId={filters.Id_user}";
        url += $"&onlyFavorites={(filters.OnlyFavorites ? "true" : "false")}";

        var response = await Client.GetAsync(url);
        return await ReadAsync<PaginatedResponse<CoffeeShop>>(response,
            status => status == HttpStatusCode.NotFound ? new NoCoffeeShopsFoundException("Coffee shops not found") : null);
    }

    public async Task<CoffeeShop?> GetCoffeeShopByIdAsync(Guid coffeeshop_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/coffeeshops/{coffeeshop_id}?role={id_role}");
        return await ReadAsync<CoffeeShop>(response,
            status => status == HttpStatusCode.NotFound ? new CoffeeShopNotFoundException("Coffee shop not found") : null);
    }

    public async Task<PaginatedResponse<CoffeeShop>>? GetCoffeeShopsByCompanyIdAsync(Guid company_id, CoffeeShopFilters filters, int page, int limit, int id_role)
    {
        var url = $"/api/core/companies/{company_id}/coffeeshops?page={page}&limit={limit}&role={id_role}";
        if (filters.Id_user != null) url += $"&userId={filters.Id_user}";
        url += $"&onlyFavorites={(filters.OnlyFavorites ? "true" : "false")}";

        var response = await Client.GetAsync(url);
        return await ReadAsync<PaginatedResponse<CoffeeShop>>(response,
            status => status == HttpStatusCode.NotFound ? new CoffeeShopsForCompanyNotFoundException("Coffee shops for company not found") : null);
    }

    public async Task DeleteCoffeeShopAsync(Guid coffeeshop_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/core/coffeeshops/{coffeeshop_id}?role={id_role}");
        await EnsureSuccess(response,
            status => status == HttpStatusCode.NotFound ? new CoffeeShopNotFoundException("Coffee shop not found") : null);
    }
}

internal sealed class CoreDrinkServiceClient : CoreServiceClientBase, IDrinkService
{
    public CoreDrinkServiceClient(HttpClient client, ILogger<CoreDrinkServiceClient> logger) : base(client, logger) { }

    public async Task<Guid> AddDrinkAsync(Drink drink, int id_role)
    {
        var payload = new CreateDrinkRequest { DrinkName = drink.Name };
        var response = await Client.PostAsJsonAsync($"/api/core/drinks?role={id_role}", payload);
        await EnsureSuccess(response, status => status switch
        {
            HttpStatusCode.Conflict => new DrinkNameAlreadyExistsException("Drink already exists"),
            HttpStatusCode.BadRequest => new DrinkIncorrectAtributeException("Invalid drink data"),
            _ => null
        });
        var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
        return created?.Id is null ? Guid.Empty : Guid.Parse(created.Id);
    }

    public async Task<PaginatedResponse<Drink>>? GetAllDrinksAsync(DrinkFilters filters, int page, int limit, int id_role)
    {
        var url = $"/api/core/drinks?page={page}&limit={limit}&role={id_role}";
        if (filters.Id_user != null) url += $"&userId={filters.Id_user}";
        if (filters.OnlyFavorites) url += "&onlyFavorites=true";
        if (!string.IsNullOrEmpty(filters.DrinkName)) url += $"&drinkName={Uri.EscapeDataString(filters.DrinkName)}";
        if (!string.IsNullOrEmpty(filters.CategoryName)) url += $"&categoryName={Uri.EscapeDataString(filters.CategoryName)}";

        var response = await Client.GetAsync(url);
        return await ReadAsync<PaginatedResponse<Drink>>(response,
            status => status == HttpStatusCode.NotFound ? new NoDrinksFoundException("Drinks not found") : null);
    }

    public async Task<Drink?> GetDrinkByIdAsync(Guid drink_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/drinks/{drink_id}?role={id_role}");
        return await ReadAsync<Drink>(response,
            status => status == HttpStatusCode.NotFound ? new DrinkNotFoundException("Drink not found") : null);
    }

    public async Task RemoveDrinkAsync(Guid drink_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/core/drinks/{drink_id}?role={id_role}");
        await EnsureSuccess(response,
            status => status == HttpStatusCode.NotFound ? new DrinkNotFoundException("Drink not found") : null);
    }

    public Task DeleteDrinkAsync(Guid drink_id, int id_role) => RemoveDrinkAsync(drink_id, id_role);
}

internal sealed class CoreDrinksCategoryServiceClient : CoreServiceClientBase, IDrinksCategoryService
{
    public CoreDrinksCategoryServiceClient(HttpClient client, ILogger<CoreDrinksCategoryServiceClient> logger) : base(client, logger) { }

    public async Task AddAsync(Guid drink_id, Guid category_id, int id_role)
    {
        var payload = new DrinksCategory(drink_id, category_id);
        var response = await Client.PostAsJsonAsync($"/api/core/drinks-categories?role={id_role}", payload);
        await EnsureSuccess(response, status => status switch
        {
            HttpStatusCode.Conflict => new DrinkAlreadyHasThisCategoryException("Drink already has this category"),
            HttpStatusCode.NotFound => new DrinkNotFoundException("Drink or category not found"),
            HttpStatusCode.BadRequest => new CategoryNotFoundException("Category not found"),
            _ => null
        });
    }

    public async Task RemoveAsync(Guid drink_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/core/drinks/{drink_id}/drinks-categories?role={id_role}");
        await EnsureSuccess(response,
            status => status == HttpStatusCode.NotFound ? new DrinkNotFoundException("Drink not found") : null);
    }

    public async Task<List<Category>?> GetCategoryByDrinkIdAsync(Guid drink_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/drinks/{drink_id}/categories?role={id_role}");
        return await ReadAsync<List<Category>>(response,
            status => status == HttpStatusCode.NotFound ? new NoDrinksCategoriesFoundException("Categories not found") : null);
    }

    public async Task<Guid> AddDrinksCategoryAsync(DrinksCategory drinksCategory, int id_role)
    {
        var response = await Client.PostAsJsonAsync($"/api/core/drinks-categories?role={id_role}", drinksCategory);
        await EnsureSuccess(response, status => status switch
        {
            HttpStatusCode.Conflict => new DrinkAlreadyHasThisCategoryException("Drink already has this category"),
            HttpStatusCode.NotFound => new DrinkNotFoundException("Drink or category not found"),
            HttpStatusCode.BadRequest => new CategoryNotFoundException("Category not found"),
            _ => null
        });
        var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
        return created?.Id is null ? Guid.Empty : Guid.Parse(created.Id);
    }

    public async Task<PaginatedResponse<DrinksCategory>>? GetAllDrinksCategoriesAsync(int page, int limit, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/drinks-categories?page={page}&limit={limit}&role={id_role}");
        return await ReadAsync<PaginatedResponse<DrinksCategory>>(response,
            status => status == HttpStatusCode.NotFound ? new NoDrinksCategoriesFoundException("Drinks categories not found") : null);
    }

    public async Task<DrinksCategory?> GetDrinksCategoryByIdAsync(Guid drinksCategory_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/drinks-categories/{drinksCategory_id}?role={id_role}");
        return await ReadAsync<DrinksCategory>(response,
            status => status == HttpStatusCode.NotFound ? new DrinkNotFoundException("Drink or category not found") : null);
    }

    public async Task RemoveAllDrinksCategoryByCategoryIdAsync(Guid category_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/core/categories/{category_id}/drinks-categories?role={id_role}");
        await EnsureSuccess(response,
            status => status == HttpStatusCode.NotFound ? new CategoryNotFoundException("Category not found") : null);
    }

    public async Task RemoveAllDrinksCategoryByDrinkIdAsync(Guid drink_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/core/drinks/{drink_id}/drinks-categories?role={id_role}");
        await EnsureSuccess(response,
            status => status == HttpStatusCode.NotFound ? new DrinkNotFoundException("Drink not found") : null);
    }

    public async Task RemoveDrinksCategoryAsync(Guid drinksCategory_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/core/drinks-categories/{drinksCategory_id}?role={id_role}");
        await EnsureSuccess(response,
            status => status == HttpStatusCode.NotFound ? new DrinkNotFoundException("Record not found") : null);
    }
}

internal sealed class CoreFavCoffeeShopsServiceClient : CoreServiceClientBase, IFavCoffeeShopsService
{
    public CoreFavCoffeeShopsServiceClient(HttpClient client, ILogger<CoreFavCoffeeShopsServiceClient> logger) : base(client, logger) { }

    public async Task AddCoffeeShopToFavsAsync(Guid user_id, Guid coffeeshop_id, int id_role)
    {
        var payload = new FavCoffeeShopsRequest(user_id, coffeeshop_id);
        var response = await Client.PostAsJsonAsync($"/api/core/favorites/coffeeshops?role={id_role}", payload);
        await EnsureSuccess(response, status => status switch
        {
            HttpStatusCode.Conflict => new CoffeeShopAlreadyIsFavoriteException("Already favorite"),
            HttpStatusCode.NotFound => new UserNotFoundException("User or coffee shop not found"),
            _ => null
        });
    }

    public async Task<PaginatedResponse<CoffeeShop>>? GetFavCoffeeShopsAsync(Guid user_id, int page, int limit, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/favorites/coffeeshops?userId={user_id}&page={page}&limit={limit}&role={id_role}");
        return await ReadAsync<PaginatedResponse<CoffeeShop>>(response,
            status => status == HttpStatusCode.NotFound ? new NoCoffeeShopsFoundException("Favorites not found") : null);
    }

    public async Task RemoveCoffeeShopFromFavsAsync(Guid user_id, Guid coffeeshop_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/core/favorites/coffeeshops?userId={user_id}&coffeeShopId={coffeeshop_id}&role={id_role}");
        await EnsureSuccess(response,
            status => status == HttpStatusCode.NotFound ? new CoffeeShopIsNotFavoriteException("Not in favorites") : null);
    }
}

internal sealed class CoreFavDrinksServiceClient : CoreServiceClientBase, IFavDrinksService
{
    public CoreFavDrinksServiceClient(HttpClient client, ILogger<CoreFavDrinksServiceClient> logger) : base(client, logger) { }

    public async Task AddDrinkToFavsAsync(Guid user_id, Guid drink_id, int id_role)
    {
        var payload = new FavDrinkRequest(user_id, drink_id);
        var response = await Client.PostAsJsonAsync($"/api/core/favorites/drinks?role={id_role}", payload);
        await EnsureSuccess(response, status => status switch
        {
            HttpStatusCode.Conflict => new DrinkAlreadyIsFavoriteException("Already favorite"),
            HttpStatusCode.NotFound => new DrinkNotFoundException("User or drink not found"),
            _ => null
        });
    }

    public async Task<PaginatedResponse<Drink>>? GetFavDrinksAsync(Guid user_id, int page, int limit, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/favorites/drinks?userId={user_id}&page={page}&limit={limit}&role={id_role}");
        return await ReadAsync<PaginatedResponse<Drink>>(response,
            status => status == HttpStatusCode.NotFound ? new NoDrinksFoundException("Favorites not found") : null);
    }

    public async Task RemoveDrinkFromFavsAsync(Guid user_id, Guid drink_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/core/favorites/drinks?userId={user_id}&drinkId={drink_id}&role={id_role}");
        await EnsureSuccess(response,
            status => status == HttpStatusCode.NotFound ? new DrinkIsNotFavoriteException("Not in favorites") : null);
    }
}

internal sealed class CoreMenuServiceClient : CoreServiceClientBase, IMenuService
{
    public CoreMenuServiceClient(HttpClient client, ILogger<CoreMenuServiceClient> logger) : base(client, logger) { }

    public async Task AddDrinkToMenuAsync(Menu menurecord, int id_role)
    {
        var payload = new CreateMenuRequest { Id_drink = menurecord.Id_drink, Price = menurecord.Price, Size = menurecord.Size };
        var response = await Client.PostAsJsonAsync($"/api/core/companies/{menurecord.Id_company}/menu?role={id_role}", payload);
        await EnsureSuccess(response, status => status switch
        {
            HttpStatusCode.NotFound => new DrinkNotFoundException("Drink or company not found"),
            HttpStatusCode.BadRequest => new MenuIncorrectAtributeException("Invalid menu data"),
            _ => null
        });
    }

    public async Task<PaginatedResponse<Company>>? GetCompaniesByDrinkIdAsync(Guid drink_id, int page, int limit, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/menu/drink/{drink_id}/companies?page={page}&limit={limit}&role={id_role}");
        return await ReadAsync<PaginatedResponse<Company>>(response,
            status => status == HttpStatusCode.NotFound ? new CompaniesByDrinkNotFoundException("Companies not found") : null);
    }

    public async Task<Drink?> GetDrinkByIdAsync(Guid drink_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/drinks/{drink_id}?role={id_role}");
        return await ReadAsync<Drink>(response);
    }

    public async Task<PaginatedResponse<Menu>>? GetMenuByCompanyIdAsync(Guid company_id, int page, int limit, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/menu/company/{company_id}?page={page}&limit={limit}&role={id_role}");
        return await ReadAsync<PaginatedResponse<Menu>>(response,
            status => status == HttpStatusCode.NotFound ? new MenuNotFoundException("Menu not found") : null);
    }

    public async Task DeleteDrinkFromMenuAsync(Guid drink_id, Guid company_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/core/companies/{company_id}/menu/{drink_id}?role={id_role}");
        await EnsureSuccess(response,
            status => status == HttpStatusCode.NotFound ? new DrinkNotFoundException("Drink or company not found") : null);
    }
}

internal sealed class CoreUserServiceClient : CoreServiceClientBase, IUserService
{
    public CoreUserServiceClient(HttpClient client, ILogger<CoreUserServiceClient> logger) : base(client, logger) { }

    public async Task<Guid> Registrate(User user)
    {
        var payload = new RegisterRequest
        {
            Login = user.Login,
            Password = user.PasswordHash,
            BirthDate = user.BirthDate,
            Email = user.Email
        };
        var response = await Client.PostAsJsonAsync($"/api/core/auth/register", payload);
        await EnsureSuccess(response, status => status switch
        {
            HttpStatusCode.Conflict => new UserLoginAlreadyExistsException("User already exists"),
            HttpStatusCode.BadRequest => new UserIncorrectAtributeException("Invalid user data"),
            _ => null
        });
        var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
        return created?.Id is null ? Guid.Empty : Guid.Parse(created.Id);
    }

    public async Task<AuthResponse> Login(string login, string password)
    {
        var payload = new LoginRequest { Login = login, Password = password };
        var response = await Client.PostAsJsonAsync($"/api/core/auth/login", payload);
        return await ReadAsync<AuthResponse>(response, status => status switch
        {
            HttpStatusCode.NotFound => new UserLoginNotFoundException("User not found"),
            HttpStatusCode.BadRequest => new UserWrongPasswordException("Wrong password"),
            _ => null
        }) ?? new AuthResponse();
    }

    public async Task<User?> GetUserByIdAsync(Guid user_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/users/{user_id}?role={id_role}");
        return await ReadAsync<User>(response,
            status => status == HttpStatusCode.NotFound ? new UserNotFoundException("User not found") : null);
    }

    public async Task<User?> GetUserByLoginAsync(string login, int id_role)
    {
        var response = await Client.GetAsync($"/api/core/users/by-login/{Uri.EscapeDataString(login)}?role={id_role}");
        return await ReadAsync<User>(response,
            status => status == HttpStatusCode.NotFound ? new UserNotFoundException("User not found") : null);
    }

    public async Task<PaginatedResponse<User>>? GetAllUsersAsync(UserFilters filters, int page, int limit, int id_role)
    {
        var url = $"/api/core/users?page={page}&limit={limit}&role={id_role}";
        if (!string.IsNullOrEmpty(filters.Login)) url += $"&login={Uri.EscapeDataString(filters.Login)}";
        if (filters.UserRole != null) url += $"&userRole={filters.UserRole}";

        var response = await Client.GetAsync(url);
        return await ReadAsync<PaginatedResponse<User>>(response,
            status => status == HttpStatusCode.NotFound ? new NoUsersFoundException("Users not found") : null);
    }

    public async Task UpdateUserRightsAsync(Guid id, int new_id_role, int id_role)
    {
        var payload = new UpdateUserRole { User_role = UserRoleExtensions.ToRoleNameFromInt(new_id_role) };
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/core/users/{id}/role?role={id_role}")
        {
            Content = JsonContent.Create(payload)
        };
        var response = await Client.SendAsync(request);
        await EnsureSuccess(response, status => status switch
        {
            HttpStatusCode.NotFound => new UserNotFoundException("User not found"),
            HttpStatusCode.BadRequest => new UserLastAdminException("Cannot update last admin"),
            _ => null
        });
    }

    public async Task DeleteUserAsync(Guid id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/core/users/{id}?role={id_role}");
        await EnsureSuccess(response, status => status switch
        {
            HttpStatusCode.NotFound => new UserNotFoundException("User not found"),
            HttpStatusCode.BadRequest => new UserLastAdminException("Cannot delete last admin"),
            _ => null
        });
    }

    public async Task UpdateUserAsync(User user, int id_role)
    {
        var payload = new UpdateUser
        {
            Login = user.Login,
            Password = user.PasswordHash,
            Email = user.Email
        };
        var response = await Client.PutAsJsonAsync($"/api/core/users/{user.Id_user}?role={id_role}", payload);
        await EnsureSuccess(response, status => status switch
        {
            HttpStatusCode.NotFound => new UserNotFoundException("User not found"),
            HttpStatusCode.BadRequest => new UserIncorrectAtributeException("Invalid user data"),
            _ => null
        });
    }
}
