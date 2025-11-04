using System.ComponentModel.DataAnnotations;

namespace CoffeeShops.Domain.Models.Enums
{
    public enum UserRole
    {
        Guest,       
        User,        // Авторизованный пользователь
        Moderator,  
        Administrator
    }
}
