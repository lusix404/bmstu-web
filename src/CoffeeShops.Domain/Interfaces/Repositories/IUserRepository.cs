using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.DTOs.User;

namespace CoffeeShops.Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByLoginAsync(string login, int id_role);
        Task<User?> GetUserByIdAsync(Guid user_id, int id_role);
        //Task<List<User>?> GetAllUsersAsync(int id_role);
        Task<(List<User>? data, int total)> GetAllUsersAsync(UserFilters filters, int page, int limit, int id_role);
        Task<Guid> AddUserAsync(User user, int id_role);
        Task RemoveUserAsync(Guid user_id, int id_role);
        //Task GrantModerRightsAsync(Guid id, int id_role);
        //Task RevokeModerRightsAsync(Guid id, int id_role);
        Task UpdateUserAsync(User user, int id_role);
        Task UpdateUserRightsAsync(Guid id, UserRole new_id_role, int id_role);
        Task DeleteUserAsync(Guid id, int id_role);
    }
}
