using Microsoft.AspNetCore.Mvc;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;
using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
using CoffeeShops.Domain.Exceptions.MenuServiceExceptions;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.ViewModels;


namespace CoffeeShops.Controllers;
public class MenuController : Controller
{
    private readonly IMenuService _menuService;

    private readonly IDrinkService _drinkService;
    private readonly ILogger<MenuController> _logger;
    public MenuController(IMenuService menuService, ILogger<MenuController> logger, IDrinkService drinkService)
    {
        _menuService = menuService;
        _logger = logger;
        _drinkService = drinkService;
    }

    public async Task<IActionResult> GetMenuByCompany(Guid companyId)
    {
        var model = new MenuListViewModel();
        try
        {
            var user = HttpContext.User;
            var roleString = user.FindFirst("Id_role");
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                try
                {
                    int id_role = int.Parse(roleString.Value);
                    model.Menu = await _menuService.GetMenuByCompanyIdAsync(companyId, id_role);
                    model.Id_company = companyId;

                    // Добавленный цикл для получения напитков
                    foreach (var menuItem in model.Menu)
                    {
                        var drink = await _menuService.GetDrinkByIdAsync(menuItem.Id_drink, id_role);
                        if (drink != null)
                        {
                            model.Drinks.Add(drink);
                        }
                    }

                    model.Id_role = id_role;
                    return View(model);
                }
                catch (CompanyNotFoundException e)
                {
                    ModelState.AddModelError("", "Данная сеть кофеен не найдена"); // Привязываем к полю Login
                    _logger.LogError(e, e.Message);
                }
                catch (MenuNotFoundException e)
                {
                    ModelState.AddModelError("", "Меню для данной сети кофеен не найдено"); // Привязываем к полю Login
                    _logger.LogError(e, e.Message);
                }
                catch (InvalidCastException e)
                {
                    ModelState.AddModelError("", "Произошла ошибка при получении меню");
                    _logger.LogError(e, e.Message);
                    return View();
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    ModelState.AddModelError("", "Произошла ошибка при получении меню");
                    return View();
                }

            }
            else
            {
                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
                _logger.LogError("", "Пользователь не прошел проверку подлинности");
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e.Message);
            ModelState.AddModelError("", "Произошла ошибка при получении меню");
            return View();
        }

        return RedirectToAction("GetAllCompanies", "Company");
    }
    public async Task<IActionResult> DeleteMenuRecord(Guid id_menu, Guid id_company)
    {
        try
        {
            var user = HttpContext.User;
            var roleString = user.FindFirst("Id_role");

            if (user.Identity is not null && user.Identity.IsAuthenticated && roleString is not null)
            {
                try
                {
                    int id_role = int.Parse(roleString.Value);
                    await _menuService.DeleteRecordFromMenuAsync(id_menu, id_role);
                    return RedirectToAction("GetMenuByCompany", new
                    {
                        companyId = id_company, // или другое значение
                        message = "Menu record deleted successfully"
                    });
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    ModelState.AddModelError("", "Произошла ошибка при удалении позиции из меню");
                }
            }
            else
            {
                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
                _logger.LogError("", "Пользователь не прошел проверку подлинности");
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e.Message);
            ModelState.AddModelError("", "Произошла ошибка при удалении позиции из меню");
        }
        return RedirectToAction("GetMenuByCompany", new
        {
            companyId = id_company, // или другое значение
            message = ""
        });
    }

    public async Task<IActionResult> GetCompaniesByDrink(Guid drink_id)
    {
        var model = new CompaniesListViewModel();
        try
        {
            var user = HttpContext.User;
            var roleString = user.FindFirst("Id_role");
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                try
                {
                    int id_role = int.Parse(roleString.Value); // Преобразуем строку в int
                    model.Companies = await _menuService.GetCompaniesByDrinkIdAsync(drink_id, id_role);
                    model.Id_role = id_role;
                    return View(model);
                }
                catch (DrinkNotFoundException e)
                {
                    _logger.LogInformation(e.Message);
                    ModelState.AddModelError("", "Напиток не был найден");
                }

                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    ModelState.AddModelError("", "Произошла ошибка при поиске сетей кофеен");
                    return View();
                }
            }

            try
            {
                model.Companies = await _menuService.GetCompaniesByDrinkIdAsync(drink_id, 1);
                model.Id_role = 1;

                return View(model);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                ModelState.AddModelError("", "Произошла ошибка при поиске сетей кофеен");
                return View();
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e.Message);
            ModelState.AddModelError("", "Произошла ошибка при получении данных");
            return View();
        }

    }

    [HttpGet]
    public async Task<IActionResult> AddRecordMenu(Guid id_company)
    {
        AddRecordMenuViewModel model = new AddRecordMenuViewModel();
        model.Id_company = id_company;
        var user = HttpContext.User;
        var roleString = user.FindFirst("Id_role");
        int id_role = int.Parse(roleString.Value);
        var alldrinks = await _drinkService.GetAllDrinksAsync(id_role);
        model.AvailableDrinks = alldrinks;
        model.Id_role = id_role;

        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRecordMenu(AddRecordMenuViewModel model)
    {
        _logger.LogInformation($"{model.Id_drink} {model.Id_company} {model.Size} {model.Price}");
        if (ModelState.IsValid)
        {
            try
            {
                var user = HttpContext.User;
                var roleString = user.FindFirst("Id_role");

                int id_role = int.Parse(roleString.Value);
                if (user.Identity is not null && user.Identity.IsAuthenticated && roleString is not null)
                {
                    var cs = new Menu(model.Id_drink, model.Id_company, model.Size, model.Size);
                    await _menuService.AddDrinkToMenuAsync(cs, id_role);
                    return RedirectToAction("GetMenuByCompany", new
                    {
                        companyId = model.Id_company, // или другое значение
                        message = "Menu record added successfully"
                    });
                }
                else
                {
                    ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
                    _logger.LogError("", "Пользователь не прошел проверку подлинности");
                }
            }
            catch (MenuIncorrectAtributeException e)
            {
                ModelState.AddModelError("", "Данные введены некорректно");
                _logger.LogError(e, e.Message);
                return View(model);
            }
            catch (InvalidDataException e)
            {
                ModelState.AddModelError("", "Данные введены некорректно");
                _logger.LogError(e, e.Message);
                return View(model);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Возникла ошибка при добавлении кофейни");
                _logger.LogError(e, e.Message);
                return View(model);
            }
        }

        else
        {

            _logger.LogWarning("ModelState is invalid");
            foreach (var error in ModelState)
            {
                _logger.LogWarning($"Field: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }
            return View(model);
        }
        return RedirectToAction("GetAllCompanies", "Company");
    }
}
