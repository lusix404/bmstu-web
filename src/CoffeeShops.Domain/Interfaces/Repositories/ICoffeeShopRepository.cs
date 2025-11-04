using CoffeeShops.Domain.Models;
using CoffeeShops.DTOs.CoffeeShop;

namespace CoffeeShops.Domain.Interfaces.Repositories
{
    public interface ICoffeeShopRepository
    {
        Task<CoffeeShop?> GetCoffeeShopByIdAsync(Guid coffeeshop_id, int id_role);
        Task<(List<CoffeeShop>? data, int total)> GetCoffeeShopsByCompanyIdAsync(Guid company_id, CoffeeShopFilters filters, int page, int limit, int id_role);
        Task<(List<CoffeeShop>? data, int total)> GetAllCoffeeShopsAsync(CoffeeShopFilters filters, int page, int limit, int id_role);
        Task<Guid> AddAsync(CoffeeShop coffeeshop, int id_role);
        Task RemoveAsync(Guid coffeeshop_id, int id_role);
    }
}
