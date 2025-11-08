using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.FavDrinks;

public class AddFavDrinks
{
    [JsonPropertyName("id_drink")]
    [Required]
    public Guid Id_drink { get; set; }
}
