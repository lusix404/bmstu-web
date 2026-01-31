using CoffeeShops.Domain.Exceptions.CoffeeShopServiceExceptions;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using Microsoft.Extensions.Logging;
using CoffeeShops.DTOs.Pagination;

namespace CoffeeShops.Services.Services
{
    public class FavCoffeeShopsService : IFavCoffeeShopsService
    {
        private readonly IFavCoffeeShopsRepository _favcoffeeshopsRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICoffeeShopRepository _coffeeshopRepository;
        private readonly ILogger<FavCoffeeShopsService> _logger;

        public FavCoffeeShopsService(
            IFavCoffeeShopsRepository favcoffeeshopsRepository,
            IUserRepository userRepository,
            ICoffeeShopRepository coffeeshopRepository,
            ILogger<FavCoffeeShopsService> logger)
        {
            _favcoffeeshopsRepository = favcoffeeshopsRepository;
            _userRepository = userRepository;
            _coffeeshopRepository = coffeeshopRepository;
            _logger = logger;
        }

        public async Task AddCoffeeShopToFavsAsync(Guid user_id, Guid coffeeshop_id, int id_role)
        {
            _logger.LogInformation("Adding coffee shop {CoffeeShopId} to favorites for user {UserId}",
                coffeeshop_id, user_id);

            try
            {
                var user = await _userRepository.GetUserByIdAsync(user_id, id_role);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", user_id);
                    throw new UserNotFoundException($"User with id={user_id} was not found");
                }

                var coffeeshop = await _coffeeshopRepository.GetCoffeeShopByIdAsync(coffeeshop_id, id_role);
                if (coffeeshop == null)
                {
                    _logger.LogWarning("Coffee shop not found: {CoffeeShopId}", coffeeshop_id);
                    throw new CoffeeShopNotFoundException($"Coffeeshop with id={coffeeshop_id} was not found");
                }

                if (user.FavoriteCoffeeShops.Any(f => f.Id_coffeeshop == coffeeshop_id))
                {
                    _logger.LogWarning("Coffee shop {CoffeeShopId} already in favorites for user {UserId}",
                        coffeeshop_id, user_id);
                    throw new CoffeeShopAlreadyIsFavoriteException(
                        $"Coffeeshop with id={coffeeshop_id} is already in user's (id={user_id}) list of favorite coffeeshops");
                }

                FavCoffeeShops added_coffeeshop = new FavCoffeeShops(user_id, coffeeshop_id);
                if (coffeeshop.CompanyName != null)
                {
                    added_coffeeshop.CoffeeShopName = coffeeshop.CompanyName;
                }
                user.FavoriteCoffeeShops.Add(added_coffeeshop);
                await _favcoffeeshopsRepository.AddAsync(added_coffeeshop, id_role);

                _logger.LogInformation("Successfully added coffee shop {CoffeeShopId} to favorites for user {UserId}",
                    coffeeshop_id, user_id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding coffee shop {CoffeeShopId} to favorites for user {UserId}",
                    coffeeshop_id, user_id);
                throw;
            }
        }

        public async Task RemoveCoffeeShopFromFavsAsync(Guid user_id, Guid coffeeshop_id, int id_role)
        {
            _logger.LogInformation("Removing coffee shop {CoffeeShopId} from favorites for user {UserId}",
                coffeeshop_id, user_id);

            try
            {
                var user = await _userRepository.GetUserByIdAsync(user_id, id_role);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", user_id);
                    throw new UserNotFoundException($"User with id={user_id} was not found");
                }

                var coffeeshop = await _coffeeshopRepository.GetCoffeeShopByIdAsync(coffeeshop_id, id_role);
                if (coffeeshop == null)
                {
                    _logger.LogWarning("Coffee shop not found: {CoffeeShopId}", coffeeshop_id);
                    throw new CoffeeShopNotFoundException($"Coffeeshop with id={coffeeshop_id} was not found");
                }
                var exists = _favcoffeeshopsRepository.GetRecordAsync(coffeeshop_id, user_id, id_role);
                if (exists == null)
                {
                    _logger.LogWarning($"Coffee shop with id={coffeeshop_id} was not found in user's (id={user_id}) list of favorite coffeeshops");
                    throw new CoffeeShopIsNotFavoriteException($"Coffee shop with id={coffeeshop_id} was not found in user's (id={user_id}) list of favorite coffeeshops");
                }

                await _favcoffeeshopsRepository.RemoveAsync(user_id, coffeeshop_id, id_role);

                _logger.LogInformation("Successfully removed coffee shop {CoffeeShopId} from favorites for user {UserId}",
                    coffeeshop_id, user_id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing coffee shop {CoffeeShopId} from favorites for user {UserId}",
                    coffeeshop_id, user_id);
                throw;
            }
        }

        //public async Task<List<FavCoffeeShops>?> GetFavCoffeeShopsAsync(Guid user_id, int id_role)
        //{
        //    _logger.LogInformation("Getting favorite coffee shops for user {UserId}", user_id);

        //    try
        //    {
        //        var user = await _userRepository.GetUserByIdAsync(user_id, id_role);
        //        if (user == null)
        //        {
        //            _logger.LogWarning("User not found: {UserId}", user_id);
        //            throw new UserNotFoundException($"User with id={user_id} was not found");
        //        }

        //        var favcoffeeshops = await _favcoffeeshopsRepository.GetAllFavCoffeeShopsAsync(user_id, id_role);
        //        if (favcoffeeshops == null || !favcoffeeshops.Any())
        //        {
        //            _logger.LogWarning("No favorite coffee shops found for user {UserId}", user_id);
        //            throw new NoCoffeeShopsFoundException(
        //                $"There is no coffeeshops in user's (id={user_id}) list of favorite coffeeshops");
        //        }

        //        _logger.LogInformation("Found {Count} favorite coffee shops for user {UserId}",
        //            favcoffeeshops.Count, user_id);

        //        return favcoffeeshops;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting favorite coffee shops for user {UserId}", user_id);
        //        throw;
        //    }
        //}
        public async Task<PaginatedResponse<CoffeeShop>>? GetFavCoffeeShopsAsync(Guid user_id, int page, int limit, int id_role)
        {
            _logger.LogInformation("Getting favorite coffee shops for user {UserId}", user_id);

            try
            {
                var user = await _userRepository.GetUserByIdAsync(user_id, id_role);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", user_id);
                    throw new UserNotFoundException($"User with id={user_id} was not found");
                }

                (var favcoffeeshops, int total) = await _favcoffeeshopsRepository.GetAllFavCoffeeShopsAsync(user_id, page, limit, id_role);
                if (favcoffeeshops == null || !favcoffeeshops.Any())
                {
                    _logger.LogWarning("No favorite coffee shops found for user {UserId}", user_id);
                    throw new NoCoffeeShopsFoundException(
                        $"There is no coffeeshops in user's (id={user_id}) list of favorite coffeeshops");
                }

                _logger.LogInformation("Found {Count} favorite coffee shops for user {UserId}",
                    favcoffeeshops.Count, user_id);

                return new PaginatedResponse<CoffeeShop> { Data = favcoffeeshops, Total = total, Limit = limit, Page = page };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite coffee shops for user {UserId}", user_id);
                throw;
            }
        }
    }
}