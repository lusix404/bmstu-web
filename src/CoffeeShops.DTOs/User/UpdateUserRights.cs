using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeeShops.DTOs.User;

public class UpdateUserRights
{
    [JsonPropertyName("id_user")]
    [Required]
    public Guid Id_user { get; set; }

    [JsonPropertyName("user_role")]
    [Required]
    public int Id_role { get; set; }

}


