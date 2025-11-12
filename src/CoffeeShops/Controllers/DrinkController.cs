//using Microsoft.AspNetCore.Mvc;
//using CoffeeShops.Domain.Models;
//using CoffeeShops.Services.Interfaces.Services;
//using Swashbuckle.AspNetCore.Annotations;
//using System.ComponentModel.DataAnnotations;
//using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
//using Microsoft.Extensions.Logging;
//using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
//using CoffeeShops.ViewModels;

//namespace CoffeeShops.Controllers;

//public class DrinkController : Controller
//{
//    private readonly IDrinkService _DrinkService;
//    private readonly IDrinksCategoryService _drinksCategoryService;
//    private readonly IFavDrinksService _favdrinksService;
//    private readonly ICategoryService _categoryService;
//    private readonly ILogger<DrinkController> _logger;
//    public DrinkController(IDrinkService DrinkService, ILogger<DrinkController> logger, IDrinksCategoryService drinksCategoryService, IFavDrinksService favdrinksService, ICategoryService categoryService)
//    {
//        _DrinkService = DrinkService;
//        _logger = logger;
//        _drinksCategoryService = drinksCategoryService;
//        _favdrinksService = favdrinksService;
//        _categoryService = categoryService;
//    }


//    public async Task<IActionResult> GetAllDrinks()
//    {
//        var model = new DrinksListViewModel();
//        try
//        {
//            var user = HttpContext.User;
//            var roleString = user.FindFirst("Id_role");
//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                try
//                {
//                    int id_role = int.Parse(roleString.Value); 
//                    var drinks = await _DrinkService.GetAllDrinksAsync(id_role) ?? new List<Drink>(); 

//                    model.Id_role = id_role;
//                    model.Drinks = new List<DrinkViewModel>(); 

//                    foreach (var drink in drinks)
//                    {
//                        var drinkViewModel = new DrinkViewModel
//                        {
//                            Id_drink = drink.Id_drink,
//                            Name = drink.Name,
//                            Categories = await _drinksCategoryService.GetCategoryByDrinkIdAsync(drink.Id_drink, id_role)
//                        };
//                        model.Drinks.Add(drinkViewModel);
//                    }

//                    return View(model);
//                }
//                catch (InvalidCastException e)
//                {
//                    ModelState.AddModelError("", "При получении напитков произошла ошибка");
//                    _logger.LogError(e, e.Message);
//                    return View();
//                }
//                catch (Exception e)
//                {
//                    ModelState.AddModelError("", "При получении напитков произошла ошибка");
//                    _logger.LogError(e, e.Message);
//                    return View();
//                }
//            }
//            else
//            {
//                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//                _logger.LogError("", "Пользователь не прошел проверку подлинности");
//            }
//        }
//        catch (Exception e)
//        {
//            ModelState.AddModelError("", "При получении напитков произошла ошибка");
//            _logger.LogError(e, e.Message);
//            return View();
//        }

//        return RedirectToAction("GetAllCompanies", "Company");

//    }
//    public async Task<IActionResult> GetFavDrinks()
//    {
//        var model = new FavDrinksListViewModel();

//        try

//        {
//            var user = HttpContext.User;
//            var id = User.FindFirst("Id");
//            var roleString = user.FindFirst("Id_role");
//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                try
//                {
//                    var id_user = Guid.Parse(id.Value);
//                    int id_role = int.Parse(roleString.Value); 
//                    var drinks = await _favdrinksService.GetFavDrinksAsync(id_user, id_role) ?? new List<FavDrinks>();

//                    model.Id_role = id_role;
//                    model.FavDrinks = new List<DrinkViewModel>(); 

//                    foreach (var drink in drinks)
//                    {
//                        var d = await _DrinkService.GetDrinkByIdAsync(drink.Id_drink, id_role);
//                        var drinkViewModel = new DrinkViewModel
//                        {
//                            Id_drink = d.Id_drink,
//                            Name = d.Name,
//                            Categories = await _drinksCategoryService.GetCategoryByDrinkIdAsync(d.Id_drink, id_role)
//                        };
//                        model.FavDrinks.Add(drinkViewModel);
//                    }

//                    return View(model);
//                }
//                catch (InvalidCastException e)
//                {
//                    ModelState.AddModelError("", "При получении пользователей произошла ошибка");
//                    _logger.LogError(e, e.Message);
//                    return View();
//                }
//                catch (Exception e)
//                {
//                    ModelState.AddModelError("", "При получении пользователей произошла ошибка");
//                    _logger.LogError(e, e.Message);
//                    return View();
//                }
//            }
//            else
//            {
//                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//                _logger.LogError("", "Пользователь не прошел проверку подлинности");
//            }
//        }
//        catch (Exception e)
//        {
//            ModelState.AddModelError("", "При получении пользователей произошла ошибка");
//            _logger.LogError(e, e.Message);
//            return View();
//        }

//        return RedirectToAction("GetAllCompanies", "Company");

//    }

//    public async Task<IActionResult> DeleteFromFavs(Guid drink_id)
//    {
//        try
//        {
//            var user = HttpContext.User;
//            var id = User.FindFirst("Id");
//            var roleString = user.FindFirst("Id_role");
//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                try
//                {
//                    //var id_user = Int32.Parse(id.Value);
//                    var id_user = Guid.Parse(id.Value);
//                    int id_role = int.Parse(roleString.Value);
//                    await _favdrinksService.RemoveDrinkFromFavsAsync(id_user, drink_id, id_role);
//                    return RedirectToAction("GetFavDrinks", new
//                    {
//                        message = "Drink removed from favorites successfully"
//                    });
//                }
//                catch (UserNotFoundException e)
//                {
//                    _logger.LogError(e, e.Message);
//                    ModelState.AddModelError("", "Пользователь не найден");
//                }
//                catch (DrinkNotFoundException e)
//                {
//                    _logger.LogError(e, e.Message);
//                    ModelState.AddModelError("", "Напиток не найден");
//                }
//                catch (DrinkIsNotFavoriteException e)
//                {
//                    _logger.LogError(e, e.Message);
//                    ModelState.AddModelError("", "Напитка нет в списке избранных");
//                }
//                catch (Exception e)
//                {
//                    _logger.LogError(e, e.Message);
//                    ModelState.AddModelError("", "Произошла ошибка по время удаления напитка из избранных");
//                }
//            }
//            else
//            {
//                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//                _logger.LogError("", "Пользователь не прошел проверку подлинности");
//            }
//        }
//        catch (Exception e)
//        {
//            ModelState.AddModelError("", "При удалении напитка из избранного произошла ошибка");
//            _logger.LogError(e, e.Message);
//        }

//        return RedirectToAction("GetFavDrinks");
//    }

//    public async Task<IActionResult> AddToFavs(Guid drink_id, Guid page)
//    {
//        try
//        {
//            var user = HttpContext.User;
//            var id = User.FindFirst("Id");
//            var roleString = user.FindFirst("Id_role");
//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                try
//                {
//                    //var id_user = Int32.Parse(id.Value);
//                    var id_user = Guid.Parse(id.Value);
//                    int id_role = int.Parse(roleString.Value);
//                    await _favdrinksService.AddDrinkToFavsAsync(id_user, drink_id, id_role);
//                    //if (page == -1)
//                    if (page == Guid.Empty)
//                    {
//                        return RedirectToAction("GetAllDrinks");
//                    }
//                    else
//                    {
//                        return RedirectToAction("GetMenuByCompany", "Menu", new
//                        {
//                            companyId = page,
//                            message = ""
//                        });
//                    }
//                }
//                catch (InvalidDataException e)
//                {
//                    _logger.LogInformation(e.Message);
//                    ModelState.AddModelError("", e.Message);
//                }
//                catch (Exception e)
//                {
//                    _logger.LogInformation(e.Message);
//                    ModelState.AddModelError("", "Data Access layer");
//                }
//            }
//            else
//            {
//                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//                _logger.LogError("", "Пользователь не прошел проверку подлинности");
//            }
//        }
//        catch (Exception e)
//        {
//            ModelState.AddModelError("", "При добавлении напитка произошла ошибка");
//            _logger.LogError(e, e.Message);
//            return View();
//        }
//        return RedirectToAction("GetFavDrinks");
//    }


//    [HttpGet]
//    public IActionResult AddDrink()
//    {
//        AddDrinkViewModel model = new AddDrinkViewModel();
//        var user = HttpContext.User;
//        var roleString = user.FindFirst("Id_role");
//        int id_role = int.Parse(roleString.Value); // Преобразуем строку в int
//        model.Id_role = id_role;

//        return View(model);
//    }

//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> AddDrink(AddDrinkViewModel model)
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
//                    var cs = new Drink(model.Name);
//                    await _DrinkService.AddDrinkAsync(cs, id_role);
//                    return RedirectToAction("GetAllDrinks");
//                }
//                else
//                {
//                    ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//                    _logger.LogError("", "Пользователь не прошел проверку подлинности");
//                }
//            }
//            catch (DrinkIncorrectAtributeException e)
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
//                ModelState.AddModelError("", "Возникла ошибка при добавлении напитка");
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
//        return RedirectToAction("GetAllDrinks");
//    }
//    public async Task<IActionResult> DeleteDrink(Guid drink_id)
//    {
//        try
//        {
//            var user = HttpContext.User;
//            var id = User.FindFirst("Id");
//            var roleString = user.FindFirst("Id_role");
//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                try
//                {
//                    //var id_user = Int32.Parse(id.Value);
//                    var id_user = Guid.Parse(id.Value);
//                    int id_role = int.Parse(roleString.Value);
//                    await _DrinkService.DeleteDrinkAsync(drink_id, id_role);
//                    return RedirectToAction("GetAllDrinks", new
//                    {
//                        message = "Drink removed successfully"
//                    });
//                }
//                catch (DrinkNotFoundException e)
//                {
//                    _logger.LogError(e, e.Message);
//                    ModelState.AddModelError("", "Напиток не найден");
//                }
//                catch (Exception e)
//                {
//                    _logger.LogError(e, e.Message);
//                    ModelState.AddModelError("", "Произошла ошибка по время удаления напитка");
//                }
//            }
//            else
//            {
//                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//                _logger.LogError("", "Пользователь не прошел проверку подлинности");
//            }
//        }
//        catch (Exception e)
//        {
//            ModelState.AddModelError("", "При получении удалении напитка");
//            _logger.LogError(e, e.Message);
//        }

//        return RedirectToAction("GetAllDrinks");
//    }

//    [HttpGet]
//    public async Task<IActionResult> ChooseCategory(Guid drink_id)
//    {
//        _logger.LogInformation("HERE1");
//        try
//        {
//            var user = HttpContext.User;
//            var id = User.FindFirst("Id");
//            var roleString = user.FindFirst("Id_role");
//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                try
//                {
//                    //var id_user = Int32.Parse(id.Value);
//                    var id_user = Guid.Parse(id.Value);
//                    int id_role = int.Parse(roleString.Value);
//                    var drink = await _DrinkService.GetDrinkByIdAsync(drink_id, id_role);
//                    var allCategories = await _categoryService.GetAllCategoriesAsync(id_role);

//                    var model = new ChooseCategoriesViewModel
//                    {
//                        Id_role = id_role,
//                        Id_drink = drink.Id_drink,
//                        Name = drink.Name,
//                        AvailableCategories = await _categoryService.GetAllCategoriesAsync(id_role)
//                    };
//                    return View(model);
//                }
//                catch (Exception e)
//                {
//                    _logger.LogError(e, e.Message);
//                    ModelState.AddModelError("", "Произошла ошибка по время  выбора категории");
//                }
//            }
//            else
//            {
//                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//                _logger.LogError("", "Пользователь не прошел проверку подлинности");
//            }
//        }
//        catch (Exception e)
//        {
//            _logger.LogError(e, e.Message);
//            ModelState.AddModelError("", "Произошла ошибка по время  выбора категории");
//        }
//        return RedirectToAction("GetAllDrinks");

//    }

//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> ChooseCategory(
//    [FromForm] Guid id_drink, // Явное указание источника данных
//    [FromForm] List<Guid> selectedCategories)
//    {
//        var user = HttpContext.User;
//        var id = User.FindFirst("Id");
//        var roleString = user.FindFirst("Id_role");
//        try
//        {

//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                if (id_drink == Guid.Empty)
//                {
//                    throw new ArgumentException("Неверный ID напитка");
//                }

//                var id_user = Guid.Parse(id.Value);
//                int id_role = int.Parse(roleString.Value);

//                foreach (var categoryId in selectedCategories ?? Enumerable.Empty<Guid>())
//                {
//                    await _drinksCategoryService.AddAsync(id_drink, categoryId, id_role);
//                }

//                return RedirectToAction("GetAllDrinks");
//            }
//            else
//            {
//                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
//                _logger.LogError("", "Пользователь не прошел проверку подлинности");
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Ошибка сохранения категорий");
//            var id_user = Guid.Parse(id.Value);
//            int id_role = int.Parse(roleString.Value);
//            var model = new ChooseCategoriesViewModel
//            {
//                Id_drink = id_drink,
//                AvailableCategories = await _categoryService.GetAllCategoriesAsync(
//                    int.Parse(User.FindFirst("Id_role").Value)),
//                Id_role = id_role
//            };

//            return View(model);
//        }
//        return RedirectToAction("GetAllDrinks");
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
using CoffeeShops.DataAccess.Models;
using CoffeeShops.DTOs.Pagination;
using CoffeeShops.DTOs.Drink;
using Microsoft.AspNetCore.Authorization;
using CoffeeShops.Domain.Models.Enums;
using Serilog;
using System.Security.Claims;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.DTOs.User;
using CoffeeShops.Domain.Exceptions.CategoryServiceExceptions;
using CoffeeShops.DTOs.DrinksCategory;



namespace CoffeeShops.Controllers;


[ApiController]
[Route("api/v1/drinks")]
[Authorize] 
public class DrinkController : BaseController
{
    private readonly IDrinkService _drinkService;
    private readonly IDrinksCategoryService _drinkscategoryService;
    private readonly ILogger<DrinkController> _logger;

    public DrinkController(IDrinkService drinkService, IDrinksCategoryService drinkscategoryService, ILogger<DrinkController> logger)
    {
        _drinkService = drinkService;
        _drinkscategoryService = drinkscategoryService;
        _logger = logger;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PaginatedResponse<DrinkResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
        Summary = "Получить все напитки",
        Description = "Возвращает пагинированный список напитков с фильтрацией"
    )]
    public async Task<IActionResult> GetAllDrinks(
       [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? category_name = null,
        [FromQuery] string? drink_name = null,
        [FromQuery] bool onlyFavorites = false)
    {
        try
        {
            Guid cur_user_id = GetCurrentUserId();
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            var filters = new DrinkFilters
            {
                CategoryName = category_name,
                DrinkName = drink_name,
                OnlyFavorites = onlyFavorites,
                Id_user = cur_user_id
            };
            _logger.LogInformation($"Category {filters.CategoryName}");

            var result = await _drinkService.GetAllDrinksAsync(filters, page, limit, cur_id_role);

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
        catch (NoDrinksFoundException ex)
        {
            return StatusCode(404, new Error(
                  message: "Напитки не найдены",
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
        Summary = "Создать новый напиток",
        Description = "Создает новый напиток (только для модераторов и администраторов)"
    )]
    public async Task<IActionResult> AddDrink([FromBody] CreateDrinkRequest drinkRequest)
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
            Drink drinkdomain = CreateDrinkRequestConverter.ConvertToDomainModel(drinkRequest);
            var drinkId = await _drinkService.AddDrinkAsync(drinkdomain, cur_id_role);

            return StatusCode(201, new CreateResponse { Id = drinkId.ToString() });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new Error(
                 message: "Переданы некорректные данные",
                 code: 400,
                 err_details: ex.Message
             ));
        }
        catch (DrinkNameAlreadyExistsException ex)
        {
            return Conflict(new Error(
                 message: "Напиток с таким именем уже существует",
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

    [HttpGet("{drinkId}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DrinkResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
     Summary = "Получить напиток по ID",
     Description = "Возвращает информацию о напитке по его идентификатору"
 )]
    public async Task<IActionResult> GetDrinkById(
     [FromRoute] Guid drinkId)
    {
        try
        {
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());


            var drink = await _drinkService.GetDrinkByIdAsync(drinkId, cur_id_role);

            return Ok(drink);
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
    Summary = "Удалить напиток",
    Description = "Удаляет напиток из системы (только для администраторов и модераторов)"
)]
    public async Task<IActionResult> DeleteDrink(
    [FromRoute] Guid drinkId)
    {
        try
        {
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            await _drinkService.DeleteDrinkAsync(drinkId, cur_id_role);

            return Ok(new { Message = "Напиток успешно удален" });
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
        catch (Exception ex)
        {
            return StatusCode(500, new Error(
                 message: "Внутренняя ошибка сервера",
                 code: 500,
                 err_details: ex.Message
             ));
        }
    }

    [HttpGet("{drinkId:guid}/categories")]
    [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDrinkCategories([FromRoute] Guid drinkId)
    {
        try
        {
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            var categories = await _drinkscategoryService.GetCategoryByDrinkIdAsync(drinkId, cur_id_role);

            return Ok(categories);
        }
        catch (DrinkNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Напиток не найден",
                 code: 404,
                  err_details: ex.Message
             ));
        }
        catch (NoDrinksCategoriesFoundException ex)
        {
            return NotFound(new Error(
                 message: "Категории для этого напитка не найдены",
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

    [HttpPost("{drinkId:guid}/categories")]
    [Authorize(Roles = "Administrator,Moderator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddCategoryToDrink(
        [FromRoute] Guid drinkId,
        [FromBody] AddDrinksCategory request) 
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

            await _drinkscategoryService.AddAsync(drinkId, request.CategoryId, cur_id_role);

            return Ok(new
            {
                message = "Категория успешно добавлена к напитку"
            });
        }
        catch (DrinkNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Напиток не найден",
                 code: 404,
                  err_details: ex.Message
             ));
        }
        catch (CategoryNotFoundException ex)
        {
            return NotFound(new Error(
                 message: "Категория не найдена",
                 code: 404,
                  err_details: ex.Message
             ));
        }
        catch (DrinkAlreadyHasThisCategoryException ex)
        {
            return Conflict(new Error(
                 message: "у напитка уже есть эта категория",
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
}