using CoffeeShops.DataAccess.Converters;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;
using Microsoft.EntityFrameworkCore;
using CoffeeShops.DataAccess.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Exceptions.CategoryServiceExceptions;

namespace CoffeeShops.DataAccess.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private IDbContextFactory _contextFactory;
    public CategoryRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid category_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var category = await _context.Categories.FindAsync(category_id);
        return category != null ? CategoryConverter.ConvertToDomainModel(category) : null;

    }

    //modified
    public async Task<(List<Category>? data, int total)> GetAllCategoriesAsync(int page, int limit, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);

        var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Skip((page - 1) * limit)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync();

        return (categories.ConvertAll(category => CategoryConverter.ConvertToDomainModel(category)), categories.Count);
    }

    public async Task<Guid> AddCategoryAsync(Category category, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);

        bool Exists = await _context.Categories.AnyAsync(d => d.Name == category.Name);

        if (Exists)
        {
            throw new CategoryUniqueException($"Категория с именем '{category.Name}' уже существует");
        }

        var categoryDb = CategoryConverter.ConvertToDbModel(category);
        await _context.AddAsync(categoryDb);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var innerException = ex.InnerException;

            if (innerException != null)
            {
                Console.WriteLine($"Inner Exception: {innerException.Message}");
            }
            else
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error while adding new category", ex);
        }
        return categoryDb.Id_category;
    }


    public async Task RemoveCategoryAsync(int category_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var categoryDb = await _context.Categories.FindAsync(category_id);
        if (categoryDb != null)
        {
            _context.Categories.Remove(categoryDb);
            await _context.SaveChangesAsync();
        }


    }

}


