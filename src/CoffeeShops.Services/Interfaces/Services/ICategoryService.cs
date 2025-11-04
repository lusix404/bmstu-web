using CoffeeShops.Domain.Models;
using CoffeeShops.DTOs.Pagination;

namespace CoffeeShops.Services.Interfaces.Services
{
    public interface ICategoryService
    {
        public Task<Category?> GetCategoryByIdAsync(Guid category_id, int id_role);
        //public Task<List<Category>?> GetAllCategoriesAsync(int id_role);
        public Task<PaginatedResponse<Category>>? GetAllCategoriesAsync(int page, int limit, int id_role);
        public Task<Guid> AddCategoryAsync(Category category, int id_role);
    }
}
