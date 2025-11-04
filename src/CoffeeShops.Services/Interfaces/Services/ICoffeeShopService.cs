using CoffeeShops.Domain.Models;
using CoffeeShops.DTOs.CoffeeShop;
using CoffeeShops.DTOs.Pagination;

namespace CoffeeShops.Services.Interfaces.Services
{
    public interface ICoffeeShopService
    {
        public Task<CoffeeShop?> GetCoffeeShopByIdAsync(Guid coffeeshop_id, int id_role);
        public Task<PaginatedResponse<CoffeeShop>>? GetCoffeeShopsByCompanyIdAsync(Guid company_id, CoffeeShopFilters filters, int page, int limit, int id_role);
        public Task<Guid> AddCoffeeShopAsync(CoffeeShop coffeeshop, int id_role);
        public Task DeleteCoffeeShopAsync(Guid coffeeshop_id, int id_role);
    }
}
