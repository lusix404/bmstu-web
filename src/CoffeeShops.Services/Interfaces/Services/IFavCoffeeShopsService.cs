using CoffeeShops.Domain.Models;
using CoffeeShops.DTOs.Pagination;

namespace CoffeeShops.Services.Interfaces.Services
{
    public interface IFavCoffeeShopsService
    {
        Task AddCoffeeShopToFavsAsync(Guid user_id, Guid coffeeshop_id, int id_role);
        Task RemoveCoffeeShopFromFavsAsync(Guid user_id, Guid coffeeshop_id, int id_role);
        Task<PaginatedResponse<CoffeeShop>>? GetFavCoffeeShopsAsync(Guid user_id, int page, int limit, int id_role);
    }
}
