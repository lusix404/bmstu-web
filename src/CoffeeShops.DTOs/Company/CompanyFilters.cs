using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShops.DTOs.Company;



public class CompanyFilters
{

    [FromQuery(Name = "id_drink")]
    public Guid? Id_drink { get; set; }
}
