using CoffeeShops.Domain.Models;
using System;

namespace CoffeeShops.DataAccess.Context;

public class DbContextFactory : IDbContextFactory
{
    private readonly GuestDbContext _guestDbContext; 
    private readonly UserDbContext _userDbContext;
    private readonly ModerDbContext _moderDbContext;
    private readonly AdminDbContext _adminDbContext;

    public DbContextFactory(GuestDbContext guestDbContext, UserDbContext userDbContext, ModerDbContext moderDbContext, AdminDbContext adminDbContext)
    {
        _moderDbContext = moderDbContext;
        _adminDbContext = adminDbContext;
        _userDbContext = userDbContext;
        _guestDbContext = guestDbContext;
    }

    public CoffeeShopsContext GetDbContext(int? user_role)
    {
        if (user_role == 1)
            return _userDbContext;
        else if (user_role == 2)
            return _moderDbContext;
        else if (user_role == 3)
            return _adminDbContext;
        else
            return _guestDbContext;

    }
}
