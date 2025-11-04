using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.FavDrinks;

public class AddFavDrinks
{
    [JsonPropertyName("id_drink")]
    [Required]
    public Guid Id_drink { get; set; }


    [JsonPropertyName("id_user")]
    [Required]
    public Guid Id_user { get; set; }
}
