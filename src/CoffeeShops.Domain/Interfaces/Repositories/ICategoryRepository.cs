using CoffeeShops.Domain.Models;

namespace CoffeeShops.Domain.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        //Task<Category?> GetCategoryByIdAsync(Guid category_id);
        //Task<List<Category>?> GetAllCategoriesAsync();

        //Task RemoveAsync(Guid category_id);
        //Task AddAsync(Category category);
        
            Task<Category?> GetCategoryByIdAsync(Guid category_id, int id_role);
        //Task<List<Category>?> GetAllCategoriesAsync(int page, int limit, int id_role);
            Task<(List<Category>? data, int total)> GetAllCategoriesAsync(int page, int limit, int id_role);
            Task<Guid> AddCategoryAsync(Category category, int id_role);
            //Task RemoveAsync(int category_id);
            //Task AddAsync(Category category);

    }
}
