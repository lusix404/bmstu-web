//using Microsoft.AspNetCore.Mvc;
//using CoffeeShops.Domain.Models;
//using CoffeeShops.Services.Interfaces.Services;
//using Swashbuckle.AspNetCore.Annotations;
//using System.ComponentModel.DataAnnotations;
//using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
//using Microsoft.Extensions.Logging;
//using CoffeeShops.ViewModels;

//namespace CoffeeShops.Controllers;
//public class CompanyController : Controller
//{
//    private readonly ICompanyService _companyService;
//    private readonly ILogger<CompanyController> _logger;

//    public CompanyController(ICompanyService companyService, ILogger<CompanyController> logger)
//    {
//        _companyService = companyService;
//        _logger = logger;
//    }

//    public async Task<IActionResult> GetAllCompanies()
//    {
//        try
//        {
//            var model = new CompaniesListViewModel();
//            var user = HttpContext.User;
//            var roleString = user.FindFirst("Id_role");

//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                if (int.TryParse(roleString?.Value, out int id_role))
//                {
//                    model.Companies = await _companyService.GetAllCompaniesAsync(id_role);
//                    model.Id_role = id_role;
//                    return View(model);
//                }
//                else
//                {
//                    _logger.LogWarning("Failed to parse role ID");
//                    ModelState.AddModelError("", "Ошибка определения роли пользователя");
//                }
//            }

//            // Если пользователь не аутентифицирован или не удалось получить роль
//            model.Companies = await _companyService.GetAllCompaniesAsync(1);
//            return View(model);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error in GetAllCompanies");
//            ModelState.AddModelError("", "Произошла ошибка при получении списка компаний");
//            return View(new CompaniesListViewModel());
//        }
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
using CoffeeShops.DTOs.Utils;
using CoffeeShops.Domain.Converters;
using System.Net;
using CoffeeShops.DataAccess.Models;
using CoffeeShops.DTOs.Pagination;
using CoffeeShops.DTOs.Company;
using Microsoft.AspNetCore.Authorization;
using CoffeeShops.Domain.Models.Enums;
using Serilog;
using System.Security.Claims;
using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
using CoffeeShops.DTOs.Menu;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Exceptions.CoffeeShopServiceExceptions;
using CoffeeShops.DTOs.CoffeeShop;
using CoffeeShops.Domain.Exceptions.MenuServiceExceptions;



namespace CoffeeShops.Controllers;


[ApiController]
[Route("api/v1/companies")]
[Authorize]
public class CompanyController : BaseController
{
    private readonly ICompanyService _companyService;
    private readonly ICoffeeShopService _coffeeshopService;
    private readonly IMenuService _menuService;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ICompanyService companyService, ICoffeeShopService coffeeshopService, IMenuService menuService, ILogger<CompanyController> logger)
    {
        _companyService = companyService;
        _logger = logger;
        _coffeeshopService = coffeeshopService;
        _menuService = menuService;
    }

    // [HttpGet("companies")]
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PaginatedResponse<CompanyResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
        Summary = "Получить все сети кофеен",
        Description = "Возвращает пагинированный список сетей кофеен"
    )]
    [AllowAnonymous] 
    public async Task<IActionResult> GetAllCompanies(
       [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] Guid? id_drink = null)
    {
        try
        {
            // Guid cur_user_id = GetCurrentUserId();
            // int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            int cur_id_role = UserRoleExtensions.ToRoleIntFromString("Ordinary_user");
            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            var filters = new CompanyFilters
            {
                Id_drink = id_drink
            };

            var result = await _companyService.GetAllCompaniesAsync(filters, page, limit, cur_id_role);

            return Ok(result);
        }
        // catch (UnauthorizedAccessException ex)
        // {
        //     return StatusCode(401, new Error(
        //          message: "Пользователь не авторизован",
        //          code: 401,
        //           err_details: ex.Message
        //      ));
        // }
        catch (NoCompaniesFoundException ex)
        {
            return StatusCode(404, new Error(
                  message: "Сети кофеен не найдены",
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
    [SwaggerOperation(
        Summary = "Создать новую сеть кофеен",
        Description = "Создает новую сеть кофеен (только для модераторов и администраторов)"
    )]
    public async Task<IActionResult> AddCompany([FromBody] CreateCompanyRequest companyRequest)
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
            Company companydomain = CreateCompanyRequestConverter.ConvertToDomainModel(companyRequest);
            var companyId = await _companyService.AddCompanyAsync(companydomain, cur_id_role);

            return StatusCode(201, new CreateResponse { Id = companyId.ToString() });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new Error(
                 message: "Переданы некорректные данные",
                 code: 400,
                 err_details: ex.Message
             ));
        }
        catch (CompanyIncorrectAtributeException ex)
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

    [HttpGet("{companyId}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CompanyResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
     Summary = "Получить сеть кофеен по ID",
     Description = "Возвращает информацию о сети кофеен по ее идентификатору"
 )]
    public async Task<IActionResult> GetCompanyById(
     [FromRoute] Guid companyId)
    {
        try
        {
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            var company = await _companyService.GetCompanyByIdAsync(companyId, cur_id_role);

            return Ok(company);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(401, new Error(
                 message: "Пользователь не авторизован",
                 code: 401,
                  err_details: ex.Message
             ));
        }
        catch (CompanyNotFoundException ex)
        {
            return NotFound(new Error(
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

    [HttpDelete("{companyId}")]
    [Authorize(Roles = "Administrator, Moderator")]
    [Produces("application/json")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
    Summary = "Удалить сеть кофеен",
    Description = "Удаляет сеть кофеен из системы (только для администраторов и модераторов)"
)]
    public async Task<IActionResult> DeleteCompany(
    [FromRoute] Guid companyId)
    {
        try
        {
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            await _companyService.DeleteCompanyAsync(companyId, cur_id_role);

            return Ok(new { Message = "Сеть кофеен успешно удалена" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(401, new Error(
                 message: "Пользователь не авторизован",
                 code: 401,
                  err_details: ex.Message
             ));
        }
        catch (CompanyNotFoundException ex)
        {
            return NotFound(new Error(
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

    [HttpGet("{companyId}/coffeeshops")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PaginatedResponse<CoffeeShopResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
        Summary = "Получить все кофейни сети кофеен",
        Description = "Возвращает пагинированный список кофеен определенной сети с фильтрацией"
    )]
    public async Task<IActionResult> GetCoffeeShopsByCompanyId(
        [FromRoute] Guid companyId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
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
                Id_company = companyId,
                OnlyFavorites = onlyFavorites,
                Id_user = cur_user_id
            };
            var exists = await _companyService.GetCompanyByIdAsync(companyId, cur_id_role);
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
        catch (CompanyNotFoundException ex)
        {
            return StatusCode(404, new Error(
                  message: "Сеть кофеен не найдена",
                  code: 404,
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

    [HttpGet("{companyId}/menu")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PaginatedResponse<MenuResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
        Summary = "Получить все позиции меню сети кофеен",
        Description = "Возвращает пагинированный список позиций меню определенной сети кофеен"
    )]
    public async Task<IActionResult> GetMenuByCompanyId(
        [FromRoute] Guid companyId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        try
        {
            Guid cur_user_id = GetCurrentUserId();
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            var exists = await _companyService.GetCompanyByIdAsync(companyId, cur_id_role);
            var result = await _menuService.GetMenuByCompanyIdAsync(companyId, page, limit, cur_id_role);

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
        catch (CompanyNotFoundException ex)
        {
            return StatusCode(404, new Error(
                  message: "Сеть кофеен не найдена",
                  code: 404,
                   err_details: ex.Message
              ));
        }
        catch (MenuNotFoundException ex)
        {
            return StatusCode(404, new Error(
                  message: "Меню не найдено",
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

    [HttpPost("{companyId}/menu")]
    [Authorize(Roles = "Administrator,Moderator")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateResponse), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [SwaggerOperation(
        Summary = "Добавить позицию в меню",
        Description = "Добавляет новую позицию в меню конкретной компании (только для модераторов и администраторов)"
    )]
    public async Task<IActionResult> AddRecordToMenu([FromRoute] Guid companyId, [FromBody] CreateMenuRequest menuRequest)
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
            Menu menudomain = new Menu(menuRequest.Id_drink, companyId, menuRequest.Size, menuRequest.Price);
            await _menuService.AddDrinkToMenuAsync(menudomain, cur_id_role);

            return StatusCode(201);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(401, new Error(
                 message: "Пользователь не авторизован",
                 code: 401,
                  err_details: ex.Message
             ));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new Error(
                 message: "Переданы некорректные данные",
                 code: 400,
                 err_details: ex.Message
             ));
        }
        catch (MenuIncorrectAtributeException ex)
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

    [HttpDelete("{companyId}/menu/{drinkId}")]
    [Authorize(Roles = "Administrator, Moderator")]
    [Produces("application/json")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
    Summary = "Удалить позицию меню",
    Description = "Удаляет позицию из меню конкретной сети кофеен (только для администраторов и модераторов)"
)]
    public async Task<IActionResult> DeleteRecordFromMenu(
    [FromRoute] Guid companyId, [FromRoute] Guid drinkId)
    {
        try
        {
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            await _menuService.DeleteDrinkFromMenuAsync(drinkId, companyId, cur_id_role);

            return Ok(new { Message = "Позиция успешно удалена из меню" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(401, new Error(
                 message: "Пользователь не авторизован",
                 code: 401,
                  err_details: ex.Message
             ));
        }
        catch (DrinkNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Напиток не найден",
                 code: 404,
                  err_details: ex.Message
             ));
        }
        catch (CompanyNotFoundException ex)
        {
            return NotFound(new Error(
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
}