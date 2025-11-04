using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using CoffeeShops.DTOs.Pagination;
using CoffeeShops.DTOs.Drink;

namespace CoffeeShops.Services.Services;

public class DrinkService : IDrinkService
{
    private readonly IDrinkRepository _drinkRepository;
    //private readonly IDrinksCategoryRepository _drinkscategoryRepository;
    private readonly ILogger<DrinkService> _logger;

    public DrinkService(
        IDrinkRepository drinkRepository,
        ILogger<DrinkService> logger)
    {
        _drinkRepository = drinkRepository;
        _logger = logger;
    }

    public async Task<Drink?> GetDrinkByIdAsync(Guid drink_id, int id_role)
    {
        _logger.LogInformation("Getting drink by ID: {DrinkId}", drink_id);

        try
        {
            var drink = await _drinkRepository.GetDrinkByIdAsync(drink_id, id_role);

            if (drink == null)
            {
                _logger.LogWarning("Drink not found: {DrinkId}", drink_id);
                throw new DrinkNotFoundException($"Drink with id={drink_id} was not found");
            }

            _logger.LogInformation("Successfully retrieved drink: {DrinkName} (ID: {DrinkId})",
                drink.Name, drink_id);
            return drink;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drink by ID: {DrinkId}", drink_id);
            throw;
        }
    }

    public async Task<PaginatedResponse<Drink>>? GetAllDrinksAsync(DrinkFilters filters, int page, int limit, int id_role)
    {
        _logger.LogInformation("Getting all drinks");

        try
        {
            (var drinks, int total) = await _drinkRepository.GetAllDrinksAsync(filters, page, limit, id_role);

            if (drinks == null || !drinks.Any())
            {
                _logger.LogWarning("No drinks found in database");
                throw new NoDrinksFoundException("There are no drinks in data base");
            }

            _logger.LogInformation("Successfully retrieved {DrinkCount} drinks", drinks.Count);
            return new PaginatedResponse<Drink> { Data = drinks, Total = total, Limit = limit, Page = page };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all drinks");
            throw;
        }
    }

    public async Task AddDrinkAsync(Drink drink, int id_role)
    {
        _logger.LogInformation("Adding new drink");

        try
        {
            if (drink == null)
            {
                _logger.LogError("Attempt to add null drink");
                throw new ArgumentNullException(nameof(drink));
            }

            if (string.IsNullOrWhiteSpace(drink.Name))
            {
                _logger.LogError("Attempt to add drink with empty name");
                throw new DrinkIncorrectAtributeException("Drink's name cannot be empty");
            }

            await _drinkRepository.AddAsync(drink, id_role);
            _logger.LogInformation("Successfully added drink: {DrinkName} (ID: {DrinkId})",
                drink.Name, drink.Id_drink);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding drink");
            throw;
        }
    }

    public async Task DeleteDrinkAsync(Guid drink_id, int id_role)
    {
        _logger.LogInformation("Deleting drink: {DrinkId}", drink_id);

        try
        {
            var drink = await _drinkRepository.GetDrinkByIdAsync(drink_id, id_role);
            if (drink == null)
            {
                _logger.LogWarning("Drink not found for deletion: {DrinkId}", drink_id);
                throw new DrinkNotFoundException($"Drink with id={drink_id} was not found");
            }

            await _drinkRepository.RemoveAsync(drink_id, id_role);
            _logger.LogInformation("Successfully deleted drink: {DrinkName} (ID: {DrinkId})",
                drink.Name, drink_id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting drink: {DrinkId}", drink_id);
            throw;
        }
    }

    //public async Task<List<Category>?> GetCategoryByDrinkIdAsync(Guid drink_id, int id_role)
    //{
    //    _logger.LogInformation("Getting categories for drink: {DrinkId}",
    //        drink_id, id_role);

    //    try
    //    {
    //        var drink = await _drinkRepository.GetDrinkByIdAsync(drink_id, id_role);
    //        if (drink == null)
    //        {
    //            _logger.LogWarning("Drink not found when getting categories: {DrinkId}", drink_id);
    //            throw new DrinkNotFoundException($"Drink with id={drink_id} was not found");
    //        }

    //        var categories = await _drinkscategoryRepository.GetAllCategoriesByDrinkIdAsync(drink_id, id_role);

    //        _logger.LogInformation("Found {CategoryCount} categories for drink: {DrinkId}",
    //            categories?.Count ?? 0, drink_id);

    //        return categories;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error getting categories for drink: {DrinkId}", drink_id);
    //        throw;
    //    }
    //}
}