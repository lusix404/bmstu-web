using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using CoffeeShops.DTOs.CoffeeShop;
using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;

namespace CoffeeShops.Domain.Converters;


public static class CreateCoffeeShopRequestConverter
{
    public static CoffeeShop ConvertToDomainModel(CreateCoffeeShopRequest r)
    {
        return new CoffeeShop(r.Id_company, r.Address, r.WorkingHours);
    }
}