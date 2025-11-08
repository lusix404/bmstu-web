using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.User;

public class UpdateUserRole
{
    [JsonPropertyName("user_role")]
    [Required]
    public string User_role { get; set; }

}


