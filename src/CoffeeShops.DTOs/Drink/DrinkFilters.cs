using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShops.DTOs.Drink;



public class DrinkFilters
{
    [FromQuery(Name = "category_name")]
    public string? CategoryName { get; set; }

    [FromQuery(Name = "drink_name")]
    public string? DrinkName { get; set; }

    [FromQuery(Name = "onlyFavorites")]
    public bool OnlyFavorites { get; set; } = false;

    [FromQuery(Name = "id_user")]
    public Guid? Id_user { get; set; }
}
