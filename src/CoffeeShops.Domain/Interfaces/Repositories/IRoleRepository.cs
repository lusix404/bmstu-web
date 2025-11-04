using CoffeeShops.Domain.Models;

namespace CoffeeShops.Domain.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        //Task<string> GetRoleByIdAsync(Guid user_id);
        //Task<Role?> GetRoleByIdAsync(Guid role_id);
        Task<Role?> GetRoleByIdAsync(int role_id);
        //Task RemoveAsync(Guid role_id);
        Task RemoveAsync(int role_id);
        Task AddAsync(Role role);

        Task<List<Role>?> GetAllRolesAsync();



    }
}
