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

namespace CoffeeShops.Controllers;

public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }


    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }


    [HttpPost]
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
            var user = await _userService.Login(model.Login, model.Password);

            var claims = new List<Claim> {
            new Claim("Id", user.Id_user.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim("Id_role", user.Id_role.ToString())
        };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));

            return RedirectToAction("GetAllCompanies", "Company");
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


    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
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
            var newUser = new User(1, model.Login, model.Password, model.BirthDate, model.Email);
            await _userService.Registrate(newUser);

            var user = await _userService.GetUserByLoginAsync(newUser.Login, 1);

            var claims = new List<Claim> {
            new Claim("Id", user.Id_user.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim("Id_role", user.Id_role.ToString())
        };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));

            return RedirectToAction("GetAllCompanies", "Company");
        }
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


    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "User");
    }

    public async Task<IActionResult> GetAllUsers()
    {
        var model = new UsersListViewModel();

        var user = HttpContext.User;
        var id = User.FindFirst("Id");
        var roleString = user.FindFirst("Id_role");
        if (user.Identity is not null && user.Identity.IsAuthenticated)
        {
            try
            {
                int id_role = int.Parse(roleString.Value);
                //var id_user = Int32.Parse(id.Value);
                var id_user = Guid.Parse(id.Value);
                model.Users = await _userService.GetAllUsersAsync(id_role);
                model.Users.RemoveAll(u => u.Id_user == id_user);
                model.Id_role = id_role;
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
        }
        else
        {
            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
            _logger.LogError("", "Пользователь не прошел проверку подлинности");
        }

        return RedirectToAction("Login", "User");
    }

    public async Task<IActionResult> GiveModerRights(Guid id)
    {
        var user = HttpContext.User;
        var roleString = user.FindFirst("Id_role");
        if (user.Identity is not null && user.Identity.IsAuthenticated)
        {
            try
            {
                int id_role = int.Parse(roleString.Value);

                await _userService.GrantModerRightsAsync(id, id_role);
                return RedirectToAction("GetAllUsers", "User");
            }
            catch (UserNotFoundException e)
            {
                ModelState.AddModelError("", "Пользователь для выдачи прав не найден");
                _logger.LogError(e, e.Message);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Произошла ошибка при попытке выдать права модератора");
                _logger.LogError(e, e.Message);
            }
        }
        else
        {
            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
            _logger.LogError("", "Пользователь не прошел проверку подлинности");
        }

        return RedirectToAction("Login", "User");

    }


    public async Task<IActionResult> TakeModerRights(Guid id)
    {
        var user = HttpContext.User;
        var roleString = user.FindFirst("Id_role");
        if (user.Identity is not null && user.Identity.IsAuthenticated)
        {
            try
            {
                int id_role = int.Parse(roleString.Value);
                await _userService.RevokeModerRightsAsync(id, id_role);
                return RedirectToAction("GetAllUsers", "User");
            }
            catch (UserNotFoundException e)
            {
                ModelState.AddModelError("", "Пользователь для возврата прав не найден");
                _logger.LogError(e, e.Message);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Произошла ошибка при попытке забрать права модератора");
                _logger.LogError(e, e.Message);
            }
        }
        else
        {
            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
            _logger.LogError("", "Пользователь не прошел проверку подлинности");
        }

        return RedirectToAction("Login", "User");

    }
}


