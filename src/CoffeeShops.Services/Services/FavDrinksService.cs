using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using System.IO;
using System.Security.Principal;
using CoffeeShops.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using CoffeeShops.DTOs.Pagination;

namespace CoffeeShops.Services.Services;

public class FavDrinksService : IFavDrinksService
{
    private readonly IFavDrinksRepository _favdrinksRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDrinkRepository _drinkRepository;
    private readonly ILogger<FavDrinksService> _logger;

    public FavDrinksService(IFavDrinksRepository favdrinksRepository, IUserRepository userRepository, IDrinkRepository drinkRepository, ILogger<FavDrinksService> logger)
    {
        _favdrinksRepository = favdrinksRepository;
        _userRepository = userRepository;
        _drinkRepository = drinkRepository;
        _logger = logger;
    }

    public async Task AddDrinkToFavsAsync(Guid user_id, Guid drink_id, int id_role)
    {
        _logger.LogInformation($"Attempting to add drink with id={drink_id} to user with id={user_id} favorites.");

        var user = await _userRepository.GetUserByIdAsync(user_id, id_role);
        if (user == null)
        {
            _logger.LogWarning($"User with id={user_id} was not found.");
            throw new UserNotFoundException($"User with id={user_id} was not found");
        }

        var drink = await _drinkRepository.GetDrinkByIdAsync(drink_id, id_role);
        if (drink == null)
        {
            _logger.LogWarning($"Drink with id={drink_id} was not found.");
            throw new DrinkNotFoundException($"Drink with id={drink_id} was not found");
        }

        if (user.FavoriteDrinks.Any(f => f.Id_drink == drink_id))
        {
            _logger.LogWarning($"Drink with id={drink_id} is already in user's (id={user_id}) list of favorite drinks.");
            throw new DrinkAlreadyIsFavoriteException($"Drink with id={drink_id} is already in user's (id={user_id}) list of favorite drinks");
        }

        FavDrinks added_drink = new FavDrinks(user_id, drink_id);
        user.FavoriteDrinks.Add(added_drink);
        await _favdrinksRepository.AddAsync(added_drink, id_role);
        _logger.LogInformation($"Drink with id={drink_id} has been added to user with id={user_id} favorites.");
    }

    public async Task RemoveDrinkFromFavsAsync(Guid user_id, Guid drink_id, int id_role)
    {
        _logger.LogInformation($"Attempting to remove drink with id={drink_id} from user with id={user_id} favorites.");

        var user = await _userRepository.GetUserByIdAsync(user_id, id_role);
        if (user == null)
        {
            _logger.LogWarning($"User with id={user_id} was not found.");
            throw new UserNotFoundException($"User with id={user_id} was not found");
        }

        var drink = await _drinkRepository.GetDrinkByIdAsync(drink_id, id_role);
        if (drink == null)
        {
            _logger.LogWarning($"Drink with id={drink_id} was not found.");
            throw new DrinkNotFoundException($"Drink with id={drink_id} was not found");
        }

        // Проверка на наличие напитка в избранном
        var favorite = user.FavoriteDrinks.FirstOrDefault(f => f.Id_drink == drink_id);
        if (favorite == null)
        {
            _logger.LogWarning($"Drink with id={drink_id} was not found in user's (id={user_id}) list of favorite drinks.");
            throw new DrinkIsNotFavoriteException($"Drink with id={drink_id} was not found in user's (id={user_id}) list of favorite drinks");
        }

        user.FavoriteDrinks.Remove(favorite);
        await _favdrinksRepository.RemoveAsync(user_id, drink_id, id_role);
        _logger.LogInformation($"Drink with id={drink_id} has been removed from user with id={user_id} favorites.");
    }

    //public async Task<List<FavDrinks>?> GetFavDrinksAsync(Guid user_id, int id_role)
    //{
    //    _logger.LogInformation($"Retrieving favorite drinks for user with id={user_id}.");

    //    var user = await _userRepository.GetUserByIdAsync(user_id, id_role);
    //    if (user == null)
    //    {
    //        _logger.LogWarning($"User with id={user_id} was not found.");
    //        throw new UserNotFoundException($"User with id={user_id} was not found");
    //    }

    //    var favdrinks = await _favdrinksRepository.GetAllFavDrinksAsync(user_id, id_role);
    //    if (favdrinks == null || !favdrinks.Any())
    //    {
    //        _logger.LogWarning($"There are no drinks in user's (id={user_id}) list of favorite drinks.");
    //        throw new NoDrinksFoundException($"There is no drinks in user's (id={user_id}) list of favorite drinks");
    //    }

    //    _logger.LogInformation($"Retrieved {favdrinks.Count} favorite drinks for user with id={user_id}.");
    //    return favdrinks;
    //}
    public async Task<PaginatedResponse<Drink>>? GetFavDrinksAsync(Guid user_id, int page, int limit, int id_role)
    {
        _logger.LogInformation("Getting favorite drinks for user {UserId}", user_id);

        try
        {
            var user = await _userRepository.GetUserByIdAsync(user_id, id_role);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", user_id);
                throw new UserNotFoundException($"User with id={user_id} was not found");
            }

            (var favdrinks, int total) = await _favdrinksRepository.GetAllFavDrinksAsync(user_id, page, limit, id_role);
            if (favdrinks == null || !favdrinks.Any())
            {
                _logger.LogWarning("No favorite drinks found for user {UserId}", user_id);
                throw new NoCoffeeShopsFoundException(
                    $"There is no drinks in user's (id={user_id}) list of favorite drinks");
            }

            _logger.LogInformation("Found {Count} favorite drinks for user {UserId}",
                favdrinks.Count, user_id);

            return new PaginatedResponse<Drink> { Data = favdrinks, Total = total, Limit = limit, Page = page };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorite coffee shops for user {UserId}", user_id);
            throw;
        }
    }
}

