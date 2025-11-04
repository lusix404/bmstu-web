using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShops.DTOs.User;


public class UserFilters
{
    [FromQuery(Name = "login")]
    public string? Login { get; set; }

    [FromQuery(Name = "user_role")]
    public int?  UserRole { get; set; }
}
