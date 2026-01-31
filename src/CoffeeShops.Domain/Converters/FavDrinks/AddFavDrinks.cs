using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using CoffeeShops.DTOs.FavDrinks;
using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;

namespace CoffeeShops.Domain.Converters;


public static class AddFavDrinksConverter
{
    public static FavDrinks ConvertToDomainModel(AddFavDrinks r)
    {
        return new FavDrinks(r.Id_drink);
    }
}