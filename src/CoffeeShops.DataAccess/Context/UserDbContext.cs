using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using System;

namespace CoffeeShops.DataAccess.Context;

public class UserDbContext : CoffeeShopsContext
{
    //public UserDbContext(DbContextOptions<UserDbContext> options)
    //: base(options) { }
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(new DbContextOptionsBuilder<CoffeeShopsContext>()
            .UseNpgsql(options.FindExtension<NpgsqlOptionsExtension>().ConnectionString)
            .Options)
    {
    }
}
