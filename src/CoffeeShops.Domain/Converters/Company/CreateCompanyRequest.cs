using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using CoffeeShops.DTOs.Company;
using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;

namespace CoffeeShops.Domain.Converters;


public static class CreateCompanyRequestConverter
{
    public static Company ConvertToDomainModel(CreateCompanyRequest r)
    {
        return new Company(r.Name, r.Website);
    }
}