using CoffeeShops.Domain.Models;

namespace CoffeeShops.Domain.Interfaces.Repositories
{
    public interface IFavDrinksRepository
    {
        Task AddAsync(FavDrinks added_drink, int id_role);
        Task RemoveAsync(Guid user_id, Guid drink_id, int id_role);
        //Task<List<FavDrinks>?> GetAllFavDrinksAsync(Guid user_id, int id_role);
        Task<(List<Drink>? data, int total)> GetAllFavDrinksAsync(Guid user_id, int page, int limit, int id_role);
        Task RemoveByDrinkIdAsync(Guid drink_id, int id_role);
        Task<FavDrinks>? GetRecordAsync(Guid drink_id, Guid user_id, int id_role);

    }
}
