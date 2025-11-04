using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoffeeShops.DTOs.Auth;

public class AuthResponse
{
    [JsonPropertyName("token")]
    [Required]
    public string Token { get; set; }

    [JsonPropertyName("id_user")]
    [Required]
    public Guid Id_user { get; set; }

    [JsonPropertyName("id_role")]
    [Required]
    public int Id_role { get; set; }
}
