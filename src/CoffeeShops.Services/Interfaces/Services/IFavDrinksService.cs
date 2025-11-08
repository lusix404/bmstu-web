using CoffeeShops.Domain.Models;
using CoffeeShops.DTOs.Pagination;

namespace CoffeeShops.Services.Interfaces.Services
{
    public interface IFavDrinksService
    {
        Task AddDrinkToFavsAsync(Guid user_id, Guid drink_id, int id_role);
        Task RemoveDrinkFromFavsAsync(Guid user_id, Guid drink_id, int id_role);
        Task<PaginatedResponse<Drink>>? GetFavDrinksAsync(Guid user_id, int page, int limit, int id_role);
    }
}
