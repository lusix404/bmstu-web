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
        Task DeleteUserAsync(Guid id, int id_role);
        Task<PaginatedResponse<User>>? GetAllUsersAsync(UserFilters filters, int page, int limit, int id_role);
        //Task UpdateUserAsync(User user, int id_role);
        Task UpdateUserAsync(User user, int id_role);
        //Task PartialUpdateUserAsync(Guid Id_user, string login, string password, string email, int id_role);
        Task UpdateUserRightsAsync(Guid id, int new_id_role, int id_role);


    }
}
