using Microsoft.AspNetCore.Mvc;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using Microsoft.Extensions.Logging;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using CoffeeShops.ViewModels;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.DTOs.Drink;
using CoffeeShops.Services.Services;
using System.Security.Claims;
using CoffeeShops.Controllers.GUI;

namespace CoffeeShops.Controllers.Gui;

[Route("api/v2/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class DrinkGuiController : BaseGuiController
{
    private readonly IDrinkService _DrinkService;
    private readonly IDrinksCategoryService _drinksCategoryService;
    private readonly IFavDrinksService _favdrinksService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<DrinkGuiController> _logger;
    public DrinkGuiController(IDrinkService DrinkService, ILogger<DrinkGuiController> logger, IDrinksCategoryService drinksCategoryService, IFavDrinksService favdrinksService, ICategoryService categoryService)
    {
        _DrinkService = DrinkService;
        _logger = logger;
        _drinksCategoryService = drinksCategoryService;
        _favdrinksService = favdrinksService;
        _categoryService = categoryService;
    }

    [HttpGet("drinks")]
    public async Task<IActionResult> GetAllDrinks()
    {
        if (!IsAuthenticated())
        {
            return RedirectToAction("Login", "User");
        }

        try
        {
            var model = new DrinksListViewModel();
            Guid cur_user_id = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid id)
    ? id
    : Guid.Empty;
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty);
            int page = 1;
            int limit = 100;

            var filters = new DrinkFilters();

            var drinks = await _DrinkService.GetAllDrinksAsync(filters, page, limit, cur_id_role);

            model.Id_role = cur_id_role;
            model.Drinks = new List<DrinkViewModel>();

            foreach (var drink in drinks.Data)
            {
                try
                {
                    var drinkViewModel = new DrinkViewModel
                    {
                        Id_drink = drink.Id_drink,
                        Name = drink.Name,
                        Categories = await _drinksCategoryService.GetCategoryByDrinkIdAsync(drink.Id_drink, cur_id_role)
                    };
                    model.Drinks.Add(drinkViewModel);
                }
                catch (Exception ex)
                {
                    var drinkViewModel = new DrinkViewModel
                    {
                        Id_drink = drink.Id_drink,
                        Name = drink.Name
                    };
                    model.Drinks.Add(drinkViewModel);
                }
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllDrinks");
            ModelState.AddModelError("", "Произошла ошибка при получении списка компаний");
            return View(new CompaniesListViewModel());
        }

    }
    //public async Task<IActionResult> GetFavDrinks()
    //{
    //    var model = new FavDrinksListViewModel();

    //    try

    //    {
    //        var user = HttpContext.User;
    //        var id = User.FindFirst("Id");
    //        var roleString = user.FindFirst("Id_role");
    //        if (user.Identity is not null && user.Identity.IsAuthenticated)
    //        {
    //            try
    //            {
    //                var id_user = Guid.Parse(id.Value);
    //                int id_role = int.Parse(roleString.Value);
    //                var drinks = await _favdrinksService.GetFavDrinksAsync(id_user, id_role) ?? new List<FavDrinks>();

    //                model.Id_role = id_role;
    //                model.FavDrinks = new List<DrinkViewModel>();

    //                foreach (var drink in drinks)
    //                {
    //                    var d = await _DrinkService.GetDrinkByIdAsync(drink.Id_drink, id_role);
    //                    var drinkViewModel = new DrinkViewModel
    //                    {
    //                        Id_drink = d.Id_drink,
    //                        Name = d.Name,
    //                        Categories = await _drinksCategoryService.GetCategoryByDrinkIdAsync(d.Id_drink, id_role)
    //                    };
    //                    model.FavDrinks.Add(drinkViewModel);
    //                }

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
    //    }
    //    catch (Exception e)
    //    {
    //        ModelState.AddModelError("", "При получении пользователей произошла ошибка");
    //        _logger.LogError(e, e.Message);
    //        return View();
    //    }

    //    return RedirectToAction("GetAllCompanies", "Company");

    //}

    //public async Task<IActionResult> DeleteFromFavs(Guid drink_id)
    //{
    //    try
    //    {
    //        var user = HttpContext.User;
    //        var id = User.FindFirst("Id");
    //        var roleString = user.FindFirst("Id_role");
    //        if (user.Identity is not null && user.Identity.IsAuthenticated)
    //        {
    //            try
    //            {
    //                //var id_user = Int32.Parse(id.Value);
    //                var id_user = Guid.Parse(id.Value);
    //                int id_role = int.Parse(roleString.Value);
    //                await _favdrinksService.RemoveDrinkFromFavsAsync(id_user, drink_id, id_role);
    //                return RedirectToAction("GetFavDrinks", new
    //                {
    //                    message = "Drink removed from favorites successfully"
    //                });
    //            }
    //            catch (UserNotFoundException e)
    //            {
    //                _logger.LogError(e, e.Message);
    //                ModelState.AddModelError("", "Пользователь не найден");
    //            }
    //            catch (DrinkNotFoundException e)
    //            {
    //                _logger.LogError(e, e.Message);
    //                ModelState.AddModelError("", "Напиток не найден");
    //            }
    //            catch (DrinkIsNotFavoriteException e)
    //            {
    //                _logger.LogError(e, e.Message);
    //                ModelState.AddModelError("", "Напитка нет в списке избранных");
    //            }
    //            catch (Exception e)
    //            {
    //                _logger.LogError(e, e.Message);
    //                ModelState.AddModelError("", "Произошла ошибка по время удаления напитка из избранных");
    //            }
    //        }
    //        else
    //        {
    //            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
    //            _logger.LogError("", "Пользователь не прошел проверку подлинности");
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        ModelState.AddModelError("", "При удалении напитка из избранного произошла ошибка");
    //        _logger.LogError(e, e.Message);
    //    }

    //    return RedirectToAction("GetFavDrinks");
    //}

    //public async Task<IActionResult> AddToFavs(Guid drink_id, Guid page)
    //{
    //    try
    //    {
    //        var user = HttpContext.User;
    //        var id = User.FindFirst("Id");
    //        var roleString = user.FindFirst("Id_role");
    //        if (user.Identity is not null && user.Identity.IsAuthenticated)
    //        {
    //            try
    //            {
    //                //var id_user = Int32.Parse(id.Value);
    //                var id_user = Guid.Parse(id.Value);
    //                int id_role = int.Parse(roleString.Value);
    //                await _favdrinksService.AddDrinkToFavsAsync(id_user, drink_id, id_role);
    //                //if (page == -1)
    //                if (page == Guid.Empty)
    //                {
    //                    return RedirectToAction("GetAllDrinks");
    //                }
    //                else
    //                {
    //                    return RedirectToAction("GetMenuByCompany", "Menu", new
    //                    {
    //                        companyId = page,
    //                        message = ""
    //                    });
    //                }
    //            }
    //            catch (InvalidDataException e)
    //            {
    //                _logger.LogInformation(e.Message);
    //                ModelState.AddModelError("", e.Message);
    //            }
    //            catch (Exception e)
    //            {
    //                _logger.LogInformation(e.Message);
    //                ModelState.AddModelError("", "Data Access layer");
    //            }
    //        }
    //        else
    //        {
    //            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
    //            _logger.LogError("", "Пользователь не прошел проверку подлинности");
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        ModelState.AddModelError("", "При добавлении напитка произошла ошибка");
    //        _logger.LogError(e, e.Message);
    //        return View();
    //    }
    //    return RedirectToAction("GetFavDrinks");
    //}


    //[HttpGet]
    //public IActionResult AddDrink()
    //{
    //    AddDrinkViewModel model = new AddDrinkViewModel();
    //    var user = HttpContext.User;
    //    var roleString = user.FindFirst("Id_role");
    //    int id_role = int.Parse(roleString.Value); // Преобразуем строку в int
    //    model.Id_role = id_role;

    //    return View(model);
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> AddDrink(AddDrinkViewModel model)
    //{
    //    if (ModelState.IsValid)
    //    {
    //        try
    //        {
    //            var user = HttpContext.User;
    //            var roleString = user.FindFirst("Id_role");


    //            if (user.Identity is not null && user.Identity.IsAuthenticated && roleString is not null)
    //            {

    //                int id_role = int.Parse(roleString.Value);
    //                var cs = new Drink(model.Name);
    //                await _DrinkService.AddDrinkAsync(cs, id_role);
    //                return RedirectToAction("GetAllDrinks");
    //            }
    //            else
    //            {
    //                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
    //                _logger.LogError("", "Пользователь не прошел проверку подлинности");
    //            }
    //        }
    //        catch (DrinkIncorrectAtributeException e)
    //        {
    //            ModelState.AddModelError("", "Данные введены некорректно");
    //            _logger.LogError(e, e.Message);
    //            return View(model);
    //        }
    //        catch (InvalidDataException e)
    //        {
    //            ModelState.AddModelError("", "Данные введены некорректно");
    //            _logger.LogError(e, e.Message);
    //            return View(model);
    //        }
    //        catch (Exception e)
    //        {
    //            ModelState.AddModelError("", "Возникла ошибка при добавлении напитка");
    //            _logger.LogError(e, e.Message);
    //            return View(model);
    //        }
    //    }

    //    else
    //    {

    //        _logger.LogWarning("ModelState is invalid");
    //        foreach (var error in ModelState)
    //        {
    //            _logger.LogWarning($"Field: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
    //        }
    //        return View(model);
    //    }
    //    return RedirectToAction("GetAllDrinks");
    //}
    //public async Task<IActionResult> DeleteDrink(Guid drink_id)
    //{
    //    try
    //    {
    //        var user = HttpContext.User;
    //        var id = User.FindFirst("Id");
    //        var roleString = user.FindFirst("Id_role");
    //        if (user.Identity is not null && user.Identity.IsAuthenticated)
    //        {
    //            try
    //            {
    //                //var id_user = Int32.Parse(id.Value);
    //                var id_user = Guid.Parse(id.Value);
    //                int id_role = int.Parse(roleString.Value);
    //                await _DrinkService.DeleteDrinkAsync(drink_id, id_role);
    //                return RedirectToAction("GetAllDrinks", new
    //                {
    //                    message = "Drink removed successfully"
    //                });
    //            }
    //            catch (DrinkNotFoundException e)
    //            {
    //                _logger.LogError(e, e.Message);
    //                ModelState.AddModelError("", "Напиток не найден");
    //            }
    //            catch (Exception e)
    //            {
    //                _logger.LogError(e, e.Message);
    //                ModelState.AddModelError("", "Произошла ошибка по время удаления напитка");
    //            }
    //        }
    //        else
    //        {
    //            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
    //            _logger.LogError("", "Пользователь не прошел проверку подлинности");
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        ModelState.AddModelError("", "При получении удалении напитка");
    //        _logger.LogError(e, e.Message);
    //    }

    //    return RedirectToAction("GetAllDrinks");
    //}

    //[HttpGet]
    //public async Task<IActionResult> ChooseCategory(Guid drink_id)
    //{
    //    _logger.LogInformation("HERE1");
    //    try
    //    {
    //        var user = HttpContext.User;
    //        var id = User.FindFirst("Id");
    //        var roleString = user.FindFirst("Id_role");
    //        if (user.Identity is not null && user.Identity.IsAuthenticated)
    //        {
    //            try
    //            {
    //                //var id_user = Int32.Parse(id.Value);
    //                var id_user = Guid.Parse(id.Value);
    //                int id_role = int.Parse(roleString.Value);
    //                var drink = await _DrinkService.GetDrinkByIdAsync(drink_id, id_role);
    //                var allCategories = await _categoryService.GetAllCategoriesAsync(id_role);

    //                var model = new ChooseCategoriesViewModel
    //                {
    //                    Id_role = id_role,
    //                    Id_drink = drink.Id_drink,
    //                    Name = drink.Name,
    //                    AvailableCategories = await _categoryService.GetAllCategoriesAsync(id_role)
    //                };
    //                return View(model);
    //            }
    //            catch (Exception e)
    //            {
    //                _logger.LogError(e, e.Message);
    //                ModelState.AddModelError("", "Произошла ошибка по время  выбора категории");
    //            }
    //        }
    //        else
    //        {
    //            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
    //            _logger.LogError("", "Пользователь не прошел проверку подлинности");
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        _logger.LogError(e, e.Message);
    //        ModelState.AddModelError("", "Произошла ошибка по время  выбора категории");
    //    }
    //    return RedirectToAction("GetAllDrinks");

    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> ChooseCategory(
    //[FromForm] Guid id_drink, // Явное указание источника данных
    //[FromForm] List<Guid> selectedCategories)
    //{
    //    var user = HttpContext.User;
    //    var id = User.FindFirst("Id");
    //    var roleString = user.FindFirst("Id_role");
    //    try
    //    {

    //        if (user.Identity is not null && user.Identity.IsAuthenticated)
    //        {
    //            if (id_drink == Guid.Empty)
    //            {
    //                throw new ArgumentException("Неверный ID напитка");
    //            }

    //            var id_user = Guid.Parse(id.Value);
    //            int id_role = int.Parse(roleString.Value);

    //            foreach (var categoryId in selectedCategories ?? Enumerable.Empty<Guid>())
    //            {
    //                await _drinksCategoryService.AddAsync(id_drink, categoryId, id_role);
    //            }

    //            return RedirectToAction("GetAllDrinks");
    //        }
    //        else
    //        {
    //            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
    //            _logger.LogError("", "Пользователь не прошел проверку подлинности");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Ошибка сохранения категорий");
    //        var id_user = Guid.Parse(id.Value);
    //        int id_role = int.Parse(roleString.Value);
    //        var model = new ChooseCategoriesViewModel
    //        {
    //            Id_drink = id_drink,
    //            AvailableCategories = await _categoryService.GetAllCategoriesAsync(
    //                int.Parse(User.FindFirst("Id_role").Value)),
    //            Id_role = id_role
    //        };

    //        return View(model);
    //    }
    //    return RedirectToAction("GetAllDrinks");
    //}


}



