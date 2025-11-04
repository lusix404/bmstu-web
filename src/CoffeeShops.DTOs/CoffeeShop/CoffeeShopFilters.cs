using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShops.DTOs.CoffeeShop;



public class CoffeeShopFilters
{
    [FromQuery(Name = "id_company")]
    public Guid? Id_company { get; set; }

    [FromQuery(Name = "onlyFavorites")]
    public bool OnlyFavorites { get; set; } = false;

    [FromQuery(Name = "id_user")]
    public Guid? Id_user { get; set; }
}
