using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.FavCoffeeShops;

public class AddFavCoffeeShops
{
    [JsonPropertyName("id_cofeeshop")]
    [Required]
    public Guid Id_cofeeshop { get; set; }


    [JsonPropertyName("id_user")]
    [Required]
    public Guid Id_user { get; set; }
}
