using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using System;

namespace CoffeeShops.DataAccess.Context;

public class GuestDbContext : CoffeeShopsContext
{
    //public GuestDbContext(DbContextOptions<GuestDbContext> options)
    //: base(options) { }

    public GuestDbContext(DbContextOptions<GuestDbContext> options)
        : base(new DbContextOptionsBuilder<CoffeeShopsContext>()
            .UseNpgsql(options.FindExtension<NpgsqlOptionsExtension>().ConnectionString)
            .Options)
    {
    }
}
