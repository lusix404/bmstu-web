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



using CoffeeShops.Controllers;
using CoffeeShops.Domain.Converters;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.DTOs.FavDrinks;
using CoffeeShops.DTOs.Drink;
using CoffeeShops.DTOs.Pagination;
using CoffeeShops.DTOs.Utils;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using CoffeeShops.DTOs.FavDrinks;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;

[ApiController]
[Route("api/v1/favdrinks")]
[Authorize]
public class FavDrinksController : BaseController
{
    private readonly IFavDrinksService _favdrinksService;
    private readonly ILogger<FavDrinksController> _logger;

    public FavDrinksController(IFavDrinksService favdrinksService, ILogger<FavDrinksController> logger)
    {
        _favdrinksService = favdrinksService;
        _logger = logger;
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateResponse), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [SwaggerOperation(
        Summary = "Добавить напиток в избранное",
        Description = "Добавляет конкретный напиток в список избранных напитков текущего пользователя"
    )]
    public async Task<IActionResult> AddDrinkToFavs([FromBody] AddFavDrinks drinkRequest)
    {
        try
        {
            Guid cur_user_id = GetCurrentUserId();
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new Error(
                    message: "Некорректные данные запроса",
                    code: 400,
                    err_details: string.Join("; ", errors)
                ));
            }
            //FavDrinks favsdomain = AddFavDrinksConverter.ConvertToDomainModel(drinkRequest);
            await _favdrinksService.AddDrinkToFavsAsync(cur_user_id, drinkRequest.Id_drink, cur_id_role);

            return StatusCode(201, $"Напиток (id={drinkRequest.Id_drink}) был успешно добавлен в список избранных для текущего пользователя(id={cur_user_id})");
        }
        catch (DrinkNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Напиток не найден",
                 code: 404,
                  err_details: ex.Message
             ));
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Пользователь не найден",
                 code: 404,
                  err_details: ex.Message
             ));
        }
        catch(DrinkAlreadyIsFavoriteException ex)
        {
            return Conflict(new Error(
                 message: "Напиток уже содержится в списке избранных напитков текущего пользователя",
                 code: 409,
                 err_details: ex.Message
             ));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Error(
                 message: "Внутренняя ошибка сервера",
                 code: 500,
                 err_details: ex.Message
             ));
        }
    }

    [HttpDelete("{drinkId}")]
    [Authorize(Roles = "Administrator, Moderator")]
    [Produces("application/json")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
    Summary = "Удалить напиток из избранного",
    Description = "Удаляет напиток из списка избранных напитков текущего пользователя"
)]
    public async Task<IActionResult> DeleteDrinkFromFavs(
    [FromRoute] Guid drinkId)
    {
        try
        {
            Guid cur_user_id = GetCurrentUserId();
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            await _favdrinksService.RemoveDrinkFromFavsAsync(cur_user_id, drinkId, cur_id_role);

            return Ok(new { Message = "Напиток успешно удален из списка избранных напитков" });
        }
        catch (DrinkNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Напиток не найден",
                 code: 404,
                  err_details: ex.Message
             ));
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Пользователь не найден",
                 code: 404,
                  err_details: ex.Message
             ));
        }
        catch (DrinkIsNotFavoriteException ex)
        {
            return Conflict(new Error(
                 message: "Напиток не найден в списке избранных напитков текущего пользователя",
                 code: 404,
                 err_details: ex.Message
             ));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Error(
                 message: "Внутренняя ошибка сервера",
                 code: 500,
                 err_details: ex.Message
             ));
        }
    }
}