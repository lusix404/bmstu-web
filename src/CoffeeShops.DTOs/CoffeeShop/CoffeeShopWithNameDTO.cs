using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.CoffeeShop;

public class CoffeeShopWithNameDto
{
    public Guid Id_coffeeshop { get; set; }
    public Guid Id_company { get; set; }
    public string Address { get; set; }
    public string WorkingHours { get; set; }
    public string CompanyName { get; set; }

}
