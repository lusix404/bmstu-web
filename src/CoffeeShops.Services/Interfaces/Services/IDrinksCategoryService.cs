using CoffeeShops.Domain.Models;

namespace CoffeeShops.Services.Interfaces.Services
{
    public interface IDrinksCategoryService
    {
        Task AddAsync(Guid drink_id, Guid category_id, int id_role);
        Task RemoveAsync(Guid drink_id, int id_role);
        public Task<List<Category>?> GetCategoryByDrinkIdAsync(Guid drink_id, int id_role);
    }
}
