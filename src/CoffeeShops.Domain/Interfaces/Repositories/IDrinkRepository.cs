using CoffeeShops.Domain.Models;
using CoffeeShops.DTOs.Drink;

namespace CoffeeShops.Domain.Interfaces.Repositories
{
    public interface IDrinkRepository
    {
        Task<Drink?> GetDrinkByIdAsync(Guid drink_id, int id_role);
        Task<(List<Drink>? data, int total)> GetAllDrinksAsync(DrinkFilters filters, int page, int limit, int id_role);
        Task<Guid> AddAsync(Drink drink, int id_role);
        Task RemoveAsync(Guid drink_id, int id_role);

    }
}

