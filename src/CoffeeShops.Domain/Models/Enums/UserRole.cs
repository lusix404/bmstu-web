using CoffeeShops.Domain.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShops.Domain.Models.Enums;

public enum UserRole
{
    Ordinary_user = 1,        // Авторизованный пользователь
    Moderator = 2,  
    Administrator = 3
}

public static class UserRoleExtensions
{
    public static string ToRoleNameFromInt(int roleId)
    {
        return roleId switch
        {
            (int)UserRole.Ordinary_user => "Ordinary_user",
            (int)UserRole.Moderator => "Moderator",
            (int)UserRole.Administrator => "Administrator",
            _ => "Indefinite_role"
        };
    }
    public static UserRole ToUserRoleEnumTypeFromInt(int roleId)
    {
        return roleId switch
        {
            (int)UserRole.Ordinary_user => UserRole.Ordinary_user,
            (int)UserRole.Moderator => UserRole.Moderator,
            (int)UserRole.Administrator => UserRole.Administrator,
            _ => UserRole.Ordinary_user
        };
    }
    public static string ToStringFromUserRole(UserRole role)
    {
        return role switch
        {
            UserRole.Ordinary_user => "Ordinary_user",
            UserRole.Moderator => "Moderator",
            UserRole.Administrator => "Administrator",
            _ => "Ordinary_user",
        };
    }
    public static int ToRoleIntFromEnumType(UserRole role)
    {
        return role switch
        {
            UserRole.Ordinary_user => (int)UserRole.Ordinary_user,
            UserRole.Moderator => (int)UserRole.Moderator,
            UserRole.Administrator => (int)UserRole.Administrator,
            _ => (int)UserRole.Ordinary_user
        };
    }

    public static int ToRoleIntFromString(string roleName)
    {
        return roleName switch
        {
            "Ordinary_user" => (int)UserRole.Ordinary_user,
            "Moderator" => (int)UserRole.Moderator,
            "Administrator" => (int)UserRole.Administrator,
            _ => 0
        };
    }
}