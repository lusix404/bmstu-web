using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.User;

public class UpdateUser
{
    [JsonPropertyName("id_user")]
    [Required]
    public Guid Id_user { get; set; }

    [JsonPropertyName("login")]
    [Required]
    public string Login { get; set; }

    [JsonPropertyName("password")]
    [Required]
    public string Password { get; set; }

    [JsonPropertyName("email")]
    [Required]
    public string Email { get; set; }

}


