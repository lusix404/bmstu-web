using CoffeeShops.DataAccess.Converters;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using CoffeeShops.DTOs.Drink;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;

namespace CoffeeShops.DataAccess.Repositories;
public class DrinkRepository : IDrinkRepository
{
    private IDbContextFactory _contextFactory;
    public DrinkRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Drink?> GetDrinkByIdAsync(Guid drink_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var drink = await _context.Drinks.FindAsync(drink_id);
        return drink != null ? DrinkConverter.ConvertToDomainModel(drink) : null;
    }
    //public async Task<int> CountDrinksAsync()
    //{
    //    return Get().CountAsync();
    //}
    //private IQueryable<Drink> GetAllDrinks()
    //{
    //    var _context = _contextFactory.GetDbContext(id_role);
    //    var query = _context.Drinks.AsQueryable();

    //    if (filters.OnlyFavorites && (filters.Id_user != null))
    //    {
    //        query = query.Where(cs => _context.FavDrinks.Any(fav => fav.Id_drink == cs.Id_drink
    //        && fav.Id_user == filters.Id_user));
    //    }
    //    if (!string.IsNullOrEmpty(filters.DrinkName))
    //    {
    //        query = query.Where(u => u.Name == filters.DrinkName);
    //    }
    //    if (!string.IsNullOrEmpty(filters.CategoryName))
    //    {
    //        query = query.Where(d => _context.DrinksCategories.Any(dc => dc.Id_drink == d.Id_drink &&
    //               dc.Category!.Name == filters.CategoryName));
    //    }
    //    return query;
    //}
    public async Task<(List<Drink> data, int total)> GetAllDrinksAsync(DrinkFilters filters, int page, int limit, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var query = _context.Drinks.AsQueryable();
        
        if (filters.OnlyFavorites && (filters.Id_user != null))
        {
            query = query.Where(cs => _context.FavDrinks.Any(fav => fav.Id_drink == cs.Id_drink
            && fav.Id_user == filters.Id_user));
        }
        if (!string.IsNullOrEmpty(filters.DrinkName))
        {
            query = query.Where(u => u.Name == filters.DrinkName);
        }
        if (!string.IsNullOrEmpty(filters.CategoryName))
        {
            query = query.Where(d => _context.DrinksCategories.Any(dc => dc.Id_drink == d.Id_drink &&
                   dc.Category!.Name == filters.CategoryName));
        }
        var total = await query.CountAsync();

        var drinks = await query
           .OrderBy(cs => cs.Name)
           .Skip((page - 1) * limit)
           .Take(limit)
           .AsNoTracking()
           .ToListAsync();
        return (drinks.ConvertAll(cs => DrinkConverter.ConvertToDomainModel(cs)), total);
    }

    public async Task<Guid> AddAsync(Drink drink, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);

        bool drinkExists = await _context.Drinks.AnyAsync(d => d.Name == drink.Name);

        if (drinkExists)
        {
            throw new DrinkNameAlreadyExistsException($"Напиток с именем '{drink.Name}' уже существует");
        }

        var drinkDb = DrinkConverter.ConvertToDbModel(drink);
        await _context.Drinks.AddAsync(drinkDb);
        await _context.SaveChangesAsync();

        var added = await _context.Drinks.FirstOrDefaultAsync(d => d.Name == drink.Name);
        return added.Id_drink;
    }

    public async Task RemoveAsync(Guid drink_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var drink = await _context.Drinks.FindAsync(drink_id);
        if (drink != null)
        {
            _context.Drinks.Remove(drink);
            await _context.SaveChangesAsync();
        }
    }
}

