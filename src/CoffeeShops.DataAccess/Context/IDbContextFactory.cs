using CoffeeShops.Domain.Models;
using System;

namespace CoffeeShops.DataAccess.Context
{
    public interface IDbContextFactory
    {
        CoffeeShopsContext GetDbContext(int? user_role);
    }
}
