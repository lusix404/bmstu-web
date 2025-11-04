using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.User;

public class UserResponse
{
    [JsonPropertyName("id_user")]
    [Required]
    public Guid Id_user { get; set; }

    [JsonPropertyName("user_role")]
    [Required]
    public int Id_role { get; set; }

    [JsonPropertyName("login")]
    [Required]
    public string Login { get; set; }


    [JsonPropertyName("birthbate")]
    [Required]
    public DateTime BirthDate { get; set; }

    [JsonPropertyName("email")]
    [Required]
    public string Email { get; set; }

}


