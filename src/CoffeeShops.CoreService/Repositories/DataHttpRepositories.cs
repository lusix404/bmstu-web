using System.Net;
using System.Net.Http.Json;
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
using Microsoft.Extensions.Logging;

namespace CoffeeShops.CoreService.Repositories;

internal abstract class HttpRepositoryBase
{
    protected readonly HttpClient Client;
    protected readonly ILogger Logger;

    protected HttpRepositoryBase(HttpClient client, ILogger logger)
    {
        Client = client;
        Logger = logger;
    }

    protected static string AppendRole(int idRole) => idRole <= 0 ? "1" : idRole.ToString();

    protected async Task<Error?> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<Error>();
        }
        catch
        {
            return null;
        }
    }
}

internal sealed class HttpCategoryRepository : HttpRepositoryBase, ICategoryRepository
{
    public HttpCategoryRepository(HttpClient client, ILogger<HttpCategoryRepository> logger) : base(client, logger) { }

    public async Task<Guid> AddCategoryAsync(Category category, int id_role)
    {
        var response = await Client.PostAsJsonAsync($"/api/data/categories?role={AppendRole(id_role)}", category);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var err = await ReadErrorAsync(response);
            throw new CategoryUniqueException(err?.ErrorDetails ?? "Category already exists");
        }

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
        return created?.Id is null ? Guid.Empty : Guid.Parse(created.Id);
    }

    public async Task<(List<Category>? data, int total)> GetAllCategoriesAsync(int page, int limit, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/categories?page={page}&limit={limit}&role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new List<Category>(), 0);
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<Category>>();
        return (payload?.Data ?? new List<Category>(), payload?.Total ?? 0);
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid category_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/categories/{category_id}?role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Category>();
    }

    public Task RemoveCategoryAsync(int category_id, int id_role) =>
        Client.DeleteAsync($"/api/data/categories/{category_id}?role={AppendRole(id_role)}");
}

internal sealed class HttpCompanyRepository : HttpRepositoryBase, ICompanyRepository
{
    public HttpCompanyRepository(HttpClient client, ILogger<HttpCompanyRepository> logger) : base(client, logger) { }

    public async Task<Guid> AddAsync(Company company, int id_role)
    {
        var response = await Client.PostAsJsonAsync($"/api/data/companies?role={AppendRole(id_role)}", company);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
        return created?.Id is null ? Guid.Empty : Guid.Parse(created.Id);
    }

    public async Task<(List<Company>? data, int total)> GetAllCompaniesAsync(CompanyFilters filters, int page, int limit, int id_role)
    {
        var query = $"/api/data/companies?page={page}&limit={limit}&role={AppendRole(id_role)}";
        if (filters.Id_drink != null)
        {
            query += $"&drinkId={filters.Id_drink}";
        }

        var response = await Client.GetAsync(query);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new List<Company>(), 0);
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<Company>>();
        return (payload?.Data ?? new List<Company>(), payload?.Total ?? 0);
    }

    public async Task<Company?> GetCompanyByIdAsync(Guid company_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/companies/{company_id}?role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Company>();
    }

    public async Task RemoveAsync(Guid company_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/companies/{company_id}?role={AppendRole(id_role)}");
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveCompanyWithAllDataAsync(Guid company_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/companies/{company_id}/with-data?role={AppendRole(id_role)}");
        response.EnsureSuccessStatusCode();
    }
}

internal sealed class HttpCoffeeShopRepository : HttpRepositoryBase, ICoffeeShopRepository
{
    public HttpCoffeeShopRepository(HttpClient client, ILogger<HttpCoffeeShopRepository> logger) : base(client, logger) { }

    public async Task<Guid> AddAsync(CoffeeShop coffeeshop, int id_role)
    {
        var response = await Client.PostAsJsonAsync($"/api/data/coffeeshops?role={AppendRole(id_role)}", coffeeshop);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
        return created?.Id is null ? Guid.Empty : Guid.Parse(created.Id);
    }

    public async Task<(List<CoffeeShop>? data, int total)> GetAllCoffeeShopsAsync(CoffeeShopFilters filters, int page, int limit, int id_role)
    {
        var query = $"/api/data/coffeeshops?page={page}&limit={limit}&role={AppendRole(id_role)}";
        if (filters.Id_company != null) query += $"&CompanyId={filters.Id_company}";
        if (filters.Id_user != null) query += $"&UserId={filters.Id_user}";
        query += $"&OnlyFavorites={(filters.OnlyFavorites ? "true" : "false")}";

        var response = await Client.GetAsync(query);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new List<CoffeeShop>(), 0);
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<CoffeeShop>>();
        return (payload?.Data ?? new List<CoffeeShop>(), payload?.Total ?? 0);
    }

    public async Task<(List<CoffeeShop>? data, int total)> GetCoffeeShopsByCompanyIdAsync(Guid company_id, CoffeeShopFilters filters, int page, int limit, int id_role)
    {
        var query = $"/api/data/companies/{company_id}/coffeeshops?page={page}&limit={limit}&role={AppendRole(id_role)}";
        if (filters.Id_user != null) query += $"&UserId={filters.Id_user}";
        query += $"&OnlyFavorites={(filters.OnlyFavorites ? "true" : "false")}";

        var response = await Client.GetAsync(query);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new List<CoffeeShop>(), 0);
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<CoffeeShop>>();
        return (payload?.Data ?? new List<CoffeeShop>(), payload?.Total ?? 0);
    }

    public async Task<CoffeeShop?> GetCoffeeShopByIdAsync(Guid coffeeshop_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/coffeeshops/{coffeeshop_id}?role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CoffeeShop>();
    }

    public async Task RemoveAllByCompanyIdAsync(Guid company_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/companies/{company_id}/coffeeshops?role={AppendRole(id_role)}");
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAsync(Guid coffeeshop_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/coffeeshops/{coffeeshop_id}?role={AppendRole(id_role)}");
        response.EnsureSuccessStatusCode();
    }
}

internal sealed class HttpDrinkRepository : HttpRepositoryBase, IDrinkRepository
{
    public HttpDrinkRepository(HttpClient client, ILogger<HttpDrinkRepository> logger) : base(client, logger) { }

    public async Task<Guid> AddAsync(Drink drink, int id_role)
    {
        var response = await Client.PostAsJsonAsync($"/api/data/drinks?role={AppendRole(id_role)}", drink);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var err = await ReadErrorAsync(response);
            throw new DrinkNameAlreadyExistsException(err?.ErrorDetails ?? "Drink already exists");
        }

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
        return created?.Id is null ? Guid.Empty : Guid.Parse(created.Id);
    }

    public async Task<(List<Drink> data, int total)> GetAllDrinksAsync(DrinkFilters filters, int page, int limit, int id_role)
    {
        var query = $"/api/data/drinks?page={page}&limit={limit}&role={AppendRole(id_role)}";
        query += $"&OnlyFavorites={(filters.OnlyFavorites ? "true" : "false")}";
        if (filters.Id_user != null) query += $"&UserId={filters.Id_user}";
        if (!string.IsNullOrEmpty(filters.DrinkName)) query += $"&DrinkName={Uri.EscapeDataString(filters.DrinkName)}";
        if (!string.IsNullOrEmpty(filters.CategoryName)) query += $"&CategoryName={Uri.EscapeDataString(filters.CategoryName)}";

        var response = await Client.GetAsync(query);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new List<Drink>(), 0);
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<Drink>>();
        return (payload?.Data ?? new List<Drink>(), payload?.Total ?? 0);
    }

    public async Task<Drink?> GetDrinkByIdAsync(Guid drink_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/drinks/{drink_id}?role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Drink>();
    }

    public async Task RemoveAsync(Guid drink_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/drinks/{drink_id}?role={AppendRole(id_role)}");
        response.EnsureSuccessStatusCode();
    }
}

internal sealed class HttpDrinksCategoryRepository : HttpRepositoryBase, IDrinksCategoryRepository
{
    public HttpDrinksCategoryRepository(HttpClient client, ILogger<HttpDrinksCategoryRepository> logger) : base(client, logger) { }

    public async Task<List<Category>?> GetAllCategoriesByDrinkIdAsync(Guid drink_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/drinks/{drink_id}/categories?role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new List<Category>();
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Category>>();
    }

    public async Task AddAsync(DrinksCategory drinksCategory, int id_role)
    {
        var response = await Client.PostAsJsonAsync($"/api/data/drinks-categories?role={AppendRole(id_role)}", drinksCategory);
        response.EnsureSuccessStatusCode();
    }

    public async Task<(List<DrinksCategory>? data, int total)> GetAllDrinksCategoriesAsync(int page, int limit, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/drinks-categories?page={page}&limit={limit}&role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new List<DrinksCategory>(), 0);
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<DrinksCategory>>();
        return (payload?.Data ?? new List<DrinksCategory>(), payload?.Total ?? 0);
    }

    public async Task<DrinksCategory?> GetDrinksCategoryByIdAsync(Guid drinksCategory_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/drinks-categories/{drinksCategory_id}?role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DrinksCategory>();
    }

    public async Task RemoveByDrinkIdAsync(Guid drink_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/drinks/{drink_id}/drinks-categories?role={AppendRole(id_role)}");
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAsync(Guid drink_id, Guid category_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/drinks-categories?drinkId={drink_id}&categoryId={category_id}&role={AppendRole(id_role)}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<DrinksCategory>? GetRecordAsync(Guid drink_id, Guid category_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/drinks-categories/record?drinkId={drink_id}&categoryId={category_id}&role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DrinksCategory>();
    }
}

internal sealed class HttpFavCoffeeShopsRepository : HttpRepositoryBase, IFavCoffeeShopsRepository
{
    public HttpFavCoffeeShopsRepository(HttpClient client, ILogger<HttpFavCoffeeShopsRepository> logger) : base(client, logger) { }

    public async Task AddAsync(FavCoffeeShops favcoffeeshop, int id_role)
    {
        var response = await Client.PostAsJsonAsync($"/api/data/favorites/coffeeshops?role={AppendRole(id_role)}", favcoffeeshop);
        response.EnsureSuccessStatusCode();
    }

    public async Task<(List<CoffeeShop>? data, int total)> GetAllFavCoffeeShopsAsync(Guid user_id, int page, int limit, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/favorites/coffeeshops?userId={user_id}&page={page}&limit={limit}&role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new List<CoffeeShop>(), 0);
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<CoffeeShop>>();
        return (payload?.Data ?? new List<CoffeeShop>(), payload?.Total ?? 0);
    }

    public async Task<FavCoffeeShops>? GetRecordAsync(Guid coffeeshop_id, Guid user_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/favorites/coffeeshops/record?coffeeShopId={coffeeshop_id}&userId={user_id}&role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FavCoffeeShops>();
    }

    public async Task RemoveAsync(Guid user_id, Guid coffeeshop_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/favorites/coffeeshops?userId={user_id}&coffeeShopId={coffeeshop_id}&role={AppendRole(id_role)}");
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveByCoffeeShopIdAsync(Guid coffeeshop_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/favorites/coffeeshops/by-shop/{coffeeshop_id}?role={AppendRole(id_role)}");
        response.EnsureSuccessStatusCode();
    }
}

internal sealed class HttpFavDrinksRepository : HttpRepositoryBase, IFavDrinksRepository
{
    public HttpFavDrinksRepository(HttpClient client, ILogger<HttpFavDrinksRepository> logger) : base(client, logger) { }

    public async Task AddAsync(FavDrinks favdrink, int id_role)
    {
        var response = await Client.PostAsJsonAsync($"/api/data/favorites/drinks?role={AppendRole(id_role)}", favdrink);
        response.EnsureSuccessStatusCode();
    }

    public async Task<(List<Drink>? data, int total)> GetAllFavDrinksAsync(Guid user_id, int page, int limit, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/favorites/drinks?userId={user_id}&page={page}&limit={limit}&role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new List<Drink>(), 0);
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<Drink>>();
        return (payload?.Data ?? new List<Drink>(), payload?.Total ?? 0);
    }

    public async Task<FavDrinks>? GetRecordAsync(Guid drink_id, Guid user_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/favorites/drinks/record?drinkId={drink_id}&userId={user_id}&role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FavDrinks>();
    }

    public async Task RemoveAsync(Guid user_id, Guid drink_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/favorites/drinks?userId={user_id}&drinkId={drink_id}&role={AppendRole(id_role)}");
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveByDrinkIdAsync(Guid drink_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/favorites/drinks/by-drink/{drink_id}?role={AppendRole(id_role)}");
        response.EnsureSuccessStatusCode();
    }
}

internal sealed class HttpMenuRepository : HttpRepositoryBase, IMenuRepository
{
    public HttpMenuRepository(HttpClient client, ILogger<HttpMenuRepository> logger) : base(client, logger) { }

    public async Task AddAsync(Menu menuRecord, int id_role)
    {
        var response = await Client.PostAsJsonAsync($"/api/data/menu?role={AppendRole(id_role)}", menuRecord);
        response.EnsureSuccessStatusCode();
    }

    public async Task<(List<Company>? data, int total)> GetCompaniesByDrinkIdAsync(Guid drink_id, int page, int limit, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/menu/drink/{drink_id}/companies?page={page}&limit={limit}&role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new List<Company>(), 0);
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<Company>>();
        return (payload?.Data ?? new List<Company>(), payload?.Total ?? 0);
    }

    public async Task<(List<Menu>? data, int total)> GetMenuByCompanyId(Guid company_id, int page, int limit, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/menu/company/{company_id}?page={page}&limit={limit}&role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new List<Menu>(), 0);
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<Menu>>();
        return (payload?.Data ?? new List<Menu>(), payload?.Total ?? 0);
    }

    public async Task RemoveAsync(Guid drink_id, Guid company_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/menu?drinkId={drink_id}&companyId={company_id}&role={AppendRole(id_role)}");
        response.EnsureSuccessStatusCode();
    }
}

internal sealed class HttpUserRepository : HttpRepositoryBase, IUserRepository
{
    public HttpUserRepository(HttpClient client, ILogger<HttpUserRepository> logger) : base(client, logger) { }

    public async Task<Guid> AddUserAsync(User user, int id_role)
    {
        var response = await Client.PostAsJsonAsync($"/api/data/users?role={AppendRole(id_role)}", user);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
        return created?.Id is null ? Guid.Empty : Guid.Parse(created.Id);
    }

    public async Task DeleteUserAsync(Guid id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/users/{id}?role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var err = await ReadErrorAsync(response);
            throw new UserLastAdminException(err?.ErrorDetails ?? "Cannot delete last admin");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UserNotFoundException("User not found");
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task<(List<User>? data, int total)> GetAllUsersAsync(UserFilters filters, int page, int limit, int id_role)
    {
        var query = $"/api/data/users?page={page}&limit={limit}&role={AppendRole(id_role)}";
        if (!string.IsNullOrEmpty(filters.Login))
        {
            query += $"&login={Uri.EscapeDataString(filters.Login)}";
        }

        if (filters.UserRole != null)
        {
            query += $"&userRole={filters.UserRole}";
        }

        var response = await Client.GetAsync(query);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new List<User>(), 0);
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<User>>();
        return (payload?.Data ?? new List<User>(), payload?.Total ?? 0);
    }

    public async Task<User?> GetUserByIdAsync(Guid user_id, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/users/id/{user_id}?role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>();
    }

    public async Task<User?> GetUserByLoginAsync(string login, int id_role)
    {
        var response = await Client.GetAsync($"/api/data/users/login/{Uri.EscapeDataString(login)}?role={AppendRole(id_role)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>();
    }

    public async Task PartialUpdateUserAsync(Guid Id_user, string login, string password, string email, int id_role)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/data/users/{Id_user}?role={AppendRole(id_role)}")
        {
            Content = JsonContent.Create(new UserPartialUpdate(login, password, email))
        };

        var response = await Client.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UserNotFoundException("User not found");
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveUserAsync(Guid user_id, int id_role)
    {
        var response = await Client.DeleteAsync($"/api/data/users/{user_id}?role={AppendRole(id_role)}");
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateUserAsync(User user, int id_role)
    {
        var response = await Client.PutAsJsonAsync($"/api/data/users/{user.Id_user}?role={AppendRole(id_role)}", user);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UserNotFoundException("User not found");
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateUserRightsAsync(Guid id, int new_id_role, int id_role)
    {
        var response = await Client.PostAsJsonAsync($"/api/data/users/{id}/role?role={AppendRole(id_role)}", new UpdateUserRoleRequest(new_id_role));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UserNotFoundException("User not found");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var err = await ReadErrorAsync(response);
            throw new UserLastAdminException(err?.ErrorDetails ?? "Cannot update last admin");
        }

        response.EnsureSuccessStatusCode();
    }
}

internal record UserPartialUpdate(string Login, string Password, string Email);
internal record UpdateUserRoleRequest(int Role);
