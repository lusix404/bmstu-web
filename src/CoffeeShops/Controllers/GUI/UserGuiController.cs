using Microsoft.AspNetCore.Mvc;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using CoffeeShops.ViewModels;
using Azure;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.DTOs.User;
using Serilog;
using System.Collections.Generic;

namespace CoffeeShops.Controllers.GUI;


[Route("api/v2/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class UserGuiController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<UserGuiController> _logger;
    public UserGuiController(IUserService userService, ILogger<UserGuiController> logger)
    {
        _userService = userService;
        _logger = logger;
    }


    [HttpGet("login")]
    public IActionResult Login()
    {
        return View();
    }


    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is invalid");
            LogModelStateErrors();
            return View(model);
        }

        try
        {
            var result = await _userService.Login(model.Login, model.Password);

            Response.Cookies.Append("access_token", result.Token, new CookieOptions
            {
                HttpOnly = false, // Чтобы JavaScript мог читать если нужно
                Secure = false,   // Для localhost, в production=true
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddDays(7)
            });
            //await HttpContext.SignInAsync(
            //    CookieAuthenticationDefaults.AuthenticationScheme,
            //    new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));

            return RedirectToAction("GetAllCompanies", "CompanyGui");
        }
        catch (UserLoginNotFoundException e)
        {
            ModelState.AddModelError("Login", "Пользователь с таким логином не найден"); // Привязываем к полю Login
            _logger.LogError(e, e.Message);
        }
        catch (UserWrongPasswordException e)
        {
            ModelState.AddModelError("Password", "Неверный пароль"); // Привязываем к полю Password
            _logger.LogError(e, e.Message);
        }
        catch (InvalidDataException e)
        {
            ModelState.AddModelError("", "Введены некорректные данные"); // Общая ошибка
            _logger.LogError(e, e.Message);
        }
        catch (Exception e)
        {
            ModelState.AddModelError("", "Произошла ошибка при входе в систему");
            _logger.LogError(e, e.Message);
        }

        return View(model);
    }


    [HttpGet("register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is invalid");
            LogModelStateErrors();
            return View(model);
        }

        try
        {
            var newUser = new User(Domain.Models.Enums.UserRole.Ordinary_user, model.Login, model.Password, model.BirthDate, model.Email);
            await _userService.Registrate(newUser);

            //var user = await _userService.GetUserByLoginAsync(newUser.Login, 1);

            //var claims = new List<Claim> {
            //new Claim("Id", user.Id_user.ToString()),
            //new Claim(ClaimTypes.Name, user.Login),
            //new Claim("Id_role", user.Id_role.ToString())

            return RedirectToAction("GetAllCompanies", "CompanyGui");
        }

        //await HttpContext.SignInAsync(
        //    CookieAuthenticationDefaults.AuthenticationScheme,
        //    new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));

        //return RedirectToAction("GetAllCompanies", "Company");
        //}
        catch (UserLoginAlreadyExistsException e)
        {
            ModelState.AddModelError("Login", "Пользователь с таким логином уже существует"); // Привязываем к полю Login
            _logger.LogError(e, e.Message);
        }
        catch (UserIncorrectAtributeException e)
        {
            // Можно уточнить, к какому полю относится ошибка
            ModelState.AddModelError("", "В поле введены некорректный данные");
            _logger.LogError(e, e.Message);
        }
        catch (InvalidDataException e)
        {
            ModelState.AddModelError("", "Введены некорректные данные");
            _logger.LogError(e, e.Message);
        }
        catch (Exception e)
        {
            ModelState.AddModelError("", "Произошла ошибка при регистрации");
            _logger.LogError(e, e.Message);
        }

        return View(model);
    }

    private void LogModelStateErrors()
    {
        foreach (var key in ModelState.Keys)
        {
            var entry = ModelState[key];
            foreach (var error in entry.Errors)
            {
                _logger.LogWarning($"Field: {key}, Error: {error.ErrorMessage}");
            }
        }
    }


    //public async Task<IActionResult> Logout()
    //{
    //    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    //    return RedirectToAction("Login", "User");
    //}
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var model = new UsersListViewModel();
        try
        {
            Guid cur_user_id = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid id)
    ? id
    : Guid.Empty;
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty);

            int page = 1;
            int limit = 100;

            var filters = new UserFilters();

            var result = await _userService.GetAllUsersAsync(filters, page, limit, cur_id_role);
            model.Users = result.Data;
            model.Users.RemoveAll(u => u.Id_user == cur_user_id);
            model.Id_role = cur_id_role;
            return View(model);
        }

        catch (InvalidCastException e)
        {
            ModelState.AddModelError("", "При получении пользователей произошла ошибка");
            _logger.LogError(e, e.Message);
            return View();
        }
        catch (Exception e)
        {
            ModelState.AddModelError("", "При получении пользователей произошла ошибка");
            _logger.LogError(e, e.Message);
            return View();
        }

        return RedirectToAction("Login", "User");
    }

    //public async Task<IActionResult> GiveModerRights(Guid id)
    //{
    //    var user = HttpContext.User;
    //    var roleString = user.FindFirst("Id_role");
    //    if (user.Identity is not null && user.Identity.IsAuthenticated)
    //    {
    //        try
    //        {
    //            int id_role = int.Parse(roleString.Value);

    //            await _userService.GrantModerRightsAsync(id, id_role);
    //            return RedirectToAction("GetAllUsers", "User");
    //        }
    //        catch (UserNotFoundException e)
    //        {
    //            ModelState.AddModelError("", "Пользователь для выдачи прав не найден");
    //            _logger.LogError(e, e.Message);
    //        }
    //        catch (Exception e)
    //        {
    //            ModelState.AddModelError("", "Произошла ошибка при попытке выдать права модератора");
    //            _logger.LogError(e, e.Message);
    //        }
    //    }
    //    else
    //    {
    //        ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
    //        _logger.LogError("", "Пользователь не прошел проверку подлинности");
    //    }

    //    return RedirectToAction("Login", "User");

    //}


    //public async Task<IActionResult> TakeModerRights(Guid id)
    //{
    //    var user = HttpContext.User;
    //    var roleString = user.FindFirst("Id_role");
    //    if (user.Identity is not null && user.Identity.IsAuthenticated)
    //    {
    //        try
    //        {
    //            int id_role = int.Parse(roleString.Value);
    //            await _userService.RevokeModerRightsAsync(id, id_role);
    //            return RedirectToAction("GetAllUsers", "User");
    //        }
    //        catch (UserNotFoundException e)
    //        {
    //            ModelState.AddModelError("", "Пользователь для возврата прав не найден");
    //            _logger.LogError(e, e.Message);
    //        }
    //        catch (Exception e)
    //        {
    //            ModelState.AddModelError("", "Произошла ошибка при попытке забрать права модератора");
    //            _logger.LogError(e, e.Message);
    //        }
    //    }
    //    else
    //    {
    //        ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
    //        _logger.LogError("", "Пользователь не прошел проверку подлинности");
    //    }

    //    return RedirectToAction("Login", "User");

    //}
}
