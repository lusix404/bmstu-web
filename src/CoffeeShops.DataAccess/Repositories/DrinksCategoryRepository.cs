using CoffeeShops.DataAccess.Converters;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using CoffeeShops.DataAccess.Models;
using Npgsql;

namespace CoffeeShops.DataAccess.Repositories;
public class DrinksCategoryRepository : IDrinksCategoryRepository
{
    private IDbContextFactory _contextFactory;
    public DrinksCategoryRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Category>> GetAllCategoriesByDrinkIdAsync(Guid drink_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var categories = await _context.DrinksCategories
            .Where(dc => dc.Id_drink == drink_id)
            .Join(_context.Categories,
                dc => dc.Id_category,
                c => c.Id_category,
                (dc, c) => c)
            .AsNoTracking()
            .ToListAsync();

        return categories?.ConvertAll(CategoryConverter.ConvertToDomainModel);
    }

    public async Task AddAsync(DrinksCategory drinkscategory, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var drinksCategory = new DrinksCategoryDb(drinkscategory.Id_drink, drinkscategory.Id_category);
        await _context.DrinksCategories.AddAsync(drinksCategory);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Guid drink_id, Guid category_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var dc = await _context.DrinksCategories
             .FirstOrDefaultAsync(fd => fd.Id_drink == drink_id && fd.Id_category == category_id);

        if (dc != null)
        {
            _context.DrinksCategories.Remove(dc);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveByDrinkIdAsync(Guid drink_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var drinksCategories = await _context.DrinksCategories
            .Where(dc => dc.Id_drink == drink_id)
            .ToListAsync();

        if (drinksCategories.Any())
        {
            _context.DrinksCategories.RemoveRange(drinksCategories);
            await _context.SaveChangesAsync();
        }
    }
}
