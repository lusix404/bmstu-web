
namespace CoffeeShops.Services.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string login, int idRole);
    }
}
