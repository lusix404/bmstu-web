//using Microsoft.AspNetCore.Mvc;
//using CoffeeShops.Domain.Models;
//using CoffeeShops.Services.Interfaces.Services;
//using Swashbuckle.AspNetCore.Annotations;
//using System.ComponentModel.DataAnnotations;
//using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication;
//using System.Security.Claims;
//using CoffeeShops.ViewModels;

//namespace CoffeeShops.Controllers;

//public class UserController : Controller
//{
//    private readonly IUserService _userService;
//    private readonly ILogger<UserController> _logger;
//    public UserController(IUserService userService, ILogger<UserController> logger)
//    {
//        _userService = userService;
//        _logger = logger;
//    }


//    [HttpGet]
//    public IActionResult Login()
//    {
//        return View();
//    }


//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> Login(LoginUserViewModel model)
//    {
//        if (!ModelState.IsValid)
//        {
//            _logger.LogWarning("ModelState is invalid");
//            LogModelStateErrors();
//            return View(model);
//        }

//        try
//        {
//            var user = await _userService.Login(model.Login, model.Password);

//            var claims = new List<Claim> {
//            new Claim("Id", user.Id_user.ToString()),
//            new Claim(ClaimTypes.Name, user.Login),
//            new Claim("Id_role", user.Id_role.ToString())
//        };

//            await HttpContext.SignInAsync(
//                CookieAuthenticationDefaults.AuthenticationScheme,
//                new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));

//            return RedirectToAction("GetAllCompanies", "Company");
//        }
//        catch (UserLoginNotFoundException e)
//        {
//            ModelState.AddModelError("Login", "Пользователь с таким логином не найден"); // Привязываем к полю Login
//            _logger.LogError(e, e.Message);
//        }
//        catch (UserWrongPasswordException e)
//        {
//            ModelState.AddModelError("Password", "Неверный пароль"); // Привязываем к полю Password
//            _logger.LogError(e, e.Message);
//        }
//        catch (InvalidDataException e)
//        {
//            ModelState.AddModelError("", "Введены некорректные данные"); // Общая ошибка
//            _logger.LogError(e, e.Message);
//        }
//        catch (Exception e)
//        {
//            ModelState.AddModelError("", "Произошла ошибка при входе в систему");
//            _logger.LogError(e, e.Message);
//        }

//        return View(model);
//    }


//    [HttpGet]
//    public IActionResult Register()
//    {
//        return View();
//    }

//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> Register(RegisterUserViewModel model)
//    {
//        if (!ModelState.IsValid)
//        {
//            _logger.LogWarning("ModelState is invalid");
//            LogModelStateErrors();
//            return View(model);
//        }

//        try
//        {
//            var newUser = new User(1, model.Login, model.Password, model.BirthDate, model.Email);
//            await _userService.Registrate(newUser);

//            var user = await _userService.GetUserByLoginAsync(newUser.Login, 1);

//            var claims = new List<Claim> {
//            new Claim("Id", user.Id_user.ToString()),
//            new Claim(ClaimTypes.Name, user.Login),
//            new Claim("Id_role", user.Id_role.ToString())
//        };

//            await HttpContext.SignInAsync(
//                CookieAuthenticationDefaults.AuthenticationScheme,
//                new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));

//            return RedirectToAction("GetAllCompanies", "Company");
//        }
//        catch (UserLoginAlreadyExistsException e)
//        {
//            ModelState.AddModelError("Login", "Пользователь с таким логином уже существует"); // Привязываем к полю Login
//            _logger.LogError(e, e.Message);
//        }
//        catch (UserIncorrectAtributeException e)
//        {
//            // Можно уточнить, к какому полю относится ошибка
//            ModelState.AddModelError("", "В поле введены некорректный данные");
//            _logger.LogError(e, e.Message);
//        }
//        catch (InvalidDataException e)
//        {
//            ModelState.AddModelError("", "Введены некорректные данные");
//            _logger.LogError(e, e.Message);
//        }
//        catch (Exception e)
//        {
//            ModelState.AddModelError("", "Произошла ошибка при регистрации");
//            _logger.LogError(e, e.Message);
//        }

//        return View(model);
//    }

//    private void LogModelStateErrors()
//    {
//        foreach (var key in ModelState.Keys)
//        {
//            var entry = ModelState[key];
//            foreach (var error in entry.Errors)
//            {
//                _logger.LogWarning($"Field: {key}, Error: {error.ErrorMessage}");
//            }
//        }
//    }


//    public async Task<IActionResult> Logout()
//    {
//        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
//        return RedirectToAction("Login", "User");
//    }

//    public async Task<IActionResult> GetAllUsers()
//    {
//        var model = new UsersListViewModel();

//        var user = HttpContext.User;
//        var id = User.FindFirst("Id");
//        var roleString = user.FindFirst("Id_role");
//        if (user.Identity is not null && user.Identity.IsAuthenticated)
//        {
//            try
//            {
//                int id_role = int.Parse(roleString.Value);
//                //var id_user = Int32.Parse(id.Value);
//                var id_user = Guid.Parse(id.Value);
//                model.Users = await _userService.GetAllUsersAsync(id_role);
//                model.Users.RemoveAll(u => u.Id_user == id_user);
//                model.Id_role = id_role;
//                return View(model);
//            }

//            catch (InvalidCastException e)
//            {
//                ModelState.AddModelError("", "При получении пользователей произошла ошибка");
//                _logger.LogError(e, e.Message);
//                return View();
//            }
//            catch (Exception e)
//            {
//                ModelState.AddModelError("", "При получении пользователей произошла ошибка");
//                _logger.LogError(e, e.Message);
//                return View();
//            }
//        }
//        else
//        {
//            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//            _logger.LogError("", "Пользователь не прошел проверку подлинности");
//        }

//        return RedirectToAction("Login", "User");
//    }

//    public async Task<IActionResult> GiveModerRights(Guid id)
//    {
//        var user = HttpContext.User;
//        var roleString = user.FindFirst("Id_role");
//        if (user.Identity is not null && user.Identity.IsAuthenticated)
//        {
//            try
//            {
//                int id_role = int.Parse(roleString.Value);

//                await _userService.GrantModerRightsAsync(id, id_role);
//                return RedirectToAction("GetAllUsers", "User");
//            }
//            catch (UserNotFoundException e)
//            {
//                ModelState.AddModelError("", "Пользователь для выдачи прав не найден");
//                _logger.LogError(e, e.Message);
//            }
//            catch (Exception e)
//            {
//                ModelState.AddModelError("", "Произошла ошибка при попытке выдать права модератора");
//                _logger.LogError(e, e.Message);
//            }
//        }
//        else
//        {
//            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//            _logger.LogError("", "Пользователь не прошел проверку подлинности");
//        }

//        return RedirectToAction("Login", "User");

//    }


//    public async Task<IActionResult> TakeModerRights(Guid id)
//    {
//        var user = HttpContext.User;
//        var roleString = user.FindFirst("Id_role");
//        if (user.Identity is not null && user.Identity.IsAuthenticated)
//        {
//            try
//            {
//                int id_role = int.Parse(roleString.Value);
//                await _userService.RevokeModerRightsAsync(id, id_role);
//                return RedirectToAction("GetAllUsers", "User");
//            }
//            catch (UserNotFoundException e)
//            {
//                ModelState.AddModelError("", "Пользователь для возврата прав не найден");
//                _logger.LogError(e, e.Message);
//            }
//            catch (Exception e)
//            {
//                ModelState.AddModelError("", "Произошла ошибка при попытке забрать права модератора");
//                _logger.LogError(e, e.Message);
//            }
//        }
//        else
//        {
//            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//            _logger.LogError("", "Пользователь не прошел проверку подлинности");
//        }

//        return RedirectToAction("Login", "User");

//    }
//}


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
using CoffeeShops.DTOs.Pagination;
using CoffeeShops.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using CoffeeShops.Domain.Models.Enums;
using Serilog;
using System.Security.Claims;


namespace CoffeeShops.Controllers;


[ApiController]
[Route("api/v1/users")]
[Authorize] 
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PaginatedResponse<UserResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
        Summary = "Получить список пользователей",
        Description = "Возвращает пагинированный список пользователей с фильтрацией (только для администраторов)"
    )]
    public async Task<IActionResult> GetAllUsers(
       [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? login = null,
        [FromQuery] int? user_role = null)
    {
        try
        {
            //// Отладка - что приходит в токене
            //_logger.LogInformation($"User authenticated: {User.Identity.IsAuthenticated}");
            //_logger.LogInformation($"User name: {User.Identity.Name}");

            //foreach (var claim in User.Claims)
            //{
            //    _logger.LogInformation($"Claim: {claim.Type} = {claim.Value}");
            //}


            Guid cur_user_id = GetCurrentUserId();
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());
            string cur_user_login = GetCurrentUserLogin();

            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            var filters = new UserFilters
            {
                Login = login,
                UserRole = user_role
            };

            var result = await _userService.GetAllUsersAsync(filters, page, limit, cur_id_role);

            //if (result.Data == null || !result.Data.Any())
            //{
            //    return NotFound(new PaginatedResponse<UserResponse>
            //    {
            //        Page = page,
            //        Limit = limit,
            //        TotalCount = 0,
            //        Data = new List<UserResponse>()
            //    });
            //}

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(401, new Error(
                 message: "Пользователь не авторизован",
                 code: 401,
                  err_details: ex.Message
             ));
        }
        catch (NoUsersFoundException ex)
        {
            return StatusCode(404, new Error(
                  message: "Пользователи не найдены",
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

    [HttpGet("id/{userId}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
     Summary = "Получить пользователя по ID",
     Description = "Возвращает информацию о пользователе по его идентификатору"
 )]
    public async Task<IActionResult> GetUserById(
     [FromRoute] Guid userId)
    {
        try
        {
            //Guid cur_user_id = GetCurrentUserId();
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());
            string cur_user_login = GetCurrentUserLogin();

            var user = await _userService.GetUserByIdAsync(userId, cur_id_role);

            return Ok(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(401, new Error(
                 message: "Пользователь не авторизован",
                 code: 401,
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
        catch (Exception ex)
        {
            return StatusCode(500, new Error(
                 message: "Внутренняя ошибка сервера",
                 code: 500,
                 err_details: ex.Message
             ));
        }
    }

    [HttpGet("login/{login}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
    Summary = "Получить пользователя по логину",
    Description = "Возвращает информацию о пользователе по его логину"
)]
    public async Task<IActionResult> GetUserByLogin(
    [FromRoute] string login)
    {
        try
        {
            Guid cur_user_id = GetCurrentUserId();
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());
            string cur_user_login = GetCurrentUserLogin();

            var user = await _userService.GetUserByLoginAsync(login, cur_id_role);

            return Ok(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(401, new Error(
                 message: "Пользователь не авторизован",
                 code: 401,
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
        catch (Exception ex)
        {
            return StatusCode(500, new Error(
                 message: "Внутренняя ошибка сервера",
                 code: 500,
                 err_details: ex.Message
             ));
        }
    }

    //[HttpPatch("{userId}")]
    //[Produces("application/json")]
    //[ProducesResponseType(typeof(UserResponse), 200)]
    //[ProducesResponseType(typeof(ErrorResponse), 400)]
    //[ProducesResponseType(typeof(ErrorResponse), 401)]
    //[ProducesResponseType(typeof(ErrorResponse), 403)]
    //[ProducesResponseType(typeof(ErrorResponse), 404)]
    //[ProducesResponseType(typeof(ErrorResponse), 500)]
    //[SwaggerOperation(
    //    Summary = "Обновить пользователя",
    //    Description = "Обновить информацию о пользователе"
    //)]
    //public async Task<IActionResult> UpdateUser(
    //    [FromRoute] Guid userId,
    //    [FromBody] UpdateUser updateRequest)
    //{
    //    try
    //    {
    //        //Guid cur_user_id = GetCurrentUserId();
    //        int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());
    //        string cur_user_login = GetCurrentUserLogin();

    //        var existing_user = await _userService.GetUserByIdAsync(userId, cur_id_role);
    //        User domainUser = new User(userId, existing_user.Id_role, updateRequest.Login, updateRequest.Password, existing_user.BirthDate, updateRequest.Email);
    //        domainUser.SetPassword(domainUser.PasswordHash);
    //        await _userService.UpdateUserAsync(domainUser, cur_id_role);

    //        return Ok(new { Message = "Пользователь успешно обновлен" });
    //    }
    //    catch (UserNotFoundException ex)
    //    {
    //        return NotFound(new Error(
    //             message: "Пользователь не найден",
    //             code: 404,
    //              err_details: ex.Message
    //         ));
    //    }
    //    catch (UnauthorizedAccessException ex)
    //    {
    //        return StatusCode(401, new Error(
    //             message: "Пользователь не авторизован",
    //             code: 401,
    //              err_details: ex.Message
    //         ));
    //    }
    //    catch (UserIncorrectAtributeException ex)
    //    {
    //        return BadRequest(new Error(
    //             message: "Переданы некорректные данные",
    //             code: 400,
    //             err_details: ex.Message
    //         ));
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new Error(
    //             message: "Внутренняя ошибка сервера",
    //             code: 500,
    //             err_details: ex.Message
    //         ));
    //    }
    //}
    [HttpPatch("me")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
        Summary = "Обновить данные профиля",
        Description = "Обновляет данные профиля (доступно только самому пользователю)"
    )]
    public async Task<IActionResult> EditProfile(
        [FromBody] UpdateUser updateRequest)
    {
        try
        {
            Guid cur_user_id = GetCurrentUserId();

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

            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());
            string cur_user_login = GetCurrentUserLogin();

            var cur_user = await _userService.GetUserByIdAsync(cur_user_id, cur_id_role);
            User domainUser = new User(cur_user_id, cur_user.Id_role, updateRequest.Login, updateRequest.Password, cur_user.BirthDate, updateRequest.Email);
            domainUser.SetPassword(domainUser.PasswordHash);
            await _userService.UpdateUserAsync(domainUser, cur_id_role);

            return Ok(new { Message = "Данные профиля успешно обновлены" });
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Пользователь не найден",
                 code: 404,
                  err_details: ex.Message
             ));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(401, new Error(
                 message: "Пользователь не авторизован",
                 code: 401,
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

    [HttpDelete("{userId}")]
    [Authorize(Roles = "Administrator")]
    [Produces("application/json")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
    Summary = "Удалить пользователя",
    Description = "Удаляет пользователя из системы (только для администраторов)"
)]
    public async Task<IActionResult> DeleteUser(
    [FromRoute] Guid userId)
    {
        try
        {
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            await _userService.DeleteUserAsync(userId, cur_id_role);

            return Ok(new { Message = "Пользователь успешно удален" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(401, new Error(
                 message: "Пользователь не авторизован",
                 code: 401,
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
        catch (UserLastAdminException ex)
        {
            return BadRequest(new Error(
                 message: "Нельзя удалить последнего пользователя-администратора",
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

    //    [HttpDelete]
    //    [Produces("application/json")]
    //    [ProducesResponseType(204)]
    //    [ProducesResponseType(typeof(ErrorResponse), 400)]
    //    [ProducesResponseType(typeof(ErrorResponse), 404)]
    //    [ProducesResponseType(typeof(ErrorResponse), 500)]
    //    [SwaggerOperation(
    //    Summary = "Удалить свой аккаунт",
    //    Description = "Удаляет аккаунт текущего пользователя"
    //)]
    //    public async Task<IActionResult> DeleteProfile()
    //    {
    //        try
    //        {
    //            Guid cur_user_id = GetCurrentUserId();
    //            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

    //            await _userService.DeleteUserAsync(cur_user_id, cur_id_role);

    //            return Ok(new { Message = "Профиль успешно удален" });
    //        }
    //        catch (UnauthorizedAccessException ex)
    //        {
    //            return StatusCode(401, new Error(
    //                 message: "Пользователь не авторизован",
    //                 code: 401,
    //                  err_details: ex.Message
    //             ));
    //        }
    //        catch (UserNotFoundException ex)
    //        {
    //            return NotFound(new Error(
    //                 message: "Пользователь не найден",
    //                 code: 404,
    //                  err_details: ex.Message
    //             ));
    //        }
    //        catch (UserLastAdminException ex)
    //        {
    //            return BadRequest(new Error(
    //                 message: "Нельзя удалить профиль последнего пользователя-администратора",
    //                 code: 400,
    //                 err_details: ex.Message
    //            ));
    //        }
    //        catch (Exception ex)
    //        {
    //            return StatusCode(500, new Error(
    //                 message: "Внутренняя ошибка сервера",
    //                 code: 500,
    //                 err_details: ex.Message
    //             ));
    //        }
    //    }

    [HttpPatch("{userId}/role")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
    Summary = "Обновить роль пользователя",
    Description = "Изменяет роль пользователя в системе (только для администратора)"
)]
    public async Task<IActionResult> UpdateUserRole(
        [FromRoute] Guid userId,
        [FromBody] UpdateUserRole request)
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

            int new_id_role = UserRoleExtensions.ToRoleIntFromString(request.User_role);
            if (new_id_role == 0 )
            {
                throw new RoleNotFoundException("Данная роль не определена в системе");
            }
            Guid cur_user_id = GetCurrentUserId();
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            if (userId == cur_user_id)
            {
                _logger.LogWarning("Пользователь {UserId} пытается изменить свою собственную роль", userId);
                return BadRequest(new Error(
                message: "Нельзя изменить собственную роль",
                code: 400
            ));
            }

            await _userService.UpdateUserRightsAsync(userId, new_id_role, cur_id_role);

            return Ok(new { Message = "Роль пользователя успешно обновлена" });
        }
        catch (RoleNotFoundException ex)
        {
            return BadRequest(new Error(
                message: "Переданы некорректные данные",
                code: 400,
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
        catch (UserLastAdminException ex)
        {
            return BadRequest(new Error(
                 message: "Нельзя изменить роль последнего администратора",
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



