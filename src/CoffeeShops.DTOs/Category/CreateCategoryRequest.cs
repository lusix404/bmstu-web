using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.Category;

public class CreateCategoryRequest
{

    [JsonPropertyName("category_name")]
    [Required]
    public string CategoryName { get; set; } = string.Empty;

}