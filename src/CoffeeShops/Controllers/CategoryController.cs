//using Microsoft.AspNetCore.Mvc;
//using CoffeeShops.Domain.Models;
//using CoffeeShops.Services.Interfaces.Services;
//using Swashbuckle.AspNetCore.Annotations;
//using Microsoft.Extensions.Logging;
//using System.ComponentModel.DataAnnotations;
//using CoffeeShops.Domain.Exceptions.CategoryServiceExceptions;
//using CoffeeShops.ViewModels;

//namespace CoffeeShops.Controllers
//{
//    public class CategoryController : Controller
//    {
//        private readonly ICategoryService _categoryService;
//        public CategoryController(ICategoryService categoryService)
//        {
//            _categoryService = categoryService;
//        }

//        public async Task<IActionResult> GetAllCategories()
//        {
//            var model = new CategoriesListViewModel();

//            var user = HttpContext.User;
//            var roleString = user.FindFirst("Id_role");
//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                try
//                {
//                    int id_role = int.Parse(roleString.Value); // Преобразуем строку в int
//                    model.Categories = await _categoryService.GetAllCategoriesAsync(id_role);
//                    model.Id_role = id_role;
//                    return View(model);
//                }
//                catch (InvalidCastException)
//                {
//                    ModelState.AddModelError("", "Data Access layer");
//                    return View();
//                }
//            }

//            try
//            {
//                model.Categories = await _categoryService.GetAllCategoriesAsync(1);

//                return View(model);
//            }
//            catch (Exception)
//            {
//                ModelState.AddModelError("", "Data Access layer");
//                return View();
//            }

//        }

//        public async Task<IActionResult> GetCategoriesByDrink()
//        {
//            var model = new CategoriesListViewModel();

//            var user = HttpContext.User;
//            var roleString = user.FindFirst("Id_role");
//            if (user.Identity is not null && user.Identity.IsAuthenticated)
//            {
//                try
//                {
//                    int id_role = int.Parse(roleString.Value); // Преобразуем строку в int
//                    model.Categories = await _categoryService.GetAllCategoriesAsync(id_role);
//                    model.Id_role = id_role;
//                    return View(model);
//                }
//                catch (InvalidCastException)
//                {
//                    ModelState.AddModelError("", "Data Access layer");
//                    return View();
//                }
//            }

//            try
//            {
//                model.Categories = await _categoryService.GetAllCategoriesAsync(1);

//                return View(model);
//            }
//            catch (Exception)
//            {
//                ModelState.AddModelError("", "Data Access layer");
//                return View();
//            }

//        }

//        //public async Task<IActionResult> AddCategory
//        //public async Task<IActionResult> GetCategoriesByDrink()
//    }
//}



using CoffeeShops.Controllers;
using CoffeeShops.Domain.Converters;
using CoffeeShops.Domain.Exceptions.CategoryServiceExceptions;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.DTOs.Category;
using CoffeeShops.DTOs.Drink;
using CoffeeShops.DTOs.Pagination;
using CoffeeShops.DTOs.Utils;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/v1/categories")]
[Authorize]
public class CategoryController : BaseController
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PaginatedResponse<CategoryResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [SwaggerOperation(
        Summary = "Получить все категории",
        Description = "Возвращает пагинированный список категорий"
    )]
    public async Task<IActionResult> GetAllCategories(
       [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        try
        {
            Guid cur_user_id = GetCurrentUserId();
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(GetCurrentUserRole());

            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            var result = await _categoryService.GetAllCategoriesAsync(page, limit, cur_id_role);

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
        catch (CategoryNotFoundException ex)
        {
            return StatusCode(404, new Error(
                  message: "Категории не найдены",
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
       Summary = "Создать новую категорию",
       Description = "Создает новую категорию (только для модераторов и администраторов)"
   )]
    public async Task<IActionResult> AddCategory([FromBody] CreateCategoryRequest categoryRequest)
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
            Category categorydomain = CreateCategoryRequestConverter.ConvertToDomainModel(categoryRequest);
            var categoryId = await _categoryService.AddCategoryAsync(categorydomain, cur_id_role);

            return StatusCode(201, new CreateResponse { Id = categoryId.ToString() });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new Error(
                 message: "Переданы некорректные данные",
                 code: 400,
                 err_details: ex.Message
             ));
        }
        catch(CategoryIncorrectAtributeException ex)
        {
            return BadRequest(new Error(
                message: "Переданы некорректные данные",
                code: 400,
                err_details: ex.Message
            ));
        }
        catch (CategoryUniqueException ex)
        {
            return Conflict(new Error(
                 message: "Категория с таким именем уже существует",
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