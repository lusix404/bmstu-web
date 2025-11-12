using CoffeeShops.Domain.Models;
using CoffeeShops.DTOs.Drink;
using CoffeeShops.DTOs.Pagination;

namespace CoffeeShops.Services.Interfaces.Services
{
    public interface IDrinkService
    {
        public Task<Drink?> GetDrinkByIdAsync(Guid drink_id, int id_role);
        public Task<PaginatedResponse<Drink>> GetAllDrinksAsync(DrinkFilters filters, int page, int limit, int id_role);
        public Task<Guid> AddDrinkAsync(Drink drink, int id_role);
        public Task DeleteDrinkAsync(Guid drink_id, int id_role);
        //public Task<List<Category>?> GetCategoryByDrinkIdAsync(Guid drink_id, int id_role);
    }
}
