//using Microsoft.AspNetCore.Mvc;
//using CoffeeShops.Domain.Models;
//using CoffeeShops.Services.Interfaces.Services;
//using Swashbuckle.AspNetCore.Annotations;
//using System.ComponentModel.DataAnnotations;
//using CoffeeShops.Domain.Exceptions.CoffeeShopServiceExceptions;
//using CoffeeShops.Domain.Exceptions.CoffeeShopServiceExceptions;
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


using CoffeeShops.Controllers;
using CoffeeShops.Domain.Converters;
using CoffeeShops.Domain.Exceptions.CoffeeShopServiceExceptions;
using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.DTOs.FavCoffeeShops;
using CoffeeShops.DTOs.CoffeeShop;
using CoffeeShops.DTOs.Pagination;
using CoffeeShops.DTOs.Utils;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using CoffeeShops.DTOs.FavCoffeeShops;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;

[ApiController]
[Route("favcoffeeshops")]
[Authorize]
public class FavCoffeeShopsController : BaseController
{
    private readonly IFavCoffeeShopsService _favcoffeeshopsService;
    private readonly ILogger<FavCoffeeShopsController> _logger;

    public FavCoffeeShopsController(IFavCoffeeShopsService favcoffeeshopsService, ILogger<FavCoffeeShopsController> logger)
    {
        _favcoffeeshopsService = favcoffeeshopsService;
        _logger = logger;
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateResponse), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [SwaggerOperation(
        Summary = "Добавить кофейню в избранное",
        Description = "Добавляет конкретную кофейню в список избранных кофеен текущего пользователя"
    )]
    public async Task<IActionResult> AddCoffeeShopToFavs([FromBody] AddFavCoffeeShops coffeeshopRequest)
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
            await _favcoffeeshopsService.AddCoffeeShopToFavsAsync(cur_user_id, coffeeshopRequest.Id_coffeeshop, cur_id_role);

            return StatusCode(201, $"Кофейня (id={coffeeshopRequest.Id_coffeeshop}) была успешно добавлена в список избранных для текущего пользователя(id={cur_user_id})");
        }
        catch (CoffeeShopNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Кофейня не найдена",
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
        catch (CoffeeShopAlreadyIsFavoriteException ex)
        {
            return Conflict(new Error(
                 message: "Кофейня уже содержится в списке избранных кофеен текущего пользователя",
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

    [HttpDelete("{coffeeshopId}")]
    [Authorize(Roles = "Administrator, Moderator")]
    [Produces("application/json")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
    Summary = "Удалить кофейню из избранного",
    Description = "Удаляет кофейню из списка избранных кофеен текущего пользователя"
)]
    public async Task<IActionResult> DeleteCoffeeShopFromFavs(
    [FromRoute] Guid coffeeshopId)
    {
        try
        {
            Guid cur_user_id = GetCurrentUserId();
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            await _favcoffeeshopsService.RemoveCoffeeShopFromFavsAsync(cur_user_id, coffeeshopId, cur_id_role);

            return Ok(new { Message = "Кофейня успешно удалена из списка избранных кофеен" });
        }
        catch (CoffeeShopNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Кофейня не найден",
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
        catch (CoffeeShopIsNotFavoriteException ex)
        {
            return Conflict(new Error(
                 message: "Кофейня не найден в списке избранных кофеен текущего пользователя",
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