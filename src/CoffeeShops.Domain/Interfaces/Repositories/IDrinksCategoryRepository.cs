using CoffeeShops.Domain.Models;

namespace CoffeeShops.Domain.Interfaces.Repositories
{
    public interface IDrinksCategoryRepository
    {
        Task<List<Category>?> GetAllCategoriesByDrinkIdAsync(Guid drink_id, int id_role);
        Task AddAsync(DrinksCategory drinksCategory, int id_role);
        Task RemoveAsync(Guid drink_id, Guid category_id, int id_role);
        Task RemoveByDrinkIdAsync(Guid drink_id, int id_role);
        Task<DrinksCategory>? GetRecordAsync(Guid drink_id, Guid category_id, int id_role);

    }
}
