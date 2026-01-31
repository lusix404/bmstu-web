using Microsoft.AspNetCore.Mvc;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces;
using CoffeeShops.Services.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.DTOs.Auth;
using CoffeeShops.DTOs.Utils;
using CoffeeShops.Domain.Converters;
using System.Net;
using CoffeeShops.DataAccess.Models;


namespace CoffeeShops.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : Controller
{
    private readonly IUserService _userService;
    public AuthController(IUserService userService)
    {
        _userService = userService;
    }


    [HttpPost("login")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
        Summary = "Аутентификация пользователя",
        Description = "Аутентифицирует пользователя и возвращает JWT-токен"
    )]
    public async Task<IActionResult> Login([FromBody] LoginRequest credentials)
    {
        if (!ModelState.IsValid)
        {
            var validationErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToArray();

            return BadRequest(new Error(
                message: "Некорректные данные",
                code: 400,
                err: new Exception($"Validation errors: {string.Join(", ", validationErrors)}")
            ));
        }

        try
        {
            var result = await _userService.Login(credentials.Login, credentials.Password);

            return Ok(result);
        }
        catch (UserLoginNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Пользователь с таким логином не найден",
                 code: 404,
                 err_details: ex.Message
             ));
        }
        catch (UserWrongPasswordException ex)
        {
            return BadRequest(new Error(
                 message: "Переданы некорректный данные",
                 code: 400,
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


    [HttpPost("register")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateResponse), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 409)]
    [SwaggerOperation(
        Summary = "Регистрация нового пользователя",
        Description = "Регистрирует нового пользователя в системе"
    )]
    public async Task<IActionResult> Register([FromBody] RegisterRequest userRegister)
    {
        try
        {
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
            User userdomain = RegisterRequestConverter.ConvertToDomainModel(userRegister);
            var userId = await _userService.Registrate(userdomain);

            return StatusCode(201, new CreateResponse { Id = userId.ToString() });
        }
        catch (UserLoginAlreadyExistsException ex)
        {
            return Conflict(new Error(
                 message: "Пользователь с таким логином уже существует",
                 code: 409,
                 err_details: ex.Message
             ));
        }
        catch (UserIncorrectAtributeException ex)
        {
            return BadRequest(new Error(
                 message: "Переданы некорректные данные",
                 code: 400,
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
