using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.DTOs.Auth;
using CoffeeShops.DTOs.Pagination;
using CoffeeShops.DTOs.User;

namespace CoffeeShops.Services.Interfaces.Services
{
    public interface IUserService
    {
        Task<AuthResponse> Login(string login, string password);
        Task<Guid> Registrate(User user);
        Task<User?> GetUserByIdAsync(Guid user_id, int id_role);
        Task<User?> GetUserByLoginAsync(string login, int id_role);
        //Task<List<User>?> GetAllUsersAsync(int id_role);
        Task<PaginatedResponse<User>>? GetAllUsersAsync(UserFilters filters, int page, int limit, int id_role);

        //Task RevokeModerRightsAsync(Guid id, int id_role);
        //Task GrantModerRightsAsync(Guid id, int id_role);


    }
}
