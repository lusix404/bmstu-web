using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeeShops.DTOs.Category;

namespace CoffeeShops.DTOs.DrinksCategory;

public class AddDrinksCategory
{
    //[JsonPropertyName("category_ids")]
    //[MinLength(1, ErrorMessage = "Должна быть указана хотя бы одна категория")]
    //public List<Guid> CategoryIds { get; set; } = new List<Guid>();

    [JsonPropertyName("category_id")]
    public Guid CategoryId { get; set; }
}
