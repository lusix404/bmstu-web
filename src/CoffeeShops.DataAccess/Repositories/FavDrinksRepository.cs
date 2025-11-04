using CoffeeShops.DataAccess.Converters;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using CoffeeShops.DataAccess.Models;
using Npgsql;

namespace CoffeeShops.DataAccess.Repositories;
public class FavDrinksRepository : IFavDrinksRepository
{
    //private readonly CoffeeShopsContext _context;

    //public FavDrinksRepository(CoffeeShopsContext context)
    //{
    //    _context = context;
    //}
    private IDbContextFactory _contextFactory;
    public FavDrinksRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddAsync(FavDrinks added_drink, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var drinkDb = FavDrinksConverter.ConvertToDbModel(added_drink);
        await _context.FavDrinks.AddAsync(drinkDb);
        await _context.SaveChangesAsync();

    }

    public async Task RemoveAsync(Guid user_id, Guid drink_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var favDrink = await _context.FavDrinks
            .FirstOrDefaultAsync(fd => fd.Id_user == user_id && fd.Id_drink == drink_id);

        if (favDrink != null)
        {
            _context.FavDrinks.Remove(favDrink);
            await _context.SaveChangesAsync();
        }

    }

    //public async Task<List<FavDrinks>> GetAllFavDrinksAsync(Guid user_id, int id_role)
    //{
    //    var _context = _contextFactory.GetDbContext(id_role);
    //    var favDrinks = await _context.FavDrinks
    //        .Where(fd => fd.Id_user == user_id)
    //        .AsNoTracking()
    //        .ToListAsync();

    //    return favDrinks.ConvertAll(d => FavDrinksConverter.ConvertToDomainModel(d));

    //}
    public async Task<(List<Drink>? data, int total)> GetAllFavDrinksAsync(Guid user_id, int page, int limit, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var query = _context.Drinks.AsQueryable();
        query = query.Where(cs => _context.FavDrinks.Any(fav => fav.Id_drink == cs.Id_drink
            && fav.Id_user == user_id));
        var total = await query.CountAsync();

        var drinks = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * limit)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return (drinks.ConvertAll(drink => DrinkConverter.ConvertToDomainModel(drink)), total);
    }


}

