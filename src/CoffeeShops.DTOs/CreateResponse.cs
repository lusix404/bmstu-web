using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoffeeShops.DTOs.Utils;

public class CreateResponse
{

    [JsonPropertyName("id")]
    [Required]
    public string Id { get; set; }
}
