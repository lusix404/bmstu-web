using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.FavCoffeeShops;

public class AddFavCoffeeShops
{
    [JsonPropertyName("id_coffeeshop")]
    [Required]
    public Guid Id_coffeeshop { get; set; }
}
