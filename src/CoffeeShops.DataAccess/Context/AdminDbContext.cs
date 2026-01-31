using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using System;

namespace CoffeeShops.DataAccess.Context;

//public class AdminDbContext : CoffeeShopsContext
//{
//    public AdminDbContext(DbContextOptions<AdminDbContext> options)
//    : base(options) { }

//}

public class AdminDbContext : CoffeeShopsContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(new DbContextOptionsBuilder<CoffeeShopsContext>()
            .UseNpgsql(options.FindExtension<NpgsqlOptionsExtension>().ConnectionString)
            .Options)
    {
    }
}
