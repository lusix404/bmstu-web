//using Microsoft.AspNetCore.Mvc;
//using CoffeeShops.Domain.Models;
//using CoffeeShops.Services.Interfaces.Services;
//using Swashbuckle.AspNetCore.Annotations;
//using System.ComponentModel.DataAnnotations;
//using CoffeeShops.Domain.Exceptions.CoffeeShopServiceExceptions;
//using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
//using CoffeeShops.Domain.Exceptions.UserServiceExceptions;

//namespace CoffeeShops.Controllers
//{
//    [ApiController]
//    [Route("api/favorite-coffee-shops")]
//    [Produces("application/json")]
//    public class FavoriteCoffeeShopsController : ControllerBase
//    {
//        private readonly IFavCoffeeShopsService _favCoffeeShopsService;

//        public FavoriteCoffeeShopsController(IFavCoffeeShopsService favCoffeeShopsService)
//        {
//            _favCoffeeShopsService = favCoffeeShopsService;
//        }

//        [HttpPost("{userId:guid}/coffee-shops/{coffeeShopId:guid}")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
//        [SwaggerOperation(
//            Summary = "Add coffee shop to favorites",
//            Description = "Adds specified coffee shop to user's favorite coffee shops list",
//            OperationId = "AddFavoriteCoffeeShop")]
//        public async Task<IActionResult> AddToFavorites(
//            [FromRoute]
//            [SwaggerParameter(Description = "User's unique identifier", Required = true)]
//            Guid userId,
//            [FromRoute]
//            [SwaggerParameter(Description = "Coffee shop's unique identifier", Required = true)]
//            Guid coffeeShopId)
//        {
//            try
//            {
//                await _favCoffeeShopsService.AddCoffeeShopToFavsAsync(userId, coffeeShopId);
//                return Ok();
//            }
//            catch (UserNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (CoffeeShopNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (CoffeeShopAlreadyIsFavoriteException ex)
//            {
//                return Conflict(ex.Message);
//            }
//            catch (Exception)
//            {
//                return StatusCode(500, "Internal server error");
//            }
//        }


//        [HttpDelete("{userId:guid}/coffee-shops/{coffeeShopId:guid}")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
//        [SwaggerOperation(
//            Summary = "Remove coffee shop from favorites",
//            Description = "Removes specified coffee shop from user's favorite coffee shops list",
//            OperationId = "RemoveFavoriteCoffeeShop")]
//        public async Task<IActionResult> RemoveFromFavorites(
//            [FromRoute]
//            [SwaggerParameter(Description = "User's unique identifier", Required = true)]
//            Guid userId,
//            [FromRoute]
//            [SwaggerParameter(Description = "Coffee shop's unique identifier", Required = true)]
//            Guid coffeeShopId)
//        {
//            try
//            {
//                await _favCoffeeShopsService.RemoveCoffeeShopFromFavsAsync(userId, coffeeShopId);
//                return Ok();
//            }
//            catch (UserNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (CoffeeShopNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (CoffeeShopIsNotFavoriteException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (Exception)
//            {
//                return StatusCode(500, "Internal server error");
//            }
//        }

       
//        [HttpGet("{userId:guid}")]
//        [ProducesResponseType(typeof(List<FavCoffeeShops>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
//        [SwaggerOperation(
//            Summary = "Get favorite coffee shops",
//            Description = "Retrieves all favorite coffee shops for specified user",
//            OperationId = "GetFavoriteCoffeeShops")]
//        public async Task<IActionResult> GetFavoriteCoffeeShops(
//            [FromRoute]
//            [SwaggerParameter(Description = "User's unique identifier", Required = true)]
//            Guid userId)
//        {
//            try
//            {
//                var favoriteCoffeeShops = await _favCoffeeShopsService.GetFavCoffeeShopsAsync(userId);
//                return Ok(favoriteCoffeeShops);
//            }
//            catch (UserNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (NoCoffeeShopsFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (Exception)
//            {
//                return StatusCode(500, "Internal server error");
//            }
//        }
//    }
//}