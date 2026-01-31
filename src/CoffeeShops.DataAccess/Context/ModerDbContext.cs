using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using System;

namespace CoffeeShops.DataAccess.Context;

public class ModerDbContext : CoffeeShopsContext
{
    //public ModerDbContext(DbContextOptions<ModerDbContext> options)
    //: base(options) { }

    public ModerDbContext(DbContextOptions<ModerDbContext> options)
        : base(new DbContextOptionsBuilder<CoffeeShopsContext>()
            .UseNpgsql(options.FindExtension<NpgsqlOptionsExtension>().ConnectionString)
            .Options)
    {
    }
}
