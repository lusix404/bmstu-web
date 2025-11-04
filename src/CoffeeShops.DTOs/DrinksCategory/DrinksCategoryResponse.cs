using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeeShops.DTOs.Category;

namespace CoffeeShops.DTOs.DrinksCategory;

public class DrinksCategoryResponse
{
    [JsonPropertyName("id_drink")]
    [Required]
    public Guid Id_drink { get; set; }

    [JsonPropertyName("drink_name")]
    [Required]
    public string DrinkName { get; set; }

    [JsonPropertyName("categories")]
    public List<CategoryResponse>? Categories { get; set; }
}
