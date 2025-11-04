using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.Menu;

public class MenuResponse
{
    [JsonPropertyName("id_drink")]
    [Required]
    public Guid Id_drink { get; set; }

    [JsonPropertyName("id_company")]
    [Required]
    public Guid Id_company { get; set; }

    [JsonPropertyName("size")]
    [Required]
    public int Size { get; set; }

    [JsonPropertyName("price")]
    [Required]
    public decimal Price { get; set; }
}


