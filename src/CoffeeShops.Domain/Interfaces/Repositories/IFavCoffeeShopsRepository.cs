using CoffeeShops.Domain.Models;

namespace CoffeeShops.Domain.Interfaces.Repositories
{
    public interface IFavCoffeeShopsRepository
    {
        Task AddAsync(FavCoffeeShops coffeeshop, int id_role);
        Task RemoveAsync(Guid user_id, Guid coffeeshop_id, int id_role);
        //Task<List<FavCoffeeShops>?> GetAllFavCoffeeShopsAsync(Guid user_id, int id_role);
        Task<(List<CoffeeShop>? data, int total)> GetAllFavCoffeeShopsAsync(Guid user_id, int page, int limit, int id_role);
        Task RemoveByCoffeeShopIdAsync(Guid coffeeshop_id, int id_role);
        Task<FavCoffeeShops>? GetRecordAsync(Guid coffeeshop_id, Guid user_id, int id_role);

    }
}
