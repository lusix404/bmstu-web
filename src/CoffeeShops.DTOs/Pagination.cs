using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoffeeShops.DTOs.Pagination;

public class PaginatedResponse<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }
}

public class PaginatedUserResponse<UserResponse> { }
public class PaginatedDrinkResponse<DrinkResponse> { }
public class PaginatedCompanyResponse<CompanyResponse> { }
public class PaginatedCoffeeShopResponse<CoffeeShopResponse> { }
public class PaginatedFavCoffeeShopsResponse<FavCoffeeShopsResponse> { }
public class PaginatedFavDrinksResponse<FavDrinksResponse> { }

