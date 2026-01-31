using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.Company;

public class CompanyResponse
{
    [JsonPropertyName("id_company")]
    [Required]
    public Guid Id_company { get; set; }

    [JsonPropertyName("name")]
    [Required]
    public string Name { get; set; }

    

    [JsonPropertyName("website")]
    public string Website { get; set; }
}
