using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.Drink;

public class CreateDrinkRequest
{
    [JsonPropertyName("drink_name")]
    [Required]
    public string DrinkName { get; set; }
}
