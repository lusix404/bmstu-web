using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Exceptions.CategoryServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CoffeeShops.Services.Services;

public class DrinksCategoryService : IDrinksCategoryService
{
    private readonly IDrinksCategoryRepository _drinkscategoryRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IDrinkRepository _drinkRepository;
    private readonly ILogger<DrinksCategoryService> _logger;

    public DrinksCategoryService(
        IDrinksCategoryRepository drinkscategoryRepository,
        ICategoryRepository categoryRepository,
        IDrinkRepository drinkRepository,
        ILogger<DrinksCategoryService> logger)
    {
        _drinkscategoryRepository = drinkscategoryRepository;
        _categoryRepository = categoryRepository;
        _drinkRepository = drinkRepository;
        _logger = logger;
    }

    public async Task AddAsync(Guid drink_id, Guid category_id, int id_role)
    {
        _logger.LogInformation("Adding category {CategoryId} to drink {DrinkId}",
            category_id, drink_id, id_role);

        try
        {
            var drink = await _drinkRepository.GetDrinkByIdAsync(drink_id, id_role);
            if (drink == null)
            {
                _logger.LogWarning("Drink not found: {DrinkId}", drink_id);
                throw new DrinkNotFoundException($"Drink with id={drink_id} was not found");
            }

            var category = await _categoryRepository.GetCategoryByIdAsync(category_id, id_role);
            if (category == null)
            {
                _logger.LogWarning("Category not found: {CategoryId}", category_id);
                throw new CategoryNotFoundException($"Category with id={category_id} was not found");
            }

            var have_category = await _drinkscategoryRepository.GetRecordAsync(drink_id, category_id, id_role);
            if (have_category != null)
            {
                _logger.LogWarning("Drink {DrinkId} already has category {CategoryId}",
                    drink_id, category_id);
                throw new DrinkAlreadyHasThisCategoryException(
                    $"Drink with id={drink_id} has already had category with id={category_id}");
            }
            //if (drink.DrinkCategories.Any(f => f.Id_drink == drink_id && f.Id_category == category_id))
            //{
            //    throw new DrinkAlreadyHasThisCategoryException($"Drink with id={drink_id} has already had category with id={category_id}");
            //}


            DrinksCategory record = new DrinksCategory(drink_id, category_id);
            //drink.Categories.Add(category);
            await _drinkscategoryRepository.AddAsync(record, id_role);

            _logger.LogInformation("Successfully added category {CategoryId} to drink {DrinkId}",
                category_id, drink_id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding category {CategoryId} to drink {DrinkId}",
                category_id, drink_id);
            throw;
        }
    }

    public async Task RemoveAsync(Guid drink_id, int id_role)
    {
        _logger.LogInformation("Removing all categories from drink {DrinkId}",
            drink_id, id_role);

        try
        {
            var drink = await _drinkRepository.GetDrinkByIdAsync(drink_id, id_role);
            if (drink == null)
            {
                _logger.LogWarning("Drink not found: {DrinkId}", drink_id);
                throw new DrinkNotFoundException($"Drink with id={drink_id} was not found");
            }

            //int categoriesCount = drink.Categories.Count;
            //drink.Categories.Clear();
            await _drinkscategoryRepository.RemoveByDrinkIdAsync(drink_id, id_role);

            _logger.LogInformation("Successfully removed all categories from drink {DrinkId}",drink_id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing categories from drink {DrinkId}", drink_id);
            throw;
        }
    }

    public async Task<List<Category>?> GetCategoryByDrinkIdAsync(Guid drink_id, int id_role)
    {
        _logger.LogInformation("Getting categories for drink {DrinkId}",
            drink_id, id_role);

        try
        {
            var drink = await _drinkRepository.GetDrinkByIdAsync(drink_id, id_role);
            if (drink == null)
            {
                _logger.LogWarning("Drink not found: {DrinkId}", drink_id);
                throw new DrinkNotFoundException($"Drink with id={drink_id} was not found");
            }

            var categories = await _drinkscategoryRepository.GetAllCategoriesByDrinkIdAsync(drink_id, id_role);

            if (categories == null || !categories.Any())
            {
                _logger.LogWarning($"There are no categories for drink with id={drink_id}");
                throw new NoDrinksCategoriesFoundException($"There are no categories for drink with id={drink_id}");
            }

            _logger.LogInformation("Found {CategoriesCount} categories for drink {DrinkId}",
                categories?.Count ?? 0, drink_id);

            return categories;
        }
        catch (NoDrinksCategoriesFoundException ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories for drink {DrinkId}", drink_id);
            throw;
        }
    }
}