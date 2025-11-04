using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeeShops.DTOs.Category;

namespace CoffeeShops.DTOs.DrinksCategory;

public class AddDrinksCategory
{
    [JsonPropertyName("id_drink")]
    [Required]
    public Guid Id_drink { get; set; }

    [JsonPropertyName("category_ids")]
    [Required]
    public List<Guid> CategoryIds { get; set; }
}
