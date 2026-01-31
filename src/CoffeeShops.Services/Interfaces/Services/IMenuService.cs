using CoffeeShops.Domain.Models;
using CoffeeShops.DTOs.Pagination;

namespace CoffeeShops.Services.Interfaces.Services
{
    public interface IMenuService
    {
        public Task<PaginatedResponse<Menu>>? GetMenuByCompanyIdAsync(Guid company_id, int page, int limit, int id_role);
        public Task<PaginatedResponse<Company>>? GetCompaniesByDrinkIdAsync(Guid drink_id, int page, int limit, int id_role);
        public Task AddDrinkToMenuAsync(Menu menurecord, int id_role);
        public Task DeleteDrinkFromMenuAsync(Guid drink_id, Guid company_id, int id_role);
        public Task<Drink?> GetDrinkByIdAsync(Guid drink_id, int id_role);


    }
}
