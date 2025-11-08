using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using CoffeeShops.DTOs.Category;
using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;

namespace CoffeeShops.Domain.Converters;


public static class CreateCategoryRequestConverter
{
    public static Category ConvertToDomainModel(CreateCategoryRequest r)
    {
        return new Category(r.CategoryName);
    }
}