using CoffeeShops.Domain.Models;

namespace CoffeeShops.Domain.Interfaces.Repositories
{
    public interface IMenuRepository
    {
        Task<(List<Menu>? data, int total)> GetMenuByCompanyId(Guid company_id, int page, int limit, int id_role);
        Task<(List<Company>? data, int total)> GetCompaniesByDrinkIdAsync(Guid drink_id, int page, int limit, int id_role);
        Task AddAsync(Menu menurecord, int id_role);
        Task RemoveAsync(Guid drink_id, Guid company_id, int id_role);
    }
}
