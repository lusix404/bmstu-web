using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.Category;

public class CreateCategoryRequest
{

    [JsonPropertyName("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

}