using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.Drink;

public class DrinkResponse
{
    [JsonPropertyName("id_drink")]
    [Required]
    public Guid Id_drink { get; set; }

    [JsonPropertyName("name")]
    [Required]
    public string Name { get; set; }
}
