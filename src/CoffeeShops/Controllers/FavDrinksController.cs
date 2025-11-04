//using Microsoft.AspNetCore.Mvc;
//using CoffeeShops.Domain.Models;
//using CoffeeShops.Services.Interfaces.Services;
//using Swashbuckle.AspNetCore.Annotations;
//using System.ComponentModel.DataAnnotations;
//using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
//using CoffeeShops.Domain.Exceptions.UserServiceExceptions;

//namespace CoffeeShops.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Produces("application/json")]
//    public class FavoriteDrinksController : ControllerBase
//    {
//        private readonly IFavDrinksService _favDrinksService;

//        public FavoriteDrinksController(IFavDrinksService favDrinksService)
//        {
//            _favDrinksService = favDrinksService;
//        }

//        [HttpPost("{userId:guid}/drinks/{drinkId:guid}")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
//        [SwaggerOperation(
//            Summary = "Add drink to favorites",
//            Description = "Adds specified drink to user's favorite drinks list",
//            OperationId = "AddFavoriteDrink")]
//        public async Task<IActionResult> AddToFavorites(
//            [FromRoute]
//            [SwaggerParameter(Description = "User's unique identifier", Required = true)]
//            Guid userId,
//            [FromRoute]
//            [SwaggerParameter(Description = "Drink's unique identifier", Required = true)]
//            Guid drinkId)
//        {
//            try
//            {
//                await _favDrinksService.AddDrinkToFavsAsync(userId, drinkId);
//                return Ok();
//            }
//            catch (UserNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (DrinkNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (DrinkAlreadyIsFavoriteException ex)
//            {
//                return Conflict(ex.Message);
//            }
//            catch (Exception)
//            {
//                return StatusCode(500, "Internal server error");
//            }
//        }


//        [HttpDelete("{userId:guid}/drinks/{drinkId:guid}")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
//        [SwaggerOperation(
//            Summary = "Remove drink from favorites",
//            Description = "Removes specified drink from user's favorite drinks list",
//            OperationId = "RemoveFavoriteDrink")]
//        public async Task<IActionResult> RemoveFromFavorites(
//            [FromRoute]
//            [SwaggerParameter(Description = "User's unique identifier", Required = true)]
//            Guid userId,
//            [FromRoute]
//            [SwaggerParameter(Description = "Drink's unique identifier", Required = true)]
//            Guid drinkId)
//        {
//            try
//            {
//                await _favDrinksService.RemoveDrinkFromFavsAsync(userId, drinkId);
//                return Ok();
//            }
//            catch (UserNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (DrinkNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (DrinkIsNotFavoriteException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (Exception)
//            {
//                return StatusCode(500, "Internal server error");
//            }
//        }

        
//        [HttpGet("{userId:guid}")]
//        [ProducesResponseType(typeof(List<FavDrinks>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
//        [SwaggerOperation(
//            Summary = "Get favorite drinks",
//            Description = "Retrieves all favorite drinks for specified user",
//            OperationId = "GetFavoriteDrinks")]
//        public async Task<IActionResult> GetFavoriteDrinks(
//            [FromRoute]
//            [SwaggerParameter(Description = "User's unique identifier", Required = true)]
//            Guid userId)
//        {
//            try
//            {
//                var favoriteDrinks = await _favDrinksService.GetFavDrinksAsync(userId);
//                return Ok(favoriteDrinks);
//            }
//            catch (UserNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (NoDrinksFoundException ex)
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