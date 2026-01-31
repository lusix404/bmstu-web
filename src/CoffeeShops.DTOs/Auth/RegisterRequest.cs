using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoffeeShops.DTOs.Auth;

public class RegisterRequest
{

    [JsonPropertyName("login")]
    [Required]
    public string Login { get; set; }

    [JsonPropertyName("password")]
    [Required]
    public string Password { get; set; }

    [JsonPropertyName("birthbate")]
    [Required]
    public DateTime BirthDate { get; set; }

    [JsonPropertyName("email")]
    [Required]
    public string Email { get; set; }
}
