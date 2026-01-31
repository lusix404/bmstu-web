using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using CoffeeShops.DTOs.Drink;
using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;

namespace CoffeeShops.Domain.Converters;


public static class CreateDrinkRequestConverter
{
    public static Drink ConvertToDomainModel(CreateDrinkRequest r)
    {
        return new Drink(r.DrinkName);
    }
}