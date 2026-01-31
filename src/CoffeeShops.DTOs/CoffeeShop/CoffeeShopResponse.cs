using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.CoffeeShop;

public class CoffeeShopResponse
{
    [JsonPropertyName("id_coffeeshop")]
    [Required]
    public Guid Id_coffeeshop { get; set; }

    [JsonPropertyName("id_company")]
    [Required]
    public Guid Id_company { get; set; }

    [JsonPropertyName("address")]
    [Required]
    public string Address { get; set; }


    [JsonPropertyName("working_hours")]
    [Required]
    public string WorkingHours { get; set; }

}
