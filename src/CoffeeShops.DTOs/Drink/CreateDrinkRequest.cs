using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.Drink;

public class CreateDrinkRequest
{
    [JsonPropertyName("name")]
    [Required]
    public string Name { get; set; }
}
