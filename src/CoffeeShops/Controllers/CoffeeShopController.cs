//using Microsoft.AspNetCore.Mvc;
//using CoffeeShops.Domain.Models;
//using CoffeeShops.Services.Interfaces.Services;
//using Swashbuckle.AspNetCore.Annotations;
//using System.ComponentModel.DataAnnotations;
//using CoffeeShops.Domain.Exceptions.CoffeeShopServiceExceptions;
//using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
//using Microsoft.Extensions.Logging;
//using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
//using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
//using CoffeeShops.Services.Services;
//using CoffeeShops.ViewModels;
//using System.Security.Claims;


//namespace CoffeeShops.Controllers;
//public class CoffeeShopController : Controller
//{

//    private readonly ICoffeeShopService _CoffeeShopService;
//    private readonly ICompanyService _CompanyService;
//    private readonly ILogger<CoffeeShopController> _logger;
//    private readonly IFavCoffeeShopsService _favcoffeeshopsService;
//    public CoffeeShopController(ICoffeeShopService CoffeeShopService, IFavCoffeeShopsService favcoffeeshopsService, ILogger<CoffeeShopController> logger, ICompanyService companyService)
//    {
//        _CoffeeShopService = CoffeeShopService;
//        _favcoffeeshopsService = favcoffeeshopsService;
//        _logger = logger;
//        _CompanyService = companyService;
//    }

//    public async Task<IActionResult> GetCoffeeShopsByCompany(Guid companyId)
//    {

//        var model = new CoffeeShopsListViewModel();
//        try
//        {
//            var user = HttpContext.User;
//            var roleString = user.FindFirst("Id_role");
//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                try
//                {
//                    int id_role = int.Parse(roleString.Value);
//                    model.CoffeeShops = await _CoffeeShopService.GetCoffeeShopsByCompanyIdAsync(companyId, id_role);
//                    model.Id_role = id_role;
//                    model.Id_company = companyId;
//                    return View(model);
//                }
//                catch (InvalidCastException e)
//                {
//                    ModelState.AddModelError("", "Произошла ошибка во время получении данных о кофейне");
//                    _logger.LogError(e, e.Message);
//                    return View();
//                }
//            }

//            try
//            {
//                model.CoffeeShops = await _CoffeeShopService.GetCoffeeShopsByCompanyIdAsync(companyId, 1);

//                return View(model);
//            }
//            catch (Exception e)
//            {
//                ModelState.AddModelError("", "Произошла ошибка во время получении данных о кофейне");
//                _logger.LogError(e, e.Message);

//                return View();
//            }
//        }
//        catch (Exception e)
//        {
//            ModelState.AddModelError("", "Произошла ошибка во время получении данных о кофейне");
//            _logger.LogError(e, e.Message);
//            return View();
//        }

//    }

//    [HttpGet]
//    public IActionResult AddCoffeeShopByCompany(Guid id_company)
//    {
//        AddCoffeeShopViewModel model = new AddCoffeeShopViewModel();
//        model.Id_company = id_company;
//        var user = HttpContext.User;
//        var roleString = user.FindFirst("Id_role");
//        int id_role = int.Parse(roleString.Value); // Преобразуем строку в int
//        model.Id_role = id_role;
//        return View(model);
//    }



//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> AddCoffeeShopByCompany(AddCoffeeShopViewModel model)
//    {
//        if (ModelState.IsValid)
//        {
//            try
//            {
//                var user = HttpContext.User;
//                var roleString = user.FindFirst("Id_role");


//                if (user.Identity is not null && user.Identity.IsAuthenticated && roleString is not null)
//                {

//                    int id_role = int.Parse(roleString.Value);
//                    var cs = new CoffeeShop(model.Id_company, model.Address, model.WorkingHours);
//                    await _CoffeeShopService.AddCoffeeShopAsync(cs, id_role);
//                    //return RedirectToAction("GetCoffeeShopsByCompany", "CoffeeShop");
//                    return RedirectToAction("GetCoffeeShopsByCompany", new
//                    {
//                        companyId = model.Id_company, // или другое значение
//                        message = "Coffee shop added successfully"
//                    });
//                }
//                else
//                {
//                    ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//                    _logger.LogError("", "Пользователь не прошел проверку подлинности");
//                }
//            }
//            catch (CoffeeShopIncorrectAtributeException e)
//            {
//                ModelState.AddModelError("", "Данные введены некорректно");
//                _logger.LogError(e, e.Message);
//                return View(model);
//            }
//            catch (InvalidDataException e)
//            {
//                ModelState.AddModelError("", "Данные введены некорректно");
//                _logger.LogError(e, e.Message);
//                return View(model);
//            }
//            catch (Exception e)
//            {
//                ModelState.AddModelError("", "Возникла ошибка при добавлении кофейни");
//                _logger.LogError(e, e.Message);
//                return View(model);
//            }
//        }

//        else
//        {

//            _logger.LogWarning("ModelState is invalid");
//            foreach (var error in ModelState)
//            {
//                _logger.LogWarning($"Field: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
//            }
//            return View(model);
//        }
//        return RedirectToAction("GetAllCompanies", "Company");
//    }


//    public async Task<IActionResult> DeleteCoffeeShop(Guid id_cs, Guid id_comp)
//    {
//        try
//        {
//            var user = HttpContext.User;
//            var roleString = user.FindFirst("Id_role");


//            if (user.Identity is not null && user.Identity.IsAuthenticated && roleString is not null)
//            {
//                int id_role = int.Parse(roleString.Value);
//                await _CoffeeShopService.DeleteCoffeeShopAsync(id_cs, id_role);
//                return RedirectToAction("GetCoffeeShopsByCompany", new
//                {
//                    companyId = id_comp,
//                    message = "Coffee shop deleted successfully"
//                });
//            }
//            else
//            {
//                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//                _logger.LogError("", "Пользователь не прошел проверку подлинности");
//            }

//        }
//        catch (InvalidDataException e)
//        {
//            _logger.LogInformation(e.Message);
//            ModelState.AddModelError("", "Произошла ошибка при удалении кофейни");
//        }
//        catch (Exception e)
//        {
//            _logger.LogInformation(e.Message);
//            ModelState.AddModelError("", "Произошла ошибка при удалении кофейни");
//        }
//        return RedirectToAction("GetCoffeeShopsByCompany", new
//        {
//            companyId = id_comp,
//            message = "Coffee shop deleted successfully"
//        });
//    }


//    public async Task<IActionResult> DeleteFromFavs(Guid coffeeshop_id)
//    {
//        try
//        {
//            var user = HttpContext.User;
//            var id = User.FindFirst("Id");
//            var roleString = user.FindFirst("Id_role");
//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                // var id_user = Int32.Parse(id.Value);
//                var id_user = Guid.Parse(id.Value);
//                int id_role = int.Parse(roleString.Value);
//                await _favcoffeeshopsService.RemoveCoffeeShopFromFavsAsync(id_user, coffeeshop_id, id_role);
//                return RedirectToAction("GetFavCoffeeShops", new
//                {
//                    message = "coffeeshop removed from favorites successfully"
//                });
//            }


//            else
//            {
//                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//                _logger.LogError("", "Пользователь не прошел проверку подлинности");
//            }
//        }
//        catch (UserNotFoundException e)
//        {
//            _logger.LogError(e, $"User not found while removing favorite coffeeshop {coffeeshop_id}");
//            ModelState.AddModelError("", "User not found");
//        }
//        catch (CoffeeShopNotFoundException e)
//        {
//            _logger.LogError(e, $"coffeeshop {coffeeshop_id} not found while removing from favorites");
//            ModelState.AddModelError("", "coffeeshop not found");
//        }
//        catch (CoffeeShopIsNotFavoriteException e)
//        {
//            _logger.LogError(e, $"coffeeshop {coffeeshop_id} was not in favorites");
//            ModelState.AddModelError("", "coffeeshop was not in favorites");
//        }
//        catch (Exception e)
//        {
//            _logger.LogError(e, $"Error removing coffeeshop {coffeeshop_id} from favorites");
//            ModelState.AddModelError("", "Error removing from favorites");
//        }

//        return RedirectToAction("GetFavcoffeeshops");
//    }

//    public async Task<IActionResult> AddToFavs(Guid coffeeshop_id, Guid page)
//    {
//        try
//        {
//            var user = HttpContext.User;
//            var id = User.FindFirst("Id");
//            var roleString = user.FindFirst("Id_role");
//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                //var id_user = Int32.Parse(id.Value);
//                var id_user = Guid.Parse(id.Value);
//                int id_role = int.Parse(roleString.Value);
//                await _favcoffeeshopsService.AddCoffeeShopToFavsAsync(id_user, coffeeshop_id, id_role);
//                //if (page == -1)
//                //{
//                //    return RedirectToAction("GetAllCoffeeshops");
//                //}
//                return RedirectToAction("GetCoffeeShopsByCompany", new
//                {
//                    companyId = page,
//                    message = ""
//                });


//            }
//            else
//            {
//                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//                _logger.LogError("", "Пользователь не прошел проверку подлинности");
//            }
//        }
//        catch (InvalidDataException e)
//        {
//            _logger.LogInformation(e.Message);
//            ModelState.AddModelError("", "При добавлении в список избранных кофеен произошла ошибка");
//        }
//        catch (Exception e)
//        {
//            _logger.LogInformation(e.Message);
//            ModelState.AddModelError("", "При добавлении в список избранных кофеен произошла ошибка");
//        }
//        return RedirectToAction("GetFavcoffeeshops");
//    }

//    public async Task<IActionResult> GetFavCoffeeShops()
//    {
//        var model = new FavCoffeeShopsListViewModel();
//        try
//        {
//            var user = HttpContext.User;
//            var roleString = user.FindFirst("Id_role");
//            var id = User.FindFirst("Id");
//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                //var id_user = Int32.Parse(id.Value);
//                var id_user = Guid.Parse(id.Value);
//                int id_role = int.Parse(roleString.Value); // Преобразуем строку в int
//                var pairs = await _favcoffeeshopsService.GetFavCoffeeShopsAsync(id_user, id_role);

//                foreach (var c in pairs)
//                {
//                    var cs = await _CoffeeShopService.GetCoffeeShopByIdAsync(c.Id_coffeeshop, id_role);
//                    var comp = await _CompanyService.GetCompanyByIdAsync(cs.Id_company, id_role);
//                    var cofcomp = new CoffeeShopWithCompany
//                    {
//                        CompanyName = comp.Name,
//                        CoffeeShop = cs
//                    };


//                    model.FavCoffeeShops.Add(cofcomp);
//                }

//                return View(model);



//            }
//            else
//            {
//                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//                _logger.LogError("", "Пользователь не прошел проверку подлинности");
//            }
//        }
//        catch (InvalidCastException e)
//        {
//            _logger.LogError(e, e.Message);
//            ModelState.AddModelError("", "При получении списка избранных кофеен произошла ошибка");
//            return View();
//        }
//        catch (Exception e)
//        {
//            ModelState.AddModelError("", "При получении списка избранных кофеен произошла ошибка");
//            _logger.LogError(e, e.Message);
//            return View();
//        }

//        return RedirectToAction("GetAllCompanies", "Company");
//    }
//}




using CoffeeShops.Controllers;
using CoffeeShops.Domain.Converters;
using CoffeeShops.Domain.Exceptions.CoffeeShopServiceExceptions;
using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.DTOs.CoffeeShop;
using CoffeeShops.DTOs.Company;
using CoffeeShops.DTOs.Drink;
using CoffeeShops.DTOs.Pagination;
using CoffeeShops.DTOs.Utils;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/v1/coffeeshops")]
[Authorize]
public class CoffeeShopController : BaseController
{
    private readonly ICoffeeShopService _coffeeshopService;
    private readonly ILogger<CoffeeShopController> _logger;

    public CoffeeShopController(ICoffeeShopService coffeeshopService, ILogger<CoffeeShopController> logger)
    {
        _coffeeshopService = coffeeshopService;
        _logger = logger;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PaginatedResponse<CoffeeShopResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
        Summary = "Получить все кофейни",
        Description = "Возвращает пагинированный список кофеен с фильтрацией"
    )]
    public async Task<IActionResult> GetAllCoffeeShops(
       [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] Guid? id_company = null,
        [FromQuery] bool onlyFavorites = false)
    {
        try
        {
            Guid cur_user_id = GetCurrentUserId();
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            var filters = new CoffeeShopFilters
            {
                Id_company = id_company,
                OnlyFavorites = onlyFavorites,
                Id_user = cur_user_id
            };

            var result = await _coffeeshopService.GetAllCoffeeShopsAsync(filters, page, limit, cur_id_role);

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
        catch (CoffeeShopNotFoundException ex)
        {
            return StatusCode(404, new Error(
                  message: "Кофейни не найдены",
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

    [HttpPost]
    [Authorize(Roles = "Administrator,Moderator")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateResponse), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [SwaggerOperation(
       Summary = "Создать новую кофейню",
       Description = "Создает новую кофейню (только для модераторов и администраторов)"
   )]
    public async Task<IActionResult> AddCoffeeShop([FromBody] CreateCoffeeShopRequest coffeeshopRequest)
    {
        try
        {
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
            CoffeeShop coffeeshopdomain = CreateCoffeeShopRequestConverter.ConvertToDomainModel(coffeeshopRequest);
            var coffeeshopId = await _coffeeshopService.AddCoffeeShopAsync(coffeeshopdomain, cur_id_role);

            return StatusCode(201, new CreateResponse { Id = coffeeshopId.ToString() });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new Error(
                 message: "Переданы некорректные данные",
                 code: 400,
                 err_details: ex.Message
             ));
        }
        catch (CoffeeShopIncorrectAtributeException ex)
        {
            return BadRequest(new Error(
                message: "Переданы некорректные данные",
                code: 400,
                err_details: ex.Message
            ));
        }
        catch (CompanyNotFoundException ex)
        {
            return StatusCode(404, new Error(
                 message: "Сеть кофеен не найдена",
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
    [HttpGet("{coffeeshopId}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CoffeeShopResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
     Summary = "Получить кофейню по ID",
     Description = "Возвращает информацию о кофейне по ее идентификатору"
 )]
    public async Task<IActionResult> GetCoffeeShopById(
     [FromRoute] Guid coffeeshopId)
    {
        try
        {
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());


            var cs = await _coffeeshopService.GetCoffeeShopByIdAsync(coffeeshopId, cur_id_role);

            return Ok(cs);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(401, new Error(
                 message: "Пользователь не авторизован",
                 code: 401,
                  err_details: ex.Message
             ));
        }
        catch (CoffeeShopNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Кофейня не найдена",
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

    [HttpDelete("{coffeeshopId}")]
    [Authorize(Roles = "Administrator, Moderator")]
    [Produces("application/json")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
    Summary = "Удалить кофейню",
    Description = "Удаляет кофейню из системы (только для администраторов и модераторов)"
)]
    public async Task<IActionResult> DeleteCoffeeShop(
    [FromRoute] Guid coffeeshopId)
    {
        try
        {
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            await _coffeeshopService.DeleteCoffeeShopAsync(coffeeshopId, cur_id_role);

            return Ok(new { Message = "Кофейня успешно удалена" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(401, new Error(
                 message: "Пользователь не авторизован",
                 code: 401,
                  err_details: ex.Message
             ));
        }
        catch (CoffeeShopNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Кофейня не найдена",
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