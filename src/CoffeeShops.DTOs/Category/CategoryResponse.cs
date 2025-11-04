using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.Category;

public class CategoryResponse
{
    [JsonPropertyName("id_category")]
    [Required]
    public Guid Id_category { get; set; }

    [JsonPropertyName("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

}